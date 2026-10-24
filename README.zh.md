# VGT Rhythm Guard

<p align="center">
  <img src="docs/branding/logo-wordmark.png" alt="VGT Rhythm Guard" width="460" />
</p>

<p align="center">
  <strong>一个将休息、晚间与睡眠直接嵌入 Windows 会话的心理健康节律系统。</strong>
</p>

<p align="center">
  <a href="README.md">English</a> · <a href="README.de.md">Deutsch</a> · <a href="README.es.md">Español</a> ·
  <a href="README.it.md">Italiano</a> · <a href="README.ru.md">Русский</a> · <strong>中文</strong> · <a href="README.ja.md">日本語</a>
</p>

![仪表盘](docs/screenshots/06-dashboard-overview.png)

## 为什么选择 Rhythm Guard

长时间工作、电脑连续运行数日、夜里屏幕常亮，会逐渐破坏睡眠。Rhythm Guard 为 Windows 建立固定框架：

- **强制休息**：白天按计划锁定屏幕。
- **晚间模式**：23:00 之后每 20 分钟提醒不要继续工作。
- **睡眠模式**：提供可配置的观看时间（默认 2 小时），随后强制睡眠或关机。
- **完全本地**：无账号、无云端、无遥测。

## 功能

| 模式 | 默认时段 | 行为 |
|---|---|---|
| **专注** | 07:00 – 23:00 | 以 120 分钟为周期工作 |
| **休息** | 每 120 分钟，持续 30 分钟 | 全屏玻璃遮罩，屏幕被锁定 |
| **晚间** | 23:00 – 07:00 | 每 20 分钟安静提醒 |
| **睡眠** | 手动 / 晚间提醒 | 观看一段时间后执行电源操作 |

- Aurora Glass 仪表盘：实时状态、每日节律图、评分与历史
- 界面支持 **7 种语言**
- **紧急退出**：按住 `ESC` 5 秒并输入 `PAUSE`（可禁用）
- 电源操作：5 分钟、1 分钟及 60 秒倒计时警告
- 驻留系统托盘；无需管理员权限

## 截图

| 锁定屏幕 | 晚间提醒 |
|---|---|
| ![休息](docs/screenshots/01-break-overlay.png) | ![晚间](docs/screenshots/03-evening-reminder.png) |

更多：[`docs/screenshots/`](docs/screenshots/) · 设计：[`docs/DESIGN.md`](docs/DESIGN.md)

## 安装

```powershell
powershell -ExecutionPolicy Bypass -File scripts\publish.ps1
# 可选：编译 Inno Setup 安装程序（installer\VGT-RhythmGuard.iss）
powershell -ExecutionPolicy Bypass -File scripts\install-autostart.ps1 -StartNow
```

安装程序按用户安装到 `%LOCALAPPDATA%\Programs\VGT Rhythm Guard`，无需管理员。本地构建的 EXE 未签名，请一次性确认 SmartScreen（*更多信息 → 仍要运行*）。

## 从源码构建

```powershell
dotnet restore VGT.RhythmGuard.slnx
dotnet build VGT.RhythmGuard.slnx -c Release
dotnet run --project tools\VGT.RhythmGuard.FactRunner -c Release
dotnet test tests\VGT.RhythmGuard.Core.Tests\VGT.RhythmGuard.Core.Tests.csproj
```

## 链接

- 架构：[`ARCHITECTURE.md`](ARCHITECTURE.md)
- 测试：32 个核心事实 + xUnit，在 CI 中运行
- 安全：[`SECURITY.md`](SECURITY.md) · 贡献：[`CONTRIBUTING.md`](CONTRIBUTING.md) · 变更：[`CHANGELOG.md`](CHANGELOG.md)

## 许可证

GNU Affero General Public License v3.0 — 见 [`LICENSE`](LICENSE)。
第三方说明：[`THIRD_PARTY_NOTICES.md`](THIRD_PARTY_NOTICES.md)。

---

<p align="center">
  <sub>Powered by <a href="https://visiongaiatechnology.de">VisionGaiaTechnology</a></sub>
</p>
