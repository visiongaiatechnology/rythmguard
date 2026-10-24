# VGT RythmGuard 1.4.1 (Beta) — Windows

**Protect Your Rhythm. Protect Yourself.**

Maintenance release of the brand-aligned Windows build: forced breaks, evening wind-down mode and
bed mode with sleep/hibernate/shutdown — plus the Aurora Glass dashboard, 7 languages,
profile-name personalization and a per-user beta installer.

## Fixed
- **Evening mode never showed a reminder.** The rhythm engine exposed only the *next* evening
  slot, which is always strictly in the future, while the coordinator waited for `now >= due`.
  That slot never arrived, so the glass reminder window never appeared. The snapshot now also
  exposes the *due* slot (`DueEveningReminder`); the reminder is shown immediately when the
  evening mode starts (e.g. 21:00 with a custom day end) and afterwards every configured
  interval (default: 20 minutes). Windows and Linux coordinators use the corrected logic.
- Regression coverage added (39 core facts): due slot is never in the future, the old coordinator
  condition provably never triggers, and a tick-loop simulation shows reminders at 23:00/23:20/23:40.

## Installer
- File: `VGT-RythmGuard-Setup-1.4.1-Beta.exe`
- Upgrades an existing per-user install in place (same app id, same data folder) — quit a running
  RythmGuard instance from the tray before running the setup
- Per-user install (`%LOCALAPPDATA%\Programs\VGT RythmGuard`) — no administrator rights
- English branded setup with Terms of Use / Beta Notice / Copyright, optional desktop shortcut
  and optional autostart
- Unsigned build: Windows SmartScreen may ask once (More info → Run anyway)

## Quality
- Windows build: PASS (0 errors)
- Core facts: 39/39 PASS
- Linux sibling release 1.4.1: selftest PASS, AppImage selftest PASS

**Beta software** — save your work before testing forced breaks or power actions.
AGPL-3.0. Source: https://github.com/visiongaiatechnology/rythmguard
