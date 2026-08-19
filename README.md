# SaveHub.UI

[![Support SaveHub](https://img.shields.io/badge/%E2%9D%A4-Support%20SaveHub-ff5f5f)](https://pay.yoco.com/savehub)

Desktop (WinForms) and cross-platform (Avalonia) frontends for
[SaveHub](https://github.com/uncle-uee/SaveHub), built on the SaveHub API packages.

> Licensed under the **GNU General Public License v3 (GPL-3.0)** — see
> [LICENSE](LICENSE). If SaveHub is useful to you, please consider supporting it:
> **https://pay.yoco.com/savehub**.

## Tabs

- **Upload / Download / Edit** — submit saves, fetch them, and replace an existing save.
- **Manage** — multi-select **bulk download** and delete, a per-save **details** panel,
  **filter** boxes for games and saves, **Delete All** for a game, and **Rename game**.
- **Library** — a manufacturer → platform → game tree read from the backup's root
  `library.json` index (cached locally). **Refresh** re-reads it; **Rebuild Index**
  regenerates it from the per-platform indexes.
- **Settings** — pick and configure a storage provider (see below).

Game pickers show `Name (id)`; names come from each platform's index — set a **Game
Title** on upload (or use **Rename game**) so saves are easy to identify.

## Build

Requires the .NET 10 SDK.

```powershell
# Against your local SaveHub API source (sibling ..\SaveHub clone):
dotnet build -p:UseLocalSaveHub=true

# Against published packages (after the API is released to NuGet.org):
dotnet build
```

## Configure a storage provider

Open the **Settings** tab, pick a provider, fill in the fields, click **Save**, then
**Test Connection**. Settings are shared with the SaveHub CLI (same per-user config
file; the path is shown at the top of the Settings tab). A short description of the
selected provider is shown under the Save / Test buttons.

Defaults suggested in the UI: GitHub repository **`Emu-Saves-Backup`** and Google Drive
folder **`EmuSavesBackup`** — change them to whatever you use.

For full details and screenshots see the SaveHub API repo's
`docs/PROVIDER-SETUP.md` guide.

### GitHub

1. Create a repository to hold your saves (e.g. **`game-saves`**). It can be
   public or private.
2. Create a **Personal Access Token**:
   - GitHub → **Settings → Developer settings → Personal access tokens**.
   - Fine-grained token: grant the target repo **Contents: Read and write**, or a
     classic token with the **`repo`** scope.
3. In SaveHub Settings → **GitHub**:
   - **Owner** — your GitHub username or org.
   - **Repository** — the repo name.
   - **Branch** — leave blank to use the repo default.
   - **Token** — paste the token (or leave blank and set the `SAVEHUB_GITHUB_TOKEN`
     environment variable so the secret stays out of the config file).
   - **Auto-merge** — only enable if you have write access; otherwise SaveHub opens a
     pull request for review.

### Supabase

1. Create a project at [supabase.com](https://supabase.com).
2. **Storage → Create bucket** (e.g. `saves`). Note its name.
3. **Project Settings → API** — copy the **Project URL**
   (`https://<ref>.supabase.co`) and an **API key**:
   - `service_role` key to upload/delete (keep it secret), or the `anon` key for
     read-only download.
4. In SaveHub Settings → **Supabase**: enter the URL, bucket, and API key. Tick
   **I own this bucket** if you administer it.

### Google Drive (Google Cloud project)

1. Go to the [Google Cloud Console](https://console.cloud.google.com) and create (or
   pick) a project.
2. **APIs & Services → Library** → enable the **Google Drive API**.
3. **APIs & Services → OAuth consent screen** → configure it (External is fine) and
   add your Google account under **Test users**.
4. **APIs & Services → Credentials → Create credentials → OAuth client ID**:
   - Application type: **Desktop app**.
   - Copy the **Client ID** and **Client secret**.
5. In SaveHub Settings → **Google Drive**:
   - **Root folder** — the app-created folder name (default **`EmuSavesBackup`**).
   - **Client ID** / **Client secret** — from step 4 (or set the client secret via the
     `SAVEHUB_GDRIVE_CLIENT_SECRET` environment variable).
   - Click **Sign in with Google** and approve the browser prompt, then
     **Test Connection**. SaveHub uses the least-privilege **`drive.file`** scope and
     only touches its own folder. The token is kept in memory and cleared when you
     close the app.
