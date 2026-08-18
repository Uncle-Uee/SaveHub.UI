using SaveHub.WinForms.Tabs;

namespace SaveHub.WinForms;

/// <summary>
/// Designer-owned shell layout for <see cref="MainForm"/>: a tab control hosting one
/// UserControl per feature tab plus the status bar. Each tab's own layout lives in its
/// respective UserControl so it can be edited independently in the designer.
/// </summary>
public sealed partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    private TabControl _tabs;
    private TabPage _tabUpload;
    private TabPage _tabDownload;
    private TabPage _tabEdit;
    private TabPage _tabManage;
    private TabPage _tabSettings;

    private UploadTab _uploadTab;
    private DownloadTab _downloadTab;
    private EditTab _editTab;
    private ManageTab _manageTab;
    private SettingsTab _settingsTab;

    private Panel _bottom;
    private Label _status;
    private Button _donate;

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
        _tabs = new TabControl();
        _tabUpload = new TabPage();
        _uploadTab = new UploadTab();
        _tabDownload = new TabPage();
        _downloadTab = new DownloadTab();
        _tabEdit = new TabPage();
        _editTab = new EditTab();
        _tabManage = new TabPage();
        _manageTab = new ManageTab();
        _tabSettings = new TabPage();
        _settingsTab = new SettingsTab();
        _bottom = new Panel();
        _status = new Label();
        _donate = new Button();
        pbar = new ProgressBar();
        _tabs.SuspendLayout();
        _tabUpload.SuspendLayout();
        _tabDownload.SuspendLayout();
        _tabEdit.SuspendLayout();
        _tabManage.SuspendLayout();
        _tabSettings.SuspendLayout();
        _bottom.SuspendLayout();
        SuspendLayout();
        // 
        // _tabs
        // 
        _tabs.Controls.Add(_tabUpload);
        _tabs.Controls.Add(_tabDownload);
        _tabs.Controls.Add(_tabEdit);
        _tabs.Controls.Add(_tabManage);
        _tabs.Controls.Add(_tabSettings);
        _tabs.Dock = DockStyle.Fill;
        _tabs.Location = new Point(0, 0);
        _tabs.Name = "_tabs";
        _tabs.SelectedIndex = 0;
        _tabs.Size = new Size(791, 713);
        _tabs.TabIndex = 0;
        _tabs.SelectedIndexChanged += Tabs_SelectedIndexChanged;
        // 
        // _tabUpload
        // 
        _tabUpload.Controls.Add(_uploadTab);
        _tabUpload.Location = new Point(4, 24);
        _tabUpload.Name = "_tabUpload";
        _tabUpload.Size = new Size(783, 685);
        _tabUpload.TabIndex = 0;
        _tabUpload.Text = "Upload";
        // 
        // _uploadTab
        // 
        _uploadTab.Dock = DockStyle.Fill;
        _uploadTab.Location = new Point(0, 0);
        _uploadTab.Name = "_uploadTab";
        _uploadTab.Size = new Size(783, 685);
        _uploadTab.TabIndex = 0;
        // 
        // _tabDownload
        // 
        _tabDownload.Controls.Add(_downloadTab);
        _tabDownload.Location = new Point(4, 24);
        _tabDownload.Name = "_tabDownload";
        _tabDownload.Size = new Size(783, 685);
        _tabDownload.TabIndex = 1;
        _tabDownload.Text = "Download";
        // 
        // _downloadTab
        // 
        _downloadTab.Dock = DockStyle.Fill;
        _downloadTab.Location = new Point(0, 0);
        _downloadTab.Name = "_downloadTab";
        _downloadTab.Size = new Size(783, 685);
        _downloadTab.TabIndex = 0;
        // 
        // _tabEdit
        // 
        _tabEdit.Controls.Add(_editTab);
        _tabEdit.Location = new Point(4, 24);
        _tabEdit.Name = "_tabEdit";
        _tabEdit.Size = new Size(783, 685);
        _tabEdit.TabIndex = 2;
        _tabEdit.Text = "Edit";
        // 
        // _editTab
        // 
        _editTab.Dock = DockStyle.Fill;
        _editTab.Location = new Point(0, 0);
        _editTab.Name = "_editTab";
        _editTab.Size = new Size(783, 685);
        _editTab.TabIndex = 0;
        // 
        // _tabManage
        // 
        _tabManage.Controls.Add(_manageTab);
        _tabManage.Location = new Point(4, 24);
        _tabManage.Name = "_tabManage";
        _tabManage.Size = new Size(783, 685);
        _tabManage.TabIndex = 3;
        _tabManage.Text = "Manage";
        // 
        // _manageTab
        // 
        _manageTab.Dock = DockStyle.Fill;
        _manageTab.Location = new Point(0, 0);
        _manageTab.Name = "_manageTab";
        _manageTab.Size = new Size(783, 685);
        _manageTab.TabIndex = 0;
        // 
        // _tabSettings
        // 
        _tabSettings.Controls.Add(_settingsTab);
        _tabSettings.Location = new Point(4, 24);
        _tabSettings.Name = "_tabSettings";
        _tabSettings.Size = new Size(783, 685);
        _tabSettings.TabIndex = 4;
        _tabSettings.Text = "Settings";
        // 
        // _settingsTab
        // 
        _settingsTab.Dock = DockStyle.Fill;
        _settingsTab.Location = new Point(0, 0);
        _settingsTab.Name = "_settingsTab";
        _settingsTab.Size = new Size(783, 685);
        _settingsTab.TabIndex = 0;
        // 
        // _bottom
        // 
        _bottom.Controls.Add(pbar);
        _bottom.Controls.Add(_status);
        _bottom.Controls.Add(_donate);
        _bottom.Dock = DockStyle.Bottom;
        _bottom.Location = new Point(0, 713);
        _bottom.Name = "_bottom";
        _bottom.Size = new Size(791, 28);
        _bottom.TabIndex = 1;
        // 
        // _status
        // 
        _status.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        _status.Location = new Point(8, 7);
        _status.Name = "_status";
        _status.Size = new Size(560, 16);
        _status.TabIndex = 0;
        _status.Text = "Ready.";
        // 
        // _donate
        // 
        _donate.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _donate.Location = new Point(680, 3);
        _donate.Name = "_donate";
        _donate.Size = new Size(100, 23);
        _donate.TabIndex = 2;
        _donate.Text = "❤ Donate";
        _donate.UseVisualStyleBackColor = true;
        _donate.Click += Donate_Click;
        // 
        // pbar
        // 
        pbar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        pbar.Location = new Point(574, 2);
        pbar.MarqueeAnimationSpeed = 30;
        pbar.Name = "pbar";
        pbar.Size = new Size(100, 23);
        pbar.Style = ProgressBarStyle.Marquee;
        pbar.TabIndex = 1;
        pbar.Visible = false;
        // 
        // MainForm
        // 
        ClientSize = new Size(791, 741);
        Controls.Add(_tabs);
        Controls.Add(_bottom);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MaximumSize = new Size(880, 800);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "SaveHub";
        _tabs.ResumeLayout(false);
        _tabUpload.ResumeLayout(false);
        _tabDownload.ResumeLayout(false);
        _tabEdit.ResumeLayout(false);
        _tabManage.ResumeLayout(false);
        _tabSettings.ResumeLayout(false);
        _bottom.ResumeLayout(false);
        ResumeLayout(false);
    }

    private ProgressBar pbar;
}
