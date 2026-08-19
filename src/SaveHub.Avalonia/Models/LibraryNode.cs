using System.Collections.ObjectModel;

namespace SaveHub.Avalonia.Models;

/// <summary>A node in the Library tree: a manufacturer, a platform, or a game.</summary>
public sealed class LibraryNode
{
    public string Title { get; }

    public ObservableCollection<LibraryNode> Children { get; } = [];

    public LibraryNode(string title)
    {
        Title = title;
    }
}
