# VGT RythmGuard

> **Защити свой ритм. Защити себя.**

<p align="center">
  <img src="docs/branding/logo-wordmark.png" alt="VGT RythmGuard" width="460" />
</p>

<p align="center">
  <strong>Система ментального ритма, которая встраивает перерывы, вечер и сон прямо в сеанс Windows.</strong>
</p>

<p align="center">
  <a href="README.md">English</a> · <a href="README.de.md">Deutsch</a> · <a href="README.es.md">Español</a> ·
  <a href="README.it.md">Italiano</a> · <strong>Русский</strong> · <a href="README.zh.md">中文</a> · <a href="README.ja.md">日本語</a>
</p>

![Dashboard](docs/screenshots/06-dashboard-overview.png)

## ⬇️ Скачать

| Платформа | Версия | Загрузка |
|---|---|---|
| 🪟 **Windows 10/11 x64** | **1.4.0 Beta** | [**Установщик Windows**](https://github.com/visiongaiatechnology/rythmguard/releases/download/v1.4.0-beta.1/VGT-RythmGuard-Setup-1.2.0-Beta.exe) · [Примечания к выпуску](https://github.com/visiongaiatechnology/rythmguard/releases/tag/v1.4.0-beta.1) |
| 🐧 **Linux x64** | **1.4.0 Beta** | [**AppImage (рекомендуется)**](https://github.com/visiongaiatechnology/rythmguard/releases/download/v1.4.0-linux-beta.1/VGT-RythmGuard-1.4.0-linux-x86_64.AppImage) · [Архив tar](https://github.com/visiongaiatechnology/rythmguard/releases/download/v1.4.0-linux-beta.1/VGT-RythmGuard-1.4.0-linux-x64.tar.gz) · [Примечания к выпуску](https://github.com/visiongaiatechnology/rythmguard/releases/tag/v1.4.0-linux-beta.1) |

> Windows: установка для текущего пользователя, без прав администратора. Linux: AppImage — самый простой способ (chmod +x и запуск). Все выпуски: [https://github.com/visiongaiatechnology/rythmguard/releases](https://github.com/visiongaiatechnology/rythmguard/releases).
## Зачем RythmGuard

Долгие рабочие сессии, компьютер, работающий днями, и светящиеся ночью экраны разрушают сон.
RythmGuard превращает Windows в устойчивую рамку:

- **Принудительные перерывы** блокируют экран в течение дня.
- **Вечерний режим** каждые 20 минут напоминает не работать после 23:00.
- **Режим кровати** даёт настраиваемое окно (по умолчанию 2 ч), затем переводит ПК в сон или выключает.
- **Полностью локально:** без аккаунта, облака и телеметрии.

## Возможности

| Режим | Окно | Поведение |
|---|---|---|
| **Фокус** | 07:00 – 23:00 | работа циклами (120 мин), затем |
| **Перерыв** | каждые 120 мин, 30 мин | полноэкранный стеклянный оверлей |
| **Вечер** | 23:00 – 07:00 | спокойное напоминание каждые 20 мин |
| **Кровать** | вручную / напоминание | время посмотреть что-то, затем действие питания |

- Aurora Glass панель: статус, график дневного ритма, оценка и история
- **7 языков** интерфейса
- **Аварийный выход:** удерживайте `ESC` 5 секунд и введите `PAUSE` (можно отключить)
- Действия питания с предупреждениями за 5 мин, 1 мин и отсчётом 60 с
- Живёт в трее; права администратора не нужны

## Скриншоты

| Экран блокировки | Вечернее напоминание |
|---|---|
| ![Перерыв](docs/screenshots/01-break-overlay.png) | ![Вечер](docs/screenshots/03-evening-reminder.png) |

Ещё: [`docs/screenshots/`](docs/screenshots/) · Дизайн: [`docs/DESIGN.md`](docs/DESIGN.md)

## Установка

```powershell
powershell -ExecutionPolicy Bypass -File scripts\publish.ps1
# Опционально: собрать установщик Inno Setup (installer\VGT-RhythmGuard.iss)
powershell -ExecutionPolicy Bypass -File scripts\install-autostart.ps1 -StartNow
```

Установщик ставит приложение для текущего пользователя
(`%LOCALAPPDATA%\Programs\VGT RythmGuard`) без прав администратора. Локальные EXE не подписаны —
один раз подтвердите SmartScreen (*Подробнее → Выполнить в любом случае*).

## Сборка из исходников

```powershell
dotnet restore VGT.RhythmGuard.slnx
dotnet build VGT.RhythmGuard.slnx -c Release
dotnet run --project tools\VGT.RhythmGuard.FactRunner -c Release
dotnet test tests\VGT.RhythmGuard.Core.Tests\VGT.RhythmGuard.Core.Tests.csproj
```

## Ссылки

- Архитектура: [`ARCHITECTURE.md`](ARCHITECTURE.md)
- Тесты: 32 факта ядра + xUnit, выполняются в CI
- Безопасность: [`SECURITY.md`](SECURITY.md) · Участие: [`CONTRIBUTING.md`](CONTRIBUTING.md) · Изменения: [`CHANGELOG.md`](CHANGELOG.md)

## Лицензия

GNU Affero General Public License v3.0 — см. [`LICENSE`](LICENSE).
Уведомления о сторонних компонентах: [`THIRD_PARTY_NOTICES.md`](THIRD_PARTY_NOTICES.md).

---

<p align="center">
  <sub>Powered by <a href="https://visiongaiatechnology.de">VisionGaiaTechnology</a></sub>
</p>
