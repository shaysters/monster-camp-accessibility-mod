param(
    [string]$Project = ".\\GameMod.csproj",
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [string]$ManagedDir = $env:GAME_MANAGED_DIR,
    [string]$MelonLoaderDir = $env:MELONLOADER_DIR,
    [string]$MelonLoaderDll = $env:MELONLOADER_DLL,
    [string]$ModsDir = $env:GAME_MODS_DIR
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $Project)) {
    throw "Project file not found: $Project"
}

if ([string]::IsNullOrWhiteSpace($ManagedDir)) {
    throw "Set -ManagedDir or GAME_MANAGED_DIR to the game's *_Data\\Managed folder."
}

if (-not (Test-Path $ManagedDir)) {
    throw "ManagedDir does not exist: $ManagedDir"
}

if ([string]::IsNullOrWhiteSpace($MelonLoaderDll) -and -not [string]::IsNullOrWhiteSpace($MelonLoaderDir)) {
    $candidate = Join-Path $MelonLoaderDir "net35\\MelonLoader.dll"
    if (Test-Path $candidate) {
        $MelonLoaderDll = $candidate
    }
}

if ([string]::IsNullOrWhiteSpace($MelonLoaderDll)) {
    throw "Set -MelonLoaderDir / MELONLOADER_DIR or -MelonLoaderDll / MELONLOADER_DLL to MelonLoader.dll."
}

if (-not (Test-Path $MelonLoaderDll)) {
    throw "MelonLoader.dll not found: $MelonLoaderDll"
}

if ([string]::IsNullOrWhiteSpace($ModsDir) -and -not [string]::IsNullOrWhiteSpace($MelonLoaderDir)) {
    $root = Split-Path -Parent $MelonLoaderDir
    $autoMods = Join-Path $root "Mods"
    if (Test-Path $autoMods) {
        $ModsDir = $autoMods
    }
}

$msbuild = Get-Command msbuild -ErrorAction SilentlyContinue
$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue

if ($msbuild) {
    & $msbuild.Source $Project "/t:Build" "/p:Configuration=$Configuration" "/p:ManagedDir=$ManagedDir" "/p:MelonLoaderDll=$MelonLoaderDll" "/nologo" "/v:m"
    if ($LASTEXITCODE -ne 0) {
        throw "MSBuild failed with exit code $LASTEXITCODE. Build artifacts were not deployed."
    }
}
elseif ($dotnet) {
    $sdks = & $dotnet.Source --list-sdks
    if (-not $sdks) {
        throw "dotnet was found but no .NET SDK is installed. Install a .NET SDK or use MSBuild from Visual Studio Build Tools."
    }

    & $dotnet.Source build $Project -c $Configuration "-p:ManagedDir=$ManagedDir" "-p:MelonLoaderDll=$MelonLoaderDll" "-nologo" "-v:minimal"
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build failed with exit code $LASTEXITCODE. Build artifacts were not deployed."
    }
}
else {
    throw "No build tool found. Install Visual Studio Build Tools (MSBuild) or .NET SDK."
}

$projectPath = (Resolve-Path $Project).Path
$projectDir = Split-Path -Parent $projectPath
$projectName = [System.IO.Path]::GetFileNameWithoutExtension($projectPath)

[xml]$projXml = Get-Content $projectPath
$dllName = $projXml.Project.PropertyGroup.AssemblyName | Select-Object -First 1
if ([string]::IsNullOrWhiteSpace($dllName)) {
    $dllName = $projectName
}

$outputDll = Join-Path $projectDir "bin\\$Configuration\\$dllName.dll"
if (-not (Test-Path $outputDll)) {
    throw "Build completed but DLL not found at expected path: $outputDll"
}

Write-Host "Built: $outputDll"

if (-not [string]::IsNullOrWhiteSpace($ModsDir)) {
    if (-not (Test-Path $ModsDir)) {
        New-Item -ItemType Directory -Path $ModsDir -Force | Out-Null
    }

    $destDll = Join-Path $ModsDir "$dllName.dll"
    Copy-Item -Path $outputDll -Destination $destDll -Force
    Write-Host "Copied: $destDll"

    $runtimeSupportFiles = @(
        "Prism\\prism.dll"
    )

    foreach ($fileName in $runtimeSupportFiles) {
        $sourcePath = Join-Path $projectDir $fileName
        if (Test-Path $sourcePath) {
            $destPath = Join-Path $ModsDir (Split-Path -Leaf $fileName)
            Copy-Item -Path $sourcePath -Destination $destPath -Force
            Write-Host "Copied: $destPath"
        }
    }
}
