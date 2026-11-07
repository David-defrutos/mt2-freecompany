# Documento generado el 2026-11-07-1204
[CmdletBinding()]
param(
    [ValidateSet('Validar', 'Instalar', 'Empaquetar')]
    [string]$Accion = 'Validar',
    [string]$Destination,
    [switch]$Simular
)
$ErrorActionPreference = 'Stop'
$python = 'C:\Users\david\miniconda3\python.exe'
$taskArgs = @((Join-Path $PSScriptRoot 'workflow.py'), $Accion.ToLowerInvariant())
if ($Destination) { $taskArgs += @('--destination', $Destination) }
if ($Simular) { $taskArgs += '--dry-run' }
& $python @taskArgs
if ($LASTEXITCODE -ne 0) { throw "FreeCompany: fallo al $Accion" }

# 2026-11-07-1204||codex-freecompany-fx||scripts/freecompany.ps1||expone acciones separadas Validar, Instalar y Empaquetar desde repositorio canónico
