; VGT RythmGuard — Inno Setup 6 (English, Beta, branded, per-user)
#define MyAppName "VGT RythmGuard"
#define MyAppVersion "1.4.0"
#define MyAppPublisher "VisionGaiaTechnology"
#define MyAppExeName "VGT.RhythmGuard.exe"
#define MyAppUrl "https://visiongaiatechnology.de"
#define MyRepoUrl "https://github.com/visiongaiatechnology/rythmguard"

[Setup]
AppId={{B7C4E2A1-3F5D-4A8B-9C2E-1D6F0A7B3C5D}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion} (Beta)
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppUrl}
AppSupportURL={#MyRepoUrl}/issues
AppUpdatesURL={#MyRepoUrl}/releases
AppCopyright=Copyright (C) 2026 VisionGaiaTechnology
VersionInfoVersion=1.4.0.0
VersionInfoProductName={#MyAppName} {#MyAppVersion} Beta
VersionInfoCompany={#MyAppPublisher}
VersionInfoCopyright=Copyright (C) 2026 VisionGaiaTechnology
VersionInfoDescription=Mental health rhythm system for Windows — Beta setup
DefaultDirName={localappdata}\Programs\VGT RythmGuard
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputDir=..\dist-installer
OutputBaseFilename=VGT-RythmGuard-Setup-{#MyAppVersion}-Beta
SetupIconFile=..\src\VGT.RhythmGuard.App\assets\app.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
WizardStyle=modern dark
WizardImageFile=assets\wizard-large.png
WizardSmallImageFile=assets\wizard-small.png
WizardImageStretch=yes
WizardImageBackColor=$1A1240
WizardSizePercent=115
LicenseFile=..\installer\TERMS.txt
InfoBeforeFile=..\installer\INSTALL_HINWEISE.txt
Compression=lzma2/max
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
CloseApplications=yes
RestartApplications=no
MinVersion=10.0.17763

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Messages]
WelcomeLabel1=Welcome to the VGT RythmGuard Beta Setup
WelcomeLabel2=Protect Your Rhythm. Protect Yourself.%n%nThis wizard installs VGT RythmGuard {#MyAppVersion} (Beta) for your Windows user account.%n%nRythmGuard anchors forced breaks, an evening wind-down mode and a bed-time power action directly into your Windows session.%n%nThis is a BETA build. No administrator rights are required and no system-wide files are modified.
LicenseLabel=Please read and accept the Terms of Use, Beta Notice and Copyright before continuing.
LicenseLabel3=Terms of Use, Beta Notice and Copyright
FinishedHeadingLabel=Your rhythm is ready — beta installed.
FinishedLabel=VGT RythmGuard {#MyAppVersion} (Beta) has been installed.%n%nStart it from the Start menu, the optional desktop shortcut, or the final page of this setup. Autostart is optional and can be changed at any time in Settings -> System.%n%nThis is beta software. Save your work before testing forced breaks or power actions.
SetupWindowTitle=VGT RythmGuard {#MyAppVersion} Beta — Setup

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked
Name: "autostart"; Description: "Start RythmGuard with Windows (recommended for a fixed rhythm)"; GroupDescription: "System integration:"; Flags: unchecked

[Files]
Source: "..\dist-final\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\README.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\docs\DESIGN.md"; DestDir: "{app}\docs"; Flags: ignoreversion
Source: "..\installer\TERMS.txt"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\VGT RythmGuard"; Filename: "{app}\{#MyAppExeName}"; Parameters: "--settings"; IconFilename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\VGT RythmGuard"; Filename: "{app}\{#MyAppExeName}"; Parameters: "--settings"; Tasks: desktopicon; IconFilename: "{app}\{#MyAppExeName}"

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "VGT.RhythmGuard"; ValueData: """{app}\{#MyAppExeName}"" --logon"; Flags: uninsdeletevalue; Tasks: autostart

[Run]
Filename: "{app}\{#MyAppExeName}"; Parameters: "--settings"; Description: "Start VGT RythmGuard now"; Flags: nowait postinstall skipifsilent
