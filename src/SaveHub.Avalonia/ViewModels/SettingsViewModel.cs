using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SaveHub.Avalonia.Common;
using SaveHub.Avalonia.Services;
using SaveHub.Core;
using SaveHub.Core.Abstractions;
using SaveHub.GitHub;
using SaveHub.GoogleDrive;
using SaveHub.Hosting;
using SaveHub.Supabase;

namespace SaveHub.Avalonia.ViewModels;

/// <summary>Settings tab: choose a storage provider and edit/test its connection.</summary>
public sealed partial class SettingsViewModel : ViewModelBase
{
    private readonly AppController _controller;
    private readonly IShellContext _shell;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowGitHub))]
    [NotifyPropertyChangedFor(nameof(ShowSupabase))]
    [NotifyPropertyChangedFor(nameof(ShowGoogle))]
    [NotifyPropertyChangedFor(nameof(ProviderDescription))]
    private int _selectedProviderIndex;

    [ObservableProperty]
    private string _configPathText = string.Empty;

    [ObservableProperty]
    private string _owner = string.Empty;

    [ObservableProperty]
    private string _repository = string.Empty;

    [ObservableProperty]
    private string _branch = string.Empty;

    [ObservableProperty]
    private bool _autoMerge;

    [ObservableProperty]
    private string _token = string.Empty;

    [ObservableProperty]
    private string _supabaseUrl = string.Empty;

    [ObservableProperty]
    private string _supabaseBucket = string.Empty;

    [ObservableProperty]
    private bool _supabaseIsOwner;

    [ObservableProperty]
    private string _supabaseKey = string.Empty;

    [ObservableProperty]
    private string _googleRoot = string.Empty;

    [ObservableProperty]
    private string _googleClientId = string.Empty;

    [ObservableProperty]
    private bool _googleIsOwner;

    [ObservableProperty]
    private string _googleSecret = string.Empty;

    [ObservableProperty]
    private string _googleStatus = string.Empty;

    public ObservableCollection<string> Providers { get; } = [];

    public bool ShowGitHub => SelectedProviderCode() == GitHubProviderFactory.ProviderName;

    public bool ShowSupabase => SelectedProviderCode() == SupabaseProviderFactory.ProviderName;

    public bool ShowGoogle => SelectedProviderCode() == GoogleDriveProviderFactory.ProviderName;

    public string ProviderDescription => SelectedProviderCode() switch
    {
        SupabaseProviderFactory.ProviderName =>
            "Supabase Storage: saves are stored in a bucket of your Supabase project. " +
            "Enter the project URL (https://<ref>.supabase.co), the bucket name, and an API key " +
            "(service_role to upload, or anon for read-only). Tick ‘I own this bucket’ if you administer it.",
        GoogleDriveProviderFactory.ProviderName =>
            "Google Drive: saves are stored in your own Drive under an app-created folder using the " +
            "least-privilege drive.file scope. Create an OAuth client in a Google Cloud project, paste the " +
            "Client ID and secret, then click ‘Sign in with Google’. The token is kept in memory only.",
        _ =>
            "GitHub: saves are contributed to a GitHub repository. Enter the repository owner and name, an " +
            "optional branch (blank = the repo default), and a personal access token with the ‘repo’ scope " +
            "(or set SAVEHUB_GITHUB_TOKEN). Enable Auto-merge only if you have write access; otherwise a pull " +
            "request is opened for review.",
    };

    internal SettingsViewModel(AppController controller, IShellContext shell)
    {
        _controller = controller;
        _shell = shell;

        foreach (ProviderDescriptor p in _controller.Providers)
        {
            Providers.Add(p.DisplayName);
        }
        ConfigPathText = $"Config file: {_controller.ConfigPath}";
        LoadSettingsIntoFields();
    }

    [RelayCommand]
    private void Save()
    {
        SaveCore();
        _shell.SetStatus($"Settings saved. Active provider: {SelectedProviderCode()}.");
    }

    [RelayCommand]
    private async Task Test()
    {
        SaveCore();
        await _shell.RunBusy("Testing connection...", async () =>
        {
            if (ShowGoogle && !_controller.GoogleHasActiveSession)
            {
                _shell.SetStatus("Waiting for Google sign-in in your browser...");
                GoogleDriveSession signIn = await _controller.SignInGoogleAsync();
                UpdateGoogleStatus(signIn);
            }

            SaveHubClient? client = _controller.TryCreateClient(out string error);
            if (client is null)
            {
                await _shell.WarnAsync(error);
                return;
            }

            ConnectionTestResult result = await _controller.TestConnectionAsync(client);
            _shell.SetStatus(result.Message);
            await _shell.WarnAsync(
                $"{result.Message}\n\nUser: {result.AuthenticatedAs}\nTarget: {result.Target}\nWrite access: {result.HasWriteAccess}\nAuto-merge effective: {result.AutoMergeEffective}");
        });
    }

    [RelayCommand]
    private async Task GoogleSignIn()
    {
        SaveCore();
        await _shell.RunBusy("Waiting for Google sign-in in your browser...", async () =>
        {
            GoogleDriveSession session = await _controller.SignInGoogleAsync();
            UpdateGoogleStatus(session);
        });
    }

    private void UpdateGoogleStatus(GoogleDriveSession session)
    {
        string who = session.AccountEmail is null ? string.Empty : $" as {session.AccountEmail}";
        TimeSpan remaining = session.ExpiresAt - DateTimeOffset.Now;
        string validFor = remaining.TotalHours >= 1
            ? $"{(int)remaining.TotalHours}h {remaining.Minutes}m"
            : $"{Math.Max(0, remaining.Minutes)}m";
        GoogleStatus = $"Signed in{who} — token expires in {validFor} (at {session.ExpiresAt.ToLocalTime():t}); it is kept in memory only and cleared when you close SaveHub.";
    }

    private string SelectedProviderCode()
    {
        return _controller.ProviderCodeAt(SelectedProviderIndex);
    }

    private void LoadSettingsIntoFields()
    {
        SettingsSnapshot settings = _controller.LoadSettings();

        GitHubProviderSettings gh = settings.GitHub;
        Owner = gh.Owner;
        Repository = gh.Repository;
        Branch = gh.Branch;
        AutoMerge = gh.AutoMerge;

        SupabaseProviderSettings sb = settings.Supabase;
        SupabaseUrl = sb.Url;
        SupabaseBucket = sb.Bucket;
        SupabaseIsOwner = sb.IsOwner;

        GoogleDriveProviderSettings gd = settings.Google;
        GoogleRoot = string.IsNullOrWhiteSpace(gd.RootFolderName) ? CommonSettings.DefaultGoogleFolder : gd.RootFolderName;
        GoogleClientId = gd.ClientId;
        GoogleIsOwner = gd.IsOwner;

        SelectedProviderIndex = settings.ActiveProviderIndex;
    }

    private void SaveCore()
    {
        switch (SelectedProviderCode())
        {
            case SupabaseProviderFactory.ProviderName:
                _controller.SaveSupabaseSettings(SupabaseUrl.Trim(), SupabaseBucket.Trim(), SupabaseIsOwner,
                    SupabaseKey.Length > 0 ? SupabaseKey : null);
                break;
            case GoogleDriveProviderFactory.ProviderName:
                _controller.SaveGoogleSettings(GoogleRoot.Trim(), GoogleClientId.Trim(), GoogleIsOwner,
                    GoogleSecret.Length > 0 ? GoogleSecret : null);
                break;
            default:
                _controller.SaveGitHubSettings(Owner.Trim(), Repository.Trim(), Branch.Trim(), AutoMerge,
                    Token.Length > 0 ? Token : null);
                break;
        }
    }
}
