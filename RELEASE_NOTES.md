# Release Notes — VGT Rhythm Guard 1.2.0 (Beta)

**This is a beta release for evaluation and personal use.**

## Highlights
- Forced breaks, evening wind-down mode and bed mode with sleep/hibernate/shutdown
- Aurora Glass dashboard, tray app, per-user autostart (optional)
- 7-language UI: English, Deutsch, Español, Italiano, Русский, 中文, 日本語
- Branded English Inno Setup installer (per-user, no administrator rights)
- Terms of Use / Beta Notice / Copyright included in the installer

## Installer
- File: `VGT-RhythmGuard-Setup-1.2.0-Beta.exe`
- SHA256: `5B4BB6F015ED35215CD39DCE157C695DF96F2F83594BE82176C5AF0623C05243`
- Unsigned build: Windows SmartScreen may ask once (More info → Run anyway)

## Quality evidence
- Build: `dotnet build VGT.RhythmGuard.slnx -c Release` — PASS
- Core facts: FactRunner — 32/32 PASS
- xUnit: executed in CI (see Actions); not executed locally due to restricted testhost
- CI: https://github.com/visiongaiatechnology/rythmguard/actions

## Notes
- No network access, no telemetry, local data only.
- Save your work before testing forced power actions.
- AGPL-3.0 license; source code in this repository.
