#Requires -Version 5.1
<#
.SYNOPSIS
    Installiert VGT RhythmGuard als Autostart für den aktuellen Windows-Benutzer.
.DESCRIPTION
    Schreibt den Startbefehl in HKCU\Software\Microsoft\Windows\CurrentVersion\Run.
    Keine Admin-Rechte erforderlich. Die App startet danach bei der Windows-Anmeldung
    im Tray und stellt den Rhythmus wieder her.
.PARAMETER ExePath
    Optionaler Pfad zur VGT.RhythmGuard.exe. Ohne Angabe wird automatisch gesucht.
.PARAMETER StartNow
    Startet die App nach der Installation sofort.
#>
[CmdletBinding()]
param(
    [string]$ExePath,
    [switch]$StartNow
)

$ErrorActionPreference = 'Stop'
$runKeyPath = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run'
$valueName = 'VGT.RhythmGuard'
$repoRoot = Split-Path -Parent $PSScriptRoot

function Find-RhythmGuardExe {
    $candidates = @(
        $ExePath,
        (Join-Path $repoRoot 'dist\VGT.RhythmGuard.exe'),
        (Join-Path $repoRoot 'src\VGT.RhythmGuard.App\bin\Release\net10.0-windows\win-x64\publish\VGT.RhythmGuard.exe'),
        (Join-Path $repoRoot 'src\VGT.RhythmGuard.App\bin\Debug\net10.0-windows\VGT.RhythmGuard.exe')
    ) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }

    foreach ($candidate in $candidates) {
        $resolved = [System.IO.Path]::GetFullPath($candidate)
        if (Test-Path -LiteralPath $resolved -PathType Leaf) {
            return $resolved
        }
    }

    throw 'VGT.RhythmGuard.exe wurde nicht gefunden. Bitte zuerst publish.ps1 ausführen oder -ExePath angeben.'
}

$exe = Find-RhythmGuardExe
Unblock-File -LiteralPath $exe -ErrorAction SilentlyContinue
$command = '"{0}" --logon' -f $exe

if (-not (Test-Path -LiteralPath $runKeyPath)) {
    New-Item -Path $runKeyPath -Force | Out-Null
}

Set-ItemProperty -Path $runKeyPath -Name $valueName -Value $command -Type String
$written = (Get-ItemProperty -Path $runKeyPath -Name $valueName).$valueName

if ($written -ne $command) {
    throw "Autostart-Eintrag konnte nicht verifiziert werden. Erwartet: $command | Vorhanden: $written"
}

Write-Host "Autostart installiert." -ForegroundColor Green
Write-Host "  Eintrag: $runKeyPath\$valueName"
Write-Host "  Befehl:  $command"
Write-Host ''
Write-Host 'Hinweis: Der Rhythmus wird nur zwischen 07:00 und 23:00 aktiv erzwungen.' -ForegroundColor Yellow
Write-Host '         Erst nach bewusster Aktivierung kann der Nächste Urlaub/Test angepasst werden.' -ForegroundColor Yellow
Write-Host 'Deaktivieren: scripts\uninstall-autostart.ps1'

if ($StartNow) {
    Start-Process -FilePath $exe -ArgumentList '--logon'
    Write-Host 'VGT RhythmGuard wurde gestartet.' -ForegroundColor Green
}
