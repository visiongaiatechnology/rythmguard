# Design: Aurora Glass

VGT RhythmGuard folgt der GLASS-2100-Designsprache, weiterentwickelt zu **Aurora Glass**:
tiefe Indigo-Nacht, weiche Aurora-Lichter, Glasflächen als Material — keine Deko-Raster,
keine generischen Dashboard-Bausteine ohne Produktbezug.

## Tokens

| Token | Wert | Verwendung |
|---|---|---|
| `Color.BackdropTop/Mid/Bottom` | `#151033` → `#1E1650` → `#241B5E` | Grundverlauf aller Flächen |
| `Color.AuroraViolet` | `#7C5CFF` | Primärer Akzent, Chart-Fokus |
| `Color.AuroraBlue` | `#4CC9F0` | Sekundärer Akzent, Chart-Verlauf |
| `Color.AuroraPink` | `#F72585` | Bett-Modus, Warnungen |
| `Color.AuroraTeal` | `#4FE3C1` | Erfolg, Pausen, Aktiv-Status |
| `Color.Glass` | `#12FFFFFF` | Standard-Glasfläche |
| `Color.GlassStrong` | `#1FFFFFFF` | Hover/aktive Flächen |
| `Color.GlassBorder` | `#30FFFFFF` | 1-px-Kanten |
| `Color.Text` / `TextSecondary` / `TextMuted` | `#F4F2FF` / `#B9B3DC` / `#847DAB` | Typografie-Hierarchie |

Typografie: **Segoe UI Variable Display** für Zahlen/Überschriften, **Segoe UI Variable Text**
für Fließtext. Radien: 26 px Shell, 18–24 px Karten, 12–16 px Controls.

## Aurora-Hintergrund

Vier radiale Glows (Violett, Blau, Pink, Teal) auf dem Indigo-Verlauf erzeugen Tiefe,
ohne Inhalte zu konkurrenzieren. Kein Muster, kein Rauschen, keine dekorativen Linien.

## Komponenten

- **Shell**: Sidebar (Navigation, Live-Modus, Nutzer) + Main (Titelzeile, Seiten).
- **KPI-Karten**: Icon-Kreis, Label, große Zahl (Light 36), Kontextzeile, optionaler Fortschritt.
- **Tagesrhythmus-Chart**: Flächenkurve mit Fokus-Plateaus und Pausen-Tälern, Teal-Bänder
  für Pausenfenster, „JETZT“-Marker mit Glow, Achsenbeschriftung aus dem konfigurierten Tagesfenster.
- **Tagesziel-Ring**: Gradient-Bogen (Violett → Blau → Teal), Score in der Mitte.
- **Pop-ups**: dunkle Glasflächen (`#EF150F33`) mit Aurora-Kanten — lesbar auf jedem Desktop.
- **Pausen-Overlay**: geblurrter Desktop + Abdunklung + zentrale Glaskarte, ruhige
  Countdown-Typografie, Vorschlags-Zeile, Notausstieg mit Haltefortschritt.
- **Bett-Countdown**: kompakte Pille unten rechts, nie aktivierend (`ShowActivated=false`).

## Bewegung & Zugänglichkeit

- Weiche Fades (200–300 ms, CubicEase Out) für Pop-ups; keine Dauer-Animationen außer
  dem Glow-Punkt im Pausen-Overlay.
- Kontrast: Primärtext > 7:1 auf Backdrop, Sekundärtext > 4.5:1; Status wird nie nur
  über Farbe transportiert (Text + Punkt).
- Alle Kernaktionen per Tastatur erreichbar; das Pausen-Overlay fängt `ESC` bewusst nur
  für den dokumentierten Notausstieg ab.
- Reduzierte Bewegung: keine kritischen Informationen hängen an Animationen.

## QA

Screenshots werden deterministisch über `VGT.RhythmGuard.exe --screenshot <verzeichnis>`
erzeugt (echtes WPF-Rendering, kein Mockup). Ablage: `docs/screenshots/`.
