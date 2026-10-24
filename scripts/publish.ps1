#Requires -Version 5.1
<#
.SYNOPSIS
    Baut eine eigenständige Windows-x64-Version von VGT RhythmGuard in dist\.
.DESCRIPTION
    Self-contained Single-File-Publish: Zielrechner brauchen keine .NET-Runtime.
    Prüft vorher, ob eine laufende Instanz die Zieldatei sperrt, und gibt dann eine
    klare Anweisung aus, statt mitten im Publish abzubrechen.
#>
[CmdletBinding()]
param(
    [string]$Configuration = 'Release',
    [string]$Runtime = 'win-x64'
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot 'src\VGT.RhythmGuard.App\VGT.RhythmGuard.App.csproj'
$output = Join-Path $repoRoot 'dist'
$targetExe = Join-Path $output 'VGT.RhythmGuard.exe'

if (-not (Test-Path -LiteralPath $project -PathType Leaf)) {
    throw "Projekt nicht gefunden: $project"
}

# Preflight: laufende Instanz sperrt die Zieldatei.
if (Test-Path -LiteralPath $targetExe -PathType Leaf) {
    try {
        $stream = [System.IO.File]::Open($targetExe, [System.IO.FileMode]::Open, [System.IO.FileAccess]::ReadWrite, [System.IO.FileShare]::None)
        $stream.Close()
    }
    catch {
        throw 'Die Zieldatei ist gesperrt. Bitte VGT RhythmGuard über das Tray-Menü beenden (Rechtsklick -> Beenden) und den Publish erneut starten.'
    }
}

Write-Host "Baue VGT RhythmGuard ($Configuration, $Runtime) ..." -ForegroundColor Cyan
dotnet publish $project `
    -c $Configuration `
    -r $Runtime `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:DebugType=None `
    -p:DebugSymbols=false `
    -o $output

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish ist mit Exitcode $LASTEXITCODE fehlgeschlagen."
}

if (-not (Test-Path -LiteralPath $targetExe -PathType Leaf)) {
    throw "Publish-Ergebnis fehlt: $targetExe"
}

Unblock-File -LiteralPath $targetExe -ErrorAction SilentlyContinue
$size = [math]::Round((Get-Item -LiteralPath $targetExe).Length / 1MB, 1)
Write-Host ''
Write-Host 'Publish erfolgreich.' -ForegroundColor Green
Write-Host "  EXE:   $targetExe ($size MB)"
Write-Host "  Ordner: $output"
Write-Host ''
Write-Host 'Nächster Schritt:'
Write-Host '  powershell -ExecutionPolicy Bypass -File scripts\install-autostart.ps1'
