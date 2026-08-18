namespace SaveHub.Avalonia.Models;

internal static class Devices
{
    /// <summary>Consoles grouped by developer for the upload UI.</summary>
    public static readonly IReadOnlyList<DeviceGroup> Groups =
    [
        new("Nintendo",
        [
            new("NES", "NES"),
            new("SNES", "SNES"),
            new("N64", "N64"),
            new("GameCube", "GC"),
            new("Wii", "WII"),
            new("Switch", "SWITCH"),
            new("Game Boy", "GB"),
            new("Game Boy Color", "GBC"),
            new("GBA", "GBA"),
            new("NDS", "DS"),
            new("3DS", "3DS"),
            new("Virtual Boy", "VB"),
        ]),
        new("Sony",
        [
            new("PS1", "PS1"),
            new("PS2", "PS2"),
            new("PS3", "PS3"),
            new("PS4", "PS4"),
            new("PS5", "PS5"),
            new("PSP", "PSP"),
            new("PS Vita", "PSV"),
        ]),
        new("Microsoft",
        [
            new("Xbox", "XBOX"),
            new("Xbox 360", "X360"),
        ]),
        new("Sega",
        [
            new("Genesis", "GENESIS"),
            new("Dreamcast", "DREAMCAST"),
        ]),
        new("Desktop Gaming",
        [
            new("PC", "PC"),
        ]),
    ];

    /// <summary>All consoles, flattened.</summary>
    public static IEnumerable<DeviceOption> All => Groups.SelectMany(g => g.Consoles);
}

/// <summary>A selectable console/handheld with its display name and SaveHub platform folder code.</summary>
public readonly record struct DeviceOption(string Display, string Code);

/// <summary>A manufacturer and the consoles grouped under it.</summary>
internal readonly record struct DeviceGroup(string Manufacturer, IReadOnlyList<DeviceOption> Consoles);
