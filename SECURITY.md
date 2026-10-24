# Security Policy

## Supported versions

| Version | Supported |
|---|---|
| 1.2.x | yes |
| < 1.2 | no |

## Reporting a vulnerability

Please report security issues **privately** using GitHub's
[private vulnerability reporting](https://github.com/visiongaiatechnology/rythmguard/security/advisories/new)
or by contacting VisionGaiaTechnology via <https://visiongaiatechnology.de>.

Do not open a public issue for an unpatched vulnerability. Please include:

- affected version and commit,
- a description of the issue and its impact,
- reproduction steps or a proof of concept,
- any suggested mitigation.

We aim to acknowledge reports within 5 business days and to coordinate disclosure
after a fix is available.

## Security model & scope

Rhythm Guard is a **wellness and self-control tool, not an operating-system security
boundary**. By design:

- `Ctrl+Alt+Del`, Task Manager, sign-out and shutdown remain available.
- The app runs without administrator rights; autostart uses `HKCU`.
- No network access, no telemetry and no cloud account are used.
- All data is stored locally in `%LOCALAPPDATA%\VGT\RhythmGuard`.
- Power actions are executed through standard Windows mechanisms after explicit
  in-app warnings (5 min, 1 min, 60 s countdown).
- The emergency exit requires holding `ESC` and typing `PAUSE`; it can be disabled
  for stricter self-control.

## Builds and signatures

Locally built executables and installers are currently **not code-signed**. Windows
SmartScreen may ask for confirmation once. For public distribution, code signing is
recommended. Never disable SmartScreen globally just to run an unsigned build.

## Secrets

This repository contains no credentials, tokens or private keys. Configuration files
are local-only and ignored by Git.
