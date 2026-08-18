using SaveHub.Core;
using SaveHub.Core.Abstractions;
using SaveHub.Core.Archiving;
using SaveHub.Core.Models;

namespace SaveHub.WinForms.Tabs;

/// <summary>Upload tab: pick save files/folder, detect metadata, and upload.</summary>
internal sealed partial class UploadTab : UserControl, ITabView
{
    private const int MaxDescription = 256;
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
        _rbSaveFolder.Checked = true;
        Upload_Browse(sender, e);
    }

    private void Upload_Browse(object? sender, EventArgs e)
    {
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
}
