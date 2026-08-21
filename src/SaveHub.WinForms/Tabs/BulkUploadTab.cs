using SaveHub.Core;
using SaveHub.Core.Abstractions;
using SaveHub.Core.Archiving;
using SaveHub.Core.Models;

namespace SaveHub.WinForms.Tabs;

/// <summary>
/// Bulk Upload tab: add a parent folder (each sub-folder = a game) or files, curate the tree, and
/// upload each item. Folders upload as save folders; files as memory cards (which also update the
/// platform's <c>!index</c> catalog).
/// </summary>
internal sealed partial class BulkUploadTab : UserControl, ITabView
{
    private MainFormController _controller = null!;
    private IShellContext _shell = null!;
    private bool _suppressCheck;
    private bool _suppressBulkFields;

    public BulkUploadTab()
    {
        InitializeComponent();
    }

    public void Initialize(MainFormController controller, IShellContext shell)
    {
        _controller = controller;
        _shell = shell;
        _platform.ComboBox.DisplayMember = "Display";
        _platform.Items.Clear();
        foreach (DeviceOption option in Devices.All)
        {
            _platform.Items.Add(option);
        }
        _platform.Enabled = false;
        SetDetailsEnabled(false);
        UpdateBulkCover(null);
    }

    public Task OnActivatedAsync()
    {
        return Task.CompletedTask;
    }

    private void Add_Folder(object? sender, EventArgs e)
    {
        using FolderBrowserDialog dialog = new FolderBrowserDialog { Description = "Select a game save folder (or a folder containing several)" };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }
        string root = dialog.SelectedPath;
        string[] subDirs = Directory.GetDirectories(root);
        IReadOnlyList<string> gameFolders = subDirs.Length > 0 ? subDirs : [root];
        foreach (string folder in gameFolders)
        {
            AddGameFolderNode(folder);
        }
    }

    private void AddGameFolderNode(string folder)
    {
        // A folder of PS1/PS2 memory cards stays as a parent node; each card is an indexed child.
        List<string> cardFiles = MemoryCardFilesIn(folder);
        if (cardFiles.Count > 0)
        {
            TreeNode group = new TreeNode(Path.GetFileName(folder.TrimEnd(Path.DirectorySeparatorChar)))
            {
                Tag = new BulkFolderNode { Path = folder, IsFolder = true, IsCardGroup = true },
                Checked = true,
                ToolTipText = "Memory cards \u2014 each uploaded and indexed individually",
            };
            foreach (string card in cardFiles)
            {
                group.Nodes.Add(CreateCardNode(card));
            }
            _tree.Nodes.Add(group);
            group.Expand();
            _shell.SetStatus($"Added {cardFiles.Count} memory card(s) under '{group.Text}'.");
            return;
        }

        string[] files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);
        string platform = _controller.DetectFolderPlatform(files) ?? string.Empty;
        TreeNode node = new TreeNode(Path.GetFileName(folder.TrimEnd(Path.DirectorySeparatorChar)))
        {
            Tag = new BulkFolderNode { Path = folder, IsFolder = true, Platform = platform },
            Checked = true,
            ToolTipText = platform.Length > 0 ? $"Platform: {platform}" : "Select a platform",
        };
        AddFolderChildren(node, folder);
        _tree.Nodes.Add(node);
        _shell.SetStatus($"Added '{node.Text}'. Pick its platform on the right if it isn't set.");
    }

    private TreeNode CreateCardNode(string file)
    {
        string platform = _controller.DetectMemoryCardPlatform(file) ?? string.Empty;
        string gameName = _controller.DetectSaveName(platform, [file]) ?? Path.GetFileName(file);
        string titleId = _controller.DetectTitleId(platform, SaveType.MemoryCard, [file]) ?? string.Empty;
        return new TreeNode(gameName)
        {
            Tag = new BulkFolderNode { Path = file, IsFolder = false, Platform = platform, TitleId = titleId },
            Checked = true,
            ToolTipText = platform.Length > 0 ? $"Platform: {platform}" : "Select a platform",
        };
    }

    private void AddCardNode(string file)
    {
        _tree.Nodes.Add(CreateCardNode(file));
    }

    private List<string> MemoryCardFilesIn(string folder)
    {
        List<string> cards = new List<string>();
        foreach (string file in Directory.GetFiles(folder, "*", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
        {
            if (_controller.DetectMemoryCardPlatform(file) is not null)
            {
                cards.Add(file);
            }
        }
        return cards;
    }

    private static void AddFolderChildren(TreeNode parent, string dir)
    {
        foreach (string sub in Directory.GetDirectories(dir).OrderBy(d => d, StringComparer.OrdinalIgnoreCase))
        {
            TreeNode dirNode = new TreeNode(Path.GetFileName(sub)) { Tag = sub, Checked = true };
            parent.Nodes.Add(dirNode);
            AddFolderChildren(dirNode, sub);
        }
        foreach (string file in Directory.GetFiles(dir).OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
        {
            parent.Nodes.Add(new TreeNode(Path.GetFileName(file)) { Tag = file, Checked = true });
        }
    }

    private void Add_Files(object? sender, EventArgs e)
    {
        using OpenFileDialog dialog = new OpenFileDialog { Multiselect = true, Title = "Select memory-card file(s)" };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }
        foreach (string file in dialog.FileNames)
        {
            AddCardNode(file);
        }
    }

    private void Edit_Name(object? sender, EventArgs e)
    {
        if (_tree.SelectedNode is { } node)
        {
            node.BeginEdit();
        }
        else
        {
            _shell.Warn("Select an item to rename.");
        }
    }

    private void Tree_AfterLabelEdit(object? sender, NodeLabelEditEventArgs e)
    {
        if (e.Label is not null && string.IsNullOrWhiteSpace(e.Label))
        {
            e.CancelEdit = true;
        }
    }

    private void Remove_Node(object? sender, EventArgs e)
    {
        if (_tree.SelectedNode is { } node)
        {
            node.Remove();
        }
        else
        {
            _shell.Warn("Select an item to remove.");
        }
    }

    private void Set_Icon(object? sender, EventArgs e)
    {
        if (_tree.SelectedNode is not { Tag: BulkFolderNode info } node || info.IsCardGroup)
        {
            _shell.Warn("Select a card or a game folder, then Set icon.");
            return;
        }
        using OpenFileDialog dialog = new OpenFileDialog { Filter = "Images|*.png;*.jpg;*.jpeg;*.webp;*.gif;*.bmp" };
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            info.IconPath = dialog.FileName;
            node.ToolTipText = $"Platform: {info.Platform} \u2014 icon set";
            UpdateBulkCover(info);
            _shell.SetStatus($"Icon set for '{node.Text}'.");
        }
    }

    private void Toggle_Expand(object? sender, EventArgs e)
    {
        if (_expand.Checked)
        {
            _tree.ExpandAll();
            _expand.Text = "Collapse all";
        }
        else
        {
            _tree.CollapseAll();
            _expand.Text = "Expand all";
        }
    }

    private void Tree_AfterSelect(object? sender, TreeViewEventArgs e)
    {
        if (e.Node?.Tag is BulkFolderNode info && !info.IsCardGroup)
        {
            _platform.Enabled = true;
            SelectPlatformInCombo(info.Platform);
            SetDetailsEnabled(true);
            LoadBulkFields(e.Node, info);
        }
        else
        {
            _platform.Enabled = false;
            SetDetailsEnabled(false);
            _suppressBulkFields = true;
            _bulkTitleId.Clear();
            _bulkGameName.Clear();
            _bulkDescription.Clear();
            _suppressBulkFields = false;
            UpdateBulkCover(null);
        }
    }

    private void SelectPlatformInCombo(string code)
    {
        for (int i = 0; i < _platform.Items.Count; i++)
        {
            if (_platform.Items[i] is DeviceOption option && option.Code == code)
            {
                _platform.SelectedIndex = i;
                return;
            }
        }
        _platform.SelectedIndex = -1;
    }

    private void Platform_Changed(object? sender, EventArgs e)
    {
        if (_tree.SelectedNode is { Tag: BulkFolderNode info } node && _platform.SelectedItem is DeviceOption option)
        {
            info.Platform = option.Code;
            node.ToolTipText = $"Platform: {option.Code}";
            UpdateBulkCover(info);
        }
    }

    private void Tree_AfterCheck(object? sender, TreeViewEventArgs e)
    {
        if (_suppressCheck || e.Node is null)
        {
            return;
        }
        _suppressCheck = true;
        SetChildrenChecked(e.Node, e.Node.Checked);
        _suppressCheck = false;
    }

    private static void SetChildrenChecked(TreeNode node, bool value)
    {
        foreach (TreeNode child in node.Nodes)
        {
            child.Checked = value;
            SetChildrenChecked(child, value);
        }
    }

    private async void Upload_All(object? sender, EventArgs e)
    {
        if (_tree.Nodes.Count == 0)
        {
            _shell.Warn("Add folders or files to upload first.");
            return;
        }
        foreach (TreeNode node in _tree.Nodes)
        {
            if (!node.Checked || node.Tag is not BulkFolderNode info)
            {
                continue;
            }
            if (info.IsCardGroup)
            {
                foreach (TreeNode child in node.Nodes)
                {
                    if (child.Checked && child.Tag is BulkFolderNode card && string.IsNullOrWhiteSpace(card.Platform))
                    {
                        _shell.Warn($"Select a platform for '{child.Text}'.");
                        return;
                    }
                }
            }
            else if (string.IsNullOrWhiteSpace(info.Platform))
            {
                _shell.Warn($"Select a platform for '{node.Text}'.");
                return;
            }
        }

        SaveHubClient? client = _shell.RequireClient();
        if (client is null)
        {
            return;
        }

        int uploaded = 0;
        Dictionary<string, List<MemoryCardIndexEntry>> cardIndex = new Dictionary<string, List<MemoryCardIndexEntry>>(StringComparer.OrdinalIgnoreCase);
        await _shell.RunBusy("Uploading...", async () =>
        {
            foreach (TreeNode node in _tree.Nodes)
            {
                if (!node.Checked || node.Tag is not BulkFolderNode info)
                {
                    continue;
                }
                if (info.IsCardGroup)
                {
                    foreach (TreeNode child in node.Nodes)
                    {
                        if (child.Checked && child.Tag is BulkFolderNode card && await UploadNode(client, child, card, cardIndex))
                        {
                            uploaded++;
                        }
                    }
                }
                else if (await UploadNode(client, node, info, cardIndex))
                {
                    uploaded++;
                }
            }

            foreach (KeyValuePair<string, List<MemoryCardIndexEntry>> pair in cardIndex)
            {
                await _controller.UpdateMemoryCardIndexAsync(client, pair.Key, pair.Value);
            }
        });
        _shell.SetStatus($"Bulk upload complete: {uploaded} item(s) uploaded.");
    }

    private async Task<bool> UploadNode(SaveHubClient client, TreeNode node, BulkFolderNode info, Dictionary<string, List<MemoryCardIndexEntry>> cardIndex)
    {
        string name = node.Text.Trim();
        SaveType saveType = info.IsFolder ? SaveType.SaveFolder : SaveType.MemoryCard;
        List<string> files = new List<string>();
        if (info.IsFolder)
        {
            CollectIncludedFiles(node, files);
        }
        else
        {
            files.Add(info.Path);
        }
        if (files.Count == 0)
        {
            return false;
        }
        string? titleId = info.TitleId.Trim().Length > 0
            ? info.TitleId.Trim()
            : (info.IsFolder ? null : _controller.DetectTitleId(info.Platform, SaveType.MemoryCard, files));
        GameIdResolution resolution = _controller.Resolve(info.Platform, saveType, files, titleId, name.Length > 0 ? name : null);
        string displayName = name.Length > 0 ? name : resolution.GameId;
        string description = info.Description.Trim().Length > 0 ? info.Description.Trim() : displayName;
        SaveUploadRequest request = new SaveUploadRequest
        {
            Platform = info.Platform,
            GameId = resolution.GameId,
            SaveType = saveType,
            Files = files,
            RootDirectory = info.IsFolder ? info.Path : null,
            Description = description,
            GameTitle = name.Length > 0 ? name : null,
            IconPath = info.IconPath,
            AutoFetchCoverArt = info.IconPath is null,
        };
        await _controller.UploadAsync(client, request, new UploadOptions());

        if (!info.IsFolder)
        {
            if (!cardIndex.TryGetValue(info.Platform, out List<MemoryCardIndexEntry>? list))
            {
                list = new List<MemoryCardIndexEntry>();
                cardIndex[info.Platform] = list;
            }
            list.Add(new MemoryCardIndexEntry(resolution.GameId, displayName));
        }
        return true;
    }

    private void Bulk_TitleIdChanged(object? sender, EventArgs e)
    {
        if (_suppressBulkFields)
        {
            return;
        }
        if (_tree.SelectedNode?.Tag is BulkFolderNode info)
        {
            info.TitleId = _bulkTitleId.Text;
            UpdateBulkCover(info);
        }
    }

    private void Bulk_GameNameChanged(object? sender, EventArgs e)
    {
        if (_suppressBulkFields)
        {
            return;
        }
        if (_tree.SelectedNode is { Tag: BulkFolderNode } node)
        {
            node.Text = _bulkGameName.Text;
        }
    }

    private void Bulk_DescriptionChanged(object? sender, EventArgs e)
    {
        if (_suppressBulkFields)
        {
            return;
        }
        if (_tree.SelectedNode?.Tag is BulkFolderNode info)
        {
            info.Description = _bulkDescription.Text;
        }
    }

    private void Bulk_Detect(object? sender, EventArgs e)
    {
        if (_tree.SelectedNode?.Tag is not BulkFolderNode info)
        {
            _shell.Warn("Select a folder or file first.");
            return;
        }
        if (string.IsNullOrWhiteSpace(info.Platform))
        {
            _shell.Warn("Select a platform first.");
            return;
        }
        List<string> files = new List<string>();
        if (info.IsFolder)
        {
            files.AddRange(Directory.GetFiles(info.Path, "*", SearchOption.AllDirectories));
        }
        else
        {
            files.Add(info.Path);
        }
        string? id = _controller.DetectTitleId(info.Platform, info.IsFolder ? SaveType.SaveFolder : SaveType.MemoryCard, files);
        if (!string.IsNullOrWhiteSpace(id))
        {
            _bulkTitleId.Text = id;
            _shell.SetStatus($"Detected Title ID: {id}");
        }
        else
        {
            _shell.SetStatus("No Title ID found on this item.");
        }
    }

    private void LoadBulkFields(TreeNode node, BulkFolderNode info)
    {
        _suppressBulkFields = true;
        _bulkTitleId.Text = info.TitleId;
        _bulkGameName.Text = node.Text;
        _bulkDescription.Text = info.Description;
        _suppressBulkFields = false;
        UpdateBulkCover(info);
    }

    private void SetDetailsEnabled(bool enabled)
    {
        _bulkTitleId.Enabled = enabled;
        _bulkGameName.Enabled = enabled;
        _bulkDescription.Enabled = enabled;
        _bulkDetect.Enabled = enabled;
    }

    private void UpdateBulkCover(BulkFolderNode? info)
    {
        Image image = ResolveBulkCover(info);
        Image? previous = _bulkCover.Image;
        _bulkCover.Image = image;
        if (previous is not null && !ReferenceEquals(previous, UiHelpers.CoverPlaceholder()))
        {
            previous.Dispose();
        }
    }

    private Image ResolveBulkCover(BulkFolderNode? info)
    {
        if (info is not null && !string.IsNullOrEmpty(info.IconPath) && File.Exists(info.IconPath))
        {
            Image? user = LoadImage(info.IconPath);
            if (user is not null)
            {
                return user;
            }
        }
        if (info is not null && !string.IsNullOrEmpty(info.Platform) && info.TitleId.Trim().Length > 0)
        {
            byte[]? cached = _controller.TryGetCachedCover(info.Platform, info.TitleId.Trim());
            if (cached is not null)
            {
                Image? cover = LoadImage(cached);
                if (cover is not null)
                {
                    return cover;
                }
            }
        }
        return UiHelpers.CoverPlaceholder();
    }

    private static Image? LoadImage(string path)
    {
        try
        {
            using System.IO.MemoryStream stream = new System.IO.MemoryStream(System.IO.File.ReadAllBytes(path));
            using System.Drawing.Image loaded = System.Drawing.Image.FromStream(stream);
            return new System.Drawing.Bitmap(loaded);
        }
        catch
        {
            return null;
        }
    }

    private static Image? LoadImage(byte[] bytes)
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

    private static void CollectIncludedFiles(TreeNode node, List<string> files)
    {
        foreach (TreeNode child in node.Nodes)
        {
            if (!child.Checked)
            {
                continue;
            }
            if (child.Tag is string path && File.Exists(path))
            {
                files.Add(path);
            }
            else
            {
                CollectIncludedFiles(child, files);
            }
        }
    }

    private sealed class BulkFolderNode
    {
        public required string Path { get; init; }
        public required bool IsFolder { get; init; }
        public bool IsCardGroup { get; init; }
        public string Platform { get; set; } = string.Empty;
        public string? IconPath { get; set; }
        public string TitleId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
