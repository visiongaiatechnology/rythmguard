#!/bin/bash
# Builds an AppImage for VGT Rhythm Guard (Linux x86_64) into dist-linux/.
set -e
export PATH="$HOME/.dotnet:/usr/local/bin:/usr/bin:/bin:/usr/sbin:/sbin"
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
VERSION="1.3.0"
OUT="$ROOT/dist-linux"
APPDIR="$OUT/VGT-RhythmGuard.AppDir"

bash "$ROOT/scripts/publish-linux.sh"

rm -rf "$APPDIR"
mkdir -p "$APPDIR/usr/bin" "$APPDIR/usr/share/applications" "$APPDIR/usr/share/icons/hicolor/256x256/apps"
cp "$OUT/publish/VGT.RhythmGuard" "$APPDIR/usr/bin/rythmguard"
cp "$ROOT/linux/vgt-rhythmg-ard.desktop" "$APPDIR/usr/share/applications/"
cp "$ROOT/linux/vgt-rhythmg-ard.desktop" "$APPDIR/"
cp "$ROOT/src/VGT.RhythmGuard.App.Linux/Assets/logo-mark.png" "$APPDIR/usr/share/icons/hicolor/256x256/apps/vgt-rhythmg-ard.png"
cp "$ROOT/src/VGT.RhythmGuard.App.Linux/Assets/logo-mark.png" "$APPDIR/vgt-rhythmg-ard.png"
cp "$ROOT/linux/AppRun" "$APPDIR/AppRun"
chmod +x "$APPDIR/AppRun" "$APPDIR/usr/bin/rythmguard"

TOOL="$ROOT/.cache/appimagetool-x86_64.AppImage"
if [ ! -x "$TOOL" ]; then
  curl -sSL "https://github.com/AppImage/appimagetool/releases/download/continuous/appimagetool-x86_64.AppImage" -o "$TOOL"
  chmod +x "$TOOL"
fi

export ARCH=x86_64
"$TOOL" --appimage-extract-and-run "$APPDIR" "$OUT/VGT-RhythmGuard-$VERSION-linux-x86_64.AppImage"
echo "AppImage created: $OUT/VGT-RhythmGuard-$VERSION-linux-x86_64.AppImage"
