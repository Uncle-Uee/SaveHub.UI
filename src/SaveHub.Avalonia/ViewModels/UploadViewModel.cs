using System.Collections.ObjectModel;
using System.IO;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SaveHub.Avalonia.Common;
using SaveHub.Avalonia.Models;
using SaveHub.Avalonia.Services;
using SaveHub.Core;
using SaveHub.Core.Abstractions;
using SaveHub.Core.Archiving;
using SaveHub.Core.Models;

namespace SaveHub.Avalonia.ViewModels;

/// <summary>Upload tab: stage up to ten memory cards (or one folder), each with its own metadata, and upload them.</summary>
public sealed partial class UploadViewModel : ViewModelBase
{
    private const int MaxDescription = 256;
    private const int MaxCards = 10;

    private readonly AppController _controller;
    private readonly IShellContext _shell;
    private readonly List<PendingSave> _items = [];
    private readonly PendingUploadStore _store;
    private PendingSave? _current;
    private bool _syncingFields;
    private bool _suppressSelection;

    [ObservableProperty]
    private string _selectedDeviceDisplay = "Select a device";

    [ObservableProperty]
    private bool _isSaveState;

    [ObservableProperty]
    private bool _isSaveFolder;

    [ObservableProperty]
    private bool _isMemoryCard = true;

    [ObservableProperty]
    private FileRow? _selectedFile;

    [ObservableProperty]
    private string _pathText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DescriptionCount))]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _titleId = string.Empty;

    [ObservableProperty]
    private string _gameName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasIconPreview))]
    private Bitmap? _iconPreview;

    public ObservableCollection<FileRow> Files { get; } = [];

    public int MaxDescriptionLength => MaxDescription;

    public string DescriptionCount => $"{Description.Length}/{MaxDescription}";

    /// <summary>True when a cover icon has been selected and can be previewed.</summary>
    public bool HasIconPreview => IconPreview is not null;

    internal UploadViewModel(AppController controller, IShellContext shell)
    {
        _controller = controller;
        _shell = shell;
        _store = new PendingUploadStore(PendingUploadStore.DefaultPath);
        _store.Clear();
    }

    // ---------------------------------------------------------------- Staging

    [RelayCommand]
    private async Task BrowseFolder()
    {
        IsSaveFolder = true;
        await Browse();
    }

    [RelayCommand]
    private async Task Browse()
    {
        SaveType type = SelectedSaveType();
        if (type == SaveType.SaveFolder)
        {
            string? folder = await _shell.PickFolderAsync("Select the save folder");
            if (folder is null)
            {
                return;
            }
            _items.Clear();
            PendingSave item = new PendingSave
            {
                Key = folder,
                SaveType = SaveType.SaveFolder,
                Files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories).ToList(),
                RootDirectory = folder,
                DisplayName = Path.GetFileName(folder.TrimEnd(Path.DirectorySeparatorChar)),
            };
            if (_controller.DetectFolderPlatform(item.Files) is { } platform)
            {
                item.Device = platform;
            }
            DetectInto(item);
            _items.Add(item);
        }
        else if (type == SaveType.SaveState)
        {
            IReadOnlyList<string> files = await _shell.PickFilesAsync("Select save-state file(s)", true, null, null);
            if (files.Count == 0)
            {
                return;
            }
            _items.Clear();
            PendingSave item = new PendingSave
            {
                Key = files[0],
                SaveType = SaveType.SaveState,
                Files = files.ToList(),
                DisplayName = Path.GetFileName(files[0]),
            };
            DetectInto(item);
            _items.Add(item);
        }
        else
        {
            IReadOnlyList<string> files = await _shell.PickFilesAsync("Select memory card(s)", true, null, null);
            if (files.Count == 0)
            {
                return;
            }
            AddCardFiles(files);
        }

        PopulateList();
        SelectItem(_items.Count > 0 ? _items[0] : null);
        Persist();
    }

    [RelayCommand]
    private async Task AddSingleFile()
    {
        IReadOnlyList<string> files = await _shell.PickFilesAsync("Add a save file", false, null, null);
        AddToUpload(files);
    }

    [RelayCommand]
    private async Task AddFiles()
    {
        IReadOnlyList<string> files = await _shell.PickFilesAsync("Add save file(s)", true, null, null);
        AddToUpload(files);
    }

    [RelayCommand]
    private async Task AddFolder()
    {
        string? folder = await _shell.PickFolderAsync("Add a save folder");
        if (folder is null)
        {
            return;
        }
        AddToUpload(Directory.GetFiles(folder, "*", SearchOption.AllDirectories));
    }

    private void AddToUpload(IReadOnlyList<string> files)
    {
        if (files.Count == 0)
        {
            return;
        }
        if (SelectedSaveType() == SaveType.MemoryCard)
        {
            AddCardFiles(files);
            PopulateList();
            SelectItem(_items.Count > 0 ? _items[^1] : null);
        }
        else if (_current is not null)
        {
            _current.Files.AddRange(files);
            RefreshRow(_current);
            PathText = ItemDetails(_current);
        }
        Persist();
    }

    [RelayCommand]
    private void RemoveFile()
    {
        if (_current is null)
        {
            _shell.SetStatus("Select a save to remove.");
            return;
        }
        _items.Remove(_current);
        _current = null;
        PopulateList();
        SelectItem(_items.Count > 0 ? _items[0] : null);
        Persist();
    }

    [RelayCommand]
    private async Task SelectIcon()
    {
        if (_current is null)
        {
            _shell.SetStatus("Select a save in the list first.");
            return;
        }
        IReadOnlyList<string> files = await _shell.PickFilesAsync(
            "Select icon", false, "Images", ["*.png", "*.jpg", "*.jpeg", "*.webp", "*.gif", "*.bmp"]);
        if (files.Count > 0)
        {
            _current.IconPath = files[0];
            UpdateCoverPreview();
            _shell.SetStatus($"Icon selected: {Path.GetFileName(_current.IconPath)}");
            Persist();
        }
    }

    private void AddCardFiles(IReadOnlyList<string> paths)
    {
        _items.RemoveAll(i => i.SaveType != SaveType.MemoryCard);
        foreach (string path in paths)
        {
            if (_items.Count >= MaxCards)
            {
                _shell.SetStatus($"Up to {MaxCards} memory cards per upload. Extra files were skipped.");
                break;
            }
            if (_items.Any(i => string.Equals(i.Key, path, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }
            PendingSave item = new PendingSave
            {
                Key = path,
                SaveType = SaveType.MemoryCard,
                Files = [path],
                DisplayName = Path.GetFileName(path),
            };
            if (_controller.DetectMemoryCardPlatform(path) is { } detected)
            {
                item.Device = detected;
            }
            DetectInto(item);
            _items.Add(item);
        }
    }

    // ---------------------------------------------------------------- Device / detection

    [RelayCommand]
    private void SelectDevice(string? code)
    {
        if (string.IsNullOrEmpty(code) || _current is null)
        {
            return;
        }
        _current.Device = code;
        SelectedDeviceDisplay = DeviceDisplay(code);
        if (code == "PC")
        {
            IsSaveFolder = true;
        }
        _current.TitleId = string.Empty;
        _current.GameName = string.Empty;
        DetectInto(_current);
        _syncingFields = true;
        TitleId = _current.TitleId;
        GameName = _current.GameName;
        _syncingFields = false;
        RefreshRow(_current);
        UpdateCoverPreview();
        Persist();
    }

    private void DetectInto(PendingSave item)
    {
        if (string.IsNullOrEmpty(item.Device) || item.Files.Count == 0)
        {
            return;
        }
        if (item.TitleId.Trim().Length == 0)
        {
            string? id = _controller.DetectTitleId(item.Device, item.SaveType, item.Files);
            if (!string.IsNullOrWhiteSpace(id))
            {
                item.TitleId = id;
            }
        }
        if (item.GameName.Trim().Length == 0 && _controller.DetectSaveName(item.Device, item.Files) is { } name)
        {
            item.GameName = name;
        }
    }

    [RelayCommand]
    private async Task DetectTitleId()
    {
        if (_current is null)
        {
            await _shell.WarnAsync("Select a save in the list first.");
            return;
        }
        if (string.IsNullOrEmpty(_current.Device))
        {
            await _shell.WarnAsync("Select a device type first.");
            return;
        }

        string? id = _controller.DetectTitleId(_current.Device, _current.SaveType, _current.Files);
        if (!string.IsNullOrWhiteSpace(id))
        {
            TitleId = id;
            _shell.SetStatus($"Detected Title ID: {id}");
        }
        else
        {
            _shell.SetStatus("No Title ID found on this save. Enter a Game Name instead (used as the folder).");
        }

        if (GameName.Trim().Length == 0 && _controller.DetectSaveName(_current.Device, _current.Files) is { } name)
        {
            GameName = name;
        }

        string lookupId = TitleId.Trim();
        if (GameName.Trim().Length == 0 && lookupId.Length > 0 &&
            _shell.TryCreateClient() is { } client &&
            await _controller.LookupExistingGameNameAsync(client, _current.Device, lookupId) is { } existingName)
        {
            GameName = existingName;
        }
    }

    // ---------------------------------------------------------------- Upload

    [RelayCommand]
    private async Task Submit()
    {
        if (_items.Count == 0)
        {
            await _shell.WarnAsync("Add at least one save to upload.");
            return;
        }
        foreach (PendingSave item in _items)
        {
            if (string.IsNullOrEmpty(item.Device))
            {
                await _shell.WarnAsync($"Select a device type for '{ItemLabel(item)}'.");
                return;
            }
            if (item.Files.Count == 0)
            {
                await _shell.WarnAsync($"'{ItemLabel(item)}' has no files.");
                return;
            }
            if (item.Description.Trim().Length == 0)
            {
                await _shell.WarnAsync($"Enter a description for '{ItemLabel(item)}'.");
                return;
            }
            if (item.SaveType == SaveType.SaveFolder &&
                string.Equals(item.Device, "PC", StringComparison.OrdinalIgnoreCase) &&
                item.GameName.Trim().Length == 0)
            {
                await _shell.WarnAsync($"Enter the game name for the desktop save folder '{ItemLabel(item)}'.");
                return;
            }
        }

        SaveHubClient? client = await _shell.RequireClientAsync();
        if (client is null)
        {
            return;
        }

        int uploaded = 0;
        await _shell.RunBusy("Uploading...", async () =>
        {
            foreach (PendingSave item in _items)
            {
                string titleId = item.TitleId.Trim();
                string gameName = item.GameName.Trim();
                GameIdResolution resolution = _controller.Resolve(
                    item.Device!, item.SaveType, item.Files,
                    titleId.Length == 0 ? null : titleId,
                    gameName.Length == 0 ? null : gameName);

                SaveUploadRequest request = new SaveUploadRequest
                {
                    Platform = item.Device!,
                    GameId = resolution.GameId,
                    SaveType = item.SaveType,
                    Files = item.Files.ToList(),
                    RootDirectory = item.SaveType == SaveType.SaveFolder ? item.RootDirectory : null,
                    Description = item.Description.Trim(),
                    GameTitle = gameName.Length == 0 ? null : gameName,
                    IconPath = item.IconPath,
                    AutoFetchCoverArt = item.IconPath is null,
                };

                SaveUploadResult result = await _controller.UploadAsync(client, request, new UploadOptions());
                if (result.Merged)
                {
                    _controller.CacheGameName(item.Device!, resolution.GameId, gameName.Length == 0 ? null : gameName);
                }
                if (!string.IsNullOrEmpty(item.IconPath))
                {
                    _controller.CacheUserCover(item.Device!, resolution.GameId, item.IconPath);
                }
                uploaded++;
            }
        });

        _items.Clear();
        _current = null;
        _store.Clear();
        PopulateList();
        ClearFields();
        _shell.SetStatus($"Upload complete: {uploaded} save(s) uploaded.");
    }

    // ---------------------------------------------------------------- Selection / field sync

    partial void OnSelectedFileChanged(FileRow? value)
    {
        if (_suppressSelection)
        {
            return;
        }
        int index = value is null ? -1 : Files.IndexOf(value);
        _current = index >= 0 && index < _items.Count ? _items[index] : null;
        if (_current is not null)
        {
            LoadCurrentIntoFields(_current);
        }
    }

    partial void OnTitleIdChanged(string value)
    {
        if (_syncingFields || _current is null)
        {
            return;
        }
        _current.TitleId = value;
        UpdateCoverPreview();
        Persist();
    }

    partial void OnGameNameChanged(string value)
    {
        if (_syncingFields || _current is null)
        {
            return;
        }
        _current.GameName = value;
        RefreshRow(_current);
        Persist();
    }

    partial void OnDescriptionChanged(string value)
    {
        if (_syncingFields || _current is null)
        {
            return;
        }
        _current.Description = value;
        Persist();
    }

    partial void OnIsMemoryCardChanged(bool value)
    {
        ApplySaveTypeChange();
    }

    partial void OnIsSaveStateChanged(bool value)
    {
        ApplySaveTypeChange();
    }

    partial void OnIsSaveFolderChanged(bool value)
    {
        ApplySaveTypeChange();
    }

    private void ApplySaveTypeChange()
    {
        if (_syncingFields || _current is null)
        {
            return;
        }
        _current.SaveType = SelectedSaveType();
        RefreshRow(_current);
        Persist();
    }

    private void SelectItem(PendingSave? item)
    {
        _current = item;
        if (item is null)
        {
            _suppressSelection = true;
            SelectedFile = null;
            _suppressSelection = false;
            ClearFields();
            return;
        }
        int index = _items.IndexOf(item);
        _suppressSelection = true;
        SelectedFile = index >= 0 && index < Files.Count ? Files[index] : null;
        _suppressSelection = false;
        LoadCurrentIntoFields(item);
    }

    private void LoadCurrentIntoFields(PendingSave item)
    {
        _syncingFields = true;
        IsMemoryCard = item.SaveType == SaveType.MemoryCard;
        IsSaveState = item.SaveType == SaveType.SaveState;
        IsSaveFolder = item.SaveType == SaveType.SaveFolder;
        SelectedDeviceDisplay = string.IsNullOrEmpty(item.Device) ? "Select a device" : DeviceDisplay(item.Device);
        TitleId = item.TitleId;
        GameName = item.GameName;
        Description = item.Description;
        PathText = ItemDetails(item);
        UpdateCoverPreview();
        _syncingFields = false;
    }

    private void ClearFields()
    {
        _syncingFields = true;
        IsMemoryCard = true;
        IsSaveState = false;
        IsSaveFolder = false;
        SelectedDeviceDisplay = "Select a device";
        TitleId = string.Empty;
        GameName = string.Empty;
        Description = string.Empty;
        PathText = string.Empty;
        UpdateCoverPreview();
        _syncingFields = false;
    }

    // ---------------------------------------------------------------- List rendering

    private void PopulateList()
    {
        _suppressSelection = true;
        Files.Clear();
        foreach (PendingSave item in _items)
        {
            Files.Add(new FileRow(ItemLabel(item), TypeLabel(item.SaveType), ItemDetails(item)));
        }
        _suppressSelection = false;
    }

    private void RefreshRow(PendingSave item)
    {
        int index = _items.IndexOf(item);
        if (index < 0 || index >= Files.Count)
        {
            return;
        }
        _suppressSelection = true;
        FileRow row = new FileRow(ItemLabel(item), TypeLabel(item.SaveType), ItemDetails(item));
        Files[index] = row;
        if (ReferenceEquals(_current, item))
        {
            SelectedFile = row;
        }
        _suppressSelection = false;
    }

    private void UpdateCoverPreview()
    {
        IconPreview = ResolveCoverImage(_current);
    }

    private Bitmap ResolveCoverImage(PendingSave? item)
    {
        if (item is not null && !string.IsNullOrEmpty(item.IconPath) && File.Exists(item.IconPath))
        {
            Bitmap? user = CoverImages.TryLoad(item.IconPath);
            if (user is not null)
            {
                return user;
            }
        }
        if (item is not null && !string.IsNullOrEmpty(item.Device) && item.TitleId.Trim().Length > 0)
        {
            byte[]? cached = _controller.TryGetCachedCover(item.Device, item.TitleId.Trim());
            if (cached is not null)
            {
                Bitmap? cover = CoverImages.TryLoad(cached);
                if (cover is not null)
                {
                    return cover;
                }
            }
        }
        return CoverImages.Placeholder();
    }

    private void Persist()
    {
        _store.Save(_items);
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

    private static string ItemLabel(PendingSave item)
    {
        string name = item.GameName.Trim();
        return name.Length > 0 ? name : item.DisplayName;
    }

    private static string TypeLabel(SaveType type)
    {
        return type switch
        {
            SaveType.SaveState => "Save State",
            SaveType.SaveFolder => "Folder",
            _ => "Memory Card",
        };
    }

    private static string ItemDetails(PendingSave item)
    {
        if (item.SaveType == SaveType.SaveFolder)
        {
            return item.RootDirectory ?? string.Empty;
        }
        if (item.SaveType == SaveType.SaveState)
        {
            return item.Files.Count == 1 ? item.Files[0] : $"{item.Files.Count} files";
        }
        return item.Files.Count > 0 ? item.Files[0] : string.Empty;
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
}
