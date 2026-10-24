# Architecture

## Overview

```
┌─────────────────────────────┐
│ VGT.RhythmGuard.App (WPF)   │
│  tray · overlays · dashboard │
│  coordinator · localization  │
│  JSON stores · power service │
└──────────────┬──────────────┘
               │ evaluates with wall-clock time + settings + bed session
┌──────────────▼──────────────┐
│ VGT.RhythmGuard.Core        │
│  RhythmSettings (validated)  │
│  RhythmEngine (stateless)    │
│  RhythmSnapshot / events     │
│  statistics · exceptions     │
└─────────────────────────────┘
```

## Core

`RhythmEngine.Evaluate(DateTime localNow, RhythmSettings settings, BedSession? bed)`
returns an immutable `RhythmSnapshot`:

- `Focus` / `Break` inside the day window with cycle number, phase progress and next break.
- `Evening` outside the day window with the next 20-minute reminder slot.
- `Bed` / `BedExpired` overrides everything while a bed session is active; expiry exposes
  the pending power action.

Scheduling rules are deterministic and unit-tested (32 facts): day anchor 07:00,
cycles of 120/30 minutes, truncated trailing focus phase, evening grid 23:00–07:00,
bed-mode override with expiry.

`RhythmSettings.Validate()` rejects invalid ranges and unknown values (typed
`RhythmValidationException`). `StatisticsCalculator` aggregates events into day/week
counters and a streak.

## App

- **RhythmCoordinator** polls once per second, maps snapshots to UI state, logs events and
  persists the runtime state atomically.
- **OverlayService** owns fullscreen overlays per monitor, the evening reminder, bed
  countdown/warning windows and the shutdown warning. It is abstracted by `IRhythmUi`.
- **SettingsWindow** is the dashboard shell (overview, settings, history) with the Aurora
  Glass design system.
- **Persistence**: `JsonConfigStore` and `JsonStateStore` write atomically (temp + move);
  `EventLog` appends JSONL and prunes after 90 days.
- **Windows integration**: `WindowsPowerService` (sleep/hibernate/shutdown),
  `AutostartService` (`HKCU\...\Run`), `SingleInstanceGuard` (local mutex).
- **Localization**: `Localizer` maps German source strings to de/en/es/it/ru/zh/ja and
  translates window trees after loading; the dashboard language selector persists
  `UiLanguage` in settings.

## Testing

- `tests/VGT.RhythmGuard.Core.Tests` — xUnit facts for engine boundaries, settings
  validation and statistics.
- `tools/VGT.RhythmGuard.FactRunner` — executes the same facts via reflection when the
  VSTest testhost cannot run (e.g., restricted sandboxes). CI runs both.

## Data locations

- Config/state/events/log: `%LOCALAPPDATA%\VGT\RhythmGuard`.
- Installer target (optional): `%LOCALAPPDATA%\Programs\VGT Rhythm Guard`.
- Autostart: `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` value `VGT.RhythmGuard`.
