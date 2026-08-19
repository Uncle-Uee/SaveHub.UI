using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SaveHub.Avalonia.Models;
using SaveHub.Avalonia.Services;
using SaveHub.Core;
using SaveHub.Core.Abstractions;
using SaveHub.Core.Archiving;
using SaveHub.Core.Models;

namespace SaveHub.Avalonia.ViewModels;

/// <summary>Upload tab: pick save files/folder, detect metadata, and upload.</summary>
public sealed partial class UploadViewModel : ViewModelBase
{
    private const int MaxDescription = 256;

    private readonly AppController _controller;
    private readonly IShellContext _shell;
    private readonly List<string> _upFilesList = [];

    private string? _upRoot;
    private string? _upIconPath;
    private string? _upDevice;

    [ObservableProperty]
    private string _selectedDeviceDisplay = "Select a device";

    [ObservableProperty]
    private bool _isSaveState;

    [ObservableProperty]
    private bool _isSaveFolder;

    [ObservableProperty]
    private bool _isMemoryCard = true;

    [ObservableProperty]
    private bool _isBulk;

    [ObservableProperty]
    private string _pathText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DescriptionCount))]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _titleId = string.Empty;

    [ObservableProperty]
    private string _gameName = string.Empty;

    public ObservableCollection<FileRow> Files { get; } = [];

    public ObservableCollection<BulkCardRow> BulkCards { get; } = [];

    public int MaxDescriptionLength => MaxDescription;

    public string DescriptionCount => $"{Description.Length}/{MaxDescription}";

    internal UploadViewModel(AppController controller, IShellContext shell)
    {
        _controller = controller;
        _shell = shell;
    }

    [RelayCommand]
    private async Task BrowseFolder()
    {
        if (IsBulk)
        {
            string? bulkFolder = await _shell.PickFolderAsync("Select the folder of memory cards");
            if (bulkFolder is null)
            {
                return;
            }
            PopulateBulkGrid(Directory.GetFiles(bulkFolder, "*", SearchOption.TopDirectoryOnly));
            return;
        }
        IsSaveFolder = true;
        await Browse();
    }

    [RelayCommand]
    private async Task Browse()
    {
        if (IsBulk)
        {
            IReadOnlyList<string> bulkFiles = await _shell.PickFilesAsync("Select memory-card files", true, null, null);
            if (bulkFiles.Count > 0)
            {
                PopulateBulkGrid(bulkFiles);
            }
            return;
        }

        SaveType type = SelectedSaveType();
        _upFilesList.Clear();
        _upRoot = null;

        if (type == SaveType.SaveFolder)
        {
            string? folder = await _shell.PickFolderAsync("Select the save folder");
            if (folder is null)
            {
                return;
            }
            _upRoot = folder;
            _upFilesList.AddRange(Directory.GetFiles(folder, "*", SearchOption.AllDirectories));
            PathText = folder;
        }
        else
        {
            IReadOnlyList<string> files = await _shell.PickFilesAsync("Select save file(s)", type != SaveType.MemoryCard, null, null);
            if (files.Count == 0)
            {
                return;
            }
            _upFilesList.AddRange(files);
            PathText = files.Count == 1 ? files[0] : $"{files.Count} files selected";
        }

        PopulateFileList();

        TitleId = string.Empty;
        GameName = string.Empty;

        if (type == SaveType.MemoryCard && _upFilesList.Count > 0 &&
            _controller.DetectMemoryCardPlatform(_upFilesList[0]) is { } detected)
        {
            SetDeviceByCode(detected);
            _shell.SetStatus($"Detected {detected} memory card.");
        }
        else if (type == SaveType.SaveFolder &&
                 _controller.DetectFolderPlatform(_upFilesList) is { } folderPlatform)
        {
            SetDeviceByCode(folderPlatform);
            _shell.SetStatus($"Detected {folderPlatform} save folder.");
        }

        DetectTitleIdCore();
    }

    [RelayCommand]
    private async Task SelectIcon()
    {
        IReadOnlyList<string> files = await _shell.PickFilesAsync(
            "Select icon", false, "Images", ["*.png", "*.jpg", "*.jpeg", "*.webp", "*.gif", "*.bmp"]);
        if (files.Count > 0)
        {
            _upIconPath = files[0];
            _shell.SetStatus($"Icon selected: {Path.GetFileName(_upIconPath)}");
        }
    }

    [RelayCommand]
    private async Task DetectTitleId()
    {
        if (string.IsNullOrEmpty(_upDevice))
        {
            await _shell.WarnAsync("Select a device type first.");
            return;
        }
        if (_upFilesList.Count == 0)
        {
            await _shell.WarnAsync("Browse and select the save file(s) first.");
            return;
        }

        string? id = _controller.DetectTitleId(_upDevice, SelectedSaveType(), _upFilesList);
        if (!string.IsNullOrWhiteSpace(id))
        {
            TitleId = id;
            _shell.SetStatus($"Detected Title ID: {id}");
        }
        else
        {
            _shell.SetStatus("No Title ID found on this save. Enter a Game Name instead (used as the folder).");
        }

        if (GameName.Trim().Length == 0 && _controller.DetectSaveName(_upDevice, _upFilesList) is { } name)
        {
            GameName = name;
        }

        string lookupId = TitleId.Trim();
        if (GameName.Trim().Length == 0 && lookupId.Length > 0 &&
            _shell.TryCreateClient() is { } client &&
            await _controller.LookupExistingGameNameAsync(client, _upDevice, lookupId) is { } existingName)
        {
            GameName = existingName;
        }
    }

    [RelayCommand]
    private async Task Submit()
    {
        if (string.IsNullOrEmpty(_upDevice))
        {
            await _shell.WarnAsync("Select a device type.");
            return;
        }
        if (IsBulk)
        {
            await SubmitBulkAsync();
            return;
        }
        if (_upFilesList.Count == 0)
        {
            await _shell.WarnAsync("Browse and select the save file(s).");
            return;
        }
        string description = Description.Trim();
        if (description.Length == 0)
        {
            await _shell.WarnAsync("Enter a description.");
            return;
        }

        string titleId = TitleId.Trim();
        string gameName = GameName.Trim();
        if (SelectedSaveType() == SaveType.SaveFolder &&
            string.Equals(_upDevice, "PC", StringComparison.OrdinalIgnoreCase) && gameName.Length == 0)
        {
            await _shell.WarnAsync("Enter the game name for this desktop save folder.");
            return;
        }
        GameIdResolution resolution = _controller.Resolve(
            _upDevice, SelectedSaveType(), _upFilesList,
            titleId.Length == 0 ? null : titleId,
            gameName.Length == 0 ? null : gameName);
        string gameId = resolution.GameId;

        SaveHubClient? client = await _shell.RequireClientAsync();
        if (client is null)
        {
            return;
        }

        SaveUploadRequest request = new SaveUploadRequest
        {
            Platform = _upDevice,
            GameId = gameId,
            SaveType = SelectedSaveType(),
            Files = _upFilesList.ToList(),
            RootDirectory = SelectedSaveType() == SaveType.SaveFolder ? _upRoot : null,
            Description = description,
            GameTitle = gameName.Length == 0 ? null : gameName,
            IconPath = _upIconPath,
            AutoFetchCoverArt = _upIconPath is null,
        };

        await _shell.RunBusy("Uploading...", async () =>
        {
            SaveUploadResult result = await _controller.UploadAsync(client, request, new UploadOptions());
            await _shell.ShowResultAsync(result);

            if (result.Merged)
            {
                _controller.CacheGameName(_upDevice, gameId, gameName.Length == 0 ? null : gameName);
            }

            if (_controller.IsNintendo(_upDevice) && _upIconPath is null)
            {
                _shell.SetStatus($"Uploaded. No cover art for {gameId} — add one later via Select Icon or the Edit tab.");
            }
        });
    }

    [RelayCommand]
    private void SelectDevice(string? code)
    {
        if (string.IsNullOrEmpty(code))
        {
            return;
        }
        ApplyDevice(code);
        TitleId = string.Empty;
        GameName = string.Empty;
        DetectTitleIdCore();
    }

    private void SetDeviceByCode(string code)
    {
        ApplyDevice(code);
    }

    private void ApplyDevice(string code)
    {
        _upDevice = code;
        SelectedDeviceDisplay = DeviceDisplay(code);
        if (code == "PC")
        {
            IsSaveFolder = true;
        }
    }

    private static string DeviceDisplay(string code)
    {
        foreach (DeviceGroup group in Devices.Groups)
        {
            foreach (DeviceOption option in group.Consoles)
            {
                if (option.Code == code)
                {
                    return $"{group.Manufacturer} \u2014 {option.Display}";
                }
            }
        }
        return code;
    }

    private SaveType SelectedSaveType()
    {
        if (IsSaveFolder)
        {
            return SaveType.SaveFolder;
        }
        if (IsMemoryCard)
        {
            return SaveType.MemoryCard;
        }
        return SaveType.SaveState;
    }

    private void DetectTitleIdCore()
    {
        if (string.IsNullOrEmpty(_upDevice) || _upFilesList.Count == 0)
        {
            return;
        }
        if (TitleId.Trim().Length == 0)
        {
            string? id = _controller.DetectTitleId(_upDevice, SelectedSaveType(), _upFilesList);
            if (!string.IsNullOrWhiteSpace(id))
            {
                TitleId = id;
                _shell.SetStatus($"Detected Title ID: {id}");
            }
        }
        if (GameName.Trim().Length == 0 && _controller.DetectSaveName(_upDevice, _upFilesList) is { } name)
        {
            GameName = name;
        }
    }

    private void PopulateFileList()
    {
        Files.Clear();
        foreach (string path in _upFilesList)
        {
            long size = new FileInfo(path).Length;
            Files.Add(new FileRow(Path.GetFileName(path), AppController.FormatSize(size), path));
        }
    }

    partial void OnIsBulkChanged(bool value)
    {
        if (value)
        {
            IsMemoryCard = true;
        }
    }

    private void PopulateBulkGrid(IReadOnlyList<string> files)
    {
        if (string.IsNullOrEmpty(_upDevice) && files.Count > 0 &&
            _controller.DetectMemoryCardPlatform(files[0]) is { } detectedPlatform)
        {
            SetDeviceByCode(detectedPlatform);
        }

        BulkCards.Clear();
        string platform = _upDevice ?? string.Empty;
        foreach (string file in files)
        {
            string id = platform.Length > 0
                ? _controller.DetectTitleId(platform, SaveType.MemoryCard, [file]) ?? string.Empty
                : string.Empty;
            string name = platform.Length > 0
                ? _controller.DetectSaveName(platform, [file]) ?? string.Empty
                : string.Empty;
            BulkCards.Add(new BulkCardRow(file, Path.GetFileName(file), name, id, BulkCardRow.ModeUploadIndex));
        }
        PathText = $"{files.Count} memory card(s) loaded";
        _shell.SetStatus($"Loaded {files.Count} memory card(s). Choose an action per card, then Upload.");
    }

    private async Task SubmitBulkAsync()
    {
        if (BulkCards.Count == 0)
        {
            await _shell.WarnAsync("Browse a folder (or files) of memory cards first.");
            return;
        }
        SaveHubClient? client = await _shell.RequireClientAsync();
        if (client is null)
        {
            return;
        }

        string platform = _upDevice!;
        List<MemoryCardIndexEntry> entries = new List<MemoryCardIndexEntry>();
        int uploaded = 0;
        await _shell.RunBusy("Uploading memory cards...", async () =>
        {
            foreach (BulkCardRow card in BulkCards)
            {
                string game = card.Game.Trim();
                string id = card.TitleId.Trim();
                GameIdResolution resolution = _controller.Resolve(
                    platform, SaveType.MemoryCard, [card.Path],
                    id.Length == 0 ? null : id,
                    game.Length == 0 ? null : game);
                string gameId = resolution.GameId;
                string displayName = game.Length > 0 ? game : gameId;
                if (card.Mode == BulkCardRow.ModeUploadIndex)
                {
                    SaveUploadRequest request = new SaveUploadRequest
                    {
                        Platform = platform,
                        GameId = gameId,
                        SaveType = SaveType.MemoryCard,
                        Files = [card.Path],
                        Description = displayName,
                        GameTitle = game.Length > 0 ? game : null,
                        AutoFetchCoverArt = true,
                    };
                    await _controller.UploadAsync(client, request, new UploadOptions());
                    uploaded++;
                }
                entries.Add(new MemoryCardIndexEntry(gameId, displayName));
            }
            await _controller.UpdateMemoryCardIndexAsync(client, platform, entries);
        });
        _shell.SetStatus($"Bulk complete: {uploaded} uploaded, {entries.Count} indexed in {platform}/{SaveNaming.MemoryCardIndexFolderName}.");
    }
}
