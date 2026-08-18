namespace SaveHub.Avalonia.Models;

/// <summary>A published save shown in the Download tab's grid, with the identifiers needed to fetch it.</summary>
public sealed record DownloadRow(
    string Name,
    string TitleId,
    string Archive,
    string Type,
    string Description,
    string System,
    string Game);
