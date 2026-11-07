# Documento generado el 2026-11-07-1206
[CmdletBinding()]
param([string]$Destination)
$ErrorActionPreference = 'Stop'
$taskArgs = @{ Accion = 'Empaquetar' }
if ($Destination) { $taskArgs.Destination = $Destination }
& (Join-Path $PSScriptRoot 'freecompany.ps1') @taskArgs

# 2026-11-07-1206||codex-freecompany-fx||scripts/crear-paquete-thunderstore.ps1||delega al flujo canónico: valida y compila, exige revisión guardada y empaqueta sin instalar ni publicar
