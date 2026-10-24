# VGT Rhythm Guard — Linux

1:1 feature parity with the Windows version, using **Avalonia UI** and native Linux integration.

## Status

**Beta** — the Linux port builds, runs under WSLg/X11 and passes its UI selftest; it is not yet
tested on every distribution/compositor. Treat power actions and forced blocking as beta features.

## Features

- Forced breaks (fullscreen Aurora Glass overlay, configurable 120/30 min cycle)
- Evening mode (23:00–07:00, reminder every 20 min)
- Bed mode with configurable window and `systemctl suspend/hibernate/poweroff`
- Aurora Glass dashboard: live status, KPI cards, daily rhythm chart, rhythm score, events, history
- 7 languages (de/en/es/it/ru/zh/ja)
- XDG autostart (`~/.config/autostart/vgt-rhythmg-ard.desktop`)
- Local-only data in `$XDG_DATA_HOME/vgt-rhythmg-ard` (fallback `~/.local/share/...`)
- Single-instance protection via file lock
- Tray icon (StatusNotifier/AppIndicator where available; graceful fallback)

## Build

Requirements: .NET SDK 10, Linux x64 (X11 or Wayland; X11 recommended for full blocking).

```bash
dotnet restore VGT.RhythmGuard.slnx
dotnet build src/VGT.RhythmGuard.App.Linux/VGT.RhythmGuard.App.Linux.csproj -c Release

# UI selftest (constructs dashboard + all overlays)
dotnet run --project src/VGT.RhythmGuard.App.Linux/VGT.RhythmGuard.App.Linux.csproj -c Release -- --selftest

# Dashboard
dotnet run --project src/VGT.RhythmGuard.App.Linux/VGT.RhythmGuard.App.Linux.csproj -c Release -- --dashboard
```

## Package

```bash
bash scripts/publish-linux.sh      # dist-linux/VGT-RhythmGuard-1.3.1-linux-x64.tar.gz
bash scripts/build-appimage.sh     # dist-linux/VGT-RhythmGuard-1.3.1-linux-x86_64.AppImage
```

CI: `.github/workflows/linux.yml` (build + fact suite + Xvfb UI selftest) and
`.github/workflows/linux-release.yml` (tag `v*-linux*` → GitHub release with tarball/AppImage).

## Limitations (beta)

- **Wayland blocking:** a normal application cannot globally block the whole desktop unless the
  compositor supports it. Full blocking works on X11; on Wayland the overlay is a fullscreen
  topmost window (workspaces/shortcuts may still escape). Use X11 for strict self-control.
- **Tray:** StatusNotifier/AppIndicator required; GNOME needs an extension. Without tray the app
  runs via autostart/dashboard.
- **Power actions:** `systemctl` may ask for polkit authentication depending on your system policy.
- Data formats are compatible with the Windows version (`config.json`, `state.json`, `events.jsonl`).
