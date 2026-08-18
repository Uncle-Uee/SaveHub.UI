using SaveHub.Core;
using SaveHub.Core.Abstractions;

namespace SaveHub.WinForms;

/// <summary>
/// Shell services a tab needs from the host <see cref="MainForm"/>: status text,
/// the busy indicator, client creation, and shared result/warning dialogs. This keeps
/// each tab decoupled from the concrete form so they can be maintained individually.
/// </summary>
internal interface IShellContext
{
    /// <summary>Creates a client for the active provider, or shows a warning and returns null.</summary>
    SaveHubClient? RequireClient();

    /// <summary>Runs <paramref name="action"/> while showing the busy indicator and status text.</summary>
    Task RunBusy(string status, Func<Task> action);

    /// <summary>Sets the status-bar text.</summary>
    void SetStatus(string text);

    /// <summary>Shows a warning message box owned by the shell.</summary>
    void Warn(string message);

    /// <summary>Shows the standard upload-result dialog and status.</summary>
    void ShowResult(SaveUploadResult result);
}
