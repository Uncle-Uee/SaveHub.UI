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
    private TabPage _tabLibrary;
    private TabPage _tabSettings;

    private UploadTab _uploadTab;
    private DownloadTab _downloadTab;
    private EditTab _editTab;
    private ManageTab _manageTab;
    private LibraryTab _libraryTab;
    private SettingsTab _settingsTab;

    private Panel _bottom;
    private Label _status;
    private Button _donate;

    private MenuStrip _menu;
    private ToolStripMenuItem _miFile;
    private ToolStripMenuItem _miQuit;
    private ToolStripMenuItem _miTools;
    private ToolStripMenuItem _miUpload;
    private ToolStripMenuItem _miDownload;
    private ToolStripMenuItem _miEdit;
    private ToolStripMenuItem _miManage;
    private ToolStripMenuItem _miLibrary;
    private ToolStripMenuItem _miSettings;
    private ToolStripMenuItem _miRebuild;
    private ToolStripMenuItem _miHelp;
    private ToolStripMenuItem _miDocs;
    private ToolStripMenuItem _miSource;
    private ToolStripMenuItem _miDonateMenu;
    private ToolStripMenuItem _miAbout;

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
        _tabLibrary = new TabPage();
        _libraryTab = new LibraryTab();
        _tabSettings = new TabPage();
        _settingsTab = new SettingsTab();
        _bottom = new Panel();
        _status = new Label();
        _donate = new Button();
        _menu = new MenuStrip();
        _miFile = new ToolStripMenuItem();
        _miQuit = new ToolStripMenuItem();
        _miTools = new ToolStripMenuItem();
        _miUpload = new ToolStripMenuItem();
        _miDownload = new ToolStripMenuItem();
        _miEdit = new ToolStripMenuItem();
        _miManage = new ToolStripMenuItem();
        _miLibrary = new ToolStripMenuItem();
        _miSettings = new ToolStripMenuItem();
        _miRebuild = new ToolStripMenuItem();
        _miHelp = new ToolStripMenuItem();
        _miDocs = new ToolStripMenuItem();
        _miSource = new ToolStripMenuItem();
        _miDonateMenu = new ToolStripMenuItem();
        _miAbout = new ToolStripMenuItem();
        pbar = new ProgressBar();
        _tabs.SuspendLayout();
        _tabUpload.SuspendLayout();
        _tabDownload.SuspendLayout();
        _tabEdit.SuspendLayout();
        _tabManage.SuspendLayout();
        _tabSettings.SuspendLayout();
        _bottom.SuspendLayout();
        _menu.SuspendLayout();
        SuspendLayout();
        // 
        // _tabs
        // 
        _tabs.Controls.Add(_tabUpload);
        _tabs.Controls.Add(_tabDownload);
        _tabs.Controls.Add(_tabEdit);
        _tabs.Controls.Add(_tabManage);
        _tabs.Controls.Add(_tabLibrary);
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
        // _tabLibrary
        // 
        _tabLibrary.Controls.Add(_libraryTab);
        _tabLibrary.Location = new Point(4, 24);
        _tabLibrary.Name = "_tabLibrary";
        _tabLibrary.Size = new Size(783, 685);
        _tabLibrary.TabIndex = 4;
        _tabLibrary.Text = "Library";
        // 
        // _libraryTab
        // 
        _libraryTab.Dock = DockStyle.Fill;
        _libraryTab.Location = new Point(0, 0);
        _libraryTab.Name = "_libraryTab";
        _libraryTab.Size = new Size(783, 685);
        _libraryTab.TabIndex = 0;
        // 
        // _tabSettings
        // 
        _tabSettings.Controls.Add(_settingsTab);
        _tabSettings.Location = new Point(4, 24);
        _tabSettings.Name = "_tabSettings";
        _tabSettings.Size = new Size(783, 685);
        _tabSettings.TabIndex = 5;
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
        // _menu
        // 
        _miFile.Text = "&File";
        _miFile.DropDownItems.Add(_miQuit);
        _miQuit.Text = "&Quit";
        _miQuit.Click += Menu_Quit;
        _miTools.Text = "&Tools";
        _miTools.DropDownItems.AddRange(new ToolStripItem[] { _miUpload, _miDownload, _miEdit, _miManage, _miLibrary, _miSettings, new ToolStripSeparator(), _miRebuild });
        _miUpload.Text = "&Upload";
        _miUpload.Tag = 0;
        _miUpload.Click += Menu_ShowTab;
        _miDownload.Text = "&Download";
        _miDownload.Tag = 1;
        _miDownload.Click += Menu_ShowTab;
        _miEdit.Text = "&Edit";
        _miEdit.Tag = 2;
        _miEdit.Click += Menu_ShowTab;
        _miManage.Text = "&Manage";
        _miManage.Tag = 3;
        _miManage.Click += Menu_ShowTab;
        _miLibrary.Text = "&Library";
        _miLibrary.Tag = 4;
        _miLibrary.Click += Menu_ShowTab;
        _miSettings.Text = "&Settings";
        _miSettings.Tag = 5;
        _miSettings.Click += Menu_ShowTab;
        _miRebuild.Text = "&Rebuild Library Index";
        _miRebuild.Click += Menu_RebuildLibrary;
        _miHelp.Text = "&Help";
        _miHelp.DropDownItems.AddRange(new ToolStripItem[] { _miDocs, _miSource, _miDonateMenu, new ToolStripSeparator(), _miAbout });
        _miDocs.Text = "&Documentation (README)";
        _miDocs.Click += Menu_OpenReadme;
        _miSource.Text = "&Source project";
        _miSource.Click += Menu_OpenSource;
        _miDonateMenu.Text = "Support / &Donate";
        _miDonateMenu.Click += Donate_Click;
        _miAbout.Text = "&About SaveHub";
        _miAbout.Click += Menu_About;
        _menu.Items.AddRange(new ToolStripItem[] { _miFile, _miTools, _miHelp });
        _menu.Location = new Point(0, 0);
        _menu.Name = "_menu";
        _menu.Size = new Size(791, 24);
        _menu.TabIndex = 3;
        // 
        // MainForm
        // 
        ClientSize = new Size(791, 741);
        Controls.Add(_tabs);
        Controls.Add(_bottom);
        Controls.Add(_menu);
        MainMenuStrip = _menu;
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
        _menu.ResumeLayout(false);
        _menu.PerformLayout();
        ResumeLayout(false);
    }

    private ProgressBar pbar;
}
