using SaveHub.Core.Models;

namespace SaveHub.Avalonia.Models;

/// <summary>A published save shown in the Edit/Manage grids, wrapping the underlying <see cref="SaveEntry"/>.</summary>
public sealed record SaveRow(string Archive, string Type, string Description, SaveEntry Entry);
