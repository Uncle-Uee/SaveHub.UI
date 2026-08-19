using SaveHub.Core;
using SaveHub.Core.Abstractions;
using SaveHub.GitHub;
using SaveHub.GoogleDrive;
using SaveHub.Hosting;
using SaveHub.Supabase;
using SaveHub.WinForms.Common;

namespace SaveHub.WinForms.Tabs;

/// <summary>Settings tab: choose a storage provider and edit/test its connection.</summary>
internal sealed partial class SettingsTab : UserControl, ITabView
{
    private MainFormController _controller = null!;
    private IShellContext _shell = null!;

    public SettingsTab()
    {
        InitializeComponent();
    }

    public void Initialize(MainFormController controller, IShellContext shell)
    {
        _controller = controller;
        _shell = shell;

        _stProvider.Items.Clear();
        foreach (ProviderDescriptor p in _controller.Providers)
        {
            _stProvider.Items.Add(p.DisplayName);
        }
        _lblConfig.Text = $"Config file: {_controller.ConfigPath}";
        LoadSettingsIntoFields();
    }

    public Task OnActivatedAsync()
    {
        return Task.CompletedTask;
    }

    private void Settings_ProviderChanged(object? sender, EventArgs e)
    {
        ShowProviderPanel();
    }

    private void ShowProviderPanel()
    {
        string code = SelectedProviderCode();
        _pnlGitHub.Visible = code == GitHubProviderFactory.ProviderName;
        _pnlSupabase.Visible = code == SupabaseProviderFactory.ProviderName;
        _pnlGoogle.Visible = code == GoogleDriveProviderFactory.ProviderName;
    }

    private string SelectedProviderCode()
    {
        return _controller.ProviderCodeAt(_stProvider.SelectedIndex);
    }

    private void LoadSettingsIntoFields()
    {
        SettingsSnapshot settings = _controller.LoadSettings();

        GitHubProviderSettings gh = settings.GitHub;
        _stOwner.Text = gh.Owner; _stRepo.Text = gh.Repository; _stBranch.Text = gh.Branch; _stAutoMerge.Checked = gh.AutoMerge;

        SupabaseProviderSettings sb = settings.Supabase;
        _sbUrl.Text = sb.Url; _sbBucket.Text = sb.Bucket; _sbOwner.Checked = sb.IsOwner;

        GoogleDriveProviderSettings gd = settings.Google;
        _gdRoot.Text = string.IsNullOrWhiteSpace(gd.RootFolderName) ? CommonSettings.DefaultGoogleFolder : gd.RootFolderName;
        _gdClientId.Text = gd.ClientId; _gdOwner.Checked = gd.IsOwner;

        _stProvider.SelectedIndex = settings.ActiveProviderIndex;
        ShowProviderPanel();
    }

    private void Settings_Save(object? sender, EventArgs e)
    {
        switch (SelectedProviderCode())
        {
            case SupabaseProviderFactory.ProviderName:
                _controller.SaveSupabaseSettings(_sbUrl.Text.Trim(), _sbBucket.Text.Trim(), _sbOwner.Checked,
                    _sbKey.Text.Length > 0 ? _sbKey.Text : null);
                break;
            case GoogleDriveProviderFactory.ProviderName:
                _controller.SaveGoogleSettings(_gdRoot.Text.Trim(), _gdClientId.Text.Trim(), _gdOwner.Checked,
                    _gdSecret.Text.Length > 0 ? _gdSecret.Text : null);
                break;
            default:
                _controller.SaveGitHubSettings(_stOwner.Text.Trim(), _stRepo.Text.Trim(), _stBranch.Text.Trim(), _stAutoMerge.Checked,
                    _stToken.Text.Length > 0 ? _stToken.Text : null);
                break;
        }
        _shell.SetStatus($"Settings saved. Active provider: {SelectedProviderCode()}.");
    }

    private async void Google_SignIn(object? sender, EventArgs e)
    {
        Settings_Save(sender, e);
        await _shell.RunBusy("Waiting for Google sign-in in your browser...", async () =>
        {
            GoogleDriveSession session = await _controller.SignInGoogleAsync();
            SetGoogleStatus(session);
        });
    }

    private async void Settings_Test(object? sender, EventArgs e)
    {
        Settings_Save(sender, e);
        if (SelectedProviderCode() == GoogleDriveProviderFactory.ProviderName && !_controller.GoogleHasActiveSession)
        {
            await _shell.RunBusy("Waiting for Google sign-in in your browser...", async () =>
            {
                GoogleDriveSession session = await _controller.SignInGoogleAsync();
                SetGoogleStatus(session);
            });
        }
        SaveHubClient? client = _shell.RequireClient();
        if (client is null)
        {
            return;
        }
        await _shell.RunBusy("Testing connection...", async () =>
        {
            ConnectionTestResult result = await _controller.TestConnectionAsync(client);
            MessageBox.Show(this,
                $"{result.Message}\n\nUser: {result.AuthenticatedAs}\nTarget: {result.Target}\nWrite access: {result.HasWriteAccess}\nAuto-merge effective: {result.AutoMergeEffective}",
                result.Success ? "Connected" : "Failed",
                MessageBoxButtons.OK,
                result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        });
    }

    private void SetGoogleStatus(GoogleDriveSession session)
    {
        string who = session.AccountEmail is null ? "" : $" as {session.AccountEmail}";
        TimeSpan remaining = session.ExpiresAt - DateTimeOffset.Now;
        string validFor = remaining.TotalHours >= 1
            ? $"{(int)remaining.TotalHours}h {remaining.Minutes}m"
            : $"{Math.Max(0, remaining.Minutes)}m";
        _gdStatus.Text = $"Signed in{who} — token expires in {validFor} (at {session.ExpiresAt.ToLocalTime():t}); it is kept in memory only and cleared when you close SaveHub.";
    }
}
