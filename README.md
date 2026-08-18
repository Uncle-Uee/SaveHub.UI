# SaveHub.UI

[![Support SaveHub](https://img.shields.io/badge/%E2%9D%A4-Support%20SaveHub-ff5f5f)](https://pay.yoco.com/savehub)

Desktop (WinForms) and — soon — cross-platform (Avalonia) frontends for
[SaveHub](https://github.com/uncle-uee/SaveHub), built on the SaveHub API packages.

> Open source under the **MIT License** — see [LICENSE](LICENSE). If SaveHub is
> useful to you, please consider supporting it: **https://pay.yoco.com/savehub**.

## Build

Requires the .NET 10 SDK.

```powershell
# Against your local SaveHub API source (sibling ..\SaveHub clone):
dotnet build -p:UseLocalSaveHub=true

# Against published packages (after the API is released to NuGet.org):
dotnet build
```
