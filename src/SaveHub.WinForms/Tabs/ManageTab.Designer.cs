namespace SaveHub.WinForms.Tabs;

partial class ManageTab
{
    private System.ComponentModel.IContainer components = null;

    private Label _lblSystem;
    private ComboBox _mgSystem;
    private Label _lblGame;
    private ComboBox _mgGame;
    private Button _btnRefresh;
    private ListView _mgList;
    private PictureBox _mgIcon;
    private Label _mgName;
    private Button _btnDelete;
    private Label _lblHint;

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
        _mgList = new ListView();
        _mgIcon = new PictureBox();
        _mgName = new Label();
        _btnDelete = new Button();
        _lblHint = new Label();
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
        // _mgList
        //
        _mgList.Location = new Point(12, 48);
        _mgList.Size = new Size(600, 560);
        _mgList.MultiSelect = true;
        //
        // _mgIcon
        //
        _mgIcon.Location = new Point(620, 48);
        _mgIcon.Size = new Size(150, 150);
        _mgIcon.SizeMode = PictureBoxSizeMode.Zoom;
        _mgIcon.BorderStyle = BorderStyle.FixedSingle;
        //
        // _mgName
        //
        _mgName.Location = new Point(620, 202);
        _mgName.Size = new Size(150, 60);
        _mgName.AutoSize = false;
        //
        // _btnDelete
        //
        _btnDelete.Text = "Delete Selected";
        _btnDelete.Location = new Point(12, 616);
        _btnDelete.Size = new Size(160, 32);
        _btnDelete.Click += Manage_DeleteSelected;
        //
        // _lblHint
        //
        _lblHint.Text = "Select one or more saves and delete them. This cannot be undone.";
        _lblHint.Location = new Point(184, 624);
        _lblHint.AutoSize = true;
        //
        // ManageTab
        //
        Controls.Add(_lblSystem);
        Controls.Add(_mgSystem);
        Controls.Add(_lblGame);
        Controls.Add(_mgGame);
        Controls.Add(_btnRefresh);
        Controls.Add(_mgList);
        Controls.Add(_mgIcon);
        Controls.Add(_mgName);
        Controls.Add(_btnDelete);
        Controls.Add(_lblHint);
        Size = new Size(784, 714);
        ((System.ComponentModel.ISupportInitialize)_mgIcon).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
