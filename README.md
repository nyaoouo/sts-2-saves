# STS2Saves

STS2Saves is a Slay the Spire 2 mod for keeping more control over run saves. It archives autosaves, lets you create manual snapshots from the pause menu, and adds a browser for loading, backing up, and deleting saved run snapshots.

## What This Mod Does

- Captures archived autosaves whenever the game saves an in-progress run.
- Adds a manual save button to the pause menu.
- Adds a load browser so you can restore earlier snapshots instead of relying on the single vanilla current-run save.
- Separates snapshots by run and by save type: autosaves and manual saves.
- Lets you export either a single snapshot or an entire archived run to a folder on disk.
- Creates rollback copies before restoring a snapshot over the active save.
- Supports both singleplayer and multiplayer host saves.

## Main Functions

### 1. Automatic Save Archiving

When Slay the Spire 2 writes the current run save, STS2Saves copies that save into its own archive.

- Singleplayer autosaves are archived automatically.
- Multiplayer host autosaves are archived automatically by default.
- Autosaves are grouped under the run they belong to.
- The default retention mode keeps only the latest 10 autosaves per run.

### 2. Manual Save Snapshots

While in a run, the pause menu gets two extra actions:

- `Save`: creates a manual snapshot of the current run.
- `Load`: opens the save browser for the current mode.

Manual snapshots are useful before risky events, elite fights, boss fights, or route changes.

### 3. Save Browser

The save browser is the main UI for managing archived runs.

It is opened from:

- The main menu `Continue` button for singleplayer archived runs.
- The multiplayer `Load` flow for multiplayer archived runs.
- The pause menu `Load` button while you are already in a run.

Inside the browser, you can:

- Browse archived runs.
- Expand a run to see `Auto` and `Manual` save groups.
- Inspect save details in the right-hand panel.
- Load a selected snapshot.
- Back up a selected snapshot.
- Back up an entire run archive.
- Delete a selected snapshot.
- Delete an entire archived run.

### 4. Safe Restore with Rollback

When you restore a snapshot, the mod first creates rollback copies of the current active save files if they exist. After that, it writes the selected archived snapshot back to the active game save location.

That gives you a safety net if you restore the wrong snapshot or want to inspect what was replaced.

### 5. Export / Backup

The `Backup` action exports files to a normal folder on disk.

- Backing up a snapshot exports the `.save` file and its metadata `.json`.
- Backing up a run exports the entire archived run folder.

This is useful for sharing test states, preserving a run before cleanup, or moving saves between setups.

## Installation

### Option 1: Install a packaged build

If you already have a built release zip, extract it into your Slay the Spire 2 `mods` folder so the mod manifest and DLL end up under a folder like this:

```text
Slay the Spire 2/
  mods/
    STS2Saves/
      STS2Saves.dll
      STS2Saves.json
```

### Option 2: Build from source

This project targets Godot .NET and references the game DLLs directly from your local Slay the Spire 2 install.

Build command:

```powershell
dotnet build D:\Games\SteamLibrary\steamapps\common\Slay the Spire 2\tools\sts-2-saves\STS2Saves.csproj
```

The project is configured to:

- Build the DLL.
- Copy the mod into your local Slay the Spire 2 `mods/STS2Saves/` folder.
- Pack a zip into `dist/STS2Saves.zip`.

## How To Use

### Create a manual save

1. Start or continue a run.
2. Open the pause menu.
3. Press `Save`.
4. A manual snapshot is added to that run's archive.

### Load an older snapshot

1. Open the save browser from the main menu, multiplayer load flow, or pause menu.
2. Select the run you want.
3. Expand `Auto` or `Manual` saves.
4. Select the snapshot.
5. Press `Load`.

The mod restores the selected snapshot into the active save location and then loads that run.

### Export a backup

1. Open the save browser.
2. Select either a snapshot or a run.
3. Press `Backup`.
4. The mod creates an export folder on disk.

### Delete old saves

1. Open the save browser.
2. Select a snapshot to delete just that save, or select a run to delete the full archive.
3. Use the appropriate delete action.

## Save Storage Layout

Archived files are stored under the current Slay the Spire 2 profile path, inside the mod's own folder.

Logical layout:

```text
<profile scoped path>/nymod.saves/
  archive/
    singleplayer/
      runs/
      rollback/
      exports/
    multiplayer/
      runs/
      rollback/
      exports/
  config.json
```

Notes:

- `runs/` contains archived autosaves and manual saves.
- `rollback/` contains copies of the active save files that existed before a restore.
- `exports/` contains snapshot or run backups created from the browser.
- `config.json` stores retention and behavior settings.

## Configuration

There is currently no in-game settings screen for this mod. Configuration is stored in `config.json` and falls back to defaults if the file does not exist.

Current settings:

- `autosave_retention_mode`: `All` or `CapLatest`
- `autosave_cap_per_run`: maximum autosaves kept when using `CapLatest`
- `capture_multiplayer_autosaves`: enables automatic multiplayer host archiving
- `manual_names_include_sequence`
- `confirm_run_deletion`

Example:

```json
{
  "schema_version": 1,
  "autosave_retention_mode": "CapLatest",
  "autosave_cap_per_run": 10,
  "capture_multiplayer_autosaves": true,
  "manual_names_include_sequence": true,
  "confirm_run_deletion": true
}
```

## Current Behavior Notes

- Multiplayer save/load actions are intended for the host, not clients.
- Archived saves are stored separately from the game's active current-run save.
- Localization is embedded into the DLL at runtime rather than shipped as loose JSON files.