namespace SaveHub.WinForms.Tabs;

partial class ManageTab
{
    private System.ComponentModel.IContainer components = null;

    private Label _lblSystem;
    private ComboBox _mgSystem;
    private Label _lblGame;
    private ComboBox _mgGame;
    private Button _btnRefresh;
    private TextBox _txtGameFilter;
    private TextBox _txtSaveFilter;
    private ListView _mgList;
    private PictureBox _mgIcon;
    private Label _mgName;
    private Label _lblDetails;
    private Button _btnDownload;
    private Button _btnDelete;
    private Button _btnDeleteAll;
    private Button _btnRename;

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
        _lblSystem = new Label();
        _mgSystem = new ComboBox();
        _lblGame = new Label();
        _mgGame = new ComboBox();
        _btnRefresh = new Button();
        _txtGameFilter = new TextBox();
        _txtSaveFilter = new TextBox();
        _mgList = new ListView();
        _mgIcon = new PictureBox();
        _mgName = new Label();
        _lblDetails = new Label();
        _btnDownload = new Button();
        _btnDelete = new Button();
        _btnDeleteAll = new Button();
        _btnRename = new Button();
        ((System.ComponentModel.ISupportInitialize)_mgIcon).BeginInit();
        SuspendLayout();
        //
        // _lblSystem
        //
        _lblSystem.Text = "System:";
        _lblSystem.Location = new Point(12, 16);
        _lblSystem.AutoSize = true;
        //
        // _mgSystem
        //
        _mgSystem.DropDownStyle = ComboBoxStyle.DropDownList;
        _mgSystem.Location = new Point(80, 12);
        _mgSystem.Size = new Size(180, 23);
        _mgSystem.SelectedIndexChanged += Manage_SystemChanged;
        //
        // _lblGame
        //
        _lblGame.Text = "Game:";
        _lblGame.Location = new Point(276, 16);
        _lblGame.AutoSize = true;
        //
        // _mgGame
        //
        _mgGame.DropDownStyle = ComboBoxStyle.DropDownList;
        _mgGame.Location = new Point(324, 12);
        _mgGame.Size = new Size(200, 23);
        _mgGame.SelectedIndexChanged += Manage_GameChanged;
        //
        // _btnRefresh
        //
        _btnRefresh.Text = "Refresh";
        _btnRefresh.Location = new Point(536, 11);
        _btnRefresh.Size = new Size(90, 26);
        _btnRefresh.Click += Manage_RefreshSystems;
        //
        // _txtGameFilter
        //
        _txtGameFilter.Location = new Point(80, 44);
        _txtGameFilter.Size = new Size(180, 23);
        _txtGameFilter.PlaceholderText = "Filter games";
        _txtGameFilter.TextChanged += GameFilter_Changed;
        //
        // _txtSaveFilter
        //
        _txtSaveFilter.Location = new Point(324, 44);
        _txtSaveFilter.Size = new Size(200, 23);
        _txtSaveFilter.PlaceholderText = "Filter saves";
        _txtSaveFilter.TextChanged += SaveFilter_Changed;
        //
        // _mgList
        //
        _mgList.Location = new Point(12, 76);
        _mgList.Size = new Size(600, 496);
        _mgList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _mgList.MultiSelect = true;
        _mgList.SelectedIndexChanged += Manage_SelectionChanged;
        //
        // _mgIcon
        //
        _mgIcon.Location = new Point(620, 76);
        _mgIcon.Size = new Size(150, 150);
        _mgIcon.SizeMode = PictureBoxSizeMode.Zoom;
        _mgIcon.BorderStyle = BorderStyle.FixedSingle;
        _mgIcon.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        //
        // _mgName
        //
        _mgName.Location = new Point(620, 232);
        _mgName.Size = new Size(150, 40);
        _mgName.AutoSize = false;
        _mgName.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        //
        // _lblDetails
        //
        _lblDetails.Location = new Point(620, 280);
        _lblDetails.Size = new Size(152, 290);
        _lblDetails.AutoSize = false;
        _lblDetails.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
        //
        // _btnDownload
        //
        _btnDownload.Text = "Download Selected";
        _btnDownload.Location = new Point(12, 584);
        _btnDownload.Size = new Size(150, 30);
        _btnDownload.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        _btnDownload.Click += Manage_DownloadSelected;
        //
        // _btnDelete
        //
        _btnDelete.Text = "Delete Selected";
        _btnDelete.Location = new Point(172, 584);
        _btnDelete.Size = new Size(150, 30);
        _btnDelete.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        _btnDelete.Click += Manage_DeleteSelected;
        //
        // _btnDeleteAll
        //
        _btnDeleteAll.Text = "Delete All (game)";
        _btnDeleteAll.Location = new Point(332, 584);
        _btnDeleteAll.Size = new Size(150, 30);
        _btnDeleteAll.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        _btnDeleteAll.Click += Manage_DeleteAll;
        //
        // _btnRename
        //
        _btnRename.Text = "Rename Game";
        _btnRename.Location = new Point(492, 584);
        _btnRename.Size = new Size(150, 30);
        _btnRename.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        _btnRename.Click += Manage_Rename;
        //
        // ManageTab
        //
        Controls.Add(_lblSystem);
        Controls.Add(_mgSystem);
        Controls.Add(_lblGame);
        Controls.Add(_mgGame);
        Controls.Add(_btnRefresh);
        Controls.Add(_txtGameFilter);
        Controls.Add(_txtSaveFilter);
        Controls.Add(_mgList);
        Controls.Add(_mgIcon);
        Controls.Add(_mgName);
        Controls.Add(_lblDetails);
        Controls.Add(_btnDownload);
        Controls.Add(_btnDelete);
        Controls.Add(_btnDeleteAll);
        Controls.Add(_btnRename);
        Size = new Size(784, 714);
        ((System.ComponentModel.ISupportInitialize)_mgIcon).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
