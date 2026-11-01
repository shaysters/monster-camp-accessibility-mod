# Monster Camp Accessibility: developer notes

A MelonLoader 0.7.2 mod (32-bit) for Monster Prom 2: Monster Camp. The source is `GameMod.csproj`, `ModMain.cs` and the `Toolkit` folder. Speech goes through Prism, loaded from `prism.dll` next to the mod.

## Building

You need the .NET SDK (version 6 or later), and the game installed with MelonLoader, because the build uses the game's own files and MelonLoader's files from the game folder.

Build, pointing at the game's `Managed` folder and its MelonLoader folder:

```
dotnet build GameMod.csproj -c Release "-p:ManagedDir=C:\Program Files (x86)\Steam\steamapps\common\Monster Prom 2 - Monster Camp\MonsterCamp_Data\Managed" "-p:MelonLoaderDir=C:\Program Files (x86)\Steam\steamapps\common\Monster Prom 2 - Monster Camp\MelonLoader"
```

The built mod is `bin\Release\GameMod.dll`. `build.ps1` does the same build and, given `-ModsDir`, also copies the mod and `prism.dll` into the game's `Mods` folder for testing.

## Packaging and releasing

Build the release zip into `dist\MonsterCampAccessibility.zip`:

```
powershell -ExecutionPolicy Bypass -File scripts\package-release.ps1
```

Add `-GameDir "<game folder>"` if the game isn't in the default Steam folder.

To publish a release:

1. Update the version in `ModMain.cs`, `changes-latest.md` and `CHANGELOG.md`.
2. Build the zip.
3. Create a draft GitHub release with the zip attached:

   ```
   gh release create vX.Y.Z --draft --notes-file changes-latest.md dist\MonsterCampAccessibility.zip
   ```

4. Run the **Release** workflow from the Actions tab with the same tag. It builds the installer, writes `loadstone-release.json`, and publishes the release.

The installer's package files are in `loadstone-packages`, and its settings are in `installer\profile.json`. The installer reads the package files straight from this repository, so changes to them reach players a few minutes after they're pushed.
