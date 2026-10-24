# Contributing to VGT Rhythm Guard

Thanks for your interest! This project focuses on a **reliable, local-first rhythm system**
for Windows — quality over feature count.

## Before you start

- Open an issue for larger features so the design can be discussed first.
- Keep changes focused; avoid unrelated refactors in the same pull request.
- Do not add telemetry, network calls or cloud dependencies.

## Development setup

Requirements: Windows 10/11 x64, .NET SDK 10, PowerShell 5.1+.

```powershell
dotnet restore VGT.RhythmGuard.slnx
dotnet build VGT.RhythmGuard.slnx -c Release
dotnet run --project tools\VGT.RhythmGuard.FactRunner -c Release
dotnet test tests\VGT.RhythmGuard.Core.Tests\VGT.RhythmGuard.Core.Tests.csproj
```

## Architecture rules

- `VGT.RhythmGuard.Core` must stay **pure**: no Windows, WPF or file I/O dependencies.
- New scheduling behavior belongs in the stateless `RhythmEngine` and needs unit tests.
- UI code lives in `VGT.RhythmGuard.App`; do not put business rules into code-behind.
- Use typed exceptions from the Core exception hierarchy.
- No placeholder or dummy implementations.

## Style

- C# latest, nullable enabled, warnings are treated as errors.
- German UI source strings are translated through `Localization/Localizer.cs`; add new
  visible strings there for all supported languages (de/en/es/it/ru/zh/ja).
- Keep accessibility in mind: readable contrast, keyboard access, no color-only state.

## Pull requests

1. Fork the repository and create a topic branch.
2. Add tests for behavior changes.
3. Run the commands above and include results in the pull request.
4. Describe user-visible changes and update `CHANGELOG.md`.
