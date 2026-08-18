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

/// <summary>Download tab: browse published saves by system and download an archive.</summary>
public sealed partial class DownloadViewModel : ViewModelBase
{
    private readonly AppController _controller;
    private readonly IShellContext _shell;

    private Dictionary<string, string> _dlNames = new(StringComparer.OrdinalIgnoreCase);

    [ObservableProperty]
    private string? _selectedSystem;

    [ObservableProperty]
    private DownloadRow? _selectedSave;

    [ObservableProperty]
    private string _nameText = string.Empty;

    [ObservableProperty]
    private Bitmap? _iconBitmap;

    public ObservableCollection<string> Systems { get; } = [];

    public ObservableCollection<DownloadRow> Saves { get; } = [];

    internal DownloadViewModel(AppController controller, IShellContext shell)
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

    [RelayCommand]
    private async Task RefreshSystems()
    {
        await LoadSystems();
    }

    [RelayCommand]
    private async Task Download()
    {
        if (SelectedSave is not { } row)
        {
            await _shell.WarnAsync("Select a save to download.");
            return;
        }
        SaveHubClient? client = await _shell.RequireClientAsync();
        if (client is null)
        {
            return;
        }

        string? destination = await _shell.PickSaveFileAsync(row.Archive);
        if (destination is null)
        {
            return;
        }

        await _shell.RunBusy("Downloading...", async () =>
        {
            bool ok = await _controller.DownloadArchiveToFileAsync(client, row.System, row.Game, row.Archive, destination);
            if (ok)
            {
                _shell.SetStatus($"Downloaded to {destination}");
            }
            else
            {
                await _shell.WarnAsync("The archive could not be found.");
            }
        });
    }

    private async Task LoadSystems()
    {
        SaveHubClient? client = _shell.TryCreateClient();
        if (client is null)
        {
            _shell.SetStatus("No storage provider is ready — open Settings to configure or sign in.");
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

    private async Task LoadDownloadSaves()
    {
        SaveHubClient? client = _shell.TryCreateClient();
        if (client is null || SelectedSystem is not string system)
        {
            return;
        }

        await _shell.RunBusy($"Loading {system} saves...", async () =>
        {
            Saves.Clear();
            IconBitmap = null;
            NameText = string.Empty;
            _dlNames = new Dictionary<string, string>(await _controller.GetGameNamesAsync(client, system), StringComparer.OrdinalIgnoreCase);
            IReadOnlyList<string> games = await _controller.ListGamesAsync(client, system);
            foreach (string game in games)
            {
                string name = _dlNames.TryGetValue(game, out string? n) ? n : game;
                IReadOnlyList<SaveEntry> saves = await _controller.ListSavesAsync(client, system, game);
                foreach (SaveEntry s in saves)
                {
                    Saves.Add(new DownloadRow(name, game, s.ArchiveName, AppController.Label(s.SaveType), s.Description ?? string.Empty, system, game));
                }
            }
        });
    }

    private async Task ShowIcon()
    {
        if (SelectedSave is not { } row)
        {
            return;
        }
        NameText = _dlNames.TryGetValue(row.Game, out string? n) ? $"{n}\n{row.Game}" : row.Game;
        SaveHubClient? client = _shell.TryCreateClient();
        if (client is null)
        {
            return;
        }
        try
        {
            byte[]? bytes = await _controller.GetGameIconAsync(client, row.System, row.Game);
            IconBitmap = bytes is null ? null : new Bitmap(new MemoryStream(bytes));
        }
        catch
        {
            IconBitmap = null;
        }
    }

    partial void OnSelectedSystemChanged(string? value)
    {
        _ = LoadDownloadSaves();
    }

    partial void OnSelectedSaveChanged(DownloadRow? value)
    {
        _ = ShowIcon();
    }
}
