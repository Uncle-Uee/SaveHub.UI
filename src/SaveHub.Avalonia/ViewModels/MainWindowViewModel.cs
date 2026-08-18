using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SaveHub.Avalonia.Services;
using SaveHub.Core;
using SaveHub.Core.Abstractions;

namespace SaveHub.Avalonia.ViewModels;

/// <summary>
/// The SaveHub desktop shell. It hosts the feature tabs (each its own view model),
/// owns the status bar, and provides shared services to the tabs via
/// <see cref="IShellContext"/>.
/// </summary>
public sealed partial class MainWindowViewModel : ViewModelBase, IShellContext
{
    private readonly AppController _controller = new();
    private readonly ViewModelBase[] _tabs;
    private IDialogService? _dialogs;

    [ObservableProperty]
    private string _statusText = "Ready.";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private int _selectedTabIndex;

    public UploadViewModel Upload { get; }

    public DownloadViewModel Download { get; }

    public EditViewModel Edit { get; }

    public ManageViewModel Manage { get; }

    public SettingsViewModel Settings { get; }

    private IDialogService Dialogs => _dialogs ?? throw new InvalidOperationException("Dialog service not attached.");

    public MainWindowViewModel()
    {
        Upload = new UploadViewModel(_controller, this);
        Download = new DownloadViewModel(_controller, this);
        Edit = new EditViewModel(_controller, this);
        Manage = new ManageViewModel(_controller, this);
        Settings = new SettingsViewModel(_controller, this);
        _tabs = [Upload, Download, Edit, Manage, Settings];
    }

    internal void AttachDialogs(IDialogService dialogs)
    {
        _dialogs = dialogs;
    }

    // ---------------------------------------------------------------- IShellContext

    public SaveHubClient? TryCreateClient()
    {
        return _controller.TryCreateClient(out _);
    }

    public async Task<SaveHubClient?> RequireClientAsync()
    {
        SaveHubClient? client = _controller.TryCreateClient(out string error);
        if (client is null)
        {
            await Dialogs.ShowMessageAsync("Not configured", error);
        }
        return client;
    }

    public async Task RunBusy(string status, Func<Task> action)
    {
        IsBusy = true;
        StatusText = status;
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            await Dialogs.ShowMessageAsync("Error", ex.Message);
            StatusText = "Error.";
            IsBusy = false;
            return;
        }
        IsBusy = false;
        // Keep any status the action set; otherwise return to idle.
        if (StatusText == status)
        {
            StatusText = "Ready.";
        }
    }

    public void SetStatus(string text)
    {
        StatusText = text;
    }

    public Task WarnAsync(string message)
    {
        return Dialogs.ShowMessageAsync("SaveHub", message);
    }

    public async Task ShowResultAsync(SaveUploadResult result)
    {
        StatusText = result.Message;
        string message = result.Message;
        if (!string.IsNullOrWhiteSpace(result.PullRequestUrl))
        {
            message += $"\n\n{result.PullRequestUrl}";
        }
        await Dialogs.ShowMessageAsync(result.Merged ? "Merged" : "Submitted", message);
    }

    public Task<bool> ConfirmAsync(string title, string message)
    {
        return Dialogs.ConfirmAsync(title, message);
    }

    public Task<IReadOnlyList<string>> PickFilesAsync(string title, bool allowMultiple, string? filterName, IReadOnlyList<string>? patterns)
    {
        return Dialogs.OpenFilesAsync(title, allowMultiple, filterName, patterns);
    }

    public Task<string?> PickFolderAsync(string title)
    {
        return Dialogs.OpenFolderAsync(title);
    }

    public Task<string?> PickSaveFileAsync(string suggestedName)
    {
        return Dialogs.SaveFileAsync("Save archive", suggestedName, "Zip archive", ["*.zip"]);
    }

    // ---------------------------------------------------------------- Commands

    [RelayCommand]
    private void Donate()
    {
        OpenUrl(SaveHubInfo.DonateUrl);
    }

    [RelayCommand]
    private void OpenSource()
    {
        OpenUrl(SaveHubInfo.ProjectUrl);
    }

    [RelayCommand]
    private void OpenReadme()
    {
        OpenUrl($"{SaveHubInfo.ProjectUrl}#readme");
    }

    [RelayCommand]
    private async Task About()
    {
        await Dialogs.ShowMessageAsync("About SaveHub",
            $"{SaveHubInfo.Product} {SaveHubInfo.Version}\n\n{SaveHubInfo.Attribution}");
    }

    [RelayCommand]
    private void ShowTab(string? index)
    {
        if (int.TryParse(index, out int value))
        {
            SelectedTabIndex = value;
        }
    }

    [RelayCommand]
    private void Quit()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }

    private void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            _ = Dialogs.ShowMessageAsync("SaveHub", ex.Message);
        }
    }

    // ---------------------------------------------------------------- Tab activation

    partial void OnSelectedTabIndexChanged(int value)
    {
        if (value >= 0 && value < _tabs.Length)
        {
            _ = _tabs[value].OnActivatedAsync();
        }
    }
}
