# Monster Camp Accessibility Mod

Version `1.0.0`

A MelonLoader accessibility mod for **Monster Prom 2: Monster Camp**.

## Links

- Game on Steam: https://store.steampowered.com/app/1140270/Monster_Prom_2_Monster_Camp/
- Game on GOG: https://www.gog.com/en/game/monster_prom_2_monster_camp
- MelonLoader installer: https://github.com/LavaGang/MelonLoader.Installer/releases

## Install with the installer (recommended)

The installer sets up everything for you, including MelonLoader. It needs 64-bit Windows 10 or later and supports the Steam version of the game. For other stores, use the manual install below.

1. Download `MonsterCampAccessibilityInstaller.exe` from the [latest release](https://github.com/shaysters/monster-camp-accessibility-mod/releases/latest).
2. Close the game, then run the installer.
   Windows SmartScreen may warn that the app is unrecognized, because the installer is not code-signed. Choose "More info", then "Run anyway".
3. The installer finds the game through Steam. If it doesn't, use Browse to pick the folder that contains `MonsterCamp.exe`.
4. Press Install. The installer adds MelonLoader if it is missing, then the mod, and removes files left by older versions of the mod.

Run the installer again at any time to update, repair, or uninstall the mod.

## Manual install

The release zip `MonsterCampAccessibility.zip` contains:

- `GameMod.dll`
- `prism.dll`
- `Prism/` (Prism license files)
- `Localization/English.lang`

1. Install MelonLoader 0.7.2 (x86) in the full game.
2. Open the game's `Mods` folder.
3. Copy the contents of the zip into that folder.
4. Start the game.

This mod supports Steam and other PC storefront builds of the full game that load MelonLoader successfully.

Common game paths:

- Steam: `C:\Program Files (x86)\Steam\steamapps\common\Monster Prom 2 - Monster Camp`
- GOG: `C:\Program Files (x86)\GOG Galaxy\Games\Monster Prom 2 - Monster Camp`

Mod path:

- `<Game Folder>\Mods`

## Screen Reader Support

The mod uses Prism for speech.

- If NVDA, JAWS, or another supported screen reader is running when the game starts, speech goes through it, and to your braille display if the screen reader supports it.
- If no screen reader is running, the mod uses built-in Windows speech.
- Start your screen reader before the game. If you start it after, restart the game to switch over.

Prism is licensed under the Mozilla Public License 2.0. Its license files are in the `Prism` folder.

## Controls

### Game Controls

- Arrow keys: move through most menus and choices
- Enter: confirm the current selection
- Escape: go back or open the pause/back prompt on supported screens

### Mod Controls

- `R`: repeat last spoken line
- `T`: read current player stats
- `C`: in an online lobby, read the online code again

## Coverage

This release covers:

- menus
- character select and customization
- packing
- story events and icebreakers
- turn announcements
- shops
- camp locations, including who is sitting at each campfire spot and how many seats are free
- endings, results, unlocks, and credits
- online menus and dialogs, including the online code when you host a match (read digit by digit)
- custom game options

## Localization

The mod follows the game's active language where Monster Camp already provides text.
Mod-added accessibility phrases can be overridden with language files in:

- `Mods/Localization/<Language>.lang`

The included `English.lang` file can be used as a translation template. Each line uses:

`key=value`

## Known Limitation

- Gallery images do not currently have alt text.

## Platform

Windows 10 or later only.

## Troubleshooting

- **No speech at all:** make sure `GameMod.dll`, `prism.dll`, and the `Prism` folder are in `Mods`, and that MelonLoader is installed. Running the installer again (Repair) fixes most problems.
- **Speech uses the Windows voice instead of your screen reader:** start your screen reader before the game, then restart the game.
- **Still stuck:** the MelonLoader log is at `<Game Folder>\MelonLoader\Latest.log`. It shows whether the mod loaded and which speech backend Prism chose.

## Building from source

For developers. You need the .NET SDK (version 6 or later) and the game installed with MelonLoader, because the build uses the game's own files and MelonLoader's files from the game folder.

Build, using the game's `Managed` folder and MelonLoader folder:

```
dotnet build GameMod.csproj -c Release "-p:ManagedDir=C:\Program Files (x86)\Steam\steamapps\common\Monster Prom 2 - Monster Camp\MonsterCamp_Data\Managed" "-p:MelonLoaderDir=C:\Program Files (x86)\Steam\steamapps\common\Monster Prom 2 - Monster Camp\MelonLoader"
```

The built mod is `bin\Release\GameMod.dll`. `build.ps1` does the same build and, given `-ModsDir`, also copies the mod and `prism.dll` into the game's `Mods` folder for testing.

Build the release zip into `dist\MonsterCampAccessibility.zip`:

```
powershell -ExecutionPolicy Bypass -File scripts\package-release.ps1
```

Add `-GameDir "<game folder>"` if the game is not in the default Steam folder.

To publish a release: update `changes-latest.md`, build the zip, create a draft GitHub release with the zip attached (`gh release create vX.Y.Z --draft --notes-file changes-latest.md dist\MonsterCampAccessibility.zip`), then run the **Release** workflow from the Actions tab with the same tag. It builds the installer, writes `loadstone-release.json`, and publishes the release. The installer's package files are in `loadstone-packages/`, and its settings are in `installer/profile.json`.

## Credits

- Shaysters
- Codex
- Claude
- The installer is built with [Loadstone](https://github.com/bradjrenshaw/loadstone) by Bradley Renshaw.
- Speech uses [Prism](https://github.com/ethindp/prism) by Ethin Probst, licensed under the Mozilla Public License 2.0.
