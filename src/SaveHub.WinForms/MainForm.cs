using SaveHub.Core;
using SaveHub.Core.Abstractions;
using SaveHub.WinForms.Tabs;
using System.Diagnostics;

namespace SaveHub.WinForms;

/// <summary>
/// The SaveHub desktop shell. It hosts the feature tabs (each its own UserControl),
/// owns the status bar, and provides shared services to the tabs via
/// <see cref="IShellContext"/>. All feature logic lives in the tabs and
/// <see cref="MainFormController"/>.
/// </summary>
public sealed partial class MainForm : Form, IShellContext
{
    private readonly MainFormController _controller = new();
    private readonly ITabView[] _views;

    public MainForm()
    {
        InitializeComponent();

        Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);

        _views = [_uploadTab, _downloadTab, _editTab, _manageTab, _settingsTab];
        foreach (ITabView view in _views)
        {
            view.Initialize(_controller, this);
        }
    }

    private async void Tabs_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_tabs.SelectedIndex >= 0 && _tabs.SelectedIndex < _views.Length)
        {
            await _views[_tabs.SelectedIndex].OnActivatedAsync();
        }
    }

    private void Donate_Click(object? sender, EventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(SaveHubInfo.DonateUrl) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Donate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    // ---------------------------------------------------------------- IShellContext

    public SaveHubClient? RequireClient()
    {
        SaveHubClient? client = _controller.TryCreateClient(out string error);
        if (client is null)
        {
            MessageBox.Show(this, error, "Not configured", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        return client;
    }

    public async Task RunBusy(string status, Func<Task> action)
    {
        SetBusy(true);
        _status.Text = status;
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _status.Text = "Error.";
            SetBusy(false);
            return;
        }
        SetBusy(false);
        // Keep any status the action set; otherwise return to idle.
        if (_status.Text == status)
        {
            _status.Text = "Ready.";
        }
    }

    public void SetStatus(string text)
    {
        _status.Text = text;
    }

    public void Warn(string message)
    {
        MessageBox.Show(this, message, "SaveHub", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    public void ShowResult(SaveUploadResult result)
    {
        _status.Text = result.Message;
        string message = result.Message;
        if (!string.IsNullOrWhiteSpace(result.PullRequestUrl))
        {
            message += $"\n\n{result.PullRequestUrl}";
        }
        MessageBox.Show(this, message, result.Merged ? "Merged" : "Submitted", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void SetBusy(bool busy)
    {
        // Marquee bar shows only while an operation is running.
        pbar.Style = ProgressBarStyle.Marquee;
        pbar.Visible = busy;
        UseWaitCursor = busy;
    }
}
