#Requires -Version 5.1
<#
.SYNOPSIS
    Entfernt den Autostart-Eintrag von VGT RhythmGuard für den aktuellen Benutzer.
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$runKeyPath = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run'
$valueName = 'VGT.RhythmGuard'

try {
    $existing = (Get-ItemProperty -Path $runKeyPath -Name $valueName -ErrorAction Stop).$valueName
}
catch {
    $existing = $null
}

if (-not $existing) {
    Write-Host 'Autostart war nicht eingerichtet. Nichts zu tun.' -ForegroundColor Yellow
    exit 0
}

Remove-ItemProperty -Path $runKeyPath -Name $valueName
$after = Get-ItemProperty -Path $runKeyPath -Name $valueName -ErrorAction SilentlyContinue

if ($null -ne $after) {
    throw 'Autostart-Eintrag konnte nicht entfernt werden.'
}

Write-Host 'Autostart entfernt.' -ForegroundColor Green
Write-Host "  Entfernter Befehl: $existing"
Write-Host 'Laufende Instanz ggf. über das Tray-Menü beenden.' -ForegroundColor Yellow
