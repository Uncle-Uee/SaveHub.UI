namespace SaveHub.Avalonia.Models;

/// <summary>A selected upload file shown in the Upload tab's file grid.</summary>
public sealed record FileRow(string Name, string Size, string Path);
