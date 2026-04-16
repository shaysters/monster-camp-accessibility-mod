# Monster Camp Accessibility Mod

Beta `0.9.0-beta`

A MelonLoader accessibility mod for **Monster Prom 2: Monster Camp**.

## Links

- Game on Steam: https://store.steampowered.com/app/1140270/Monster_Prom_2_Monster_Camp/
- Game on GOG: https://www.gog.com/en/game/monster_prom_2_monster_camp
- MelonLoader installer: https://github.com/LavaGang/MelonLoader.Installer/releases

## Included

- `GameMod.dll`
- `Tolk.dll`
- `nvdaControllerClient32.dll`
- `nvdaControllerClient64.dll`
- `Localization/English.lang`

## Install

1. Install MelonLoader in the full game.
2. Open the game's `Mods` folder.
3. Copy the contents of this package into that folder.
4. Start the game.

This mod supports Steam and other PC storefront builds of the full game that load MelonLoader successfully.

Common game paths:

- Steam: `C:\Program Files (x86)\Steam\steamapps\common\Monster Prom 2 - Monster Camp`
- GOG: `C:\Program Files (x86)\GOG Galaxy\Games\Monster Prom 2 - Monster Camp`

Mod path:

- `<Game Folder>\Mods`

### Linux

1. Install MelonLoader for the game's Linux setup.
2. Open the game's `Mods` folder.
3. Copy the contents of this package into that folder.
4. Make sure at least one supported speech command is installed:
   `espeak-ng`, `espeak`, or `spd-say`
5. Start the game.

## Screen Reader Support

On Windows, the mod uses:
1. `Tolk.dll`
2. Direct NVDA through `nvdaControllerClient32.dll` or `nvdaControllerClient64.dll`

On Linux, the mod includes support for:
- `espeak-ng`
- `espeak`
- `spd-say`

## Controls

### Game Controls

- Arrow keys: move through most menus and choices
- Enter: confirm the current selection
- Escape: go back or open the pause/back prompt on supported screens

### Mod Controls

- `R`: repeat last spoken line
- `T`: read current player stats

## Coverage

This release covers:

- menus
- character select and customization
- packing
- story events and icebreakers
- turn announcements
- shops
- camp locations
- endings, results, unlocks, and credits
- online menus and dialogs
- custom game options

## Localization

The mod follows the game's active language where Monster Camp already provides text.
Mod-added accessibility phrases can be overridden with language files in:

- `Mods/Localization/<Language>.lang`

The included `English.lang` file can be used as a translation template. Each line uses:

`key=value`

## Known Limitation

- Gallery images do not currently have alt text.

## Beta Testing

This beta is intended for user testing in the full game. Linux speech support is included, but has not been runtime-verified in this test cycle.

## Credits

- Shaysters
- Codex
