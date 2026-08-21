namespace SaveHub.WinForms.Tabs;

partial class BulkUploadTab
{
    private System.ComponentModel.IContainer components = null;

    private Label _info;
    private TreeView _tree;
    private ToolStrip _toolbar;
    private ToolStripDropDownButton _add;
    private ToolStripMenuItem _addFolder;
    private ToolStripMenuItem _addFiles;
    private ToolStripButton _edit;
    private ToolStripButton _remove;
    private ToolStripButton _setIcon;
    private ToolStripButton _expand;
    private ToolStripLabel _platformLabel;
    private ToolStripComboBox _platform;
    private GroupBox _details;
    private PictureBox _bulkCover;
    private TextBox _bulkTitleId;
    private Button _bulkDetect;
    private TextBox _bulkGameName;
    private TextBox _bulkDescription;
    private Button _btnUpload;
    private ToolTip _tips;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        _info = new Label();
        _tree = new TreeView();
        _toolbar = new ToolStrip();
        _add = new ToolStripDropDownButton();
        _addFolder = new ToolStripMenuItem();
        _addFiles = new ToolStripMenuItem();
        _edit = new ToolStripButton();
        _remove = new ToolStripButton();
        _setIcon = new ToolStripButton();
        _expand = new ToolStripButton();
        _platformLabel = new ToolStripLabel();
        _platform = new ToolStripComboBox();
        _details = new GroupBox();
        _bulkCover = new PictureBox();
        _bulkTitleId = new TextBox();
        _bulkDetect = new Button();
        _bulkGameName = new TextBox();
        _bulkDescription = new TextBox();
        _btnUpload = new Button();
        _tips = new ToolTip();
        _toolbar.SuspendLayout();
        _details.SuspendLayout();
        SuspendLayout();
        // 
        // _info
        // 
        _info.AutoSize = false;
        _info.Location = new Point(12, 10);
        _info.Name = "_info";
        _info.Size = new Size(980, 34);
        _info.Text = "Add a parent folder (e.g. My Games) to list each game, or add memory-card files. Each folder uploads as a save folder; each file as a memory card. Untick to exclude items; set a per-item platform and icon.";
        // 
        // _tree
        // 
        _tree.CheckBoxes = true;
        _tree.HideSelection = false;
        _tree.LabelEdit = true;
        _tree.Location = new Point(12, 48);
        _tree.Name = "_tree";
        _tree.Size = new Size(830, 410);
        _tree.TabIndex = 0;
        _tree.AfterCheck += Tree_AfterCheck;
        _tree.AfterSelect += Tree_AfterSelect;
        _tree.AfterLabelEdit += Tree_AfterLabelEdit;
        // 
        // _toolbar
        // 
        _toolbar.AutoSize = false;
        _toolbar.Dock = DockStyle.None;
        _toolbar.GripStyle = ToolStripGripStyle.Hidden;
        _toolbar.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
        _toolbar.Location = new Point(850, 48);
        _toolbar.Name = "_toolbar";
        _toolbar.Size = new Size(142, 410);
        _toolbar.TabIndex = 1;
        _toolbar.Items.AddRange(new ToolStripItem[]
        {
            _add, _edit, _remove, _setIcon, new ToolStripSeparator(),
            _expand, new ToolStripSeparator(), _platformLabel, _platform,
        });
        // 
        // _add
        // 
        _add.AutoSize = false;
        _add.DisplayStyle = ToolStripItemDisplayStyle.Text;
        _add.Size = new Size(132, 26);
        _add.Text = "Add";
        _add.DropDownItems.AddRange(new ToolStripItem[] { _addFolder, _addFiles });
        // 
        // _addFolder / _addFiles
        // 
        _addFolder.Text = "Add folder\u2026";
        _addFolder.Click += Add_Folder;
        _addFiles.Text = "Add file(s)\u2026";
        _addFiles.Click += Add_Files;
        // 
        // _edit
        // 
        _edit.AutoSize = false;
        _edit.DisplayStyle = ToolStripItemDisplayStyle.Text;
        _edit.Size = new Size(132, 26);
        _edit.Text = "Edit name";
        _edit.Click += Edit_Name;
        // 
        // _remove
        // 
        _remove.AutoSize = false;
        _remove.DisplayStyle = ToolStripItemDisplayStyle.Text;
        _remove.Size = new Size(132, 26);
        _remove.Text = "Remove";
        _remove.Click += Remove_Node;
        // 
        // _setIcon
        // 
        _setIcon.AutoSize = false;
        _setIcon.DisplayStyle = ToolStripItemDisplayStyle.Text;
        _setIcon.Size = new Size(132, 26);
        _setIcon.Text = "Set icon\u2026";
        _setIcon.Click += Set_Icon;
        // 
        // _expand
        // 
        _expand.AutoSize = false;
        _expand.CheckOnClick = true;
        _expand.DisplayStyle = ToolStripItemDisplayStyle.Text;
        _expand.Size = new Size(132, 26);
        _expand.Text = "Expand all";
        _expand.Click += Toggle_Expand;
        // 
        // _platformLabel
        // 
        _platformLabel.Text = "Platform";
        // 
        // _platform
        // 
        _platform.AutoSize = false;
        _platform.DropDownStyle = ComboBoxStyle.DropDownList;
        _platform.Size = new Size(132, 23);
        _platform.SelectedIndexChanged += Platform_Changed;
        // 
        // _details
        // 
        _details.Controls.Add(_bulkCover);
        _details.Controls.Add(_bulkTitleId);
        _details.Controls.Add(_bulkDetect);
        _details.Controls.Add(_bulkGameName);
        _details.Controls.Add(_bulkDescription);
        _details.Controls.Add(new Label { Text = "Title ID", Location = new Point(156, 26), AutoSize = true });
        _details.Controls.Add(new Label { Text = "Game Name", Location = new Point(156, 60), AutoSize = true });
        _details.Controls.Add(new Label { Text = "Description", Location = new Point(156, 94), AutoSize = true });
        _details.Location = new Point(12, 466);
        _details.Name = "_details";
        _details.Size = new Size(980, 170);
        _details.TabIndex = 3;
        _details.TabStop = false;
        _details.Text = "Details";
        // 
        // _bulkCover
        // 
        _bulkCover.BorderStyle = BorderStyle.FixedSingle;
        _bulkCover.Location = new Point(14, 24);
        _bulkCover.Name = "_bulkCover";
        _bulkCover.Size = new Size(130, 130);
        _bulkCover.SizeMode = PictureBoxSizeMode.Zoom;
        _bulkCover.TabStop = false;
        // 
        // _bulkTitleId
        // 
        _bulkTitleId.Location = new Point(236, 23);
        _bulkTitleId.Name = "_bulkTitleId";
        _bulkTitleId.Size = new Size(220, 23);
        _bulkTitleId.PlaceholderText = "e.g. SLUS-12345 (auto-detected)";
        _bulkTitleId.TextChanged += Bulk_TitleIdChanged;
        // 
        // _bulkDetect
        // 
        _bulkDetect.Location = new Point(462, 22);
        _bulkDetect.Name = "_bulkDetect";
        _bulkDetect.Size = new Size(90, 25);
        _bulkDetect.Text = "Detect";
        _bulkDetect.Click += Bulk_Detect;
        // 
        // _bulkGameName
        // 
        _bulkGameName.Location = new Point(236, 57);
        _bulkGameName.Name = "_bulkGameName";
        _bulkGameName.Size = new Size(480, 23);
        _bulkGameName.PlaceholderText = "Shown in your library";
        _bulkGameName.TextChanged += Bulk_GameNameChanged;
        // 
        // _bulkDescription
        // 
        _bulkDescription.Location = new Point(236, 91);
        _bulkDescription.Multiline = true;
        _bulkDescription.Name = "_bulkDescription";
        _bulkDescription.ScrollBars = ScrollBars.Vertical;
        _bulkDescription.Size = new Size(600, 64);
        _bulkDescription.PlaceholderText = "Describe this save";
        _bulkDescription.TextChanged += Bulk_DescriptionChanged;
        // 
        // _btnUpload
        // 
        _btnUpload.Location = new Point(12, 644);
        _btnUpload.Name = "_btnUpload";
        _btnUpload.Size = new Size(160, 30);
        _btnUpload.TabIndex = 2;
        _btnUpload.Text = "Upload All";
        _btnUpload.Click += Upload_All;
        // 
        // tooltips
        // 
        _tips.SetToolTip(_btnUpload, "Upload every ticked item");
        // 
        // BulkUploadTab
        // 
        Controls.Add(_info);
        Controls.Add(_tree);
        Controls.Add(_toolbar);
        Controls.Add(_details);
        Controls.Add(_btnUpload);
        Name = "BulkUploadTab";
        Size = new Size(1004, 685);
        _toolbar.ResumeLayout(false);
        _toolbar.PerformLayout();
        _details.ResumeLayout(false);
        _details.PerformLayout();
        ResumeLayout(false);
    }
}
