using SaveHub.Core;
using SaveHub.Core.Abstractions;
using SaveHub.Core.Archiving;
using SaveHub.Core.Models;
using SaveHub.WinForms.Common;

namespace SaveHub.WinForms.Tabs;

/// <summary>Upload tab: stage up to ten memory cards (or one folder), each with its own metadata, and upload them.</summary>
internal sealed partial class UploadTab : UserControl, ITabView
{
    private const int MaxDescription = 256;
    private const int MaxCards = 10;

    private MainFormController _controller = null!;
    private IShellContext _shell = null!;
    private readonly List<ComboBox> _deviceCombos = [];
    private readonly List<PendingSave> _items = [];
    private PendingUploadStore _store = null!;
    private PendingSave? _current;
    private bool _suppressDeviceChange;
    private bool _suppressSelection;
    private bool _syncingFields;

    public UploadTab()
    {
        InitializeComponent();
        UiHelpers.ConfigureListView(_upFiles, ("Name", 240), ("Type", 90), ("Details", 320));
        _upFiles.FullRowSelect = true;
        _upFiles.MultiSelect = false;
        _upFiles.SelectedIndexChanged += Upload_SelectionChanged;
        _upTitleId.TextChanged += Upload_TitleIdChanged;
        _upGameName.TextChanged += Upload_GameNameChanged;
        _rbMemoryCard.CheckedChanged += Upload_SaveTypeChanged;
        _rbSaveState.CheckedChanged += Upload_SaveTypeChanged;
        _rbSaveFolder.CheckedChanged += Upload_SaveTypeChanged;
    }

    public void Initialize(MainFormController controller, IShellContext shell)
    {
        _controller = controller;
        _shell = shell;
        _store = new PendingUploadStore(PendingUploadStore.DefaultPath);
        _store.Clear();
        BuildDeviceCombos();
        UpdateCoverPreview();
    }

    public Task OnActivatedAsync()
    {
        return Task.CompletedTask;
    }

    private void BuildDeviceCombos()
    {
        _devicePanel.Controls.Clear();
        _deviceCombos.Clear();
        foreach (DeviceGroup group in Devices.Groups)
        {
            Label label = new Label { Text = group.Manufacturer, AutoSize = true, Margin = new Padding(0, 6, 0, 0) };
            ComboBox combo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 228, DisplayMember = "Display" };
            combo.Items.Add(new DeviceOption($"\u2014 {group.Manufacturer} \u2014", ""));
            foreach (DeviceOption console in group.Consoles)
            {
                combo.Items.Add(console);
            }
            combo.SelectedIndex = 0;
            combo.SelectedIndexChanged += Device_ComboChanged;
            _deviceCombos.Add(combo);
            _devicePanel.Controls.Add(label);
            _devicePanel.Controls.Add(combo);
        }
    }

    // ---------------------------------------------------------------- Field write-back

    private void Upload_DescriptionChanged(object? sender, EventArgs e)
    {
        _upDescCount.Text = $"{_upDescription.TextLength}/{MaxDescription}";
        if (_syncingFields || _current is null)
        {
            return;
        }
        _current.Description = _upDescription.Text;
        Persist();
    }

    private void Upload_TitleIdChanged(object? sender, EventArgs e)
    {
        if (_syncingFields || _current is null)
        {
            return;
        }
        _current.TitleId = _upTitleId.Text;
        UpdateCoverPreview();
        Persist();
    }

    private void Upload_GameNameChanged(object? sender, EventArgs e)
    {
        if (_syncingFields || _current is null)
        {
            return;
        }
        _current.GameName = _upGameName.Text;
        RefreshRow(_current);
        Persist();
    }

    private void Upload_SaveTypeChanged(object? sender, EventArgs e)
    {
        if (_syncingFields || _current is null || sender is not RadioButton { Checked: true })
        {
            return;
        }
        _current.SaveType = SelectedSaveType();
        RefreshRow(_current);
        Persist();
    }

    private void Device_ComboChanged(object? sender, EventArgs e)
    {
        if (_suppressDeviceChange || sender is not ComboBox cb)
        {
            return;
        }
        if (cb.SelectedItem is not DeviceOption { Code.Length: > 0 } option)
        {
            return;
        }
        _suppressDeviceChange = true;
        foreach (ComboBox other in _deviceCombos)
        {
            if (!ReferenceEquals(other, cb))
            {
                other.SelectedIndex = 0;
            }
        }
        _suppressDeviceChange = false;

        if (_current is null)
        {
            return;
        }
        _current.Device = option.Code;
        if (option.Code == "PC")
        {
            _rbSaveFolder.Checked = true;
        }
        _upTitleId.Clear();
        _upGameName.Clear();
        DetectInto(_current);
        _syncingFields = true;
        _upTitleId.Text = _current.TitleId;
        _upGameName.Text = _current.GameName;
        _syncingFields = false;
        RefreshRow(_current);
        UpdateCoverPreview();
        Persist();
    }

    // ---------------------------------------------------------------- Staging

    private void Upload_BrowseFolder(object? sender, EventArgs e)
    {
        _rbSaveFolder.Checked = true;
        Upload_Browse(sender, e);
    }

    private void Upload_Browse(object? sender, EventArgs e)
    {
        SaveType type = SelectedSaveType();
        if (type == SaveType.SaveFolder)
        {
            using FolderBrowserDialog dialog = new FolderBrowserDialog { Description = "Select the save folder" };
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            _items.Clear();
            PendingSave item = new PendingSave
            {
                Key = dialog.SelectedPath,
                SaveType = SaveType.SaveFolder,
                Files = Directory.GetFiles(dialog.SelectedPath, "*", SearchOption.AllDirectories).ToList(),
                RootDirectory = dialog.SelectedPath,
                DisplayName = Path.GetFileName(dialog.SelectedPath.TrimEnd(Path.DirectorySeparatorChar)),
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
            using OpenFileDialog dialog = new OpenFileDialog { Multiselect = true, Title = "Select save-state file(s)" };
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            _items.Clear();
            PendingSave item = new PendingSave
            {
                Key = dialog.FileNames[0],
                SaveType = SaveType.SaveState,
                Files = dialog.FileNames.ToList(),
                DisplayName = Path.GetFileName(dialog.FileNames[0]),
            };
            DetectInto(item);
            _items.Add(item);
        }
        else
        {
            using OpenFileDialog dialog = new OpenFileDialog { Multiselect = true, Title = "Select memory card(s)" };
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            AddCardFiles(dialog.FileNames);
        }

        PopulateList();
        SelectItem(_items.Count > 0 ? _items[0] : null);
        Persist();
    }

    private void Upload_AddMenu(object? sender, EventArgs e)
    {
        _addMenu.Show(_upAdd, new System.Drawing.Point(0, _upAdd.Height));
    }

    private void Upload_AddSingleFile(object? sender, EventArgs e)
    {
        using OpenFileDialog dialog = new OpenFileDialog { Multiselect = false, Title = "Add a save file" };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }
        AddFilesToUpload(dialog.FileNames);
    }

    private void Upload_AddMultipleFiles(object? sender, EventArgs e)
    {
        using OpenFileDialog dialog = new OpenFileDialog { Multiselect = true, Title = "Add save file(s)" };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }
        AddFilesToUpload(dialog.FileNames);
    }

    private void Upload_AddFolder(object? sender, EventArgs e)
    {
        using FolderBrowserDialog dialog = new FolderBrowserDialog { Description = "Add a save folder" };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }
        AddFilesToUpload(Directory.GetFiles(dialog.SelectedPath, "*", SearchOption.AllDirectories));
    }

    private void AddFilesToUpload(IReadOnlyList<string> paths)
    {
        if (paths.Count == 0)
        {
            return;
        }
        if (SelectedSaveType() == SaveType.MemoryCard)
        {
            AddCardFiles(paths);
            PopulateList();
            SelectItem(_items.Count > 0 ? _items[^1] : null);
        }
        else if (_current is not null)
        {
            _current.Files.AddRange(paths);
            RefreshRow(_current);
            _upPath.Text = ItemDetails(_current);
        }
        Persist();
    }

    private void Upload_EditName(object? sender, EventArgs e)
    {
        _upGameName.Focus();
        _upGameName.SelectAll();
    }

    private void Upload_RemoveFile(object? sender, EventArgs e)
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

    private void Upload_SelectIcon(object? sender, EventArgs e)
    {
        if (_current is null)
        {
            _shell.SetStatus("Select a save in the list first.");
            return;
        }
        using OpenFileDialog dialog = new OpenFileDialog { Filter = "Images|*.png;*.jpg;*.jpeg;*.webp;*.gif;*.bmp" };
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _current.IconPath = dialog.FileName;
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
                _shell.Warn($"Up to {MaxCards} memory cards per upload. Extra files were skipped.");
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

    // ---------------------------------------------------------------- Selection / field sync

    private void Upload_SelectionChanged(object? sender, EventArgs e)
    {
        if (_suppressSelection)
        {
            return;
        }
        int index = _upFiles.SelectedIndices.Count > 0 ? _upFiles.SelectedIndices[0] : -1;
        _current = index >= 0 && index < _items.Count ? _items[index] : null;
        if (_current is not null)
        {
            LoadCurrentIntoFields(_current);
        }
    }

    private void SelectItem(PendingSave? item)
    {
        _current = item;
        if (item is null)
        {
            ClearFields();
            return;
        }
        int index = _items.IndexOf(item);
        if (index >= 0 && index < _upFiles.Items.Count)
        {
            _suppressSelection = true;
            _upFiles.Items[index].Selected = true;
            _upFiles.Items[index].Focused = true;
            _suppressSelection = false;
        }
        LoadCurrentIntoFields(item);
    }

    private void LoadCurrentIntoFields(PendingSave item)
    {
        _syncingFields = true;
        _suppressDeviceChange = true;
        SetDeviceByCode(item.Device ?? string.Empty);
        _rbMemoryCard.Checked = item.SaveType == SaveType.MemoryCard;
        _rbSaveState.Checked = item.SaveType == SaveType.SaveState;
        _rbSaveFolder.Checked = item.SaveType == SaveType.SaveFolder;
        _upTitleId.Text = item.TitleId;
        _upGameName.Text = item.GameName;
        _upDescription.Text = item.Description;
        _upDescCount.Text = $"{item.Description.Length}/{MaxDescription}";
        _upPath.Text = ItemDetails(item);
        UpdateCoverPreview();
        _suppressDeviceChange = false;
        _syncingFields = false;
    }

    private void ClearFields()
    {
        _syncingFields = true;
        _suppressDeviceChange = true;
        SetDeviceByCode(string.Empty);
        _rbMemoryCard.Checked = true;
        _rbSaveState.Checked = false;
        _rbSaveFolder.Checked = false;
        _upTitleId.Clear();
        _upGameName.Clear();
        _upDescription.Clear();
        _upDescCount.Text = $"0/{MaxDescription}";
        _upPath.Text = string.Empty;
        UpdateCoverPreview();
        _suppressDeviceChange = false;
        _syncingFields = false;
    }

    private void SetDeviceByCode(string code)
    {
        foreach (ComboBox combo in _deviceCombos)
        {
            int index = 0;
            for (int i = 0; i < combo.Items.Count; i++)
            {
                if (combo.Items[i] is DeviceOption option && option.Code == code)
                {
                    index = i;
                    break;
                }
            }
            combo.SelectedIndex = index;
        }
    }

    // ---------------------------------------------------------------- Detection

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

    private async void Upload_DetectTitleId(object? sender, EventArgs e)
    {
        if (_current is null)
        {
            _shell.Warn("Select a save in the list first.");
            return;
        }
        if (string.IsNullOrEmpty(_current.Device))
        {
            _shell.Warn("Select a device type first.");
            return;
        }

        string? id = _controller.DetectTitleId(_current.Device, _current.SaveType, _current.Files);
        if (!string.IsNullOrWhiteSpace(id))
        {
            _upTitleId.Text = id;
            _shell.SetStatus($"Detected Title ID: {id}");
        }
        else
        {
            _shell.SetStatus("No Title ID found on this save. Enter a Game Name instead (used as the folder).");
        }

        if (_upGameName.Text.Trim().Length == 0 && _controller.DetectSaveName(_current.Device, _current.Files) is { } name)
        {
            _upGameName.Text = name;
        }

        string lookupId = _upTitleId.Text.Trim();
        if (_upGameName.Text.Trim().Length == 0 && lookupId.Length > 0 &&
            _controller.TryCreateClient(out _) is { } client &&
            await _controller.LookupExistingGameNameAsync(client, _current.Device, lookupId) is { } existingName)
        {
            _upGameName.Text = existingName;
        }
    }

    // ---------------------------------------------------------------- Upload

    private async void Upload_Submit(object? sender, EventArgs e)
    {
        if (_items.Count == 0)
        {
            _shell.Warn("Add at least one save to upload.");
            return;
        }
        foreach (PendingSave item in _items)
        {
            if (string.IsNullOrEmpty(item.Device))
            {
                _shell.Warn($"Select a device type for '{ItemLabel(item)}'.");
                return;
            }
            if (item.Files.Count == 0)
            {
                _shell.Warn($"'{ItemLabel(item)}' has no files.");
                return;
            }
            if (item.Description.Trim().Length == 0)
            {
                _shell.Warn($"Enter a description for '{ItemLabel(item)}'.");
                return;
            }
            if (item.SaveType == SaveType.SaveFolder &&
                string.Equals(item.Device, "PC", StringComparison.OrdinalIgnoreCase) &&
                item.GameName.Trim().Length == 0)
            {
                _shell.Warn($"Enter the game name for the desktop save folder '{ItemLabel(item)}'.");
                return;
            }
        }

        SaveHubClient? client = _shell.RequireClient();
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

    // ---------------------------------------------------------------- List rendering

    private void PopulateList()
    {
        _suppressSelection = true;
        _upFiles.Items.Clear();
        foreach (PendingSave item in _items)
        {
            _upFiles.Items.Add(new ListViewItem([ItemLabel(item), TypeLabel(item.SaveType), ItemDetails(item)]));
        }
        _suppressSelection = false;
    }

    private void RefreshRow(PendingSave item)
    {
        int index = _items.IndexOf(item);
        if (index < 0 || index >= _upFiles.Items.Count)
        {
            return;
        }
        ListViewItem row = _upFiles.Items[index];
        row.SubItems[0].Text = ItemLabel(item);
        row.SubItems[1].Text = TypeLabel(item.SaveType);
        row.SubItems[2].Text = ItemDetails(item);
    }

    private void UpdateCoverPreview()
    {
        _upIconLabel.Text = "Cover";
        _upIconLabel.Visible = true;
        _upIconPreview.Visible = true;
        SetPreviewImage(ResolveCoverImage(_current));
    }

    private void SetPreviewImage(Image image)
    {
        Image? previous = _upIconPreview.Image;
        _upIconPreview.Image = image;
        if (previous is not null && !ReferenceEquals(previous, UiHelpers.CoverPlaceholder()))
        {
            previous.Dispose();
        }
    }

    private Image ResolveCoverImage(PendingSave? item)
    {
        if (item is not null && !string.IsNullOrEmpty(item.IconPath) && File.Exists(item.IconPath))
        {
            Image? user = TryLoadImage(File.ReadAllBytes(item.IconPath));
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
                Image? cover = TryLoadImage(cached);
                if (cover is not null)
                {
                    return cover;
                }
            }
        }
        return UiHelpers.CoverPlaceholder();
    }

    private static Image? TryLoadImage(byte[] bytes)
    {
        try
        {
            using System.IO.MemoryStream stream = new System.IO.MemoryStream(bytes);
            using System.Drawing.Image loaded = System.Drawing.Image.FromStream(stream);
            return new System.Drawing.Bitmap(loaded);
        }
        catch
        {
            return null;
        }
    }

    private void Persist()
    {
        _store.Save(_items);
    }

    private SaveType SelectedSaveType()
    {
        return _rbSaveState.Checked ? SaveType.SaveState :
            _rbSaveFolder.Checked ? SaveType.SaveFolder : SaveType.MemoryCard;
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
}
