# Monster Camp Accessibility

Monster Camp Accessibility is a mod that makes Monster Prom 2: Monster Camp, the multiplayer dating sim by Beautiful Glitch, playable with a screen reader. It reads menus, character select and customization, packing, story events and icebreakers, turns, shops, camp locations and campfire seats, endings, results, unlocks and credits, the online menus, and the online code when you host a match.

It works with NVDA, JAWS and other Windows screen readers, and falls back to Windows' built-in voices if no screen reader is running. If your screen reader has a braille display connected, everything the mod reads goes to braille too.

- Join the Discord to chat, get help or report problems: <https://discord.gg/KNDXhztGGK>
- Support shaysters on Ko-fi: <https://ko-fi.com/shaysters>

Version 1.0.1. See [CHANGELOG.md](CHANGELOG.md) for what has changed.

## Requirements

- Monster Prom 2: Monster Camp for Windows (the full game), from Steam or GOG:
  - Steam: <https://store.steampowered.com/app/1140270/Monster_Prom_2_Monster_Camp/>
  - GOG: <https://www.gog.com/en/game/monster_prom_2_monster_camp>
- Windows 10 or later.
- A screen reader such as NVDA or JAWS (optional; without one, the mod uses Windows' built-in speech).

The mod has been tested with the Steam version. The GOG and DRM-free versions should work too, as long as MelonLoader loads in them, but they haven't been tested yet.

## Installing

1. Close the game if it's running.
2. Download **MonsterCampAccessibilityInstaller.exe** from the [latest release](https://github.com/shaysters/monster-camp-accessibility-mod/releases/latest). It needs 64-bit Windows 10 or later.
3. Run it. Windows SmartScreen may warn that the app is unrecognized, because the installer is not code-signed. Choose **More info**, then **Run anyway**.
4. The installer finds your Steam copy of the game by itself. For a GOG or DRM-free copy, or if it can't find the game, use the Browse button next to the game folder and choose the folder that contains `MonsterCamp.exe`.
5. Choose **Install**. The installer adds MelonLoader if it's missing, then the mod, and removes files left by older versions of the mod.
6. Start your screen reader, then start the game.

### Updating and uninstalling

Keep the installer. Run it again to update to the newest version, repair the mod, or uninstall it. Close the game first. The installer says on its first page when an update is available, and the mod tells you too, once the title menu has been read.

### Installing by hand

1. Install MelonLoader 0.7.2 (x86) into the game with the [MelonLoader installer](https://github.com/LavaGang/MelonLoader.Installer/releases).
2. Download **MonsterCampAccessibility.zip** from the [latest release](https://github.com/shaysters/monster-camp-accessibility-mod/releases/latest). It contains `GameMod.dll`, `prism.dll`, the `Prism` folder (Prism's license files) and `Localization\English.lang`.
3. Extract everything in it into the `Mods` folder inside the game folder. Common game folders are:

   ```
   Steam: C:\Program Files (x86)\Steam\steamapps\common\Monster Prom 2 - Monster Camp
   GOG:   C:\Program Files (x86)\GOG Galaxy\Games\Monster Prom 2 - Monster Camp
   ```

4. Start your screen reader, then start the game.

To uninstall a manual install, delete `GameMod.dll`, `prism.dll`, the `Prism` folder and the `Localization` folder from `Mods`. To remove MelonLoader too, use the MelonLoader installer's uninstall option.

## Keys

| Key | What it does |
|---|---|
| Arrow keys | Move through most menus and choices. |
| Enter | Confirm the current selection. |
| Escape | Go back, or open the pause or back prompt on screens that have one. |
| r | Repeat the last thing the mod said. |
| t | Read the current player's stats. |
| c | In an online lobby, read the online code again. |

Notes:

- Campfire spots say who is sitting there and how many seats are free.
- When you host an online match, the online code is read digit by digit.

## Languages

The mod follows the game's language wherever Monster Camp has its own text. The mod's own phrases can be translated with a language file in:

```
Mods\Localization\<Language>.lang
```

The included `English.lang` can be used as a template. Each line is `key=value`.

## Known limitations

- Gallery images don't have descriptions yet.

## Troubleshooting

MelonLoader keeps a log in the game folder:

```
MelonLoader\Latest.log
```

It shows whether the mod loaded and which speech output Prism chose. If something goes wrong, include the log when you report it.

- **No speech at all:** make sure `GameMod.dll`, `prism.dll` and the `Prism` folder are in `Mods`, and that MelonLoader is installed. Running the installer again and choosing Repair fixes most problems.
- **Windows speech talks instead of your screen reader:** start your screen reader before the game. If you start it after, restart the game.

To report a problem or suggest something, open an issue on the [GitHub page](https://github.com/shaysters/monster-camp-accessibility-mod/issues).

## Credits

This mod was developed by [Shaysters](https://github.com/shaysters), with code written by Codex (OpenAI) and Claude (Anthropic).

The installer is built with [Loadstone](https://github.com/bradjrenshaw/loadstone) by [Bradley Renshaw](https://github.com/bradjrenshaw).

Speech is provided by [Prism](https://github.com/ethindp/prism) by Ethin Probst, under the MPL-2.0 license. Its license files are in the `Prism` folder.

Monster Prom 2: Monster Camp is made by Beautiful Glitch. This mod isn't affiliated with or endorsed by Beautiful Glitch.

## For developers

Build and release instructions are in [DEVELOPING.md](DEVELOPING.md).
