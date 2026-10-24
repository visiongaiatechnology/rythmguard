# VGT RythmGuard

> **リズムを守る。自分を守る。**

<p align="center">
  <img src="docs/branding/logo-wordmark.png" alt="VGT RythmGuard" width="460" />
</p>

<p align="center">
  <strong>休憩・夜・睡眠を Windows セッションに直接組み込むメンタルヘルス・リズムシステム。</strong>
</p>

<p align="center">
  <a href="README.md">English</a> · <a href="README.de.md">Deutsch</a> · <a href="README.es.md">Español</a> ·
  <a href="README.it.md">Italiano</a> · <a href="README.ru.md">Русский</a> · <a href="README.zh.md">中文</a> · <strong>日本語</strong>
</p>

![ダッシュボード](docs/screenshots/06-dashboard-overview.png)

## ⬇️ ダウンロード

| プラットフォーム | バージョン | ダウンロード |
|---|---|---|
| 🪟 **Windows 10/11 x64** | **1.4.1 Beta** | [**Windows インストーラー**](https://github.com/visiongaiatechnology/rythmguard/releases/download/v1.4.1-beta.1/VGT-RythmGuard-Setup-1.4.1-Beta.exe) · [リリースノート](https://github.com/visiongaiatechnology/rythmguard/releases/tag/v1.4.1-beta.1) |
| 🐧 **Linux x64** | **1.4.1 Beta** | [**AppImage（推奨）**](https://github.com/visiongaiatechnology/rythmguard/releases/download/v1.4.1-linux-beta.1/VGT-RythmGuard-1.4.1-linux-x86_64.AppImage) · [tar アーカイブ](https://github.com/visiongaiatechnology/rythmguard/releases/download/v1.4.1-linux-beta.1/VGT-RythmGuard-1.4.1-linux-x64.tar.gz) · [リリースノート](https://github.com/visiongaiatechnology/rythmguard/releases/tag/v1.4.1-linux-beta.1) |

> Windows：ユーザー単位のインストール（管理者権限不要）。Linux：AppImage が最も簡単（chmod +x して実行）。すべてのリリース：[https://github.com/visiongaiatechnology/rythmguard/releases](https://github.com/visiongaiatechnology/rythmguard/releases)。
## RythmGuard について

長時間の作業、何日も点いたままの PC、夜も光り続ける画面は睡眠を蝕みます。
RythmGuard は Windows に固定された枠組みを与えます：

- **強制休憩**：日中の一定時間、画面をブロックします。
- **夜間モード**：23:00 以降、20 分ごとに作業を止めるよう促します。
- **ベッドモード**：設定可能な視聴時間（既定 2 時間）の後、スリープまたはシャットダウンを強制します。
- **完全ローカル**：アカウント・クラウド・テレメトリなし。

## 機能

| モード | 既定の時間帯 | 動作 |
|---|---|---|
| **フォーカス** | 07:00 – 23:00 | 120 分サイクルで作業 |
| **休憩** | 120 分ごと、30 分間 | 全画面ガラスオーバーレイで画面をブロック |
| **夜間** | 23:00 – 07:00 | 20 分ごとに静かなリマインダー |
| **ベッド** | 手動 / 夜間リマインダー | 視聴時間の後に電源操作 |

- Aurora Glass ダッシュボード：ライブ状態、日次リズムグラフ、スコア、履歴
- UI は **7 言語** 対応
- **緊急脱出**：`ESC` を 5 秒押して `PAUSE` と入力（無効化可能）
- 電源操作：5 分・1 分・60 秒のカウントダウン警告
- タスクトレイ常駐、管理者権限不要

## スクリーンショット

| ブロック画面 | 夜のリマインダー |
|---|---|
| ![休憩](docs/screenshots/01-break-overlay.png) | ![夜間](docs/screenshots/03-evening-reminder.png) |

その他：[`docs/screenshots/`](docs/screenshots/) · デザイン：[`docs/DESIGN.md`](docs/DESIGN.md)

## インストール

```powershell
powershell -ExecutionPolicy Bypass -File scripts\publish.ps1
# 任意：Inno Setup インストーラをビルド（installer\VGT-RhythmGuard.iss）
powershell -ExecutionPolicy Bypass -File scripts\install-autostart.ps1 -StartNow
```

インストーラはユーザー単位で `%LOCALAPPDATA%\Programs\VGT RythmGuard` に導入し、
管理者権限は不要です。ローカルビルドの EXE は署名されていないため、SmartScreen を一度
許可してください（*詳細情報 → 実行*）。

## ソースからビルド

```powershell
dotnet restore VGT.RhythmGuard.slnx
dotnet build VGT.RhythmGuard.slnx -c Release
dotnet run --project tools\VGT.RhythmGuard.FactRunner -c Release
dotnet test tests\VGT.RhythmGuard.Core.Tests\VGT.RhythmGuard.Core.Tests.csproj
```

## リンク

- アーキテクチャ：[`ARCHITECTURE.md`](ARCHITECTURE.md)
- テスト：32 のコアファクト + xUnit（CI で実行）
- セキュリティ：[`SECURITY.md`](SECURITY.md) · コントリビュート：[`CONTRIBUTING.md`](CONTRIBUTING.md) · 変更履歴：[`CHANGELOG.md`](CHANGELOG.md)

## ライセンス

GNU Affero General Public License v3.0 — [`LICENSE`](LICENSE) を参照。
第三者通知：[`THIRD_PARTY_NOTICES.md`](THIRD_PARTY_NOTICES.md)。

---

<p align="center">
  <sub>Powered by <a href="https://visiongaiatechnology.de">VisionGaiaTechnology</a></sub>
</p>
