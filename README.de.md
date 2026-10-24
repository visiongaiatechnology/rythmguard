# VGT RythmGuard

> **Schütze deinen Rhythmus. Schütze dich selbst.**

<p align="center">
  <img src="docs/branding/logo-wordmark.png" alt="VGT RythmGuard" width="480" />
</p>

<p align="center">
  <strong>Ein Mental-Health-Rhythmus-System, das Pausen, Abend und Schlaf fest in die Windows-Session einhängt.</strong>
</p>

<p align="center">
  <a href="README.md">English</a> ·
  <strong>Deutsch</strong> ·
  <a href="README.es.md">Español</a> ·
  <a href="README.it.md">Italiano</a> ·
  <a href="README.ru.md">Русский</a> ·
  <a href="README.zh.md">中文</a> ·
  <a href="README.ja.md">日本語</a>
</p>

![Dashboard](docs/screenshots/06-dashboard-overview.png)

## ⬇️ Download

| Plattform | Version | Download |
|---|---|---|
| 🪟 **Windows 10/11 x64** | **1.4.1 Beta** | [**Windows-Installer**](https://github.com/visiongaiatechnology/rythmguard/releases/download/v1.4.1-beta.1/VGT-RythmGuard-Setup-1.4.1-Beta.exe) · [Versionshinweise](https://github.com/visiongaiatechnology/rythmguard/releases/tag/v1.4.1-beta.1) |
| 🐧 **Linux x64** | **1.4.1 Beta** | [**AppImage (empfohlen)**](https://github.com/visiongaiatechnology/rythmguard/releases/download/v1.4.1-linux-beta.1/VGT-RythmGuard-1.4.1-linux-x86_64.AppImage) · [Tar-Archiv](https://github.com/visiongaiatechnology/rythmguard/releases/download/v1.4.1-linux-beta.1/VGT-RythmGuard-1.4.1-linux-x64.tar.gz) · [Versionshinweise](https://github.com/visiongaiatechnology/rythmguard/releases/tag/v1.4.1-linux-beta.1) |

> Windows: Per-User-Installer, keine Administratorrechte nötig. Linux: AppImage ist der einfachste Weg (chmod +x und starten). Alle Releases: [https://github.com/visiongaiatechnology/rythmguard/releases](https://github.com/visiongaiatechnology/rythmguard/releases).
## Worum es geht

Lange Arbeitssessions, ein PC, der tagelang läuft, und Bildschirme, die nachts an bleiben,
zerstören den Schlaf. RythmGuard gibt Windows einen festen Rahmen:

- **Erzwungene Pausen** blockieren den Bildschirm tagsüber für eine konfigurierbare Zeit.
- **Abendmodus** erinnert ab 23:00 alle 20 Minuten daran, nicht weiterzuarbeiten.
- **Bett-Modus** gibt ein konfigurierbares Fenster (Standard: 2 Std) und zwingt den PC
  danach in Ruhezustand oder Herunterfahren.
- **Komplett lokal:** kein Konto, keine Cloud, keine Telemetrie.

## Funktionen

| Modus | Standardfenster | Verhalten |
|---|---|---|
| **Fokus** | 07:00 – 23:00 | Arbeit in Zyklen (120 Min), danach |
| **Pause** | alle 120 Min, 30 Min lang | Vollbild-Glas-Overlay auf allen Monitoren |
| **Abend** | 23:00 – 07:00 | ruhige Erinnerung alle 20 Min |
| **Bett** | manuell / Abend-Erinnerung | Fenster zum Anschauen, dann Energie-Aktion |

- Aurora-Glass-Dashboard mit Live-Status, Tagesrhythmus-Chart, Score-Ring und Verlauf
- **7 Sprachen** in der Oberfläche (Deutsch, Englisch, Spanisch, Italienisch, Russisch, Chinesisch, Japanisch)
- **Notausstieg:** `ESC` 5 Sekunden halten, dann `PAUSE` tippen (abschaltbar)
- Energie-Aktionen mit Warnungen bei 5 Min, 1 Min und 60-Sekunden-Countdown
- Läuft im Tray; keine Administratorrechte nötig

## Screenshots

| Sperrbildschirm | Abend-Erinnerung |
|---|---|
| ![Pause](docs/screenshots/01-break-overlay.png) | ![Abend](docs/screenshots/03-evening-reminder.png) |

Weitere Bilder: [`docs/screenshots/`](docs/screenshots/) · Design: [`docs/DESIGN.md`](docs/DESIGN.md)

## Installation

```powershell
# App bauen (erzeugt dist\VGT.RhythmGuard.exe)
powershell -ExecutionPolicy Bypass -File scripts\publish.ps1

# Optional: Inno-Setup-Installer kompilieren (Inno Setup 6+ erforderlich)
#   installer\VGT-RhythmGuard.iss mit ISCC.exe bauen

# Optional: Autostart für den aktuellen Benutzer einrichten (ohne Admin)
powershell -ExecutionPolicy Bypass -File scripts\install-autostart.ps1 -StartNow
```

Der Installer installiert nach `%LOCALAPPDATA%\Programs\VGT RythmGuard` (per-user, kein
Administrator nötig) und bietet optional Autostart an. Lokal gebaute EXEs sind nicht signiert —
SmartScreen einmalig über *Weitere Informationen → Trotzdem ausführen* bestätigen.

## Aus dem Quellcode bauen

```powershell
dotnet restore VGT.RhythmGuard.slnx
dotnet build VGT.RhythmGuard.slnx -c Release
dotnet run --project tools\VGT.RhythmGuard.FactRunner -c Release
dotnet test tests\VGT.RhythmGuard.Core.Tests\VGT.RhythmGuard.Core.Tests.csproj
```

## Architektur, Tests, Sicherheit

- Architektur: [`ARCHITECTURE.md`](ARCHITECTURE.md)
- Tests: Core-Fakten (32) + xUnit, ausgeführt in CI ([`.github/workflows/build.yml`](.github/workflows/build.yml))
- Sicherheit: keine Netzwerkverbindung, keine Telemetrie; siehe [`SECURITY.md`](SECURITY.md)
- Beiträge: [`CONTRIBUTING.md`](CONTRIBUTING.md) · Änderungen: [`CHANGELOG.md`](CHANGELOG.md)

## Lizenz

GNU Affero General Public License v3.0 — siehe [`LICENSE`](LICENSE).
Drittanbieter und Herkunft der Assets: [`THIRD_PARTY_NOTICES.md`](THIRD_PARTY_NOTICES.md).

---

<p align="center">
  <sub>Powered by <a href="https://visiongaiatechnology.de">VisionGaiaTechnology</a></sub>
</p>
