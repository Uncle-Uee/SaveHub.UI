using SaveHub.Core;
using SaveHub.Core.Abstractions;

namespace SaveHub.Avalonia.Services;

/// <summary>
/// Shell services a tab view model needs from the host window: status text, the busy
/// indicator, client creation, shared dialogs, and file pickers. This keeps each tab
/// decoupled from the concrete window so they can be maintained individually.
/// </summary>
internal interface IShellContext
{
    /// <summary>Creates a client for the active provider, or shows a warning and returns null.</summary>
    Task<SaveHubClient?> RequireClientAsync();

    /// <summary>Creates a client without prompting; returns null on failure.</summary>
    SaveHubClient? TryCreateClient();

    /// <summary>Runs <paramref name="action"/> while showing the busy indicator and status text.</summary>
    Task RunBusy(string status, Func<Task> action);

    /// <summary>Sets the status-bar text.</summary>
    void SetStatus(string text);

    /// <summary>Shows a warning message dialog owned by the shell.</summary>
    Task WarnAsync(string message);

    /// <summary>Shows the standard upload-result dialog and status.</summary>
    Task ShowResultAsync(SaveUploadResult result);

    /// <summary>Shows a yes/no confirmation dialog.</summary>
    Task<bool> ConfirmAsync(string title, string message);

    /// <summary>Prompts for one or more files, returning their local paths (empty if cancelled).</summary>
    Task<IReadOnlyList<string>> PickFilesAsync(string title, bool allowMultiple, string? filterName, IReadOnlyList<string>? patterns);

    /// <summary>Prompts for a folder, returning its local path (null if cancelled).</summary>
    Task<string?> PickFolderAsync(string title);

    /// <summary>Prompts for a save-file destination, returning its local path (null if cancelled).</summary>
    Task<string?> PickSaveFileAsync(string suggestedName);
}
