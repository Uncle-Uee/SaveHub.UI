using System.Collections.ObjectModel;
using System.IO;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SaveHub.Avalonia.Models;
using SaveHub.Avalonia.Services;
using SaveHub.Core;
using SaveHub.Core.Models;

namespace SaveHub.Avalonia.ViewModels;

/// <summary>Manage tab: browse and delete published saves (supports multi-select).</summary>
public sealed partial class ManageViewModel : ViewModelBase
{
    private readonly AppController _controller;
    private readonly IShellContext _shell;
    private readonly List<SaveRow> _selected = [];
    private readonly List<GameOption> _allGames = [];
    private readonly List<SaveRow> _allSaves = [];

    [ObservableProperty]
    private string? _selectedSystem;

    [ObservableProperty]
    private GameOption? _selectedGame;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelection))]
    [NotifyPropertyChangedFor(nameof(SelectedArchive))]
    [NotifyPropertyChangedFor(nameof(SelectedType))]
    [NotifyPropertyChangedFor(nameof(SelectedIndex))]
    [NotifyPropertyChangedFor(nameof(SelectedDescription))]
    private SaveRow? _selectedSave;

    [ObservableProperty]
    private string _gameFilter = string.Empty;

    [ObservableProperty]
    private string _saveFilter = string.Empty;

    [ObservableProperty]
    private string _nameText = string.Empty;

    [ObservableProperty]
    private Bitmap? _iconBitmap;

    public ObservableCollection<string> Systems { get; } = [];

    public ObservableCollection<GameOption> Games { get; } = [];

    public ObservableCollection<SaveRow> Saves { get; } = [];

    public bool HasSelection => SelectedSave is not null;

    public string SelectedArchive => SelectedSave?.Archive ?? string.Empty;

    public string SelectedType => SelectedSave?.Type ?? string.Empty;

    public string SelectedIndex => SelectedSave is { } row ? row.Entry.Index.ToString() : string.Empty;

    public string SelectedDescription => string.IsNullOrWhiteSpace(SelectedSave?.Entry.Description) ? "(none)" : SelectedSave!.Entry.Description!;

    internal ManageViewModel(AppController controller, IShellContext shell)
    {
        _controller = controller;
        _shell = shell;
    }

    public override async Task OnActivatedAsync()
    {
        if (Systems.Count == 0)
        {
            await LoadSystems();
        }
    }

    public void SetSelection(IEnumerable<SaveRow> rows)
    {
        _selected.Clear();
        _selected.AddRange(rows);
    }

    [RelayCommand]
    private async Task RefreshSystems()
    {
        await LoadSystems();
    }

    [RelayCommand]
    private async Task DownloadSelected()
    {
        if (_selected.Count == 0)
        {
            await _shell.WarnAsync("Select one or more saves to download.");
            return;
        }
        SaveHubClient? client = await _shell.RequireClientAsync();
        if (client is null)
        {
            return;
        }
        string? folder = await _shell.PickFolderAsync("Choose a folder for the downloaded saves");
        if (folder is null)
        {
            return;
        }

        List<SaveRow> rows = _selected.ToList();
        await _shell.RunBusy($"Downloading {rows.Count} save(s)...", async () =>
        {
            int ok = 0;
            foreach (SaveRow row in rows)
            {
                SaveEntry entry = row.Entry;
                string destination = Path.Combine(folder, entry.ArchiveName);
                if (await _controller.DownloadArchiveToFileAsync(client, entry.Platform, entry.GameId, entry.ArchiveName, destination))
                {
                    ok++;
                }
            }
            _shell.SetStatus($"Downloaded {ok} of {rows.Count} save(s) to {folder}.");
        });
    }

    [RelayCommand]
    private async Task DeleteSelected()
    {
        if (_selected.Count == 0)
        {
            await _shell.WarnAsync("Select one or more saves to delete.");
            return;
        }
        List<SaveEntry> entries = _selected.Select(r => r.Entry).ToList();

        bool confirm = await _shell.ConfirmAsync("Confirm delete", $"Delete {entries.Count} save(s)? This cannot be undone.");
        if (!confirm)
        {
            return;
        }

        SaveHubClient? client = await _shell.RequireClientAsync();
        if (client is null)
        {
            return;
        }

        await _shell.RunBusy($"Deleting {entries.Count} save(s)...", async () =>
        {
            foreach (SaveEntry entry in entries)
            {
                await _controller.DeleteSaveAsync(client, entry.Platform, entry.GameId, entry.ArchiveName);
            }
            _shell.SetStatus($"Deleted {entries.Count} save(s).");
            await LoadManageSaves();
        });
    }

    [RelayCommand]
    private async Task DeleteAll()
    {
        if (_allSaves.Count == 0 || SelectedGame is not { } game)
        {
            await _shell.WarnAsync("Select a game with saves first.");
            return;
        }
        List<SaveEntry> entries = _allSaves.Select(r => r.Entry).ToList();

        bool confirm = await _shell.ConfirmAsync("Confirm delete",
            $"Delete ALL {entries.Count} save(s) for {game.Display}? This cannot be undone.");
        if (!confirm)
        {
            return;
        }

        SaveHubClient? client = await _shell.RequireClientAsync();
        if (client is null)
        {
            return;
        }

        await _shell.RunBusy($"Deleting {entries.Count} save(s)...", async () =>
        {
            foreach (SaveEntry entry in entries)
            {
                await _controller.DeleteSaveAsync(client, entry.Platform, entry.GameId, entry.ArchiveName);
            }
            _shell.SetStatus($"Deleted all {entries.Count} save(s) for {game.Display}.");
            await LoadManageSaves();
        });
    }

    [RelayCommand]
    private async Task RenameGame()
    {
        if (SelectedSystem is not string system || SelectedGame is not { } game)
        {
            await _shell.WarnAsync("Select a game to rename first.");
            return;
        }
        int idx = game.Display.LastIndexOf(" (", StringComparison.Ordinal);
        string suggested = idx > 0 ? game.Display[..idx] : string.Empty;
        string? input = await _shell.PromptAsync("Rename game", $"Display name for {game.Id}:", suggested);
        if (input is null)
        {
            return;
        }
        string name = input.Trim();
        if (name.Length == 0)
        {
            await _shell.WarnAsync("Enter a name.");
            return;
        }
        SaveHubClient? client = await _shell.RequireClientAsync();
        if (client is null)
        {
            return;
        }
        await _shell.RunBusy("Renaming...", async () =>
        {
            await _controller.SetGameNameAsync(client, system, game.Id, name);
            _shell.SetStatus($"Renamed {game.Id} to {name}.");
            await LoadManageGames();
        });
    }

    private async Task LoadSystems()
    {
        SaveHubClient? client = _shell.TryCreateClient();
        if (client is null)
        {
            _shell.SetStatus(NoProviderMessage);
            return;
        }
        await _shell.RunBusy("Loading systems...", async () =>
        {
            string? current = SelectedSystem;
            Systems.Clear();
            foreach (string p in await _controller.ListPlatformsAsync(client))
            {
                Systems.Add(p);
            }
            if (current is not null && Systems.Contains(current))
            {
                SelectedSystem = current;
            }
        });
    }

    private async Task LoadManageGames()
    {
        SaveHubClient? client = _shell.TryCreateClient();
        if (client is null || SelectedSystem is not string system)
        {
            return;
        }
        await _shell.RunBusy($"Loading {system} games...", async () =>
        {
            _allGames.Clear();
            _allSaves.Clear();
            Saves.Clear();
            IconBitmap = null;
            NameText = string.Empty;
            IReadOnlyDictionary<string, string> names = await _controller.GetPlatformNamesAsync(client, system);
            foreach (string game in await _controller.ListGamesAsync(client, system))
            {
                _allGames.Add(new GameOption(game, AppController.GameDisplay(game, names)));
            }
            ApplyGameFilter();
        });
    }

    private async Task LoadManageSaves()
    {
        SaveHubClient? client = _shell.TryCreateClient();
        if (client is null || SelectedSystem is not string system || SelectedGame is not { } game)
        {
            return;
        }
        await _shell.RunBusy("Loading saves...", async () =>
        {
            _allSaves.Clear();
            foreach (SaveEntry s in await _controller.ListSavesAsync(client, system, game.Id))
            {
                _allSaves.Add(new SaveRow(s.ArchiveName, AppController.Label(s.SaveType), s.Description ?? string.Empty, s));
            }
            ApplySaveFilter();

            IReadOnlyDictionary<string, string> names = await _controller.GetPlatformNamesAsync(client, system);
            NameText = names.TryGetValue(game.Id, out string? n) ? $"{n}\n{game.Id}" : game.Id;
            try
            {
                byte[]? icon = await _controller.GetGameIconAsync(client, system, game.Id);
                IconBitmap = icon is null ? null : new Bitmap(new MemoryStream(icon));
            }
            catch
            {
                IconBitmap = null;
            }
        });
    }

    partial void OnSelectedSystemChanged(string? value)
    {
        _ = LoadManageGames();
    }

    partial void OnSelectedGameChanged(GameOption? value)
    {
        _ = LoadManageSaves();
    }

    partial void OnGameFilterChanged(string value)
    {
        ApplyGameFilter();
    }

    partial void OnSaveFilterChanged(string value)
    {
        ApplySaveFilter();
    }

    private void ApplyGameFilter()
    {
        string filter = GameFilter.Trim();
        Games.Clear();
        foreach (GameOption game in _allGames)
        {
            if (filter.Length == 0 ||
                game.Id.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                game.Display.Contains(filter, StringComparison.OrdinalIgnoreCase))
            {
                Games.Add(game);
            }
        }
    }

    private void ApplySaveFilter()
    {
        string filter = SaveFilter.Trim();
        Saves.Clear();
        foreach (SaveRow row in _allSaves)
        {
            if (filter.Length == 0 ||
                row.Archive.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                row.Type.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                row.Description.Contains(filter, StringComparison.OrdinalIgnoreCase))
            {
                Saves.Add(row);
            }
        }
    }
}
