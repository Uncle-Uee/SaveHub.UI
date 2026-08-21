namespace SaveHub.Avalonia.Models;

/// <summary>A staged upload item shown in the Upload tab's list.</summary>
public sealed record FileRow(string Name, string Type, string Details);
