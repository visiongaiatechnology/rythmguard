# VGT Rhythm Guard

<p align="center">
  <img src="docs/branding/logo-wordmark.png" alt="VGT Rhythm Guard" width="480" />
</p>

<p align="center">
  <strong>Un sistema de ritmo para la salud mental que ancla pausas, tardes y sueño directamente en tu sesión de Windows.</strong>
</p>

<p align="center">
  <a href="README.md">English</a> ·
  <a href="README.de.md">Deutsch</a> ·
  <strong>Español</strong> ·
  <a href="README.it.md">Italiano</a> ·
  <a href="README.ru.md">Русский</a> ·
  <a href="README.zh.md">中文</a> ·
  <a href="README.ja.md">日本語</a>
</p>

![Dashboard](docs/screenshots/06-dashboard-overview.png)

## Por qué Rhythm Guard

Sesiones largas, un PC encendido durante días y pantallas que siguen iluminadas por la noche
desgastan el sueño. Rhythm Guard convierte Windows en un marco fijo:

- **Pausas obligatorias** bloquean la pantalla durante el día.
- **Modo noche** recuerda cada 20 minutos que no sigas trabajando después de las 23:00.
- **Modo cama** concede un tiempo configurable (2 h por defecto) y luego fuerza suspensión o apagado.
- **100 % local:** sin cuenta, sin nube, sin telemetría.

## Funciones

| Modo | Ventana | Comportamiento |
|---|---|---|
| **Enfoque** | 07:00 – 23:00 | trabajo en ciclos (120 min), después |
| **Pausa** | cada 120 min, 30 min | superposición de cristal a pantalla completa |
| **Noche** | 23:00 – 07:00 | recordatorio tranquilo cada 20 min |
| **Cama** | manual / recordatorio | tiempo para ver algo y luego acción de energía |

- Panel Aurora Glass con estado en vivo, gráfico del ritmo diario y puntuación
- **7 idiomas** de interfaz
- **Salida de emergencia:** mantener `ESC` 5 segundos y escribir `PAUSE` (desactivable)
- Acciones de energía con avisos a los 5 min, 1 min y cuenta atrás de 60 s
- Vive en la bandeja del sistema; no requiere administrador

## Capturas

| Pantalla de bloqueo | Recordatorio nocturno |
|---|---|
| ![Pausa](docs/screenshots/01-break-overlay.png) | ![Noche](docs/screenshots/03-evening-reminder.png) |

Más: [`docs/screenshots/`](docs/screenshots/) · Diseño: [`docs/DESIGN.md`](docs/DESIGN.md)

## Instalación

```powershell
powershell -ExecutionPolicy Bypass -File scripts\publish.ps1
# Opcional: compilar el instalador Inno Setup (installer\VGT-RhythmGuard.iss)
powershell -ExecutionPolicy Bypass -File scripts\install-autostart.ps1 -StartNow
```

El instalador es por usuario (`%LOCALAPPDATA%\Programs\VGT Rhythm Guard`) y no requiere
administrador. Las EXE locales no están firmadas: acepta SmartScreen una vez
(*Más información → Ejecutar de todas formas*).

## Compilar desde el código

```powershell
dotnet restore VGT.RhythmGuard.slnx
dotnet build VGT.RhythmGuard.slnx -c Release
dotnet run --project tools\VGT.RhythmGuard.FactRunner -c Release
dotnet test tests\VGT.RhythmGuard.Core.Tests\VGT.RhythmGuard.Core.Tests.csproj
```

## Enlaces

- Arquitectura: [`ARCHITECTURE.md`](ARCHITECTURE.md)
- Pruebas: 32 hechos del núcleo + xUnit, ejecutados en CI
- Seguridad: [`SECURITY.md`](SECURITY.md) · Contribuir: [`CONTRIBUTING.md`](CONTRIBUTING.md) · Cambios: [`CHANGELOG.md`](CHANGELOG.md)

## Licencia

GNU Affero General Public License v3.0 — ver [`LICENSE`](LICENSE).
Avisos de terceros: [`THIRD_PARTY_NOTICES.md`](THIRD_PARTY_NOTICES.md).

---

<p align="center">
  <sub>Powered by <a href="https://visiongaiatechnology.de">VisionGaiaTechnology</a></sub>
</p>
