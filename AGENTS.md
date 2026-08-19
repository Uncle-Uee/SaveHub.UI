# AGENTS.md — SaveHub.UI

Frontends for **SaveHub**. This repo consumes the SaveHub API as NuGet packages
(`SaveHub.Core`, `.Hosting`, `.GitHub`, `.Supabase`, `.GoogleDrive`); it contains
no API source.

## Projects
- `src/SaveHub.WinForms` — Windows desktop app (net10.0-windows).
- `src/SaveHub.Avalonia` — cross-platform desktop UI (Windows/Linux/macOS), MVVM
  (CommunityToolkit.Mvvm). Full parity with the WinForms tabs
  (Upload/Download/Edit/Manage/Library/Settings). Android/Browser heads planned later.

## Tabs (both UIs, kept in parity)
- **Upload / Download / Edit** — submit, fetch, and replace saves. Upload has a
  **Bulk cards** mode: pick a folder of memory cards and choose per card, via a
  dropdown, whether to **Upload + Index** (make `NN.zip` and add an index row) or
  **Index only**; all cards are catalogued in `PLATFORM/!index/README.md`
  (`UpdateMemoryCardIndexAsync`). A **PC** save-folder upload requires a game name.
- **Manage** — bulk delete/download, per-save details, game+save filters, delete-all,
  and **Rename game** (`SetGameNameAsync`).
- **Library** — manufacturer → platform → game tree from the root `library.json`
  index, cached locally at `%APPDATA%/SaveHub/library-cache.json`; Refresh reads the
  backend index, Rebuild regenerates it.
- **Settings** — provider config + test. Providers: GitHub, GitLab, Bitbucket,
  Supabase, Google Drive (each with its own panel). The repository field is left
  blank for new users; the default name shows only as a placeholder hint.
- Edit/Manage game pickers show `Name (id)` via `GetGameNamesAsync`.
- Both UIs have a **menu bar**: File (Quit), Tools (switch tabs + **Rebuild Library
  Index** → updates the backend index and local cache), Help (README/source/donate/about).
  Avalonia = `Menu`; WinForms = `MenuStrip`.

## Referencing the API
- Package versions are centralized in `Directory.Packages.props`.
- Default: `PackageReference` to the published packages.
- Co-dev against local API source (sibling `..\SaveHub` clone):
  `dotnet build -p:UseLocalSaveHub=true` (switches to `ProjectReference`).
- Before anything is published to NuGet: build with `-p:UseLocalSaveHub=true`, or
  run `..\SaveHub\pack-api.ps1` then restore.

## Conventions
- Code style: see `.github/copilot-instructions.md`.
- Reusable constants live in a `Common/` folder per app as `static readonly` fields:
  `CommonSettings` (default repo/folder names, cache file name) and, for Avalonia,
  `CommonStrings` (shared UI text). Add new shared values there, not scattered in classes.
- License: GPL-3.0-or-later (the apps). Keep the copyright notice in LICENSE.
  The SaveHub API libraries this consumes are LGPL-3.0-or-later.
- The donate link comes from `SaveHubInfo.DonateUrl` in the Core package — do not
  hardcode it.
