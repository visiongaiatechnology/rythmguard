using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using ToolTip = System.Windows.Controls.ToolTip;

namespace VGT.RhythmGuard.App.Localization;

/// <summary>
/// Leichtgewichtige UI-Lokalisierung für sieben Sprachen.
/// XAML-Texte bleiben als deutsche Quelltexte im Markup; die Übersetzung wird beim
/// Laden der Fenster über eine Katalogzuordnung angewendet. Originaltexte werden
/// zwischengespeichert, damit ein Sprachwechsel ohne Fensterneustart möglich ist.
/// </summary>
internal static class Localizer
{
    public const string German = "de";

    public static string CurrentLanguage { get; private set; } = German;

    public static event EventHandler? LanguageChanged;

    private static readonly Dictionary<string, Dictionary<string, string>> Catalog = BuildCatalog();

    private static readonly Dictionary<object, string> Originals = new(ReferenceEqualityComparer.Instance);

    public static void Initialize(string? languageSetting)
    {
        var code = ResolveLanguage(languageSetting);
        if (string.Equals(code, CurrentLanguage, StringComparison.Ordinal))
        {
            return;
        }

        CurrentLanguage = code;
        LanguageChanged?.Invoke(null, EventArgs.Empty);
    }

    public static string ResolveLanguage(string? setting)
    {
        if (string.IsNullOrWhiteSpace(setting) || string.Equals(setting, "system", StringComparison.OrdinalIgnoreCase))
        {
            var ui = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            return IsSupported(ui) ? ui : "en";
        }

        var normalized = setting.Trim().ToLowerInvariant();
        return IsSupported(normalized) ? normalized : "en";
    }

    public static bool IsSupported(string code) =>
        code is "de" or "en" or "es" or "it" or "ru" or "zh" or "ja";

    /// <summary>Übersetzt einen deutschen Quelltext (de = Identität).</summary>
    public static string T(string german)
    {
        if (CurrentLanguage == German || string.IsNullOrEmpty(german))
        {
            return german;
        }

        return Catalog.TryGetValue(german, out var translations)
               && translations.TryGetValue(CurrentLanguage, out var translated)
            ? translated
            : german;
    }

    public static string Format(string germanTemplate, params object[] args)
        => string.Format(CultureInfo.CurrentCulture, T(germanTemplate), args);

    /// <summary>Übersetzt alle statischen Texte eines Fensters; bei Sprachwechsel erneut aufrufbar.</summary>
    public static void TranslateWindow(Window window)
    {
        if (window is null || CurrentLanguage == German)
        {
            return;
        }

        TranslateElement(window);
        if (!string.IsNullOrWhiteSpace(window.Title))
        {
            window.Title = T(window.Title);
        }
    }

    private static void TranslateElement(DependencyObject element)
    {
        switch (element)
        {
            case TextBlock textBlock when !string.IsNullOrEmpty(textBlock.Text):
                textBlock.Text = TranslateValue(textBlock, textBlock.Text);
                break;
            case ContentControl contentControl when contentControl.Content is string text:
                contentControl.Content = TranslateValue(contentControl, text);
                break;
            case ToolTip toolTip when toolTip.Content is string tooltipText:
                toolTip.Content = TranslateValue(toolTip, tooltipText);
                break;
        }

        var childCount = VisualTreeHelper.GetChildrenCount(element);
        for (var index = 0; index < childCount; index++)
        {
            TranslateElement(VisualTreeHelper.GetChild(element, index));
        }
    }

    private static string TranslateValue(object element, string current)
    {
        if (!Originals.TryGetValue(element, out var original))
        {
            original = current;
            Originals[element] = original;
        }

        return T(original);
    }

    /// <summary>Übersetzt ein Fenster automatisch nach dem Laden (inkl. späterer Sprachwechsel).</summary>
    public static void EnableAutoTranslation(Window window)
    {
        window.Loaded += (_, _) => TranslateWindow(window);
        window.Closed += (_, _) => Originals.Remove(window);
    }

    private static Dictionary<string, Dictionary<string, string>> BuildCatalog() => new()
    {
        ["Übersicht"] = new() { ["en"] = "Overview", ["es"] = "Resumen", ["it"] = "Panoramica", ["ru"] = "Обзор", ["zh"] = "总览", ["ja"] = "概要" },
        ["Einstellungen"] = new() { ["en"] = "Settings", ["es"] = "Ajustes", ["it"] = "Impostazioni", ["ru"] = "Настройки", ["zh"] = "设置", ["ja"] = "設定" },
        ["Verlauf"] = new() { ["en"] = "History", ["es"] = "Historial", ["it"] = "Cronologia", ["ru"] = "История", ["zh"] = "历史", ["ja"] = "履歴" },
        ["AKTUELLER MODUS"] = new() { ["en"] = "CURRENT MODE", ["es"] = "MODO ACTUAL", ["it"] = "MODALITÀ ATTUALE", ["ru"] = "ТЕКУЩИЙ РЕЖИМ", ["zh"] = "当前模式", ["ja"] = "現在のモード" },
        ["Dein Rhythmus-System"] = new() { ["en"] = "Your rhythm system", ["es"] = "Tu sistema de ritmo", ["it"] = "Il tuo sistema ritmico", ["ru"] = "Твоя система ритма", ["zh"] = "你的节律系统", ["ja"] = "あなたのリズムシステム" },
        ["NÄCHSTE PAUSE"] = new() { ["en"] = "NEXT BREAK", ["es"] = "PRÓXIMA PAUSA", ["it"] = "PROSSIMA PAUSA", ["ru"] = "СЛЕДУЮЩИЙ ПЕРЕРЫВ", ["zh"] = "下次休息", ["ja"] = "次の休憩" },
        ["PAUSEN HEUTE"] = new() { ["en"] = "BREAKS TODAY", ["es"] = "PAUSAS HOY", ["it"] = "PAUSE OGGI", ["ru"] = "ПЕРЕРЫВЫ СЕГОДНЯ", ["zh"] = "今日休息", ["ja"] = "今日の休憩" },
        ["ABENDMODUS"] = new() { ["en"] = "EVENING MODE", ["es"] = "MODO NOCHE", ["it"] = "MODALITÀ SERALE", ["ru"] = "ВЕЧЕРНИЙ РЕЖИМ", ["zh"] = "晚间模式", ["ja"] = "夜間モード" },
        ["STREAK"] = new() { ["en"] = "STREAK", ["es"] = "RACHA", ["it"] = "SERIE", ["ru"] = "СЕРИЯ", ["zh"] = "连续天数", ["ja"] = "連続記録" },
        ["Tage mit Pausen"] = new() { ["en"] = "days with breaks", ["es"] = "días con pausas", ["it"] = "giorni con pause", ["ru"] = "дней с перерывами", ["zh"] = "有休息的天数", ["ja"] = "休憩のある日" },
        ["Tag mit Pausen"] = new() { ["en"] = "day with breaks", ["es"] = "día con pausas", ["it"] = "giorno con pause", ["ru"] = "день с перерывами", ["zh"] = "有休息的一天", ["ja"] = "休憩のある日" },
        ["Tagesrhythmus"] = new() { ["en"] = "Daily rhythm", ["es"] = "Ritmo diario", ["it"] = "Ritmo quotidiano", ["ru"] = "Дневной ритм", ["zh"] = "每日节律", ["ja"] = "一日のリズム" },
        ["Tagesziel"] = new() { ["en"] = "Daily goal", ["es"] = "Objetivo diario", ["it"] = "Obiettivo giornaliero", ["ru"] = "Цель дня", ["zh"] = "每日目标", ["ja"] = "今日の目標" },
        ["Rhythmus-Score"] = new() { ["en"] = "Rhythm score", ["es"] = "Puntuación del ritmo", ["it"] = "Punteggio ritmico", ["ru"] = "Ритмический счёт", ["zh"] = "节律评分", ["ja"] = "リズムスコア" },
        ["Fokus"] = new() { ["en"] = "Focus", ["es"] = "Enfoque", ["it"] = "Focus", ["ru"] = "Фокус", ["zh"] = "专注", ["ja"] = "フォーカス" },
        ["Pause"] = new() { ["en"] = "Break", ["es"] = "Pausa", ["it"] = "Pausa", ["ru"] = "Перерыв", ["zh"] = "休息", ["ja"] = "休憩" },
        ["Pausen"] = new() { ["en"] = "Breaks", ["es"] = "Pausas", ["it"] = "Pause", ["ru"] = "Перерывы", ["zh"] = "休息次数", ["ja"] = "休憩回数" },
        ["Abbrüche"] = new() { ["en"] = "Cancellations", ["es"] = "Cancelaciones", ["it"] = "Interruzioni", ["ru"] = "Отмены", ["zh"] = "中断次数", ["ja"] = "中断" },
        ["Abend-Erinnerungen"] = new() { ["en"] = "Evening reminders", ["es"] = "Recordatorios nocturnos", ["it"] = "Promemoria serali", ["ru"] = "Вечерние напоминания", ["zh"] = "晚间提醒", ["ja"] = "夜のリマインダー" },
        ["Letzte Ereignisse"] = new() { ["en"] = "Recent events", ["es"] = "Últimos eventos", ["it"] = "Eventi recenti", ["ru"] = "Последние события", ["zh"] = "最近事件", ["ja"] = "最近のイベント" },
        ["HEUTE"] = new() { ["en"] = "TODAY", ["es"] = "HOY", ["it"] = "OGGI", ["ru"] = "СЕГОДНЯ", ["zh"] = "今天", ["ja"] = "今日" },
        ["Systemstatus"] = new() { ["en"] = "System status", ["es"] = "Estado del sistema", ["it"] = "Stato del sistema", ["ru"] = "Состояние системы", ["zh"] = "系统状态", ["ja"] = "システム状態" },
        ["Rhythmus-Engine"] = new() { ["en"] = "Rhythm engine", ["es"] = "Motor de ritmo", ["it"] = "Motore del ritmo", ["ru"] = "Ритмический движок", ["zh"] = "节律引擎", ["ja"] = "リズムエンジン" },
        ["aktiv"] = new() { ["en"] = "active", ["es"] = "activo", ["it"] = "attivo", ["ru"] = "активно", ["zh"] = "运行中", ["ja"] = "稼働中" },
        ["inaktiv"] = new() { ["en"] = "inactive", ["es"] = "inactivo", ["it"] = "inattivo", ["ru"] = "неактивно", ["zh"] = "未启用", ["ja"] = "停止中" },
        ["Mit Windows starten"] = new() { ["en"] = "Start with Windows", ["es"] = "Iniciar con Windows", ["it"] = "Avvia con Windows", ["ru"] = "Запуск с Windows", ["zh"] = "随 Windows 启动", ["ja"] = "Windows と同時に起動" },
        ["Pause jetzt"] = new() { ["en"] = "Break now", ["es"] = "Pausa ahora", ["it"] = "Pausa ora", ["ru"] = "Перерыв сейчас", ["zh"] = "立即休息", ["ja"] = "今すぐ休憩" },
        ["Aktives Fenster"] = new() { ["en"] = "Active window", ["es"] = "Ventana activa", ["it"] = "Finestra attiva", ["ru"] = "Активное окно", ["zh"] = "活动时段", ["ja"] = "稼働時間帯" },
        ["Fokusphase"] = new() { ["en"] = "Focus phase", ["es"] = "Fase de enfoque", ["it"] = "Fase di focus", ["ru"] = "Фаза фокуса", ["zh"] = "专注阶段", ["ja"] = "集中フェーズ" },
        ["Erinnerungen nach 23:00 anzeigen"] = new() { ["en"] = "Show reminders after 23:00", ["es"] = "Mostrar recordatorios después de las 23:00", ["it"] = "Mostra promemoria dopo le 23:00", ["ru"] = "Показывать напоминания после 23:00", ["zh"] = "23:00 后显示提醒", ["ja"] = "23:00 以降にリマインダーを表示" },
        ["Erinnerungs-Intervall"] = new() { ["en"] = "Reminder interval", ["es"] = "Intervalo de recordatorios", ["it"] = "Intervallo promemoria", ["ru"] = "Интервал напоминаний", ["zh"] = "提醒间隔", ["ja"] = "リマインダー間隔" },
        ["Bett-Modus"] = new() { ["en"] = "Bed mode", ["es"] = "Modo cama", ["it"] = "Modalità letto", ["ru"] = "Режим кровати", ["zh"] = "睡眠模式", ["ja"] = "ベッドモード" },
        ["Dauer bis Energie-Aktion"] = new() { ["en"] = "Duration until power action", ["es"] = "Duración hasta la acción de energía", ["it"] = "Durata fino all'azione energia", ["ru"] = "Длительность до действия питания", ["zh"] = "距离电源操作的时间", ["ja"] = "電源操作までの時間" },
        ["Aktion danach"] = new() { ["en"] = "Action afterwards", ["es"] = "Acción posterior", ["it"] = "Azione successiva", ["ru"] = "Действие после", ["zh"] = "之后的操作", ["ja"] = "その後の動作" },
        ["Ruhezustand"] = new() { ["en"] = "Sleep", ["es"] = "Suspender", ["it"] = "Sospensione", ["ru"] = "Сон", ["zh"] = "睡眠", ["ja"] = "スリープ" },
        ["Sanft herunterfahren"] = new() { ["en"] = "Shut down gently", ["es"] = "Apagar suavemente", ["it"] = "Spegni dolcemente", ["ru"] = "Мягкое завершение", ["zh"] = "平稳关机", ["ja"] = "穏やかに終了" },
        ["Hart herunterfahren"] = new() { ["en"] = "Force shutdown", ["es"] = "Apagado forzado", ["it"] = "Spegnimento forzato", ["ru"] = "Принудительное завершение", ["zh"] = "强制关机", ["ja"] = "強制終了" },
        ["Notausstieg"] = new() { ["en"] = "Emergency exit", ["es"] = "Salida de emergencia", ["it"] = "Uscita di emergenza", ["ru"] = "Аварийный выход", ["zh"] = "紧急退出", ["ja"] = "緊急脱出" },
        ["ESC-Haltezeit"] = new() { ["en"] = "ESC hold time", ["es"] = "Tiempo de ESC", ["it"] = "Tempo di pressione ESC", ["ru"] = "Время удержания ESC", ["zh"] = "ESC 按住时间", ["ja"] = "ESC 長押し時間" },
        ["Speichern"] = new() { ["en"] = "Save", ["es"] = "Guardar", ["it"] = "Salva", ["ru"] = "Сохранить", ["zh"] = "保存", ["ja"] = "保存" },
        ["Schließen"] = new() { ["en"] = "Close", ["es"] = "Cerrar", ["it"] = "Chiudi", ["ru"] = "Закрыть", ["zh"] = "关闭", ["ja"] = "閉じる" },
        ["Einstellungen werden beim Speichern übernommen."] = new() { ["en"] = "Settings are applied when saved.", ["es"] = "Los ajustes se aplican al guardar.", ["it"] = "Le impostazioni vengono applicate al salvataggio.", ["ru"] = "Настройки применяются при сохранении.", ["zh"] = "保存后应用设置。", ["ja"] = "保存時に設定が適用されます。" },
        ["Sprache"] = new() { ["en"] = "Language", ["es"] = "Idioma", ["it"] = "Lingua", ["ru"] = "Язык", ["zh"] = "语言", ["ja"] = "言語" },
        ["Zeit für dich."] = new() { ["en"] = "Time for you.", ["es"] = "Tiempo para ti.", ["it"] = "Tempo per te.", ["ru"] = "Время для себя.", ["zh"] = "属于你的时间。", ["ja"] = "あなたの時間です。" },
        ["Jetzt ist Pause."] = new() { ["en"] = "Break time now.", ["es"] = "Ahora toca una pausa.", ["it"] = "Ora è il momento della pausa.", ["ru"] = "Пора сделать перерыв.", ["zh"] = "现在休息一下。", ["ja"] = "今は休憩です。" },
        ["ERZWUNGENE PAUSE"] = new() { ["en"] = "ENFORCED BREAK", ["es"] = "PAUSA OBLIGATORIA", ["it"] = "PAUSA OBBLIGATORIA", ["ru"] = "ВЫНУЖДЕННЫЙ ПЕРЕРЫВ", ["zh"] = "强制休息", ["ja"] = "強制休憩" },
        ["SICHERHEITSMODUS"] = new() { ["en"] = "SAFETY MODE", ["es"] = "MODO DE SEGURIDAD", ["it"] = "MODALITÀ SICUREZZA", ["ru"] = "РЕЖИМ БЕЗОПАСНОСТИ", ["zh"] = "安全模式", ["ja"] = "セーフティモード" },
        ["Der PC wird gleich heruntergefahren."] = new() { ["en"] = "The PC is about to shut down.", ["es"] = "El PC se apagará en breve.", ["it"] = "Il PC si spegnerà a breve.", ["ru"] = "Компьютер скоро выключится.", ["zh"] = "电脑即将关机。", ["ja"] = "まもなくPCがシャットダウンします。" },
        ["Bitte speichere deine Arbeit. Danach fährt der Rechner herunter."] = new() { ["en"] = "Please save your work. The PC will shut down afterwards.", ["es"] = "Guarda tu trabajo. Después, el PC se apagará.", ["it"] = "Salva il tuo lavoro. Poi il PC si spegnerà.", ["ru"] = "Сохраните работу. Затем компьютер выключится.", ["zh"] = "请保存工作，随后电脑将关机。", ["ja"] = "作業を保存してください。その後PCがシャットダウンします。" },
        ["Der Bildschirm bleibt aus. Steh auf, atme, trink Wasser."] = new() { ["en"] = "Stay off-screen. Stand up, breathe, drink water.", ["es"] = "Pantalla apagada. Levántate, respira, bebe agua.", ["it"] = "Schermo spento. Alzati, respira, bevi acqua.", ["ru"] = "Экран выключен. Встань, дыши, выпей воды.", ["zh"] = "屏幕保持关闭。站起来，呼吸，喝水。", ["ja"] = "画面はオフのまま。立って、呼吸して、水を飲みましょう。" },
        ["Notausstieg: ESC 5 Sekunden halten"] = new() { ["en"] = "Emergency exit: hold ESC for 5 seconds", ["es"] = "Salida de emergencia: mantén ESC 5 segundos", ["it"] = "Uscita di emergenza: tieni premuto ESC per 5 secondi", ["ru"] = "Аварийный выход: удерживайте ESC 5 секунд", ["zh"] = "紧急退出：按住 ESC 5 秒", ["ja"] = "緊急脱出：ESC を5秒間押し続ける" },
        ["Zeit, loszulassen."] = new() { ["en"] = "Time to let go.", ["es"] = "Hora de soltar.", ["it"] = "È ora di lasciar andare.", ["ru"] = "Пора отпустить.", ["zh"] = "该放松了。", ["ja"] = "そろそろ手放しましょう。" },
        ["Du hast für heute genug gearbeitet. Ab jetzt darf der Tag ruhig enden."] = new() { ["en"] = "You have done enough for today. Let the day end quietly.", ["es"] = "Has trabajado suficiente por hoy. Deja que el día termine tranquilo.", ["it"] = "Hai lavorato abbastanza per oggi. Lascia che la giornata finisca con calma.", ["ru"] = "На сегодня достаточно. Пусть день спокойно завершится.", ["zh"] = "今天已经够努力了。让一天安静地结束吧。", ["ja"] = "今日は十分に働きました。静かに一日を終えましょう。" },
        ["Später"] = new() { ["en"] = "Later", ["es"] = "Más tarde", ["it"] = "Più tardi", ["ru"] = "Позже", ["zh"] = "稍后", ["ja"] = "後で" },
        ["Bett-Modus starten"] = new() { ["en"] = "Start bed mode", ["es"] = "Iniciar modo cama", ["it"] = "Avvia modalità letto", ["ru"] = "Запустить режим кровати", ["zh"] = "启动睡眠模式", ["ja"] = "ベッドモードを開始" },
        ["Pause wirklich abbrechen?"] = new() { ["en"] = "Really cancel the break?", ["es"] = "¿Cancelar la pausa de verdad?", ["it"] = "Annullare davvero la pausa?", ["ru"] = "Действительно прервать перерыв?", ["zh"] = "确定要中断休息吗？", ["ja"] = "本当に休憩を中断しますか？" },
        ["Tippe PAUSE zur Bestätigung:"] = new() { ["en"] = "Type PAUSE to confirm:", ["es"] = "Escribe PAUSE para confirmar:", ["it"] = "Digita PAUSE per confermare:", ["ru"] = "Введите PAUSE для подтверждения:", ["zh"] = "输入 PAUSE 以确认：", ["ja"] = "確認のため PAUSE と入力：" },
        ["Weitermachen"] = new() { ["en"] = "Keep going", ["es"] = "Seguir", ["it"] = "Continua", ["ru"] = "Продолжить", ["zh"] = "继续", ["ja"] = "続ける" },
        ["Notausstieg bestätigen"] = new() { ["en"] = "Confirm emergency exit", ["es"] = "Confirmar salida de emergencia", ["it"] = "Conferma uscita di emergenza", ["ru"] = "Подтвердить аварийный выход", ["zh"] = "确认紧急退出", ["ja"] = "緊急脱出を確認" },
        ["Dein Rhythmus wird vorbereitet …"] = new() { ["en"] = "Preparing your rhythm …", ["es"] = "Preparando tu ritmo …", ["it"] = "Preparando il tuo ritmo …", ["ru"] = "Готовим твой ритм …", ["zh"] = "正在准备你的节律 …", ["ja"] = "リズムを準備しています …" },
        ["BETT-MODUS"] = new() { ["en"] = "BED MODE", ["es"] = "MODO CAMA", ["it"] = "MODALITÀ LETTO", ["ru"] = "РЕЖИМ КРОВАТИ", ["zh"] = "睡眠模式", ["ja"] = "ベッドモード" },
        ["danach Ruhezustand"] = new() { ["en"] = "then sleep", ["es"] = "luego suspender", ["it"] = "poi sospensione", ["ru"] = "затем сон", ["zh"] = "然后睡眠", ["ja"] = "その後スリープ" },
        ["Bett-Modus endet in {0} Minuten"] = new() { ["en"] = "Bed mode ends in {0} minutes", ["es"] = "El modo cama termina en {0} minutos", ["it"] = "Modalità letto termina tra {0} minuti", ["ru"] = "Режим кровати завершится через {0} мин", ["zh"] = "睡眠模式将在 {0} 分钟后结束", ["ja"] = "ベッドモードは {0} 分後に終了します" },
        ["Danach: {0}"] = new() { ["en"] = "Then: {0}", ["es"] = "Después: {0}", ["it"] = "Poi: {0}", ["ru"] = "Затем: {0}", ["zh"] = "然后：{0}", ["ja"] = "その後：{0}" },
        ["FOKUS"] = new() { ["en"] = "FOCUS", ["es"] = "ENFOQUE", ["it"] = "FOCUS", ["ru"] = "ФОКУС", ["zh"] = "专注", ["ja"] = "フォーカス" },
    };
}
