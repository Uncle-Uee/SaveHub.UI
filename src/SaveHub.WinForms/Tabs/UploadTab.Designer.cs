namespace SaveHub.WinForms.Tabs;

partial class UploadTab
{
    private System.ComponentModel.IContainer components = null;

    private GroupBox _saveBox;
    private Button _btnBrowse;
    private Button _btnBrowseFolder;
    private Button _btnIcon;
    private TextBox _upPath;
    private ListView _upFiles;
    private Button _upAdd;
    private Button _upEdit;
    private Button _upRemove;
    private ContextMenuStrip _addMenu;
    private ToolStripMenuItem _addSingleFile;
    private ToolStripMenuItem _addMultipleFiles;
    private ToolStripMenuItem _addFolderItem;

    private GroupBox _detailsBox;
    private TextBox _upGameName;
    private TextBox _upTitleId;
    private Button _btnDetect;
    private TextBox _upDescription;
    private Label _upDescCount;

    private GroupBox _deviceBox;
    private FlowLayoutPanel _devicePanel;

    private GroupBox _saveTypeBox;
    private RadioButton _rbMemoryCard;
    private RadioButton _rbSaveState;
    private RadioButton _rbSaveFolder;

    private Button _btnUpload;
    private Label _upIconLabel;
    private PictureBox _upIconPreview;
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
        _saveBox = new GroupBox();
        _btnBrowse = new Button();
        _btnBrowseFolder = new Button();
        _btnIcon = new Button();
        _upPath = new TextBox();
        _upFiles = new ListView();
        _upAdd = new Button();
        _upEdit = new Button();
        _upRemove = new Button();
        _addMenu = new ContextMenuStrip();
        _addSingleFile = new ToolStripMenuItem();
        _addMultipleFiles = new ToolStripMenuItem();
        _addFolderItem = new ToolStripMenuItem();
        _detailsBox = new GroupBox();
        _btnDetect = new Button();
        _upGameName = new TextBox();
        _upTitleId = new TextBox();
        _upDescription = new TextBox();
        _upDescCount = new Label();
        _deviceBox = new GroupBox();
        _devicePanel = new FlowLayoutPanel();
        _saveTypeBox = new GroupBox();
        _rbMemoryCard = new RadioButton();
        _rbSaveState = new RadioButton();
        _rbSaveFolder = new RadioButton();
        _btnUpload = new Button();
        _upIconLabel = new Label();
        _upIconPreview = new PictureBox();
        _tips = new ToolTip();
        _saveBox.SuspendLayout();
        _detailsBox.SuspendLayout();
        _deviceBox.SuspendLayout();
        _saveTypeBox.SuspendLayout();
        SuspendLayout();
        // 
        // _saveBox
        // 
        _saveBox.Controls.Add(_btnBrowse);
        _saveBox.Controls.Add(_btnBrowseFolder);
        _saveBox.Controls.Add(_btnIcon);
        _saveBox.Controls.Add(_upPath);
        _saveBox.Controls.Add(_upFiles);
        _saveBox.Controls.Add(_upAdd);
        _saveBox.Controls.Add(_upEdit);
        _saveBox.Controls.Add(_upRemove);
        _saveBox.Location = new Point(8, 8);
        _saveBox.Name = "_saveBox";
        _saveBox.Size = new Size(720, 300);
        _saveBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _saveBox.TabIndex = 0;
        _saveBox.TabStop = false;
        _saveBox.Text = "Save";
        // 
        // _btnBrowse
        // 
        _btnBrowse.Location = new Point(12, 24);
        _btnBrowse.Name = "_btnBrowse";
        _btnBrowse.Size = new Size(100, 28);
        _btnBrowse.TabIndex = 0;
        _btnBrowse.Text = "Add File";
        _btnBrowse.Click += Upload_Browse;
        // 
        // _btnBrowseFolder
        // 
        _btnBrowseFolder.Location = new Point(118, 24);
        _btnBrowseFolder.Name = "_btnBrowseFolder";
        _btnBrowseFolder.Size = new Size(116, 28);
        _btnBrowseFolder.TabIndex = 1;
        _btnBrowseFolder.Text = "Add Folder";
        _btnBrowseFolder.Click += Upload_BrowseFolder;
        // 
        // _btnIcon
        // 
        _btnIcon.Location = new Point(240, 24);
        _btnIcon.Name = "_btnIcon";
        _btnIcon.Size = new Size(110, 28);
        _btnIcon.TabIndex = 2;
        _btnIcon.Text = "Select Icon...";
        _btnIcon.Click += Upload_SelectIcon;
        // 
        // _upPath
        // 
        _upPath.Location = new Point(12, 60);
        _upPath.Name = "_upPath";
        _upPath.ReadOnly = true;
        _upPath.Size = new Size(696, 23);
        _upPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _upPath.TabIndex = 3;
        // 
        // _upFiles
        // 
        _upFiles.Location = new Point(12, 92);
        _upFiles.Name = "_upFiles";
        _upFiles.Size = new Size(616, 196);
        _upFiles.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _upFiles.TabIndex = 4;
        _upFiles.UseCompatibleStateImageBehavior = false;
        // 
        // _upAdd
        // 
        _upAdd.Location = new Point(636, 92);
        _upAdd.Name = "_upAdd";
        _upAdd.Size = new Size(72, 26);
        _upAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _upAdd.TabIndex = 5;
        _upAdd.Text = "Add \u25BE";
        _upAdd.Click += Upload_AddMenu;
        // 
        // _addMenu
        // 
        _addMenu.Items.AddRange(new ToolStripItem[] { _addSingleFile, _addMultipleFiles, _addFolderItem });
        _addSingleFile.Text = "Single File";
        _addSingleFile.Click += Upload_AddSingleFile;
        _addMultipleFiles.Text = "Multiple Files";
        _addMultipleFiles.Click += Upload_AddMultipleFiles;
        _addFolderItem.Text = "Add Folder";
        _addFolderItem.Click += Upload_AddFolder;
        // 
        // _upEdit
        // 
        _upEdit.Location = new Point(636, 124);
        _upEdit.Name = "_upEdit";
        _upEdit.Size = new Size(72, 26);
        _upEdit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _upEdit.TabIndex = 6;
        _upEdit.Text = "Edit";
        _upEdit.Click += Upload_EditName;
        // 
        // _upRemove
        // 
        _upRemove.Location = new Point(636, 156);
        _upRemove.Name = "_upRemove";
        _upRemove.Size = new Size(72, 26);
        _upRemove.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _upRemove.TabIndex = 7;
        _upRemove.Text = "Remove";
        _upRemove.Click += Upload_RemoveFile;
        // 
        // _detailsBox
        // 
        _detailsBox.Controls.Add(_btnDetect);
        _detailsBox.Controls.Add(_upGameName);
        _detailsBox.Controls.Add(_upTitleId);
        _detailsBox.Controls.Add(_upDescription);
        _detailsBox.Controls.Add(_upDescCount);
        _detailsBox.Location = new Point(8, 314);
        _detailsBox.Name = "_detailsBox";
        _detailsBox.Size = new Size(720, 236);
        _detailsBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _detailsBox.TabIndex = 1;
        _detailsBox.TabStop = false;
        _detailsBox.Text = "Details";
        // 
        // _btnDetect
        // 
        _btnDetect.Location = new Point(598, 57);
        _btnDetect.Name = "_btnDetect";
        _btnDetect.Size = new Size(102, 25);
        _btnDetect.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _btnDetect.TabIndex = 3;
        _btnDetect.Text = "Detect";
        _btnDetect.Click += Upload_DetectTitleId;
        // 
        // _upGameName
        // 
        _upGameName.Location = new Point(120, 24);
        _upGameName.Name = "_upGameName";
        _upGameName.Size = new Size(580, 23);
        _upGameName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _upGameName.TabIndex = 4;
        _upGameName.PlaceholderText = "Game name \u2014 shown in your library; required for PC save folders";
        // 
        // _upTitleId
        // 
        _upTitleId.Location = new Point(120, 58);
        _upTitleId.Name = "_upTitleId";
        _upTitleId.Size = new Size(250, 23);
        _upTitleId.TabIndex = 5;
        _upTitleId.PlaceholderText = "Title ID, e.g. SLUS-12345 (auto-detected for PlayStation)";
        // 
        // _upDescription
        // 
        _upDescription.Location = new Point(120, 92);
        _upDescription.Multiline = true;
        _upDescription.Name = "_upDescription";
        _upDescription.ScrollBars = ScrollBars.Vertical;
        _upDescription.Size = new Size(580, 96);
        _upDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _upDescription.TabIndex = 6;
        _upDescription.PlaceholderText = "Describe this save (e.g. \u2018Chapter 5, all upgrades\u2019)";
        _upDescription.TextChanged += Upload_DescriptionChanged;
        // 
        // _upDescCount
        // 
        _upDescCount.AutoSize = true;
        _upDescCount.Location = new Point(120, 192);
        _upDescCount.Name = "_upDescCount";
        _upDescCount.Size = new Size(36, 15);
        _upDescCount.TabIndex = 7;
        _upDescCount.Text = "0/256";
        // 
        // _deviceBox
        // 
        _deviceBox.Controls.Add(_devicePanel);
        _deviceBox.Location = new Point(740, 8);
        _deviceBox.Name = "_deviceBox";
        _deviceBox.Size = new Size(260, 300);
        _deviceBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _deviceBox.TabIndex = 2;
        _deviceBox.TabStop = false;
        _deviceBox.Text = "Device Type";
        // 
        // _devicePanel
        // 
        _devicePanel.AutoScroll = true;
        _devicePanel.FlowDirection = FlowDirection.TopDown;
        _devicePanel.Location = new Point(10, 20);
        _devicePanel.Name = "_devicePanel";
        _devicePanel.Size = new Size(244, 270);
        _devicePanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _devicePanel.TabIndex = 0;
        _devicePanel.WrapContents = false;
        // 
        // _saveTypeBox
        // 
        _saveTypeBox.Controls.Add(_rbMemoryCard);
        _saveTypeBox.Controls.Add(_rbSaveState);
        _saveTypeBox.Controls.Add(_rbSaveFolder);
        _saveTypeBox.Location = new Point(740, 320);
        _saveTypeBox.Name = "_saveTypeBox";
        _saveTypeBox.Size = new Size(260, 100);
        _saveTypeBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _saveTypeBox.TabIndex = 3;
        _saveTypeBox.TabStop = false;
        _saveTypeBox.Text = "Save Type";
        // 
        // _rbMemoryCard
        // 
        _rbMemoryCard.AutoSize = true;
        _rbMemoryCard.Checked = true;
        _rbMemoryCard.Location = new Point(14, 22);
        _rbMemoryCard.Name = "_rbMemoryCard";
        _rbMemoryCard.Size = new Size(98, 19);
        _rbMemoryCard.TabIndex = 0;
        _rbMemoryCard.TabStop = true;
        _rbMemoryCard.Text = "Memory Card";
        // 
        // _rbSaveState
        // 
        _rbSaveState.AutoSize = true;
        _rbSaveState.Location = new Point(14, 45);
        _rbSaveState.Name = "_rbSaveState";
        _rbSaveState.Size = new Size(78, 19);
        _rbSaveState.TabIndex = 1;
        _rbSaveState.Text = "Save State";
        // 
        // _rbSaveFolder
        // 
        _rbSaveFolder.AutoSize = true;
        _rbSaveFolder.Location = new Point(14, 68);
        _rbSaveFolder.Name = "_rbSaveFolder";
        _rbSaveFolder.Size = new Size(58, 19);
        _rbSaveFolder.TabIndex = 2;
        _rbSaveFolder.Text = "Folder";
        // 
        // _btnUpload
        // 
        _btnUpload.Location = new Point(740, 600);
        _btnUpload.Name = "_btnUpload";
        _btnUpload.Size = new Size(260, 26);
        _btnUpload.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _btnUpload.TabIndex = 4;
        _btnUpload.Text = "Upload";
        _btnUpload.Click += Upload_Submit;
        // 
        // _upIconLabel
        // 
        _upIconLabel.AutoSize = true;
        _upIconLabel.Location = new Point(806, 432);
        _upIconLabel.Name = "_upIconLabel";
        _upIconLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _upIconLabel.Text = "Selected icon";
        _upIconLabel.Visible = false;
        // 
        // _upIconPreview
        // 
        _upIconPreview.BorderStyle = BorderStyle.FixedSingle;
        _upIconPreview.Location = new Point(806, 452);
        _upIconPreview.Name = "_upIconPreview";
        _upIconPreview.Size = new Size(128, 128);
        _upIconPreview.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _upIconPreview.SizeMode = PictureBoxSizeMode.Zoom;
        _upIconPreview.TabStop = false;
        _upIconPreview.Visible = false;
        // 
        // tooltips
        // 
        _tips.SetToolTip(_btnBrowse, "Start a new upload from a save file");
        _tips.SetToolTip(_btnBrowseFolder, "Start a new upload from a save folder");
        _tips.SetToolTip(_btnIcon, "Pick a cover image for this game (shown on the right)");
        _tips.SetToolTip(_btnDetect, "Detect the title id / game name from the selected save");
        _tips.SetToolTip(_upAdd, "Add a single file, multiple files, or a folder to this upload");
        _tips.SetToolTip(_upEdit, "Rename this upload (edits the Game Name)");
        _tips.SetToolTip(_upRemove, "Remove the selected file from this upload");
        _tips.SetToolTip(_upGameName, "Shown in your library; required for PC save folders");
        _tips.SetToolTip(_upTitleId, "e.g. SLUS-12345 (auto-detected for PlayStation)");
        // 
        // UploadTab
        // 
        Controls.Add(_saveBox);
        Controls.Add(_detailsBox);
        Controls.Add(_deviceBox);
        Controls.Add(_saveTypeBox);
        Controls.Add(_upIconLabel);
        Controls.Add(_upIconPreview);
        Controls.Add(_btnUpload);
        Name = "UploadTab";
        Size = new Size(1008, 660);
        _saveBox.ResumeLayout(false);
        _saveBox.PerformLayout();
        _detailsBox.ResumeLayout(false);
        _detailsBox.PerformLayout();
        _deviceBox.ResumeLayout(false);
        _saveTypeBox.ResumeLayout(false);
        _saveTypeBox.PerformLayout();
        ResumeLayout(false);
    }
}
