# Credits & third-party licenses

The SaveHub desktop apps are built with the help of these open-source projects. Each is
used under its own license (linked below); their copyright notices are retained here as
required.

## UI libraries

| Library | Used by | License | Copyright |
| --- | --- | --- | --- |
| [Avalonia](https://github.com/AvaloniaUI/Avalonia) (`Avalonia`, `Avalonia.Desktop`, `Avalonia.Themes.Fluent`, `Avalonia.Controls.DataGrid`, `Avalonia.Fonts.Inter`) | Avalonia app | MIT | © .NET Foundation and Contributors; AvaloniaUI OÜ |
| [Inter typeface](https://github.com/rsms/inter) (bundled via `Avalonia.Fonts.Inter`) | Avalonia app | SIL Open Font License 1.1 | © The Inter Project Authors |
| [.NET Community Toolkit — MVVM](https://github.com/CommunityToolkit/dotnet) (`CommunityToolkit.Mvvm`) | Avalonia app | MIT | © .NET Foundation and Contributors |
| [Windows Forms](https://github.com/dotnet/winforms) & [.NET runtime](https://github.com/dotnet/runtime) | both apps | MIT | © .NET Foundation and Contributors |
| [Avalonia DevTools](https://github.com/AvaloniaUI/Avalonia) (`AvaloniaUI.DiagnosticsSupport`) | Avalonia app (**Debug only** — not shipped in releases) | MIT | © .NET Foundation and Contributors |

The apps also consume the SaveHub API packages (`SaveHub.*`), which are
LGPL-3.0-or-later — © Ubaidullah Effendi; see the
[SaveHub API repository](https://github.com/uncle-uee/SaveHub).

Full license texts are reproduced in [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt),
and are also available at each project's repository linked above. SaveHub's own license
is in [LICENSE](LICENSE) (GPL-3.0-or-later for the apps).
