param(
    [string]$DllPath,
    [string]$Destination
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$manifest = Get-Content -LiteralPath (Join-Path $repo 'manifest.json') -Raw | ConvertFrom-Json
$version = [string]$manifest.version_number
if ($version -notmatch '^\d+\.\d+\.\d+$') { throw "Version no valida: $version" }

if (-not $DllPath) { $DllPath = Join-Path $repo 'mt2_freecompany.Plugin.dll' }
$DllPath = (Resolve-Path -LiteralPath $DllPath).Path
$dllVersion = [Diagnostics.FileVersionInfo]::GetVersionInfo($DllPath).FileVersion
if ($dllVersion -notlike "$version.*") {
    throw "Version del DLL ($dllVersion) distinta del manifest ($version)"
}
$projectVersion = [regex]::Match(
    (Get-Content -LiteralPath (Join-Path $repo 'src\mt2_freecompany.Plugin.csproj') -Raw),
    '<Version>([^<]+)</Version>'
).Groups[1].Value
if ($projectVersion -ne $version) {
    throw "Version del proyecto ($projectVersion) distinta del manifest ($version)"
}
if (-not $Destination) {
    $Destination = Join-Path $repo "frutos-FreeCompany-$version.zip"
}
$Destination = [IO.Path]::GetFullPath($Destination)
if (Test-Path -LiteralPath $Destination) { throw "El ZIP ya existe: $Destination" }

$pluginSource = Get-Content -LiteralPath (Join-Path $repo 'src\Plugin.cs') -Raw
$declared = [regex]::Matches($pluginSource, '"(json/[A-Za-z0-9_/]+\.json)"') |
    ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique
$actual = Get-ChildItem -LiteralPath (Join-Path $repo 'json') -Filter '*.json' -Recurse -File |
    ForEach-Object { $_.FullName.Substring($repo.Length + 1).Replace('\', '/') } | Sort-Object -Unique
$missing = @($declared | Where-Object { $_ -notin $actual })
$extra = @($actual | Where-Object { $_ -notin $declared })
if ($missing.Count -or $extra.Count) {
    throw "JSON distintos de Plugin.cs. Faltan: $($missing -join ', '); sobran: $($extra -join ', ')"
}

$stage = Join-Path $repo ('.package-stage-' + [guid]::NewGuid().ToString('N'))
$resolvedRepo = [IO.Path]::GetFullPath($repo).TrimEnd('\')
$resolvedStage = [IO.Path]::GetFullPath($stage)
if (-not $resolvedStage.StartsWith($resolvedRepo + '\', [StringComparison]::OrdinalIgnoreCase)) {
    throw "Carpeta temporal inesperada: $resolvedStage"
}

try {
    $plugins = Join-Path $stage 'plugins'
    New-Item -ItemType Directory -Path $plugins -Force | Out-Null
    foreach ($file in @('manifest.json', 'icon.png', 'README.md', 'CHANGELOG.md', 'LICENSE')) {
        Copy-Item -LiteralPath (Join-Path $repo $file) -Destination $stage
    }
    Copy-Item -LiteralPath $DllPath -Destination (Join-Path $plugins 'mt2_freecompany.Plugin.dll')
    Copy-Item -LiteralPath (Join-Path $repo 'NOTICE.md') -Destination $plugins
    Copy-Item -LiteralPath (Join-Path $repo 'json') -Destination $plugins -Recurse
    # Only ship runtime texture paths actually referenced by the registered JSON.
    $texturePaths = @($declared | ForEach-Object {
        $jsonText = Get-Content -LiteralPath (Join-Path $repo $_) -Raw
        [regex]::Matches($jsonText, '"(textures/[^"\r\n]+)"') |
            ForEach-Object { $_.Groups[1].Value }
    } | Sort-Object -Unique)
    foreach ($texturePath in $texturePaths) {
        $sourceTexture = [IO.Path]::GetFullPath((Join-Path $repo $texturePath))
        $targetTexture = [IO.Path]::GetFullPath((Join-Path $plugins $texturePath))
        if (-not $sourceTexture.StartsWith((Join-Path $resolvedRepo 'textures') + '\', [StringComparison]::OrdinalIgnoreCase) -or
            -not $targetTexture.StartsWith($plugins + '\textures\', [StringComparison]::OrdinalIgnoreCase)) {
            throw "Ruta de textura inesperada: $texturePath"
        }
        New-Item -ItemType Directory -Path ([IO.Path]::GetDirectoryName($targetTexture)) -Force | Out-Null
        Copy-Item -LiteralPath $sourceTexture -Destination $targetTexture
    }

    Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $Destination

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [IO.Compression.ZipFile]::OpenRead($Destination)
    try {
        $entries = @($zip.Entries | ForEach-Object FullName)
        foreach ($required in @('manifest.json', 'icon.png', 'README.md',
                'plugins/mt2_freecompany.Plugin.dll', 'plugins/json/plugin.json',
                'plugins/NOTICE.md')) {
            if ($required -notin $entries) { throw "Falta en ZIP: $required" }
        }
        if (-not @($entries | Where-Object { $_ -like 'plugins/textures/*.png' }).Count) {
            throw 'Faltan las texturas en plugins/textures/'
        }
        if (@($entries | Where-Object { $_ -match '^(json/|textures/|src/|\.git/)' -or
                    $_ -match '(\.dll\.bak$|spell_requisition\.json$|equip_letter_of_credit\.json$)' }).Count) {
            throw 'El ZIP contiene rutas prohibidas u obsoletas.'
        }
        foreach ($json in $declared) {
            if ("plugins/$json" -notin $entries) { throw "Falta en ZIP: plugins/$json" }
        }
        Write-Output "Paquete verificado: $Destination ($($declared.Count) JSON)"
    }
    finally { $zip.Dispose() }
}
finally {
    if (Test-Path -LiteralPath $stage) { Remove-Item -LiteralPath $stage -Recurse -Force }
}

# 2026-10-27-2204||codex-freecompany-fx||scripts/crear-paquete-thunderstore.ps1||H8: empaqueta solo texturas referenciadas; conserva recursos sin uso en trabajo y evita paths fuera de textures
