namespace SaveHub.WinForms.Tabs;

partial class SettingsTab
{
    private System.ComponentModel.IContainer components = null;

    private GroupBox _box;
    private Label _lblProvider;
    private ComboBox _stProvider;

    private Panel _pnlGitHub;
    private TextBox _stOwner;
    private TextBox _stRepo;
    private TextBox _stBranch;
    private TextBox _stToken;
    private CheckBox _stAutoMerge;

    private Panel _pnlSupabase;
    private TextBox _sbUrl;
    private TextBox _sbBucket;
    private TextBox _sbKey;
    private CheckBox _sbOwner;

    private Panel _pnlGoogle;
    private TextBox _gdRoot;
    private TextBox _gdClientId;
    private TextBox _gdSecret;
    private CheckBox _gdOwner;
    private Button _btnSignIn;
    private Label _gdStatus;

    private Button _btnSave;
    private Button _btnTest;
    private Label _lblConfig;

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
        _box = new GroupBox();
        _lblProvider = new Label();
        _stProvider = new ComboBox();
        _pnlGitHub = new Panel();
        _stOwner = new TextBox();
        _stRepo = new TextBox();
        _stBranch = new TextBox();
        _stToken = new TextBox();
        _stAutoMerge = new CheckBox();
        _pnlSupabase = new Panel();
        _sbUrl = new TextBox();
        _sbBucket = new TextBox();
        _sbKey = new TextBox();
        _sbOwner = new CheckBox();
        _pnlGoogle = new Panel();
        _gdRoot = new TextBox();
        _gdClientId = new TextBox();
        _gdSecret = new TextBox();
        _gdOwner = new CheckBox();
        _btnSignIn = new Button();
        _gdStatus = new Label();
        _btnSave = new Button();
        _btnTest = new Button();
        _lblConfig = new Label();
        _box.SuspendLayout();
        _pnlGitHub.SuspendLayout();
        _pnlSupabase.SuspendLayout();
        _pnlGoogle.SuspendLayout();
        SuspendLayout();
        //
        // _box
        //
        _box.Text = "Storage connection";
        _box.Location = new Point(12, 12);
        _box.Size = new Size(760, 360);
        _box.Controls.Add(_lblProvider);
        _box.Controls.Add(_stProvider);
        _box.Controls.Add(_pnlGitHub);
        _box.Controls.Add(_pnlSupabase);
        _box.Controls.Add(_pnlGoogle);
        _box.Controls.Add(_btnSave);
        _box.Controls.Add(_btnTest);
        //
        // _lblProvider
        //
        _lblProvider.Text = "Provider:";
        _lblProvider.Location = new Point(16, 22);
        _lblProvider.AutoSize = true;
        //
        // _stProvider
        //
        _stProvider.DropDownStyle = ComboBoxStyle.DropDownList;
        _stProvider.Location = new Point(120, 18);
        _stProvider.Size = new Size(200, 23);
        _stProvider.SelectedIndexChanged += Settings_ProviderChanged;
        //
        // _pnlGitHub
        //
        _pnlGitHub.Location = new Point(12, 48);
        _pnlGitHub.Size = new Size(748, 250);
        _pnlGitHub.Controls.Add(new Label { Text = "Owner:", Location = new Point(4, 8), AutoSize = true });
        _pnlGitHub.Controls.Add(new Label { Text = "Repository:", Location = new Point(4, 42), AutoSize = true });
        _pnlGitHub.Controls.Add(new Label { Text = "Branch:", Location = new Point(4, 76), AutoSize = true });
        _pnlGitHub.Controls.Add(new Label { Text = "Token:", Location = new Point(4, 110), AutoSize = true });
        _pnlGitHub.Controls.Add(new Label { Text = "(or SAVEHUB_GITHUB_TOKEN env var)", Location = new Point(430, 110), AutoSize = true });
        _pnlGitHub.Controls.Add(_stOwner);
        _pnlGitHub.Controls.Add(_stRepo);
        _pnlGitHub.Controls.Add(_stBranch);
        _pnlGitHub.Controls.Add(_stToken);
        _pnlGitHub.Controls.Add(_stAutoMerge);
        _pnlGitHub.Controls.Add(new Label
        {
            Text = "What this is: SaveHub keeps your saves as files in a GitHub repository you own. "
                 + "Every upload is sent as a pull request (owners can auto-merge), so changes are "
                 + "tracked and easy to undo. It needs a personal access token with Contents and "
                 + "Pull requests (read & write) on that one repo \u2014 paste it above or set the "
                 + "SAVEHUB_GITHUB_TOKEN environment variable. SaveHub only touches this repository.",
            Location = new Point(4, 172),
            Size = new Size(738, 74),
            AutoSize = false,
            ForeColor = SystemColors.GrayText,
        });
        //
        // _stOwner / _stRepo / _stBranch / _stToken / _stAutoMerge
        //
        _stOwner.Location = new Point(120, 4); _stOwner.Size = new Size(300, 23);
        _stRepo.Location = new Point(120, 38); _stRepo.Size = new Size(300, 23);
        _stBranch.Location = new Point(120, 72); _stBranch.Size = new Size(300, 23);
        _stToken.Location = new Point(120, 106); _stToken.Size = new Size(300, 23); _stToken.UseSystemPasswordChar = true;
        _stAutoMerge.Text = "Enable auto-merge (at your own risk)";
        _stAutoMerge.Location = new Point(120, 140); _stAutoMerge.AutoSize = true;
        //
        // _pnlSupabase
        //
        _pnlSupabase.Location = new Point(12, 48);
        _pnlSupabase.Size = new Size(748, 250);
        _pnlSupabase.Visible = false;
        _pnlSupabase.Controls.Add(new Label { Text = "Project URL:", Location = new Point(4, 8), AutoSize = true });
        _pnlSupabase.Controls.Add(new Label { Text = "Bucket:", Location = new Point(4, 42), AutoSize = true });
        _pnlSupabase.Controls.Add(new Label { Text = "API Key:", Location = new Point(4, 76), AutoSize = true });
        _pnlSupabase.Controls.Add(new Label { Text = "(or SAVEHUB_SUPABASE_KEY env var)", Location = new Point(120, 100), AutoSize = true });
        _pnlSupabase.Controls.Add(_sbUrl);
        _pnlSupabase.Controls.Add(_sbBucket);
        _pnlSupabase.Controls.Add(_sbKey);
        _pnlSupabase.Controls.Add(_sbOwner);
        _pnlSupabase.Controls.Add(new Label
        {
            Text = "What this is: SaveHub keeps your saves as files in a Supabase Storage bucket. "
                 + "If you own the bucket they publish straight away; otherwise they go to a "
                 + "'pending/' folder for the owner to review. It needs your project URL, the bucket "
                 + "name, and an API key with write access to that bucket \u2014 paste the key above or "
                 + "set the SAVEHUB_SUPABASE_KEY environment variable. SaveHub only accesses this bucket.",
            Location = new Point(4, 172),
            Size = new Size(738, 74),
            AutoSize = false,
            ForeColor = SystemColors.GrayText,
        });
        //
        // _sbUrl / _sbBucket / _sbKey / _sbOwner
        //
        _sbUrl.Location = new Point(120, 4); _sbUrl.Size = new Size(400, 23);
        _sbBucket.Location = new Point(120, 38); _sbBucket.Size = new Size(300, 23);
        _sbKey.Location = new Point(120, 72); _sbKey.Size = new Size(400, 23); _sbKey.UseSystemPasswordChar = true;
        _sbOwner.Text = "I own this bucket (publish directly)";
        _sbOwner.Location = new Point(120, 128); _sbOwner.AutoSize = true;
        //
        // _pnlGoogle
        //
        _pnlGoogle.Location = new Point(12, 48);
        _pnlGoogle.Size = new Size(748, 250);
        _pnlGoogle.Visible = false;
        _pnlGoogle.Controls.Add(new Label { Text = "Folder name:", Location = new Point(4, 8), AutoSize = true });
        _pnlGoogle.Controls.Add(new Label { Text = "(created in your Drive)", Location = new Point(428, 8), AutoSize = true });
        _pnlGoogle.Controls.Add(new Label { Text = "OAuth Client ID:", Location = new Point(4, 42), AutoSize = true });
        _pnlGoogle.Controls.Add(new Label { Text = "Client Secret:", Location = new Point(4, 76), AutoSize = true });
        _pnlGoogle.Controls.Add(new Label
        {
            Text = "What signing in does: SaveHub creates one folder (named above) in your Google Drive "
                 + "and uses it only to store and download your game saves. It uses Google's limited "
                 + "\"drive.file\" permission, so it can see and manage only the files it creates in that "
                 + "folder — it cannot read, open, or touch any of your other Drive files, photos, or "
                 + "documents. Your sign-in is kept in memory only and is cleared when you close SaveHub.",
            Location = new Point(4, 168),
            Size = new Size(738, 74),
            AutoSize = false,
            ForeColor = SystemColors.GrayText,
        });
        _pnlGoogle.Controls.Add(_gdRoot);
        _pnlGoogle.Controls.Add(_gdClientId);
        _pnlGoogle.Controls.Add(_gdSecret);
        _pnlGoogle.Controls.Add(_gdOwner);
        _pnlGoogle.Controls.Add(_btnSignIn);
        _pnlGoogle.Controls.Add(_gdStatus);
        //
        // _gdRoot / _gdClientId / _gdSecret / _gdOwner / _btnSignIn / _gdStatus
        //
        _gdRoot.Location = new Point(120, 4); _gdRoot.Size = new Size(300, 23);
        _gdClientId.Location = new Point(120, 38); _gdClientId.Size = new Size(400, 23);
        _gdSecret.Location = new Point(120, 72); _gdSecret.Size = new Size(400, 23); _gdSecret.UseSystemPasswordChar = true;
        _gdOwner.Text = "I own this Drive folder (publish directly)";
        _gdOwner.Location = new Point(120, 100); _gdOwner.AutoSize = true;
        _btnSignIn.Text = "Sign in with Google";
        _btnSignIn.Location = new Point(120, 128); _btnSignIn.Size = new Size(160, 30);
        _btnSignIn.Click += Google_SignIn;
        _gdStatus.Text = "Not signed in.";
        _gdStatus.Location = new Point(290, 136); _gdStatus.AutoSize = true;
        //
        // _btnSave
        //
        _btnSave.Text = "Save";
        _btnSave.Location = new Point(120, 300);
        _btnSave.Size = new Size(90, 30);
        _btnSave.Click += Settings_Save;
        //
        // _btnTest
        //
        _btnTest.Text = "Test Connection";
        _btnTest.Location = new Point(220, 300);
        _btnTest.Size = new Size(140, 30);
        _btnTest.Click += Settings_Test;
        //
        // _lblConfig
        //
        _lblConfig.Text = "Config file:";
        _lblConfig.Location = new Point(12, 380);
        _lblConfig.AutoSize = true;
        //
        // SettingsTab
        //
        Controls.Add(_box);
        Controls.Add(_lblConfig);
        Size = new Size(784, 714);
        _box.ResumeLayout(false);
        _box.PerformLayout();
        _pnlGitHub.ResumeLayout(false);
        _pnlGitHub.PerformLayout();
        _pnlSupabase.ResumeLayout(false);
        _pnlSupabase.PerformLayout();
        _pnlGoogle.ResumeLayout(false);
        _pnlGoogle.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
