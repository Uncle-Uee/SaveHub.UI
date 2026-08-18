namespace SaveHub.WinForms.Tabs;

partial class EditTab
{
    private System.ComponentModel.IContainer components = null;

    private Label _lblSystem;
    private ComboBox _edSystem;
    private Label _lblGame;
    private ComboBox _edGame;
    private Button _btnRefresh;
    private ListView _edList;
    private PictureBox _edIcon;
    private Label _edName;
    private GroupBox _replaceBox;
    private Button _btnBrowse;
    private TextBox _edPath;
    private TextBox _edDescription;
    private Button _btnUpdate;

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
        _edSystem = new ComboBox();
        _lblGame = new Label();
        _edGame = new ComboBox();
        _btnRefresh = new Button();
        _edList = new ListView();
        _edIcon = new PictureBox();
        _edName = new Label();
        _replaceBox = new GroupBox();
        _btnBrowse = new Button();
        _edPath = new TextBox();
        _edDescription = new TextBox();
        _btnUpdate = new Button();
        ((System.ComponentModel.ISupportInitialize)_edIcon).BeginInit();
        SuspendLayout();
        //
        // _lblSystem
        //
        _lblSystem.Text = "System:";
        _lblSystem.Location = new Point(12, 16);
        _lblSystem.AutoSize = true;
        //
        // _edSystem
        //
        _edSystem.DropDownStyle = ComboBoxStyle.DropDownList;
        _edSystem.Location = new Point(80, 12);
        _edSystem.Size = new Size(180, 23);
        _edSystem.SelectedIndexChanged += Edit_SystemChanged;
        //
        // _lblGame
        //
        _lblGame.Text = "Game:";
        _lblGame.Location = new Point(276, 16);
        _lblGame.AutoSize = true;
        //
        // _edGame
        //
        _edGame.DropDownStyle = ComboBoxStyle.DropDownList;
        _edGame.Location = new Point(324, 12);
        _edGame.Size = new Size(200, 23);
        _edGame.SelectedIndexChanged += Edit_GameChanged;
        //
        // _btnRefresh
        //
        _btnRefresh.Text = "Refresh";
        _btnRefresh.Location = new Point(536, 11);
        _btnRefresh.Size = new Size(90, 26);
        _btnRefresh.Click += Edit_RefreshSystems;
        //
        // _edList
        //
        _edList.Location = new Point(12, 48);
        _edList.Size = new Size(600, 300);
        _edList.SelectedIndexChanged += Edit_SelectionChanged;
        //
        // _edIcon
        //
        _edIcon.Location = new Point(620, 48);
        _edIcon.Size = new Size(150, 150);
        _edIcon.SizeMode = PictureBoxSizeMode.Zoom;
        _edIcon.BorderStyle = BorderStyle.FixedSingle;
        //
        // _edName
        //
        _edName.Location = new Point(620, 202);
        _edName.Size = new Size(150, 60);
        _edName.AutoSize = false;
        //
        // _replaceBox
        //
        _replaceBox.Text = "Replace selected save";
        _replaceBox.Location = new Point(12, 356);
        _replaceBox.Size = new Size(760, 180);
        _replaceBox.Controls.Add(new Label { Text = "Description:", Location = new Point(12, 70), AutoSize = true });
        _replaceBox.Controls.Add(_btnBrowse);
        _replaceBox.Controls.Add(_edPath);
        _replaceBox.Controls.Add(_edDescription);
        _replaceBox.Controls.Add(_btnUpdate);
        //
        // _btnBrowse
        //
        _btnBrowse.Text = "Browse new file(s)/folder...";
        _btnBrowse.Location = new Point(12, 28);
        _btnBrowse.Size = new Size(200, 28);
        _btnBrowse.Click += Edit_Browse;
        //
        // _edPath
        //
        _edPath.Location = new Point(220, 30);
        _edPath.Size = new Size(524, 23);
        _edPath.ReadOnly = true;
        //
        // _edDescription
        //
        _edDescription.Location = new Point(90, 66);
        _edDescription.Size = new Size(654, 60);
        _edDescription.Multiline = true;
        _edDescription.MaxLength = MaxDescription;
        //
        // _btnUpdate
        //
        _btnUpdate.Text = "Update Save (keep same number)";
        _btnUpdate.Location = new Point(90, 136);
        _btnUpdate.Size = new Size(260, 32);
        _btnUpdate.Click += Edit_Update;
        //
        // EditTab
        //
        Controls.Add(_lblSystem);
        Controls.Add(_edSystem);
        Controls.Add(_lblGame);
        Controls.Add(_edGame);
        Controls.Add(_btnRefresh);
        Controls.Add(_edList);
        Controls.Add(_edIcon);
        Controls.Add(_edName);
        Controls.Add(_replaceBox);
        Size = new Size(784, 714);
        ((System.ComponentModel.ISupportInitialize)_edIcon).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
