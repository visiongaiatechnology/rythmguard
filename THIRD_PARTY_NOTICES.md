# Third-Party Notices

VGT Rhythm Guard is licensed under **AGPL-3.0** (see `LICENSE`).

## Runtime & frameworks

| Component | License | Usage |
|---|---|---|
| .NET 10 / Windows Desktop Runtime | MIT | application runtime (self-contained publish) |
| Windows API / WPF / WinForms | Microsoft licensing terms | operating-system integration |

## Build & test dependencies (NuGet)

| Package | License | Usage |
|---|---|---|
| xunit (xunit.core, xunit.assert) | Apache-2.0 | unit testing |
| xunit.runner.visualstudio | Apache-2.0 | test adapter |
| Microsoft.NET.Test.Sdk | MIT | test host |
| coverlet.collector | MIT | coverage collector |

Dependency versions are pinned in `tests/VGT.RhythmGuard.Core.Tests/VGT.RhythmGuard.Core.Tests.csproj`.

## Build tools

| Tool | License | Usage |
|---|---|---|
| Inno Setup 6 | Inno Setup License (free) | optional Windows installer build (`installer/VGT-RhythmGuard.iss`) |

Inno Setup is **not redistributed** with this repository; it is only required to compile the
installer locally.

## Assets

- `logo-mark.png`, `logo-wordmark.png`, app icon and installer wizard images are original
  VisionGaiaTechnology brand assets for this project.
- Screenshots in `docs/screenshots/` were produced from the running application.
- No third-party stock photos, fonts or icon packs are bundled. UI uses system fonts
  (Segoe UI family).

If you believe an asset or dependency is missing from this notice, please open an issue
before redistribution.
