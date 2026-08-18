using System.Collections.ObjectModel;
using System.IO;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SaveHub.Avalonia.Models;
using SaveHub.Avalonia.Services;
using SaveHub.Core;
using SaveHub.Core.Abstractions;
using SaveHub.Core.Models;

namespace SaveHub.Avalonia.ViewModels;

/// <summary>Edit tab: replace an existing save's contents/description in place.</summary>
public sealed partial class EditViewModel : ViewModelBase
{
    private readonly AppController _controller;
    private readonly IShellContext _shell;
    private readonly List<string> _edFilesList = [];

    private string? _edRoot;
    private SaveEntry? _edSelectedEntry;

    [ObservableProperty]
    private string? _selectedSystem;

    [ObservableProperty]
    private string? _selectedGame;

    [ObservableProperty]
    private SaveRow? _selectedSave;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _pathText = string.Empty;

    [ObservableProperty]
    private string _nameText = string.Empty;

    [ObservableProperty]
    private Bitmap? _iconBitmap;

    public ObservableCollection<string> Systems { get; } = [];

    public ObservableCollection<string> Games { get; } = [];

    public ObservableCollection<SaveRow> Saves { get; } = [];

    internal EditViewModel(AppController controller, IShellContext shell)
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
    private async Task Browse()
    {
        if (_edSelectedEntry is null)
        {
            await _shell.WarnAsync("Select a save to replace first.");
            return;
        }
        _edFilesList.Clear();
        _edRoot = null;

        if (_edSelectedEntry.SaveType == SaveType.SaveFolder)
        {
            string? folder = await _shell.PickFolderAsync("Select the replacement folder");
            if (folder is null)
            {
                return;
            }
            _edRoot = folder;
            _edFilesList.AddRange(Directory.GetFiles(folder, "*", SearchOption.AllDirectories));
            PathText = folder;
        }
        else
        {
            IReadOnlyList<string> files = await _shell.PickFilesAsync(
                "Select replacement file(s)", _edSelectedEntry.SaveType != SaveType.MemoryCard, null, null);
            if (files.Count == 0)
            {
                return;
            }
            _edFilesList.AddRange(files);
            PathText = files.Count == 1 ? files[0] : $"{files.Count} files selected";
        }
    }

    [RelayCommand]
    private async Task Update()
    {
        if (_edSelectedEntry is null)
        {
            await _shell.WarnAsync("Select a save to replace.");
            return;
        }
        if (_edFilesList.Count == 0)
        {
            await _shell.WarnAsync("Browse the replacement file(s)/folder.");
            return;
        }
        string description = Description.Trim();
        if (description.Length == 0)
        {
            await _shell.WarnAsync("Enter a description.");
            return;
        }
        SaveHubClient? client = await _shell.RequireClientAsync();
        if (client is null)
        {
            return;
        }

        SaveEntry entry = _edSelectedEntry;
        SaveUploadRequest request = new SaveUploadRequest
        {
            Platform = entry.Platform,
            GameId = entry.GameId,
            SaveType = entry.SaveType,
            Files = _edFilesList.ToList(),
            RootDirectory = entry.SaveType == SaveType.SaveFolder ? _edRoot : null,
            Description = description,
            AutoFetchCoverArt = false,
        };

        await _shell.RunBusy($"Updating {entry.ArchiveName}...", async () =>
        {
            SaveUploadResult result = await _controller.UploadAsync(client, request, new UploadOptions { TargetIndex = entry.Index });
            await _shell.ShowResultAsync(result);
            await LoadEditSaves();
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

    private async Task LoadEditGames()
    {
        SaveHubClient? client = _shell.TryCreateClient();
        if (client is null || SelectedSystem is not string system)
        {
            return;
        }
        await _shell.RunBusy($"Loading {system} games...", async () =>
        {
            Games.Clear();
            Saves.Clear();
            foreach (string game in await _controller.ListGamesAsync(client, system))
            {
                Games.Add(game);
            }
        });
    }

    private async Task LoadEditSaves()
    {
        SaveHubClient? client = _shell.TryCreateClient();
        if (client is null || SelectedSystem is not string system || SelectedGame is not string game)
        {
            return;
        }
        await _shell.RunBusy("Loading saves...", async () =>
        {
            Saves.Clear();
            foreach (SaveEntry s in await _controller.ListSavesAsync(client, system, game))
            {
                Saves.Add(new SaveRow(s.ArchiveName, AppController.Label(s.SaveType), s.Description ?? string.Empty, s));
            }

            IReadOnlyDictionary<string, string> names = await _controller.GetGameNamesAsync(client, system);
            NameText = names.TryGetValue(game, out string? n) ? $"{n}\n{game}" : game;
            try
            {
                byte[]? icon = await _controller.GetGameIconAsync(client, system, game);
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
        _ = LoadEditGames();
    }

    partial void OnSelectedGameChanged(string? value)
    {
        _ = LoadEditSaves();
    }

    partial void OnSelectedSaveChanged(SaveRow? value)
    {
        _edSelectedEntry = value?.Entry;
        if (_edSelectedEntry is not null)
        {
            Description = _edSelectedEntry.Description ?? string.Empty;
        }
    }
}
