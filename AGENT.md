# AGENT.md — SaveHub.UI

Frontends for **SaveHub**. This repo consumes the SaveHub API as NuGet packages
(`SaveHub.Core`, `.Hosting`, `.GitHub`, `.Supabase`, `.GoogleDrive`); it contains
no API source.

## Projects
- `src/SaveHub.WinForms` — Windows desktop app (net10.0-windows).
- (planned) `src/SaveHub.Avalonia` — cross-platform UI (Windows/Linux/macOS/Android).

## Referencing the API
- Package versions are centralized in `Directory.Packages.props`.
- Default: `PackageReference` to the published packages.
- Co-dev against local API source (sibling `..\SaveHub` clone):
  `dotnet build -p:UseLocalSaveHub=true` (switches to `ProjectReference`).
- Before anything is published to NuGet: build with `-p:UseLocalSaveHub=true`, or
  run `..\SaveHub\pack-api.ps1` then restore.

## Conventions
- Code style: see `.github/copilot-instructions.md`.
- License: MIT (© 2026 Ubaidullah Effendi). Keep the copyright notice.
- The donate link comes from `SaveHubInfo.DonateUrl` in the Core package — do not
  hardcode it.
