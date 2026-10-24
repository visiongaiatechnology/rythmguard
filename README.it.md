# VGT Rhythm Guard

<p align="center">
  <img src="docs/branding/logo-wordmark.png" alt="VGT Rhythm Guard" width="460" />
</p>

<p align="center">
  <strong>Un sistema di ritmo per la salute mentale che integra pause, serate e sonno direttamente nella sessione di Windows.</strong>
</p>

<p align="center">
  <a href="README.md">English</a> · <a href="README.de.md">Deutsch</a> · <a href="README.es.md">Español</a> ·
  <strong>Italiano</strong> · <a href="README.ru.md">Русский</a> · <a href="README.zh.md">中文</a> · <a href="README.ja.md">日本語</a>
</p>

![Dashboard](docs/screenshots/06-dashboard-overview.png)

## ⬇️ Download

| Piattaforma | Versione | Download |
|---|---|---|
| 🪟 **Windows 10/11 x64** | **1.2.0 Beta** | [**Installatore Windows**](https://github.com/visiongaiatechnology/rythmguard/releases/download/v1.2.0-beta.1/VGT-RhythmGuard-Setup-1.2.0-Beta.exe) · [Note di rilascio](https://github.com/visiongaiatechnology/rythmguard/releases/tag/v1.2.0-beta.1) |
| 🐧 **Linux x64** | **1.3.1 Beta** | [**AppImage (consigliato)**](https://github.com/visiongaiatechnology/rythmguard/releases/download/v1.3.1-linux-beta.1/VGT-RhythmGuard-1.3.1-linux-x86_64.AppImage) · [Archivio tar](https://github.com/visiongaiatechnology/rythmguard/releases/download/v1.3.1-linux-beta.1/VGT-RhythmGuard-1.3.1-linux-x64.tar.gz) · [Note di rilascio](https://github.com/visiongaiatechnology/rythmguard/releases/tag/v1.3.1-linux-beta.1) |

> Windows: installazione per utente, senza diritti di amministratore. Linux: AppImage è il modo più semplice (chmod +x ed eseguire). Tutte le release: [https://github.com/visiongaiatechnology/rythmguard/releases](https://github.com/visiongaiatechnology/rythmguard/releases).
## Perché Rhythm Guard

Sessioni di lavoro lunghe, PC accesi per giorni e schermi illuminati di notte rovinano il sonno.
Rhythm Guard trasforma Windows in una cornice fissa:

- **Pause obbligatorie** bloccano lo schermo durante il giorno.
- **Modalità serale** ricorda ogni 20 minuti di non continuare a lavorare dopo le 23:00.
- **Modalità letto** concede un intervallo configurabile (2 h di default) e poi forza sospensione o spegnimento.
- **Tutto locale:** nessun account, nessun cloud, nessuna telemetria.

## Funzioni

| Modalità | Finestra | Comportamento |
|---|---|---|
| **Focus** | 07:00 – 23:00 | lavoro a cicli (120 min), poi |
| **Pausa** | ogni 120 min, 30 min | overlay di vetro a schermo intero |
| **Serale** | 23:00 – 07:00 | promemoria tranquillo ogni 20 min |
| **Letto** | manuale / promemoria | tempo per guardare qualcosa, poi azione energia |

- Dashboard Aurora Glass con stato live, grafico del ritmo, punteggio e cronologia
- **7 lingue** dell'interfaccia
- **Uscita di emergenza:** tieni `ESC` 5 secondi e digita `PAUSE` (disattivabile)
- Azioni energia con avvisi a 5 min, 1 min e conto alla rovescia di 60 s
- Vive nella tray; nessun permesso di amministratore

## Screenshot

| Blocco schermo | Promemoria serale |
|---|---|
| ![Pausa](docs/screenshots/01-break-overlay.png) | ![Sera](docs/screenshots/03-evening-reminder.png) |

Altri: [`docs/screenshots/`](docs/screenshots/) · Design: [`docs/DESIGN.md`](docs/DESIGN.md)

## Installazione

```powershell
powershell -ExecutionPolicy Bypass -File scripts\publish.ps1
# Opzionale: compilare l'installer Inno Setup (installer\VGT-RhythmGuard.iss)
powershell -ExecutionPolicy Bypass -File scripts\install-autostart.ps1 -StartNow
```

L'installer è per utente (`%LOCALAPPDATA%\Programs\VGT Rhythm Guard`), senza admin.
Gli eseguibili locali non sono firmati: conferma SmartScreen una volta
(*Ulteriori informazioni → Esegui comunque*).

## Compilare dai sorgenti

```powershell
dotnet restore VGT.RhythmGuard.slnx
dotnet build VGT.RhythmGuard.slnx -c Release
dotnet run --project tools\VGT.RhythmGuard.FactRunner -c Release
dotnet test tests\VGT.RhythmGuard.Core.Tests\VGT.RhythmGuard.Core.Tests.csproj
```

## Link

- Architettura: [`ARCHITECTURE.md`](ARCHITECTURE.md)
- Test: 32 fatti del core + xUnit, eseguiti in CI
- Sicurezza: [`SECURITY.md`](SECURITY.md) · Contributi: [`CONTRIBUTING.md`](CONTRIBUTING.md) · Modifiche: [`CHANGELOG.md`](CHANGELOG.md)

## Licenza

GNU Affero General Public License v3.0 — vedi [`LICENSE`](LICENSE).
Note di terze parti: [`THIRD_PARTY_NOTICES.md`](THIRD_PARTY_NOTICES.md).

---

<p align="center">
  <sub>Powered by <a href="https://visiongaiatechnology.de">VisionGaiaTechnology</a></sub>
</p>
