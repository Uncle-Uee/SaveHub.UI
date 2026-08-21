namespace SaveHub.WinForms.Tabs;

partial class DownloadTab
{
    private System.ComponentModel.IContainer components = null;

    private Label _lblSystem;
    private ComboBox _dlSystem;
    private Button _btnRefresh;
    private ListView _dlList;
    private PictureBox _dlIcon;
    private Label _dlName;
    private Button _btnDownload;

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
        _dlSystem = new ComboBox();
        _btnRefresh = new Button();
        _dlList = new ListView();
        _dlIcon = new PictureBox();
        _dlName = new Label();
        _btnDownload = new Button();
        ((System.ComponentModel.ISupportInitialize)_dlIcon).BeginInit();
        SuspendLayout();
        //
        // _lblSystem
        //
        _lblSystem.Text = "System:";
        _lblSystem.Location = new Point(12, 16);
        _lblSystem.AutoSize = true;
        //
        // _dlSystem
        //
        _dlSystem.DropDownStyle = ComboBoxStyle.DropDownList;
        _dlSystem.Location = new Point(80, 12);
        _dlSystem.Size = new Size(200, 23);
        _dlSystem.SelectedIndexChanged += Download_SystemChanged;
        //
        // _btnRefresh
        //
        _btnRefresh.Text = "Refresh Systems";
        _btnRefresh.Location = new Point(290, 11);
        _btnRefresh.Size = new Size(130, 26);
        _btnRefresh.Click += Download_RefreshSystems;
        //
        // _dlList
        //
        _dlList.Location = new Point(12, 48);
        _dlList.Size = new Size(600, 612);
        _dlList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _dlList.SelectedIndexChanged += Download_SelectionChanged;
        //
        // _dlIcon
        //
        _dlIcon.Location = new Point(620, 48);
        _dlIcon.Size = new Size(150, 150);
        _dlIcon.SizeMode = PictureBoxSizeMode.Zoom;
        _dlIcon.BorderStyle = BorderStyle.FixedSingle;
        _dlIcon.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        //
        // _dlName
        //
        _dlName.Location = new Point(620, 202);
        _dlName.Size = new Size(150, 60);
        _dlName.AutoSize = false;
        _dlName.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        //
        // _btnDownload
        //
        _btnDownload.Text = "Download Selected";
        _btnDownload.Location = new Point(620, 270);
        _btnDownload.Size = new Size(150, 28);
        _btnDownload.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _btnDownload.Click += Download_Selected;
        //
        // DownloadTab
        //
        Controls.Add(_lblSystem);
        Controls.Add(_dlSystem);
        Controls.Add(_btnRefresh);
        Controls.Add(_dlList);
        Controls.Add(_dlIcon);
        Controls.Add(_dlName);
        Controls.Add(_btnDownload);
        Size = new Size(784, 714);
        ((System.ComponentModel.ISupportInitialize)_dlIcon).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
