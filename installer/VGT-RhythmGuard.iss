; VGT Rhythm Guard — Inno Setup 6 Script
; Per-User-Installation ohne Administratorrechte, inkl. optionaler Autostart-Integration.

#define MyAppName "VGT Rhythm Guard"
#define MyAppVersion "1.2.0"
#define MyAppPublisher "VisionGaiaTechnology"
#define MyAppExeName "VGT.RhythmGuard.exe"

[Setup]
AppId={{B7C4E2A1-3F5D-4A8B-9C2E-1D6F0A7B3C5D}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
VersionInfoVersion=1.2.0.0
VersionInfoProductName={#MyAppName}
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription=Mental-Health-Rhythmus-System für Windows
DefaultDirName={localappdata}\Programs\VGT Rhythm Guard
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputDir=..\dist-installer
OutputBaseFilename=VGT-RhythmGuard-Setup-{#MyAppVersion}
SetupIconFile=..\src\VGT.RhythmGuard.App\assets\app.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
WizardStyle=modern dark
WizardImageFile=assets\wizard-large.png
WizardSmallImageFile=assets\wizard-small.png
WizardImageStretch=yes
WizardImageBackColor=$1A1240
WizardSizePercent=115
Compression=lzma2/max
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
CloseApplications=yes
RestartApplications=no
InfoBeforeFile=..\installer\INSTALL_HINWEISE.txt
MinVersion=10.0.17763

[Languages]
Name: "german"; MessagesFile: "compiler:Languages\German.isl"

[Messages]
WelcomeLabel1=Willkommen bei VGT Rhythm Guard
WelcomeLabel2=Dieses Setup installiert dein Mental-Health-Rhythmus-System für Windows:%n%nPausen am Tag, Abendmodus und Bett-Modus – als fester Bestandteil deines Alltags.%n%nDie Installation erfolgt ausschließlich für deinen Benutzer, ohne Administratorrechte und ohne Systemveränderungen.
FinishedHeadingLabel=Dein Rhythmus ist bereit.
FinishedLabel=VGT Rhythm Guard wurde erfolgreich installiert.%n%nStarte die App über das Startmenü oder die Desktop-Verknüpfung. Den Autostart aktivierst du bewusst selbst – in den Einstellungen der App oder über die Setup-Option.
SetupWindowTitle=VGT Rhythm Guard – Setup

[Tasks]
Name: "desktopicon"; Description: "Desktop-Verknüpfung erstellen"; GroupDescription: "Zusätzliche Verknüpfungen:"; Flags: unchecked
Name: "autostart"; Description: "Mit Windows starten (empfohlen für den festen Rhythmus)"; GroupDescription: "Systemintegration:"; Flags: unchecked

[Files]
Source: "..\dist-final\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\README.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\docs\DESIGN.md"; DestDir: "{app}\docs"; Flags: ignoreversion

[Icons]
Name: "{group}\VGT Rhythm Guard"; Filename: "{app}\{#MyAppExeName}"; Parameters: "--settings"; IconFilename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\VGT Rhythm Guard"; Filename: "{app}\{#MyAppExeName}"; Parameters: "--settings"; Tasks: desktopicon; IconFilename: "{app}\{#MyAppExeName}"

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "VGT.RhythmGuard"; ValueData: """{app}\{#MyAppExeName}"" --logon"; Flags: uninsdeletevalue; Tasks: autostart

[Run]
Filename: "{app}\{#MyAppExeName}"; Parameters: "--settings"; Description: "VGT Rhythm Guard jetzt starten"; Flags: nowait postinstall skipifsilent
