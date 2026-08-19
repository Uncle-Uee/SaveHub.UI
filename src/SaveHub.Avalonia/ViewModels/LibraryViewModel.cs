using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SaveHub.Avalonia.Models;
using SaveHub.Avalonia.Services;
using SaveHub.Core;
using SaveHub.Core.Models;

namespace SaveHub.Avalonia.ViewModels;

/// <summary>Library tab: a manufacturer → platform → game tree, backed by the root library index.</summary>
public sealed partial class LibraryViewModel : ViewModelBase
{
    private readonly AppController _controller;
    private readonly IShellContext _shell;

    [ObservableProperty]
    private string _summary = "No library loaded yet.";

    public ObservableCollection<LibraryNode> Nodes { get; } = [];

    internal LibraryViewModel(AppController controller, IShellContext shell)
    {
        _controller = controller;
        _shell = shell;
    }

    public override async Task OnActivatedAsync()
    {
        if (Nodes.Count == 0)
        {
            Populate(_controller.LoadLocalLibrary());
            await RefreshFromBackend(false);
        }
    }

    [RelayCommand]
    private async Task Refresh()
    {
        await RefreshFromBackend(false);
    }

    [RelayCommand]
    private async Task Rebuild()
    {
        await RefreshFromBackend(true);
    }

    /// <summary>Rebuilds the backend index and local cache (used by the menu bar).</summary>
    public Task RebuildAsync()
    {
        return RefreshFromBackend(true);
    }

    private async Task RefreshFromBackend(bool rebuild)
    {
        SaveHubClient? client = rebuild ? await _shell.RequireClientAsync() : _shell.TryCreateClient();
        if (client is null)
        {
            if (!rebuild)
            {
                _shell.SetStatus("Showing cached library — open Settings to connect for a fresh copy.");
            }
            return;
        }

        await _shell.RunBusy(rebuild ? "Rebuilding library index..." : "Loading library...", async () =>
        {
            LibraryIndex index = rebuild
                ? await _controller.RebuildLibraryIndexAsync(client)
                : await _controller.GetLibraryIndexAsync(client);
            _controller.SaveLocalLibrary(index);
            Populate(index);
        });
    }

    private void Populate(LibraryIndex index)
    {
        Nodes.Clear();

        Dictionary<string, LibraryNode> byManufacturer = new(StringComparer.OrdinalIgnoreCase);
        foreach ((string platform, Dictionary<string, string> games) in index.Platforms)
        {
            string manufacturer = Devices.ManufacturerFor(platform);
            if (!byManufacturer.TryGetValue(manufacturer, out LibraryNode? manufacturerNode))
            {
                manufacturerNode = new LibraryNode(manufacturer);
                byManufacturer[manufacturer] = manufacturerNode;
            }

            LibraryNode platformNode = new LibraryNode($"{platform} ({games.Count})");
            foreach ((string id, string name) in games.OrderBy(g => g.Value, StringComparer.OrdinalIgnoreCase))
            {
                platformNode.Children.Add(new LibraryNode(
                    string.Equals(name, id, StringComparison.OrdinalIgnoreCase) ? id : $"{name} ({id})"));
            }
            manufacturerNode.Children.Add(platformNode);
        }

        foreach (string manufacturer in ManufacturerOrder())
        {
            if (byManufacturer.TryGetValue(manufacturer, out LibraryNode? node))
            {
                Nodes.Add(node);
                byManufacturer.Remove(manufacturer);
            }
        }
        foreach (LibraryNode node in byManufacturer.Values)
        {
            Nodes.Add(node);
        }

        int totalGames = index.Platforms.Values.Sum(g => g.Count);
        Summary = $"{index.Platforms.Count} platform(s), {totalGames} game(s).";
    }

    private static IEnumerable<string> ManufacturerOrder()
    {
        foreach (DeviceGroup group in Devices.Groups)
        {
            yield return group.Manufacturer;
        }
        yield return "Other";
    }
}
