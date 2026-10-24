#!/bin/bash
# Builds the self-contained Linux x64 release (tarball) into dist-linux/.
set -e
export PATH="$HOME/.dotnet:/usr/local/bin:/usr/bin:/bin:/usr/sbin:/sbin"
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
VERSION="1.3.1"
OUT="$ROOT/dist-linux"
ARCH="linux-x64"

rm -rf "$OUT"
dotnet publish "$ROOT/src/VGT.RhythmGuard.App.Linux/VGT.RhythmGuard.App.Linux.csproj" \
  -c Release -r "$ARCH" --self-contained true \
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true \
  -p:DebugType=None -p:DebugSymbols=false \
  -o "$OUT/publish"

mkdir -p "$OUT/package"
cp "$OUT/publish/VGT.RhythmGuard" "$OUT/package/rythmguard"
cp "$ROOT/linux/vgt-rhythmg-ard.desktop" "$OUT/package/"
cp "$ROOT/src/VGT.RhythmGuard.App.Linux/Assets/logo-mark.png" "$OUT/package/vgt-rhythmg-ard.png"
cp "$ROOT/README.md" "$OUT/package/README.md"

tar -C "$OUT/package" -czf "$OUT/VGT-RhythmGuard-$VERSION-$ARCH.tar.gz" .
echo "Release package created: $OUT/VGT-RhythmGuard-$VERSION-$ARCH.tar.gz"
