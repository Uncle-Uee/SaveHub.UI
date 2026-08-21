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
- **Upload** — stage **up to 10 memory cards OR one folder** (save folder / save state) and upload
  them in one go. Each staged item keeps its **own metadata** (device, save type, title id, game
  name, description, cover icon); selecting a row in the list reloads that item's data. Metadata is
  mirrored to a session scratch file (`%APPDATA%/SaveHub/pending-upload.json`, cleared on launch and
  after upload) so switching between cards never loses data. Upload has an Add/Edit/Remove file
  toolbar and previews the selected cover icon. Single uploads go to their game folders only and
  **never** touch the `!index` catalog (that is bulk-only). A **PC** save-folder upload requires a
  game name. PS1/PS2 memory cards auto-select the console (broadened detection covers raw plus
  wrapped `.gme`/`.vgs`/`.vmp` images); PS3+ folder saves auto-detect the console from `PARAM.SFO`
  (or the title-id in the folder name), falling back to a manual device pick. Both UIs
  preview the cover for the selected item (user icon → cached cover → a generated “No Cover”
  placeholder); user-supplied covers are cached on upload like auto-fetched ones.
- **Download / Edit** — fetch and replace a single save; show the game name and a cover-icon preview.
- **Bulk Upload** — add a parent folder (each sub-folder = a game) or files into a collapsible
  **tree** with a right-side toolbar (Add folder/file, Edit name, Remove, **Set icon…**,
  Expand/Collapse toggle, **per-node platform**). Each top-level folder uploads as a save folder
  (`PLATFORM/<GameName>/NN-folder.zip`); each file as a memory card that also updates the platform's
  `!index` catalog (`UpdateMemoryCardIndexAsync`). A folder that itself holds PS1/PS2 memory cards
  is kept as a parent node with each card listed under it and uploaded/indexed individually (game
  name and title id read from each card). Checkboxes exclude items (non-destructive); Edit
  renames only the repo game folder; Set icon gives a per-folder cover. Selecting a top-level node
  shows a **Details** panel (cover preview + **Detect** Title ID, Game Name, Description) whose
  values are used on upload. WinForms `BulkUploadTab` (`TreeView` + `ToolStrip` + Details group box);
  Avalonia `BulkUploadViewModel`/`View` (`TreeView` + button bar + Details panel, `BulkNode`).
- **Manage** — bulk delete/download, per-save details, game+save filters, delete-all,
  and **Rename game** (`SetGameNameAsync`).
- **Library** — manufacturer → platform → game tree from the root `library.json`
  index, cached locally at `%APPDATA%/SaveHub/library-cache.json`; Refresh reads the
  backend index, Rebuild regenerates it.
- **Settings** — provider config + test. Providers: GitHub, GitLab, Bitbucket,
  Supabase, Google Drive (each with its own panel). The repository field is left
  blank for new users; the default name shows only as a placeholder hint.
- Edit/Manage game pickers show `Name (id)` via `GetGameNamesAsync`.
- Both UIs have a **menu bar**: File (**Open Data Folder**, **Open Cover Cache**, Quit), Tools
  (**Rebuild Library Index** → updates the backend index and local cache), Help
  (README/source/donate/about).
  Avalonia = `Menu`; WinForms = `MenuStrip`.

## Referencing the API
- Package versions are centralized in `Directory.Packages.props`.
- Default: `PackageReference` to the published packages.
- Co-dev against local API source (sibling `..\SaveHub` clone):
  `dotnet build -p:UseLocalSaveHub=true` (switches to `ProjectReference`).
- Before anything is published to NuGet: build with `-p:UseLocalSaveHub=true`, or
  run `..\SaveHub\pack-api.ps1` then restore.
- A fresh clone (no sibling `..\SaveHub`) restores `SaveHub.*` from NuGet.org — the
  `savehub-local` feed is only a dev convenience and is skipped (NU1801) when absent.

## Releasing
- `.github/workflows/release.yml` builds the WinForms and Avalonia Windows apps
  (win-x64, self-contained single-file) on a published GitHub Release and uploads the
  zips to it. It restores `SaveHub.*` from NuGet.org, so **publish the SaveHub API
  packages first** (matching the versions in `Directory.Packages.props`), then publish
  the UI release. Linux/macOS/Android heads come after the first Windows release.

## Conventions
- Code style: see `.github/copilot-instructions.md`.
- Reusable constants live in a `Common/` folder per app as `static readonly` fields:
  `CommonSettings` (default repo/folder names, cache file name) and, for Avalonia,
  `CommonStrings` (shared UI text). Add new shared values there, not scattered in classes.
- License: GPL-3.0-or-later (the apps). Keep the copyright notice in LICENSE.
  The SaveHub API libraries this consumes are LGPL-3.0-or-later.
- The donate link comes from `SaveHubInfo.DonateUrl` in the Core package — do not
  hardcode it.
