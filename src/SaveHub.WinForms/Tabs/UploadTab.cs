using SaveHub.Core;
using SaveHub.Core.Abstractions;
using SaveHub.Core.Archiving;
using SaveHub.Core.Models;

namespace SaveHub.WinForms.Tabs;

/// <summary>Upload tab: pick save files/folder, detect metadata, and upload.</summary>
internal sealed partial class UploadTab : UserControl, ITabView
{
    private const int MaxDescription = 256;
    private const string BulkModeUploadIndex = "Upload + Index";
    private const string BulkModeIndexOnly = "Index only";
    private MainFormController _controller = null!;
    private IShellContext _shell = null!;
    private readonly List<ComboBox> _deviceCombos = [];
    private bool _suppressDeviceChange;
    private readonly List<string> _upFilesList = [];
    private string? _upRoot;
    private string? _upIconPath;
    private string? _upDevice;

    public UploadTab()
    {
        InitializeComponent();
        UiHelpers.ConfigureListView(_upFiles, ("Name", 200), ("Size", 90), ("Path", 176));
        ConfigureBulkGrid();
    }

    public void Initialize(MainFormController controller, IShellContext shell)
    {
        _controller = controller;
        _shell = shell;
        BuildDeviceCombos();
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
            combo.Items.Add(new DeviceOption($"— {group.Manufacturer} —", ""));
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

    private void Upload_DescriptionChanged(object? sender, EventArgs e)
    {
        _upDescCount.Text = $"{_upDescription.TextLength}/{MaxDescription}";
    }

    private void Device_ComboChanged(object? sender, EventArgs e)
    {
        if (_suppressDeviceChange || sender is not ComboBox cb)
        {
            return;
        }
        if (cb.SelectedItem is DeviceOption { Code.Length: > 0 } option)
        {
            _upDevice = option.Code;
            if (option.Code == "PC")
            {
                _rbSaveFolder.Checked = true;
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
            _upTitleId.Clear();
            _upGameName.Clear();
            DetectTitleId();
        }
    }

    private void SetDeviceByCode(string code)
    {
        _suppressDeviceChange = true;
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
            if (index > 0)
            {
                _upDevice = code;
            }
        }
        _suppressDeviceChange = false;
    }

    private SaveType SelectedSaveType()
    {
        return _rbSaveState.Checked ? SaveType.SaveState :
            _rbSaveFolder.Checked ? SaveType.SaveFolder : SaveType.MemoryCard;
    }

    private void Upload_BrowseFolder(object? sender, EventArgs e)
    {
        if (_upBulk.Checked)
        {
            BrowseBulkFolder();
            return;
        }
        _rbSaveFolder.Checked = true;
        Upload_Browse(sender, e);
    }

    private void Upload_Browse(object? sender, EventArgs e)
    {
        if (_upBulk.Checked)
        {
            using OpenFileDialog bulkDialog = new OpenFileDialog { Multiselect = true, Title = "Select memory-card files" };
            if (bulkDialog.ShowDialog(this) == DialogResult.OK)
            {
                PopulateBulkGrid(bulkDialog.FileNames);
            }
            return;
        }

        SaveType type = SelectedSaveType();
        _upFilesList.Clear();
        _upRoot = null;

        if (type == SaveType.SaveFolder)
        {
            using FolderBrowserDialog dialog = new FolderBrowserDialog { Description = "Select the save folder" };
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            _upRoot = dialog.SelectedPath;
            _upFilesList.AddRange(Directory.GetFiles(_upRoot, "*", SearchOption.AllDirectories));
            _upPath.Text = _upRoot;
        }
        else
        {
            using OpenFileDialog dialog = new OpenFileDialog { Multiselect = type != SaveType.MemoryCard };
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            _upFilesList.AddRange(dialog.FileNames);
            _upPath.Text = _upFilesList.Count == 1 ? _upFilesList[0] : $"{_upFilesList.Count} files selected";
        }

        PopulateFileList();

        _upTitleId.Clear();
        _upGameName.Clear();

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

        DetectTitleId();
    }

    private void Upload_SelectIcon(object? sender, EventArgs e)
    {
        using OpenFileDialog dialog = new OpenFileDialog { Filter = "Images|*.png;*.jpg;*.jpeg;*.webp;*.gif;*.bmp" };
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _upIconPath = dialog.FileName;
            _shell.SetStatus($"Icon selected: {Path.GetFileName(_upIconPath)}");
        }
    }

    private void DetectTitleId()
    {
        if (string.IsNullOrEmpty(_upDevice) || _upFilesList.Count == 0)
        {
            return;
        }
        if (_upTitleId.Text.Trim().Length == 0)
        {
            string? id = _controller.DetectTitleId(_upDevice, SelectedSaveType(), _upFilesList);
            if (!string.IsNullOrWhiteSpace(id))
            {
                _upTitleId.Text = id;
                _shell.SetStatus($"Detected Title ID: {id}");
            }
        }
        if (_upGameName.Text.Trim().Length == 0 && _controller.DetectSaveName(_upDevice, _upFilesList) is { } name)
        {
            _upGameName.Text = name;
        }
    }

    private async void Upload_DetectTitleId(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_upDevice))
        {
            _shell.Warn("Select a device type first.");
            return;
        }
        if (_upFilesList.Count == 0)
        {
            _shell.Warn("Browse and select the save file(s) first.");
            return;
        }

        string? id = _controller.DetectTitleId(_upDevice, SelectedSaveType(), _upFilesList);
        if (!string.IsNullOrWhiteSpace(id))
        {
            _upTitleId.Text = id;
            _shell.SetStatus($"Detected Title ID: {id}");
        }
        else
        {
            _shell.SetStatus("No Title ID found on this save. Enter a Game Name instead (used as the folder).");
        }

        if (_upGameName.Text.Trim().Length == 0 && _controller.DetectSaveName(_upDevice, _upFilesList) is { } name)
        {
            _upGameName.Text = name;
        }

        string lookupId = _upTitleId.Text.Trim();
        if (_upGameName.Text.Trim().Length == 0 && lookupId.Length > 0 &&
            _controller.TryCreateClient(out _) is { } client &&
            await _controller.LookupExistingGameNameAsync(client, _upDevice, lookupId) is { } existingName)
        {
            _upGameName.Text = existingName;
        }
    }

    private async void Upload_Submit(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_upDevice))
        {
            _shell.Warn("Select a device type.");
            return;
        }
        if (_upBulk.Checked)
        {
            await SubmitBulkAsync();
            return;
        }
        if (_upFilesList.Count == 0)
        {
            _shell.Warn("Browse and select the save file(s).");
            return;
        }
        string description = _upDescription.Text.Trim();
        if (description.Length == 0)
        {
            _shell.Warn("Enter a description.");
            return;
        }

        string titleId = _upTitleId.Text.Trim();
        string gameName = _upGameName.Text.Trim();
        if (SelectedSaveType() == SaveType.SaveFolder &&
            string.Equals(_upDevice, "PC", StringComparison.OrdinalIgnoreCase) && gameName.Length == 0)
        {
            _shell.Warn("Enter the game name for this desktop save folder.");
            return;
        }
        GameIdResolution resolution = _controller.Resolve(
            _upDevice, SelectedSaveType(), _upFilesList,
            titleId.Length == 0 ? null : titleId,
            gameName.Length == 0 ? null : gameName);
        string gameId = resolution.GameId;

        SaveHubClient? client = _shell.RequireClient();
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
            GameTitle = string.IsNullOrWhiteSpace(_upGameName.Text) ? null : _upGameName.Text.Trim(),
            IconPath = _upIconPath,
            AutoFetchCoverArt = _upIconPath is null,
        };

        await _shell.RunBusy("Uploading...", async () =>
        {
            SaveUploadResult result = await _controller.UploadAsync(client, request, new UploadOptions());
            _shell.ShowResult(result);

            if (result.Merged)
            {
                _controller.CacheGameName(_upDevice, gameId, string.IsNullOrWhiteSpace(_upGameName.Text) ? null : _upGameName.Text.Trim());
            }

            if (_controller.IsNintendo(_upDevice) && _upIconPath is null)
            {
                _shell.SetStatus($"Uploaded. No cover art for {gameId} — add one later via Select Icon or the Edit tab.");
            }
        });
    }

    private void PopulateFileList()
    {
        _upFiles.Items.Clear();
        foreach (string path in _upFilesList)
        {
            long size = new FileInfo(path).Length;
            _upFiles.Items.Add(new ListViewItem([Path.GetFileName(path), MainFormController.FormatSize(size), path]));
        }
    }

    private void ConfigureBulkGrid()
    {
        _bulkGrid.AutoGenerateColumns = false;
        _bulkGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        DataGridViewTextBoxColumn gameCol = new DataGridViewTextBoxColumn { HeaderText = "Game", FillWeight = 38 };
        DataGridViewTextBoxColumn idCol = new DataGridViewTextBoxColumn { HeaderText = "ID", FillWeight = 22 };
        DataGridViewComboBoxColumn modeCol = new DataGridViewComboBoxColumn { HeaderText = "Action", FillWeight = 24 };
        modeCol.Items.Add(BulkModeUploadIndex);
        modeCol.Items.Add(BulkModeIndexOnly);
        DataGridViewTextBoxColumn fileCol = new DataGridViewTextBoxColumn { HeaderText = "File", ReadOnly = true, FillWeight = 28 };
        _bulkGrid.Columns.AddRange(gameCol, idCol, modeCol, fileCol);
        _bulkGrid.DataError += Bulk_DataError;
    }

    private void Bulk_DataError(object? sender, DataGridViewDataErrorEventArgs e)
    {
        e.Cancel = true;
    }

    private void Upload_BulkChanged(object? sender, EventArgs e)
    {
        bool bulk = _upBulk.Checked;
        if (bulk)
        {
            _rbMemoryCard.Checked = true;
        }
        _rbSaveState.Enabled = !bulk;
        _rbSaveFolder.Enabled = !bulk;
        _bulkGrid.Visible = bulk;
        _upFiles.Visible = !bulk;
        _btnUpload.Text = bulk ? "Upload All" : "Upload";
    }

    private void BrowseBulkFolder()
    {
        using FolderBrowserDialog dialog = new FolderBrowserDialog { Description = "Select the folder of memory cards" };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }
        string[] files = Directory.GetFiles(dialog.SelectedPath, "*", SearchOption.TopDirectoryOnly);
        if (files.Length == 0)
        {
            _shell.Warn("That folder has no files.");
            return;
        }
        _upPath.Text = dialog.SelectedPath;
        PopulateBulkGrid(files);
    }

    private void PopulateBulkGrid(IReadOnlyList<string> files)
    {
        if (string.IsNullOrEmpty(_upDevice) && files.Count > 0 &&
            _controller.DetectMemoryCardPlatform(files[0]) is { } detectedPlatform)
        {
            SetDeviceByCode(detectedPlatform);
        }

        _bulkGrid.Rows.Clear();
        string platform = _upDevice ?? string.Empty;
        foreach (string file in files)
        {
            string id = platform.Length > 0
                ? _controller.DetectTitleId(platform, SaveType.MemoryCard, new[] { file }) ?? string.Empty
                : string.Empty;
            string name = platform.Length > 0
                ? _controller.DetectSaveName(platform, new[] { file }) ?? string.Empty
                : string.Empty;
            int rowIndex = _bulkGrid.Rows.Add(name, id, BulkModeUploadIndex, Path.GetFileName(file));
            _bulkGrid.Rows[rowIndex].Tag = file;
        }
        _shell.SetStatus($"Loaded {files.Count} memory card(s). Choose an action per card, then Upload All.");
    }

    private async Task SubmitBulkAsync()
    {
        if (_bulkGrid.Rows.Count == 0)
        {
            _shell.Warn("Browse a folder (or files) of memory cards first.");
            return;
        }
        SaveHubClient? client = _shell.RequireClient();
        if (client is null)
        {
            return;
        }

        string platform = _upDevice!;
        List<MemoryCardIndexEntry> entries = new List<MemoryCardIndexEntry>();
        int uploaded = 0;
        await _shell.RunBusy("Uploading memory cards...", async () =>
        {
            foreach (DataGridViewRow row in _bulkGrid.Rows)
            {
                if (row.Tag is not string path)
                {
                    continue;
                }
                string game = (Convert.ToString(row.Cells[0].Value) ?? string.Empty).Trim();
                string id = (Convert.ToString(row.Cells[1].Value) ?? string.Empty).Trim();
                string mode = Convert.ToString(row.Cells[2].Value) ?? BulkModeUploadIndex;
                GameIdResolution resolution = _controller.Resolve(
                    platform, SaveType.MemoryCard, new[] { path },
                    id.Length == 0 ? null : id,
                    game.Length == 0 ? null : game);
                string gameId = resolution.GameId;
                string displayName = game.Length > 0 ? game : gameId;
                if (mode == BulkModeUploadIndex)
                {
                    SaveUploadRequest request = new SaveUploadRequest
                    {
                        Platform = platform,
                        GameId = gameId,
                        SaveType = SaveType.MemoryCard,
                        Files = new[] { path },
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
