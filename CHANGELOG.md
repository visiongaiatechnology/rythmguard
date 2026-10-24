# Changelog

All notable changes to VGT Rhythm Guard are documented here.
Format: [Keep a Changelog](https://keepachangelog.com/), versioning: [SemVer](https://semver.org/).

## [1.2.0] — 2026-10-24

### Added
- UI localization for German, English, Spanish, Italian, Russian, Chinese and Japanese.
- Language selection in Settings → System.
- Dynamic Windows profile name in the dashboard sidebar.
- “Powered by VisionGaiaTechnology” branding with link to https://visiongaiatechnology.de.
- Per-user Inno Setup installer with Aurora Glass styling, dark-mode wizard and optional
  desktop shortcut / autostart tasks.
- Splash screen with the RhythmGuard wordmark and animated progress line.
- Branded app icon and tray icon.

### Changed
- Break overlay and dashboard now use the RhythmGuard logo/wordmark.
- Window drag uses native Windows caption handling for frameless and elevated windows.
- Version raised to 1.2.0 across app, splash and installer.

### Fixed
- Startup crashes now produce a visible error dialog and an entry in `app.log`.
- Slider initialization could throw before all UI elements existed.
- Pop-ups are dark glass surfaces and readable on any desktop background.
- Footer overlapping content in the dashboard settings page.

## [1.1.0] — 2026-10-24

### Added
- Aurora Glass dashboard (overview, settings, history) with live status, daily rhythm
  chart, rhythm score, recent events and system status.
- Core scheduler: 07:00–23:00 day window, 120/30 cycles, truncated final focus phase,
  evening reminder grid from 23:00, bed-mode override.
- Event history (`events.jsonl`, 90-day retention) and statistics incl. streak.
- Localized workflow support tools and testhost-free fact runner.

## [1.0.0] — 2026-10-24

### Added
- Initial rhythm system: forced breaks, evening reminders, bed mode with power actions
  (sleep, hibernate, gentle/hard shutdown), emergency exit, tray application,
  local configuration, autostart.
