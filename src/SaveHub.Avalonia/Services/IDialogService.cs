namespace SaveHub.Avalonia.Services;

/// <summary>Platform dialog operations (message boxes and file pickers) bound to a top-level window.</summary>
internal interface IDialogService
{
    Task ShowMessageAsync(string title, string message);

    Task<bool> ConfirmAsync(string title, string message);

    Task<IReadOnlyList<string>> OpenFilesAsync(string title, bool allowMultiple, string? filterName, IReadOnlyList<string>? patterns);

    Task<string?> OpenFolderAsync(string title);

    Task<string?> SaveFileAsync(string title, string suggestedName, string? filterName, IReadOnlyList<string>? patterns);
}
