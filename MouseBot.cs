// MouseBot v2 - Fare hareketsiz kaldığında bilgisayarın uykuya / ekran kilidine
// geçmesini engelleyen sistem tepsisi uygulaması.
// Derleme: build.bat  (C# 5 / .NET Framework 4 - ek kurulum gerekmez)

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace MouseBot
{
    // ------------------------------------------------------------------ Win32

    static class Native
    {
        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO { public uint cbSize; public uint dwTime; }

        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT { public int dx, dy; public uint mouseData, dwFlags, time; public IntPtr dwExtraInfo; }

        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT { public ushort wVk, wScan; public uint dwFlags, time; public IntPtr dwExtraInfo; }

        [StructLayout(LayoutKind.Explicit)]
        struct InputUnion { [FieldOffset(0)] public MOUSEINPUT mi; [FieldOffset(0)] public KEYBDINPUT ki; }

        [StructLayout(LayoutKind.Sequential)]
        struct INPUT { public uint type; public InputUnion u; }

        [DllImport("user32.dll")] static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
        [DllImport("user32.dll", SetLastError = true)] static extern uint SendInput(uint n, INPUT[] inputs, int size);
        [DllImport("kernel32.dll")] public static extern uint SetThreadExecutionState(uint esFlags);
        [DllImport("user32.dll")] public static extern bool DestroyIcon(IntPtr h);
        [DllImport("user32.dll")] public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint mods, uint vk);
        [DllImport("user32.dll")] public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern int RegisterWindowMessage(string s);
        [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr hWnd, int msg, IntPtr w, IntPtr l);
        [DllImport("dwmapi.dll")] public static extern int DwmSetWindowAttribute(IntPtr hWnd, int attr, ref int value, int size);

        public const uint ES_CONTINUOUS = 0x80000000, ES_SYSTEM_REQUIRED = 0x1, ES_DISPLAY_REQUIRED = 0x2;
        public const uint MOD_ALT = 0x1, MOD_CONTROL = 0x2, MOD_NOREPEAT = 0x4000;

        public static double IdleSeconds()
        {
            var info = new LASTINPUTINFO();
            info.cbSize = (uint)Marshal.SizeOf(info);
            if (!GetLastInputInfo(ref info)) return 0;
            return unchecked((uint)Environment.TickCount - info.dwTime) / 1000.0;
        }

        static INPUT Mouse(int dx, int dy)
        {
            var i = new INPUT();
            i.type = 0; i.u.mi.dx = dx; i.u.mi.dy = dy; i.u.mi.dwFlags = 0x0001; // MOUSEEVENTF_MOVE
            return i;
        }

        static INPUT Key(ushort vk, bool up)
        {
            var i = new INPUT();
            i.type = 1; i.u.ki.wVk = vk; i.u.ki.dwFlags = up ? 2u : 0u; // KEYEVENTF_KEYUP
            return i;
        }

        static bool Send(params INPUT[] inputs)
        {
            return SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT))) == inputs.Length;
        }

        public static bool MoveMouse(int dx, int dy) { return Send(Mouse(dx, dy)); }
        public static bool NudgeInvisible() { return Send(Mouse(1, 0), Mouse(-1, 0)); }
        public static bool TapF15() { return Send(Key(0x7E, false), Key(0x7E, true)); }
    }

    // --------------------------------------------------------------- Ayarlar

    class Settings
    {
        public int IdleSeconds = 240;          // 4 dakika
        public int Mode = 1;                   // 0 görünmez, 1 küçük hareket, 2 F15 tuşu
        public int Pixels = 5;
        public bool Enabled = true;
        public bool KeepAwakeApi = true;
        public bool KeepDisplayOn = true;
        public bool ShowNotifications = true;
        public bool HotkeyEnabled = true;
        public bool PauseOnBattery = false;
        public bool ScheduleEnabled = false;
        public int StartMin = 9 * 60, EndMin = 18 * 60;
        public int Days = 62;                  // bit = DayOfWeek; 62 = Pzt-Cum
        public int Theme = 0;                  // 0 sistem, 1 açık, 2 koyu
        public string Language = "auto";       // "auto" veya tr/en/de/es/fr/ru/zh
        public int TotalJiggles, TodayJiggles;
        public string TodayDate = "";
        public long LastJiggleTicks;

        public static string Dir { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MouseBot"); } }
        static string FilePath { get { return Path.Combine(Dir, "settings.ini"); } }

        public static Settings Load(out bool firstRun)
        {
            var s = new Settings();
            firstRun = !File.Exists(FilePath);
            if (firstRun) return s;
            var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (var line in File.ReadAllLines(FilePath))
                {
                    var p = line.Split(new[] { '=' }, 2);
                    if (p.Length == 2) d[p[0].Trim()] = p[1].Trim();
                }
            }
            catch { return s; }

            s.IdleSeconds = Clamp(Int(d, "IdleSeconds", s.IdleSeconds), 10, 7200);
            s.Mode = Clamp(Int(d, "Mode", s.Mode), 0, 2);
            s.Pixels = Clamp(Int(d, "Pixels", s.Pixels), 1, 50);
            s.Enabled = Bool(d, "Enabled", s.Enabled);
            s.KeepAwakeApi = Bool(d, "KeepAwakeApi", s.KeepAwakeApi);
            s.KeepDisplayOn = Bool(d, "KeepDisplayOn", s.KeepDisplayOn);
            s.ShowNotifications = Bool(d, "ShowNotifications", s.ShowNotifications);
            s.HotkeyEnabled = Bool(d, "HotkeyEnabled", s.HotkeyEnabled);
            s.PauseOnBattery = Bool(d, "PauseOnBattery", s.PauseOnBattery);
            s.ScheduleEnabled = Bool(d, "ScheduleEnabled", s.ScheduleEnabled);
            s.StartMin = Clamp(Int(d, "StartMin", s.StartMin), 0, 1439);
            s.EndMin = Clamp(Int(d, "EndMin", s.EndMin), 0, 1439);
            s.Days = Clamp(Int(d, "Days", s.Days), 0, 127);
            s.Theme = Clamp(Int(d, "Theme", s.Theme), 0, 2);
            string lg; s.Language = d.TryGetValue("Language", out lg) ? lg : "tr"; // v2.0 yalnızca Türkçeydi
            s.TotalJiggles = Math.Max(0, Int(d, "TotalJiggles", 0));
            s.TodayJiggles = Math.Max(0, Int(d, "TodayJiggles", 0));
            string td; if (d.TryGetValue("TodayDate", out td)) s.TodayDate = td;
            long lj; if (d.TryGetValue("LastJiggle", out td) && long.TryParse(td, out lj)) s.LastJiggleTicks = lj;
            if (d.ContainsKey("ShowWelcome")) firstRun = true; // kurulum sihirbazı yazar; ilk kayıtta silinir
            return s;
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Dir);
                File.WriteAllLines(FilePath, new[] {
                    "IdleSeconds=" + IdleSeconds, "Mode=" + Mode, "Pixels=" + Pixels,
                    "Enabled=" + Enabled, "KeepAwakeApi=" + KeepAwakeApi, "KeepDisplayOn=" + KeepDisplayOn,
                    "ShowNotifications=" + ShowNotifications, "HotkeyEnabled=" + HotkeyEnabled,
                    "PauseOnBattery=" + PauseOnBattery, "ScheduleEnabled=" + ScheduleEnabled,
                    "StartMin=" + StartMin, "EndMin=" + EndMin, "Days=" + Days, "Theme=" + Theme, "Language=" + Language,
                    "TotalJiggles=" + TotalJiggles, "TodayJiggles=" + TodayJiggles, "TodayDate=" + TodayDate, "LastJiggle=" + LastJiggleTicks
                });
            }
            catch { }
        }

        public void ResetDefaults()
        {
            var d = new Settings();
            IdleSeconds = d.IdleSeconds; Mode = d.Mode; Pixels = d.Pixels; Enabled = d.Enabled;
            KeepAwakeApi = d.KeepAwakeApi; KeepDisplayOn = d.KeepDisplayOn; ShowNotifications = d.ShowNotifications;
            HotkeyEnabled = d.HotkeyEnabled; PauseOnBattery = d.PauseOnBattery; ScheduleEnabled = d.ScheduleEnabled;
            StartMin = d.StartMin; EndMin = d.EndMin; Days = d.Days; Theme = d.Theme;
        }

        public bool InSchedule(DateTime now)
        {
            if ((Days & (1 << (int)now.DayOfWeek)) == 0) return false;
            int m = now.Hour * 60 + now.Minute;
            if (StartMin == EndMin) return true;
            if (StartMin < EndMin) return m >= StartMin && m < EndMin;
            return m >= StartMin || m < EndMin; // gece yarısını aşan aralık
        }

        static string Today { get { return DateTime.Today.ToString("yyyy-MM-dd"); } }
        public int TodayCount { get { return TodayDate == Today ? TodayJiggles : 0; } }
        public DateTime? LastJiggle { get { return LastJiggleTicks > 0 ? new DateTime(LastJiggleTicks) : (DateTime?)null; } }

        public void RecordJiggle()
        {
            if (TodayDate != Today) { TodayDate = Today; TodayJiggles = 0; }
            TodayJiggles++;
            TotalJiggles++;
            LastJiggleTicks = DateTime.Now.Ticks;
        }

        public static string HhMm(int min) { return (min / 60).ToString("00") + ":" + (min % 60).ToString("00"); }
        static int Clamp(int v, int lo, int hi) { return Math.Max(lo, Math.Min(hi, v)); }
        static int Int(Dictionary<string, string> d, string k, int def) { string v; int n; return d.TryGetValue(k, out v) && int.TryParse(v, out n) ? n : def; }
        static bool Bool(Dictionary<string, string> d, string k, bool def) { string v; bool b; return d.TryGetValue(k, out v) && bool.TryParse(v, out b) ? b : def; }
    }

    // ------------------------------------------------------------ Diller

    static class Lang
    {
        public static readonly string[] Codes = { "tr", "en", "de", "es", "fr", "ru", "zh" };
        public static readonly string[] Names = { "Türkçe", "English", "Deutsch", "Español", "Français", "Русский", "中文（简体）" };
        static int idx = 1;
        public static string Code { get { return Codes[idx]; } }

        // "auto" = Windows arayüz dili (desteklenmiyorsa İngilizce)
        public static void Apply(string setting)
        {
            string c = setting;
            if (string.IsNullOrEmpty(c) || c == "auto") c = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            int i = Array.IndexOf(Codes, c);
            idx = i >= 0 ? i : 1;
        }

        // Çince için Windows'un Çince arayüz yazı tipi; diğerleri Segoe UI (eksik glifler karışık yedeklerden gelmesin)
        public static Font UiFont(float size, bool semibold = false, GraphicsUnit unit = GraphicsUnit.Point)
        {
            if (Code == "zh") return new Font("Microsoft YaHei UI", size, semibold ? FontStyle.Bold : FontStyle.Regular, unit);
            return new Font(semibold ? "Segoe UI Semibold" : "Segoe UI", size, FontStyle.Regular, unit);
        }

        public static string T(string key)
        {
            string[] v;
            return Table.TryGetValue(key, out v) ? v[idx] : key;
        }

        public static string F(string key, params object[] args) { return string.Format(T(key), args); }
        public static string[] List(string key) { return T(key).Split('|'); }

        //                                         Türkçe, English, Deutsch, Español, Français, Русский, 中文
        static readonly Dictionary<string, string[]> Table = new Dictionary<string, string[]>
        {
            { "u_s", new[] { "sn", "s", "Sek.", "s", "s", "с", "秒" } },
            { "u_m", new[] { "dk", "min", "Min.", "min", "min", "мин", "分" } },
            { "u_h", new[] { "sa", "h", "Std.", "h", "h", "ч", "小时" } },
            { "min1", new[] { "{0} dakika", "{0} minute", "{0} Minute", "{0} minuto", "{0} minute", "{0} мин", "{0} 分钟" } },
            { "mins", new[] { "{0} dakika", "{0} minutes", "{0} Minuten", "{0} minutos", "{0} minutes", "{0} мин", "{0} 分钟" } },
            { "hour1", new[] { "{0} saat", "{0} hour", "{0} Stunde", "{0} hora", "{0} heure", "{0} ч", "{0} 小时" } },
            { "hours", new[] { "{0} saat", "{0} hours", "{0} Stunden", "{0} horas", "{0} heures", "{0} ч", "{0} 小时" } },
            { "hk", new[] { "Ctrl+Alt+M", "Ctrl+Alt+M", "Strg+Alt+M", "Ctrl+Alt+M", "Ctrl+Alt+M", "Ctrl+Alt+M", "Ctrl+Alt+M" } },

            { "ready_t", new[] { "MouseBot hazır", "MouseBot is ready", "MouseBot ist bereit", "MouseBot está listo", "MouseBot est prêt", "MouseBot готов", "MouseBot 已就绪" } },
            { "ready_b", new[] {
                "Sağ alttaki fare simgesinden kontrol edebilirsiniz. Simge görünmüyorsa ^ okuna tıklayıp görev çubuğuna sürükleyin.",
                "Control it from the mouse icon at the bottom right. If you can't see it, click the ^ arrow and drag it onto the taskbar.",
                "Steuern Sie es über das Maussymbol unten rechts. Falls es fehlt, klicken Sie auf den Pfeil ^ und ziehen Sie es in die Taskleiste.",
                "Contrólalo desde el icono del ratón abajo a la derecha. Si no lo ves, haz clic en la flecha ^ y arrástralo a la barra de tareas.",
                "Contrôlez-le depuis l'icône de souris en bas à droite. Si elle n'apparaît pas, cliquez sur la flèche ^ et glissez-la dans la barre des tâches.", "Управляйте программой через значок мыши в правом нижнем углу. Если его не видно, нажмите стрелку ^ и перетащите значок на панель задач.", "可通过右下角的鼠标图标进行控制。如果看不到图标，请点击 ^ 箭头并将其拖到任务栏上。" } },

            { "m_inactive", new[] { "Aktif değil (açmak için tıkla)", "Inactive (click to turn on)", "Inaktiv (zum Einschalten klicken)", "Inactivo (clic para activar)", "Inactif (cliquez pour activer)", "Неактивен (нажмите, чтобы включить)", "未启用（点击启用）" } },
            { "m_pause", new[] { "Duraklat", "Pause", "Pausieren", "Pausar", "Mettre en pause", "Приостановить", "暂停" } },
            { "m_resume", new[] { "Devam et", "Resume", "Fortsetzen", "Reanudar", "Reprendre", "Продолжить", "继续" } },
            { "m_idle", new[] { "Bekleme süresi", "Idle time", "Wartezeit", "Tiempo de espera", "Délai d'attente", "Время ожидания", "等待时间" } },
            { "m_settings", new[] { "Ayarlar...", "Settings...", "Einstellungen...", "Configuración...", "Paramètres...", "Настройки...", "设置..." } },
            { "m_exit", new[] { "Çıkış", "Exit", "Beenden", "Salir", "Quitter", "Выход", "退出" } },
            { "test_now", new[] { "Şimdi test et", "Test now", "Jetzt testen", "Probar ahora", "Tester maintenant", "Проверить", "立即测试" } },

            { "st_active", new[] { "Aktif", "Active", "Aktiv", "Activo", "Actif", "Активен", "已启用" } },
            { "st_paused", new[] { "Duraklatıldı", "Paused", "Pausiert", "En pausa", "En pause", "Приостановлен", "已暂停" } },
            { "st_outside", new[] { "Çalışma saati dışında", "Outside working hours", "Außerhalb der Arbeitszeit", "Fuera del horario", "Hors des heures de travail", "Вне рабочего времени", "非工作时间" } },
            { "st_battery", new[] { "Pilde beklemede", "Waiting on battery", "Pausiert (Akku)", "En espera (batería)", "En attente (batterie)", "Ожидание (батарея)", "等待中（电池）" } },
            { "st_off", new[] { "Kapalı", "Off", "Aus", "Apagado", "Désactivé", "Выключен", "已关闭" } },

            { "d_active", new[] {
                "Fare {0} hareketsiz kalınca otomatik hareket ettirilir.",
                "The mouse is moved automatically after {0} of inactivity.",
                "Die Maus wird nach {0} Inaktivität automatisch bewegt.",
                "El ratón se mueve automáticamente tras {0} de inactividad.",
                "La souris est déplacée automatiquement après {0} d'inactivité.", "Мышь автоматически сдвигается после {0} бездействия.", "鼠标闲置 {0} 后将自动移动。" } },
            { "d_paused", new[] {
                "Saat {0} itibarıyla kendiliğinden devam eder.",
                "Resumes automatically at {0}.",
                "Wird um {0} automatisch fortgesetzt.",
                "Se reanuda automáticamente a las {0}.",
                "Reprend automatiquement à {0}.", "Автоматически продолжит работу в {0}.", "将于 {0} 自动继续。" } },
            { "d_outside", new[] {
                "Çalışma saatleri {0}–{1}. Bu saatler dışında bilgisayar normal davranır.",
                "Working hours {0}–{1}. Outside these hours the computer behaves normally.",
                "Arbeitszeit {0}–{1}. Außerhalb davon verhält sich der Computer normal.",
                "Horario {0}–{1}. Fuera de él, el equipo funciona con normalidad.",
                "Heures de travail {0}–{1}. En dehors, l'ordinateur fonctionne normalement.", "Рабочее время {0}–{1}. В остальное время компьютер работает как обычно.", "工作时间 {0}–{1}。其他时间电脑按正常设置运行。" } },
            { "d_battery", new[] {
                "Şarj kablosu takılınca otomatik olarak devam eder.",
                "Resumes automatically when the charger is plugged in.",
                "Wird automatisch fortgesetzt, sobald das Ladegerät angeschlossen ist.",
                "Se reanuda automáticamente al conectar el cargador.",
                "Reprend automatiquement lorsque le chargeur est branché.", "Продолжит работу при подключении зарядного устройства.", "接通电源后将自动继续。" } },
            { "d_off", new[] {
                "Bilgisayar normal uyku ayarlarına göre davranır.",
                "The computer follows its normal sleep settings.",
                "Der Computer folgt seinen normalen Energieeinstellungen.",
                "El equipo sigue su configuración de suspensión normal.",
                "L'ordinateur suit ses paramètres de veille habituels.", "Компьютер использует обычные настройки сна.", "电脑按正常睡眠设置运行。" } },

            { "s_next", new[] { "sonraki hareket {0}", "next move in {0}", "nächste Bewegung in {0}", "próximo movimiento en {0}", "prochain mouvement dans {0}", "следующее движение через {0}", "{0} 后下一次移动" } },
            { "s_resumes", new[] { "{0} itibarıyla devam", "resumes at {0}", "weiter um {0}", "se reanuda a las {0}", "reprend à {0}", "продолжит в {0}", "{0} 继续" } },
            { "s_starts", new[] { "{0} itibarıyla başlar", "starts at {0}", "beginnt um {0}", "empieza a las {0}", "commence à {0}", "начнёт в {0}", "{0} 开始" } },
            { "s_charger", new[] { "şarjda devam eder", "resumes on charger", "weiter mit Ladegerät", "se reanuda al cargar", "reprend sur secteur", "продолжит от сети", "接通电源后继续" } },
            { "s_noprot", new[] { "koruma yok", "no protection", "kein Schutz", "sin protección", "aucune protection", "защита выключена", "无保护" } },

            { "n_resumed_t", new[] { "MouseBot devam ediyor", "MouseBot resumed", "MouseBot läuft wieder", "MouseBot se ha reanudado", "MouseBot a repris", "MouseBot снова работает", "MouseBot 已继续" } },
            { "n_resumed_b", new[] { "Duraklatma süresi doldu.", "The pause has ended.", "Die Pause ist beendet.", "La pausa ha terminado.", "La pause est terminée.", "Пауза закончилась.", "暂停已结束。" } },
            { "n_paused_t", new[] { "MouseBot duraklatıldı", "MouseBot paused", "MouseBot pausiert", "MouseBot en pausa", "MouseBot en pause", "MouseBot приостановлен", "MouseBot 已暂停" } },
            { "n_paused_b", new[] { "Saat {0} itibarıyla otomatik devam edecek.", "It will resume automatically at {0}.", "Wird um {0} automatisch fortgesetzt.", "Se reanudará automáticamente a las {0}.", "Reprendra automatiquement à {0}.", "Работа продолжится автоматически в {0}.", "将于 {0} 自动继续。" } },
            { "n_on", new[] { "MouseBot açıldı", "MouseBot turned on", "MouseBot eingeschaltet", "MouseBot activado", "MouseBot activé", "MouseBot включён", "MouseBot 已启用" } },
            { "n_off", new[] { "MouseBot kapatıldı", "MouseBot turned off", "MouseBot ausgeschaltet", "MouseBot desactivado", "MouseBot désactivé", "MouseBot выключен", "MouseBot 已关闭" } },
            { "n_hotkey_b", new[] { "Ctrl+Alt+M ile tekrar değiştirebilirsiniz.", "Press Ctrl+Alt+M to toggle it again.", "Mit Strg+Alt+M erneut umschalten.", "Pulsa Ctrl+Alt+M para cambiarlo de nuevo.", "Appuyez sur Ctrl+Alt+M pour basculer à nouveau.", "Нажмите Ctrl+Alt+M, чтобы переключить снова.", "按 Ctrl+Alt+M 可再次切换。" } },
            { "n_bg_t", new[] { "MouseBot arka planda çalışıyor", "MouseBot is running in the background", "MouseBot läuft im Hintergrund", "MouseBot sigue en segundo plano", "MouseBot fonctionne en arrière-plan", "MouseBot работает в фоне", "MouseBot 正在后台运行" } },
            { "n_bg_b", new[] {
                "Tamamen kapatmak için simgeye sağ tıklayıp Çıkış'ı seçin.",
                "To quit completely, right-click the icon and choose Exit.",
                "Zum Beenden mit der rechten Maustaste auf das Symbol klicken und Beenden wählen.",
                "Para cerrarlo del todo, haz clic derecho en el icono y elige Salir.",
                "Pour quitter, faites un clic droit sur l'icône et choisissez Quitter.", "Чтобы выйти полностью, щёлкните значок правой кнопкой и выберите «Выход».", "要完全退出，请右键单击图标并选择“退出”。" } },

            { "days", new[] { "Pzt|Sal|Çar|Per|Cum|Cmt|Paz", "Mon|Tue|Wed|Thu|Fri|Sat|Sun", "Mo|Di|Mi|Do|Fr|Sa|So", "Lun|Mar|Mié|Jue|Vie|Sáb|Dom", "Lun|Mar|Mer|Jeu|Ven|Sam|Dim", "Пн|Вт|Ср|Чт|Пт|Сб|Вс", "周一|周二|周三|周四|周五|周六|周日" } },
            { "hint0", new[] {
                "İmleç yerinden oynamaz; Windows'a yalnızca 1 piksellik ileri-geri sinyal gönderilir. En az fark edilen mod.",
                "The cursor doesn't move; Windows only receives a 1-pixel back-and-forth signal. The least noticeable mode.",
                "Der Mauszeiger bleibt stehen; Windows erhält nur ein 1-Pixel-Signal hin und zurück. Der unauffälligste Modus.",
                "El cursor no se mueve; Windows solo recibe una señal de ida y vuelta de 1 píxel. El modo más discreto.",
                "Le curseur ne bouge pas ; Windows reçoit seulement un signal aller-retour de 1 pixel. Le mode le plus discret.", "Курсор не двигается; Windows получает только сигнал сдвига на 1 пиксель туда и обратно. Самый незаметный режим.", "光标不会移动；仅向 Windows 发送 1 像素的往返信号。最不易察觉的模式。" } },
            { "hint1", new[] {
                "İmleç seçilen piksel kadar kayar ve hemen geri döner. Hareketi gözle görebilirsiniz.",
                "The cursor moves by the chosen number of pixels and returns immediately. The movement is visible.",
                "Der Mauszeiger bewegt sich um die gewählte Pixelzahl und kehrt sofort zurück. Die Bewegung ist sichtbar.",
                "El cursor se desplaza los píxeles elegidos y vuelve de inmediato. El movimiento es visible.",
                "Le curseur se déplace du nombre de pixels choisi puis revient aussitôt. Le mouvement est visible.", "Курсор сдвигается на выбранное число пикселей и сразу возвращается. Движение заметно.", "光标会移动所选像素后立即返回，可以看到移动。" } },
            { "hint2", new[] {
                "Klavyede bulunmayan F15 tuşu sinyali gönderilir. Hiçbir uygulamayı etkilemez; Teams/Slack 'uzakta' durumunu da önler.",
                "Sends the F15 key, which keyboards don't have. It doesn't affect any app and also prevents the Teams/Slack 'away' status.",
                "Sendet die Taste F15, die Tastaturen nicht haben. Beeinflusst keine App und verhindert auch den Teams/Slack-Status „Abwesend“.",
                "Envía la tecla F15, que los teclados no tienen. No afecta a ninguna aplicación y evita el estado 'ausente' de Teams/Slack.",
                "Envoie la touche F15, absente des claviers. N'affecte aucune application et évite le statut « absent » de Teams/Slack.", "Отправляет клавишу F15, которой нет на клавиатурах. Не влияет на программы и не даёт Teams/Slack показать статус «Нет на месте».", "发送键盘上不存在的 F15 键。不会影响任何应用，还能防止 Teams/Slack 显示“离开”状态。" } },

            { "tabs", new[] { "Genel|Zamanlama|Gelişmiş|İstatistik", "General|Schedule|Advanced|Statistics", "Allgemein|Zeitplan|Erweitert|Statistik", "General|Horario|Avanzado|Estadísticas", "Général|Planning|Avancé|Statistiques", "Общие|Расписание|Расширенные|Статистика", "常规|计划|高级|统计" } },
            { "sent", new[] { "✓ Gönderildi", "✓ Sent", "✓ Gesendet", "✓ Enviado", "✓ Envoyé", "✓ Отправлено", "✓ 已发送" } },
            { "send_fail", new[] { "Gönderilemedi", "Failed to send", "Fehlgeschlagen", "No se pudo enviar", "Échec de l'envoi", "Не удалось", "发送失败" } },
            { "defaults", new[] { "Varsayılanlar", "Defaults", "Standard", "Restablecer", "Par défaut", "По умолчанию", "恢复默认" } },
            { "close", new[] { "Kapat", "Close", "Schließen", "Cerrar", "Fermer", "Закрыть", "关闭" } },

            { "idle_time", new[] { "Hareketsizlik süresi", "Idle time", "Inaktivitätsdauer", "Tiempo de inactividad", "Délai d'inactivité", "Время бездействия", "闲置时间" } },
            { "quick", new[] { "Hızlı seçim (dk)", "Quick pick (min)", "Schnellwahl (Min.)", "Selección rápida (min)", "Choix rapide (min)", "Быстрый выбор (мин)", "快速选择（分钟）" } },
            { "move_type", new[] { "Hareket türü", "Movement type", "Bewegungsart", "Tipo de movimiento", "Type de mouvement", "Тип движения", "移动方式" } },
            { "modes", new[] { "Görünmez hareket|Küçük fare hareketi|Tuş sinyali (F15)", "Invisible movement|Small mouse movement|Key signal (F15)", "Unsichtbare Bewegung|Kleine Mausbewegung|Tastensignal (F15)", "Movimiento invisible|Pequeño movimiento|Señal de tecla (F15)", "Mouvement invisible|Petit mouvement|Signal de touche (F15)", "Невидимое движение|Небольшое движение|Сигнал клавиши (F15)", "隐形移动|轻微移动鼠标|按键信号 (F15)" } },
            { "move_amount", new[] { "Hareket miktarı", "Movement distance", "Bewegungsweite", "Distancia", "Distance", "Расстояние", "移动距离" } },
            { "pixels", new[] { "piksel", "pixels", "Pixel", "píxeles", "pixels", "пикселей", "像素" } },

            { "sched_only", new[] { "Sadece belirli saat ve günlerde çalış", "Only run on certain hours and days", "Nur zu bestimmten Zeiten und Tagen", "Solo en ciertas horas y días", "Seulement à certaines heures et jours", "Работать только в заданные часы и дни", "仅在指定时间和日期运行" } },
            { "hours_range", new[] { "Saat aralığı", "Hours", "Uhrzeit", "Horario", "Horaires", "Часы", "时间段" } },
            { "days_lbl", new[] { "Günler", "Days", "Tage", "Días", "Jours", "Дни", "日期" } },
            { "battery", new[] { "Pil ile çalışırken beklemeye al", "Pause while on battery", "Im Akkubetrieb pausieren", "Pausar con batería", "Pause sur batterie", "Приостанавливать при работе от батареи", "使用电池时暂停" } },
            { "sched_hint", new[] {
                "Zamanlama dışında ve pildeyken MouseBot beklemeye geçer; bilgisayar kendi güç ayarlarına göre davranır.",
                "Outside the schedule and on battery, MouseBot waits; the computer follows its own power settings.",
                "Außerhalb des Zeitplans und im Akkubetrieb wartet MouseBot; der Computer folgt seinen Energieeinstellungen.",
                "Fuera del horario y con batería, MouseBot queda en espera; el equipo sigue su configuración de energía.",
                "Hors planning et sur batterie, MouseBot attend ; l'ordinateur suit ses propres réglages d'alimentation.", "Вне расписания и при работе от батареи MouseBot ждёт; компьютер следует своим настройкам питания.", "在计划时间外或使用电池时，MouseBot 会等待；电脑按自身电源设置运行。" } },

            { "opt_api", new[] { "Uyku modunu engelle (Windows güç API'si)", "Prevent sleep (Windows power API)", "Energiesparmodus verhindern (Windows-API)", "Evitar la suspensión (API de Windows)", "Empêcher la veille (API Windows)", "Запретить спящий режим (API Windows)", "阻止睡眠（Windows 电源 API）" } },
            { "opt_display", new[] { "Ekranın kapanmasını engelle", "Keep the display on", "Bildschirm eingeschaltet lassen", "Mantener la pantalla encendida", "Garder l'écran allumé", "Не выключать экран", "保持屏幕常亮" } },
            { "opt_startup", new[] { "Windows açılışında otomatik başlat", "Start with Windows", "Mit Windows starten", "Iniciar con Windows", "Lancer au démarrage de Windows", "Запускать вместе с Windows", "开机时自动启动" } },
            { "opt_notify", new[] { "Bildirimleri göster", "Show notifications", "Benachrichtigungen anzeigen", "Mostrar notificaciones", "Afficher les notifications", "Показывать уведомления", "显示通知" } },
            { "opt_hotkey", new[] { "Kısayol: Ctrl+Alt+M ile aç / kapat", "Shortcut: Ctrl+Alt+M to toggle", "Tastenkürzel: Strg+Alt+M zum Umschalten", "Atajo: Ctrl+Alt+M para activar/desactivar", "Raccourci : Ctrl+Alt+M pour activer/désactiver", "Ctrl+Alt+M: включить / выключить", "快捷键：Ctrl+Alt+M 开启/关闭" } },
            { "err_startup", new[] { "Başlangıç ayarı değiştirilemedi:", "Couldn't change the startup setting:", "Autostart-Einstellung konnte nicht geändert werden:", "No se pudo cambiar el inicio automático:", "Impossible de modifier le démarrage automatique :", "Не удалось изменить автозапуск:", "无法更改开机启动设置：" } },
            { "err_hotkey", new[] {
                "Ctrl+Alt+M başka bir uygulama tarafından kullanılıyor, kısayol etkinleştirilemedi.",
                "Ctrl+Alt+M is used by another application, so the shortcut couldn't be enabled.",
                "Strg+Alt+M wird von einer anderen Anwendung verwendet; das Tastenkürzel konnte nicht aktiviert werden.",
                "Otra aplicación usa Ctrl+Alt+M, así que no se pudo activar el atajo.",
                "Ctrl+Alt+M est utilisé par une autre application ; le raccourci n'a pas pu être activé.", "Сочетание Ctrl+Alt+M занято другой программой, его не удалось включить.", "Ctrl+Alt+M 已被其他应用占用，无法启用快捷键。" } },
            { "theme", new[] { "Tema", "Theme", "Design", "Tema", "Thème", "Тема", "主题" } },
            { "themes", new[] { "Sistem|Açık|Koyu", "System|Light|Dark", "System|Hell|Dunkel", "Sistema|Claro|Oscuro", "Système|Clair|Sombre", "Системная|Светлая|Тёмная", "跟随系统|浅色|深色" } },
            { "language", new[] { "Dil", "Language", "Sprache", "Idioma", "Langue", "Язык", "语言" } },
            { "lang_auto", new[] { "Sistem dili", "System language", "Systemsprache", "Idioma del sistema", "Langue du système", "Язык системы", "系统语言" } },

            { "stat_keys", new[] { "Bugün yapılan hareket|Toplam hareket|Son hareket|Bu oturumun süresi", "Moves today|Total moves|Last move|This session", "Bewegungen heute|Bewegungen gesamt|Letzte Bewegung|Diese Sitzung", "Movimientos hoy|Movimientos totales|Último movimiento|Esta sesión", "Mouvements aujourd'hui|Mouvements au total|Dernier mouvement|Cette session", "Движений сегодня|Всего движений|Последнее движение|Текущий сеанс", "今日移动次数|总移动次数|上次移动|本次运行时长" } },
            { "reset_stats", new[] { "İstatistikleri sıfırla", "Reset statistics", "Statistik zurücksetzen", "Restablecer estadísticas", "Réinitialiser les statistiques", "Сбросить статистику", "重置统计" } },
            { "confirm_stats", new[] { "Tüm istatistikler sıfırlansın mı?", "Reset all statistics?", "Alle Statistiken zurücksetzen?", "¿Restablecer todas las estadísticas?", "Réinitialiser toutes les statistiques ?", "Сбросить всю статистику?", "确定要重置所有统计吗？" } },
            { "settings_word", new[] { "Ayarlar", "Settings", "Einstellungen", "Configuración", "Paramètres", "Настройки", "设置" } },
            { "confirm_defaults", new[] {
                "Tüm ayarlar varsayılan değerlere dönsün mü?\n(İstatistikler, başlangıç ve dil ayarı korunur.)",
                "Restore all settings to their defaults?\n(Statistics, startup and language are kept.)",
                "Alle Einstellungen auf Standardwerte zurücksetzen?\n(Statistik, Autostart und Sprache bleiben erhalten.)",
                "¿Restablecer todos los ajustes predeterminados?\n(Se conservan las estadísticas, el inicio automático y el idioma.)",
                "Rétablir tous les paramètres par défaut ?\n(Les statistiques, le démarrage automatique et la langue sont conservés.)", "Вернуть все настройки по умолчанию?\n(Статистика, автозапуск и язык сохранятся.)", "恢复所有默认设置？\n（统计、开机启动和语言设置将保留。）" } },

            { "hdr_protect", new[] { "koruma açık", "protection on", "Schutz aktiv", "protección activa", "protection active", "защита включена", "保护已开启" } },
            { "r_next", new[] { "sonraki hareket", "until next move", "bis zur nächsten", "hasta el próximo", "avant le prochain", "до движения", "距下次移动" } },
            { "r_resume", new[] { "sonra devam", "until resume", "bis Fortsetzung", "para reanudar", "avant reprise", "до продолжения", "距继续" } },
            { "r_start", new[] { "başlangıç", "start", "Beginn", "inicio", "début", "начало", "开始" } },
            { "r_battery_c", new[] { "Pil", "Battery", "Akku", "Batería", "Batterie", "Батарея", "电池" } },
            { "r_battery", new[] { "şarj bekleniyor", "waiting for charger", "wartet auf Strom", "esperando cargador", "attente du secteur", "ждём зарядку", "等待接通电源" } },
            { "idle_lbl", new[] { "Hareketsiz süre:  {0}", "Idle for:  {0}", "Inaktiv seit:  {0}", "Inactivo:  {0}", "Inactif depuis :  {0}", "Бездействие:  {0}", "闲置时间：  {0}" } },
            { "today_lbl", new[] { "Bugün {0} hareket", "{0} moves today", "Heute {0} Bewegungen", "{0} movimientos hoy", "{0} mouvements aujourd'hui", "Сегодня: {0}", "今日 {0} 次移动" } },
            { "setup_title", new[] { "MouseBot Kurulumu", "MouseBot Setup", "MouseBot-Setup", "Instalación de MouseBot", "Installation de MouseBot", "Установка MouseBot", "MouseBot 安装程序" } },
            { "steps", new[] { "Hoş geldiniz|Seçenekler|Kurulum|Tamamlandı", "Welcome|Options|Install|Finish", "Willkommen|Optionen|Installation|Fertig", "Bienvenida|Opciones|Instalación|Listo", "Bienvenue|Options|Installation|Terminé", "Приветствие|Параметры|Установка|Готово", "欢迎|选项|安装|完成" } },
            { "w_head", new[] { "MouseBot'a hoş geldiniz", "Welcome to MouseBot", "Willkommen bei MouseBot", "Bienvenido a MouseBot", "Bienvenue dans MouseBot", "Добро пожаловать в MouseBot", "欢迎使用 MouseBot" } },
            { "w_body", new[] { "Bu sihirbaz MouseBot'u bilgisayarınıza kuracak.\n\nMouseBot, fare hareketsiz kaldığında bilgisayarın uykuya geçmesini ve ekranın kilitlenmesini engeller.\n\nDevam etmek için İleri'ye tıklayın.", "This wizard will install MouseBot on your computer.\n\nMouseBot keeps your computer from sleeping or locking the screen while the mouse is idle.\n\nClick Next to continue.", "Dieser Assistent installiert MouseBot auf Ihrem Computer.\n\nMouseBot verhindert Energiesparmodus und Bildschirmsperre, solange die Maus nicht bewegt wird.\n\nKlicken Sie auf Weiter, um fortzufahren.", "Este asistente instalará MouseBot en tu equipo.\n\nMouseBot evita que el equipo se suspenda o bloquee la pantalla mientras el ratón está inactivo.\n\nHaz clic en Siguiente para continuar.", "Cet assistant va installer MouseBot sur votre ordinateur.\n\nMouseBot empêche la mise en veille et le verrouillage de l'écran lorsque la souris est inactive.\n\nCliquez sur Suivant pour continuer.", "Мастер установит MouseBot на ваш компьютер.\n\nMouseBot не даёт компьютеру уснуть или заблокировать экран, пока мышь не используется.\n\nНажмите «Далее», чтобы продолжить.", "本向导将在您的电脑上安装 MouseBot。\n\n当鼠标闲置时，MouseBot 可防止电脑进入睡眠或锁定屏幕。\n\n点击“下一步”继续。" } },
            { "w_upgrade", new[] { "MouseBot zaten kurulu; en son sürüme güncellenecek.", "MouseBot is already installed and will be updated.", "MouseBot ist bereits installiert und wird aktualisiert.", "MouseBot ya está instalado y se actualizará.", "MouseBot est déjà installé et sera mis à jour.", "MouseBot уже установлен и будет обновлён.", "MouseBot 已安装，将进行更新。" } },
            { "o_head", new[] { "Kurulum seçenekleri", "Installation options", "Installationsoptionen", "Opciones de instalación", "Options d'installation", "Параметры установки", "安装选项" } },
            { "o_folder", new[] { "Kurulum klasörü", "Install folder", "Installationsordner", "Carpeta de instalación", "Dossier d'installation", "Папка установки", "安装文件夹" } },
            { "o_browse", new[] { "Gözat...", "Browse...", "Durchsuchen...", "Examinar...", "Parcourir...", "Обзор...", "浏览..." } },
            { "o_desktop", new[] { "Masaüstüne kısayol oluştur", "Create a desktop shortcut", "Desktopverknüpfung erstellen", "Crear acceso directo en el escritorio", "Créer un raccourci sur le bureau", "Создать ярлык на рабочем столе", "创建桌面快捷方式" } },
            { "o_note", new[] { "Yönetici izni gerekmez; yalnızca bu kullanıcı için kurulur.", "No administrator rights needed; installs for the current user only.", "Keine Administratorrechte nötig; nur für den aktuellen Benutzer.", "No requiere permisos de administrador; solo para el usuario actual.", "Aucun droit administrateur requis ; pour l'utilisateur actuel uniquement.", "Права администратора не нужны; только для текущего пользователя.", "无需管理员权限；仅为当前用户安装。" } },
            { "o_badpath", new[] { "Lütfen geçerli bir klasör seçin.", "Please choose a valid folder.", "Bitte wählen Sie einen gültigen Ordner.", "Elige una carpeta válida.", "Veuillez choisir un dossier valide.", "Выберите допустимую папку.", "请选择有效的文件夹。" } },
            { "i_head", new[] { "Kuruluyor...", "Installing...", "Wird installiert...", "Instalando...", "Installation...", "Установка...", "正在安装..." } },
            { "i_close_app", new[] { "Çalışan MouseBot kapatılıyor", "Closing running MouseBot", "Laufendes MouseBot wird beendet", "Cerrando MouseBot", "Fermeture de MouseBot", "Закрытие запущенного MouseBot", "正在关闭运行中的 MouseBot" } },
            { "i_copy", new[] { "Dosyalar kopyalanıyor", "Copying files", "Dateien werden kopiert", "Copiando archivos", "Copie des fichiers", "Копирование файлов", "正在复制文件" } },
            { "i_shortcuts", new[] { "Kısayollar oluşturuluyor", "Creating shortcuts", "Verknüpfungen werden erstellt", "Creando accesos directos", "Création des raccourcis", "Создание ярлыков", "正在创建快捷方式" } },
            { "i_register", new[] { "Sisteme kaydediliyor", "Registering with Windows", "Registrierung bei Windows", "Registrando en Windows", "Enregistrement dans Windows", "Регистрация в Windows", "正在注册到 Windows" } },
            { "f_head", new[] { "Kurulum tamamlandı", "Setup complete", "Installation abgeschlossen", "Instalación completada", "Installation terminée", "Установка завершена", "安装完成" } },
            { "f_body", new[] { "MouseBot başarıyla kuruldu. Başlat menüsünden açabilir, Windows Ayarlar > Uygulamalar bölümünden kaldırabilirsiniz.", "MouseBot has been installed. Open it from the Start menu; you can uninstall it from Windows Settings > Apps.", "MouseBot wurde installiert. Starten Sie es über das Startmenü; deinstallieren können Sie es unter Einstellungen > Apps.", "MouseBot se ha instalado. Ábrelo desde el menú Inicio; puedes desinstalarlo en Configuración > Aplicaciones.", "MouseBot a été installé. Ouvrez-le depuis le menu Démarrer ; désinstallez-le via Paramètres > Applications.", "MouseBot установлен. Запускайте его из меню «Пуск»; удалить можно в «Параметры» > «Приложения».", "MouseBot 已成功安装。可从开始菜单打开，并可在 Windows 设置 > 应用 中卸载。" } },
            { "f_launch", new[] { "MouseBot'u şimdi başlat", "Launch MouseBot now", "MouseBot jetzt starten", "Iniciar MouseBot ahora", "Lancer MouseBot maintenant", "Запустить MouseBot сейчас", "立即启动 MouseBot" } },
            { "b_back", new[] { "< Geri", "< Back", "< Zurück", "< Atrás", "< Précédent", "< Назад", "< 上一步" } },
            { "b_next", new[] { "İleri >", "Next >", "Weiter >", "Siguiente >", "Suivant >", "Далее >", "下一步 >" } },
            { "b_install", new[] { "Kur", "Install", "Installieren", "Instalar", "Installer", "Установить", "安装" } },
            { "b_cancel", new[] { "İptal", "Cancel", "Abbrechen", "Cancelar", "Annuler", "Отмена", "取消" } },
            { "b_finish", new[] { "Bitir", "Finish", "Fertig stellen", "Finalizar", "Terminer", "Готово", "完成" } },
            { "c_cancel", new[] { "Kurulumdan çıkılsın mı?", "Exit setup?", "Installation abbrechen?", "¿Salir de la instalación?", "Quitter l'installation ?", "Прервать установку?", "确定要退出安装吗？" } },
            { "e_install", new[] { "Kurulum başarısız oldu:", "Setup failed:", "Installation fehlgeschlagen:", "La instalación falló:", "L'installation a échoué :", "Ошибка установки:", "安装失败：" } },
            { "u_title", new[] { "MouseBot'u kaldır", "Uninstall MouseBot", "MouseBot deinstallieren", "Desinstalar MouseBot", "Désinstaller MouseBot", "Удаление MouseBot", "卸载 MouseBot" } },
            { "u_confirm", new[] { "MouseBot ve tüm ayarları bilgisayarınızdan kaldırılsın mı?", "Remove MouseBot and all its settings from your computer?", "MouseBot und alle Einstellungen von diesem Computer entfernen?", "¿Quitar MouseBot y toda su configuración del equipo?", "Supprimer MouseBot et tous ses paramètres de cet ordinateur ?", "Удалить MouseBot и все его настройки с компьютера?", "确定要从电脑中移除 MouseBot 及其所有设置吗？" } },
            { "u_done", new[] { "MouseBot kaldırıldı.", "MouseBot has been removed.", "MouseBot wurde entfernt.", "MouseBot se ha eliminado.", "MouseBot a été supprimé.", "MouseBot удалён.", "MouseBot 已卸载。" } },
            { "last_short", new[] { "son {0}", "last {0}", "zuletzt {0}", "último {0}", "dernier {0}", "посл. {0}", "上次 {0}" } },
        };
    }

    static class Startup
    {
        const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        const string Name = "MouseBot";
        static string Command { get { return "\"" + Application.ExecutablePath + "\""; } }

        public static bool IsEnabled()
        {
            try { using (var k = Registry.CurrentUser.OpenSubKey(RunKey)) return k != null && k.GetValue(Name) != null; }
            catch { return false; }
        }

        public static void Set(bool on)
        {
            using (var k = Registry.CurrentUser.OpenSubKey(RunKey, true))
            {
                if (k == null) return;
                if (on) k.SetValue(Name, Command); else k.DeleteValue(Name, false);
            }
        }

        // exe taşındıysa başlangıç kaydını yeni konuma güncelle
        public static void RefreshPath()
        {
            try
            {
                using (var k = Registry.CurrentUser.OpenSubKey(RunKey, true))
                {
                    if (k == null) return;
                    var v = k.GetValue(Name) as string;
                    if (v != null && !string.Equals(v, Command, StringComparison.OrdinalIgnoreCase)) k.SetValue(Name, Command);
                }
            }
            catch { }
        }
    }

    // ------------------------------------------------------ Tema ve çizim

    enum EngineState { Off, Paused, OutsideSchedule, OnBattery, Active }

    static class Palette
    {
        public static bool Dark;
        public static Color Bg, Card, Text, Sub, Border, Input, Hover, Accent, Track;
        public static readonly Color Button = Hex(0x16A34A), ButtonHover = Hex(0x15803D);
        public static readonly Color Amber = Hex(0xF59E0B);

        public static void Apply(int theme)
        {
            Dark = theme == 2 || (theme == 0 && SystemIsDark());
            if (Dark)
            {
                Bg = Hex(0x17191E); Card = Hex(0x22252B); Text = Hex(0xF3F4F6); Sub = Hex(0x9CA3AF);
                Border = Hex(0x343842); Input = Hex(0x2B2F37); Hover = Hex(0x353A45); Accent = Hex(0x22C55E); Track = Hex(0x3A3F4A);
            }
            else
            {
                Bg = Hex(0xF3F4F6); Card = Hex(0xFFFFFF); Text = Hex(0x111827); Sub = Hex(0x6B7280);
                Border = Hex(0xE5E7EB); Input = Hex(0xF9FAFB); Hover = Hex(0xEEF0F3); Accent = Hex(0x16A34A); Track = Hex(0xE5E7EB);
            }
        }

        public static bool SystemIsDark()
        {
            try
            {
                using (var k = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    object v = k == null ? null : k.GetValue("AppsUseLightTheme");
                    return v is int && (int)v == 0;
                }
            }
            catch { return false; }
        }

        public static Color StateColor(EngineState st)
        {
            if (st == EngineState.Active) return Accent;
            if (st == EngineState.Off) return Sub;
            return Amber;
        }

        public static Color Hex(int rgb) { return Color.FromArgb((rgb >> 16) & 255, (rgb >> 8) & 255, rgb & 255); }
    }

    static class Gfx
    {
        public static GraphicsPath RoundRect(RectangleF r, float radius)
        {
            var p = new GraphicsPath();
            float d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            if (d <= 0) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        public static void LogoColors(EngineState st, out Color top, out Color bottom)
        {
            if (st == EngineState.Active) { top = Palette.Hex(0x34D399); bottom = Palette.Hex(0x059669); }
            else if (st == EngineState.Off) { top = Palette.Hex(0xA1A1AA); bottom = Palette.Hex(0x52525B); }
            else { top = Palette.Hex(0xFCD34D); bottom = Palette.Hex(0xD97706); }
        }

        // Yuvarlatılmış kare üzerinde beyaz fare simgesi
        public static void DrawLogo(Graphics g, float x, float y, float s, EngineState st)
        {
            Color c1, c2;
            LogoColors(st, out c1, out c2);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            var bg = new RectangleF(x, y, s, s);
            using (var path = RoundRect(bg, s * 0.24f))
            using (var br = new LinearGradientBrush(new PointF(x, y - 1), new PointF(x + s, y + s + 1), c1, c2))
                g.FillPath(br, path);

            float mw = s * 0.46f, mh = s * 0.66f, mx = x + (s - mw) / 2, my = y + s * 0.17f;
            using (var body = RoundRect(new RectangleF(mx, my, mw, mh), mw / 2))
                g.FillPath(Brushes.White, body);

            Color line = Color.FromArgb((c1.R + c2.R) / 2, (c1.G + c2.G) / 2, (c1.B + c2.B) / 2);
            using (var pen = new Pen(line, Math.Max(1f, s * 0.06f)))
            {
                float split = my + mh * 0.40f;
                g.DrawLine(pen, x + s / 2, my, x + s / 2, split);
                g.DrawLine(pen, mx, split, mx + mw, split);
            }
        }

        public static Bitmap RenderLogo(int size, EngineState st)
        {
            var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp)) DrawLogo(g, 0, 0, size, st);
            return bmp;
        }

        public static Icon MakeIcon(int size, EngineState st)
        {
            using (var bmp = RenderLogo(size, st))
            {
                IntPtr h = bmp.GetHicon();
                var icon = (Icon)Icon.FromHandle(h).Clone();
                Native.DestroyIcon(h);
                return icon;
            }
        }

        // build.bat tarafından exe ikonu (MouseBot.ico) üretmek için kullanılır
        public static void WriteIco(string path)
        {
            int[] sizes = { 16, 20, 24, 32, 40, 48, 64, 128, 256 };
            var pngs = new List<byte[]>();
            foreach (int s in sizes)
                using (var bmp = RenderLogo(s, EngineState.Active))
                using (var ms = new MemoryStream()) { bmp.Save(ms, ImageFormat.Png); pngs.Add(ms.ToArray()); }

            using (var w = new BinaryWriter(File.Create(path)))
            {
                w.Write((short)0); w.Write((short)1); w.Write((short)sizes.Length);
                int offset = 6 + 16 * sizes.Length;
                for (int i = 0; i < sizes.Length; i++)
                {
                    byte dim = (byte)(sizes[i] >= 256 ? 0 : sizes[i]);
                    w.Write(dim); w.Write(dim); w.Write((byte)0); w.Write((byte)0);
                    w.Write((short)1); w.Write((short)32);
                    w.Write(pngs[i].Length); w.Write(offset);
                    offset += pngs[i].Length;
                }
                foreach (var p in pngs) w.Write(p);
            }
        }

        public static string Clock(double seconds)
        {
            int s = (int)Math.Max(0, Math.Round(seconds));
            if (s >= 3600) return (s / 3600) + ":" + (s % 3600 / 60).ToString("00") + ":" + (s % 60).ToString("00");
            return (s / 60) + ":" + (s % 60).ToString("00");
        }

        public static string Words(int sec)
        {
            string us = Lang.T("u_s"), um = Lang.T("u_m"), uh = Lang.T("u_h");
            if (sec < 60) return sec + " " + us;
            if (sec >= 3600) return (sec / 3600) + " " + uh + " " + (sec % 3600 / 60) + " " + um;
            int m = sec / 60, s = sec % 60;
            return s == 0 ? m + " " + um : m + " " + um + " " + s + " " + us;
        }
    }

    // ---------------------------------------------------- Özel kontroller

    class ToggleSwitch : Control
    {
        bool on;
        public event EventHandler CheckedChanged;

        public ToggleSwitch()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;
            Size = new Size(44, 24);
        }

        public bool Checked
        {
            get { return on; }
            set
            {
                if (on == value) return;
                on = value;
                Invalidate();
                if (CheckedChanged != null) CheckedChanged(this, EventArgs.Empty);
            }
        }

        protected override void OnClick(EventArgs e) { base.OnClick(e); Focus(); Checked = !Checked; }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space) Checked = !Checked;
            base.OnKeyDown(e);
        }

        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }
        protected override void OnEnabledChanged(EventArgs e) { base.OnEnabledChanged(e); Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(BackColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float h = Height - 3, w = Width - 3;
            var r = new RectangleF(1.5f, 1.5f, w, h);
            Color track = on ? Palette.Accent : (Palette.Dark ? Palette.Hex(0x4B5563) : Palette.Hex(0xD1D5DB));
            if (!Enabled) track = Color.FromArgb(110, track);
            using (var p = Gfx.RoundRect(r, h / 2))
            using (var b = new SolidBrush(track)) g.FillPath(b, p);
            if (Focused && ShowFocusCues)
                using (var p = Gfx.RoundRect(r, h / 2))
                using (var pen = new Pen(Color.FromArgb(120, Palette.Accent), 1.5f)) g.DrawPath(pen, p);
            float d = h - 6;
            float x = on ? r.Right - d - 3 : r.X + 3;
            using (var b = new SolidBrush(Enabled ? Color.White : Color.FromArgb(200, 255, 255, 255)))
                g.FillEllipse(b, x, r.Y + 3, d, d);
        }
    }

    class Ring : Control
    {
        public float Value;
        public Color RingColor = Color.Green;
        public string Center = "", Caption = "";

        public Ring()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(BackColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            float t = Width * 0.075f;
            var r = new RectangleF(t / 2 + 2, t / 2 + 2, Width - t - 4, Height - t - 4);
            using (var pen = new Pen(Palette.Track, t)) g.DrawEllipse(pen, r);
            if (Value > 0.002f)
                using (var pen = new Pen(RingColor, t) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                    g.DrawArc(pen, r, -90, 360f * Math.Min(1f, Value));

            // TextRenderer (GDI) Çince/Rusça gibi yazılar için otomatik yazı tipi yedeği kullanır
            var flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;
            float fs = Width * (Center.Length > 5 ? 0.12f : 0.19f);
            using (var f = Lang.UiFont(fs, true, GraphicsUnit.Pixel))
                TextRenderer.DrawText(g, Center, f, new Rectangle(0, (int)(Height * 0.26f), Width, (int)(Height * 0.32f)), Palette.Text, flags);
            using (var f = Lang.UiFont(Width * 0.082f, false, GraphicsUnit.Pixel))
                TextRenderer.DrawText(g, Caption, f, new Rectangle(4, (int)(Height * 0.56f), Width - 8, (int)(Height * 0.18f)), Palette.Sub, flags);
        }
    }

    class TabStrip : Control
    {
        public string[] Items = new string[0];
        int selected;
        public event EventHandler SelectedChanged;

        public TabStrip()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;
        }

        public int Selected
        {
            get { return selected; }
            set
            {
                if (value == selected || value < 0 || value >= Items.Length) return;
                selected = value;
                Invalidate();
                if (SelectedChanged != null) SelectedChanged(this, EventArgs.Empty);
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (Items.Length > 0) Selected = Math.Min(Items.Length - 1, e.X * Items.Length / Math.Max(1, Width));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(BackColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float w = (float)Width / Math.Max(1, Items.Length);
            using (var pen = new Pen(Palette.Border)) g.DrawLine(pen, 0, Height - 1, Width, Height - 1);
            for (int i = 0; i < Items.Length; i++)
            {
                bool sel = i == selected;
                var rect = new Rectangle((int)(i * w), 0, (int)w, Height - 4);
                using (var f = new Font(Font, sel ? FontStyle.Bold : FontStyle.Regular))
                    TextRenderer.DrawText(g, Items[i], f, rect, sel ? Palette.Accent : Palette.Sub,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                if (sel)
                    using (var b = new SolidBrush(Palette.Accent))
                    using (var p = Gfx.RoundRect(new RectangleF(i * w + w * 0.2f, Height - 3, w * 0.6f, 3), 1.5f))
                        g.FillPath(b, p);
            }
        }
    }

    class Card : Panel
    {
        public Card()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            BackColor = Palette.Card;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Palette.Bg);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            using (var p = Gfx.RoundRect(r, 10))
            {
                using (var b = new SolidBrush(Palette.Card)) g.FillPath(b, p);
                using (var pen = new Pen(Palette.Border)) g.DrawPath(pen, p);
            }
        }
    }

    class LogoBox : Control
    {
        public EngineState State = EngineState.Active;

        public LogoBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            Gfx.DrawLogo(e.Graphics, 0, 0, Math.Min(Width, Height) - 1, State);
        }
    }

    class MenuColors : ProfessionalColorTable
    {
        public override Color ToolStripDropDownBackground { get { return Palette.Card; } }
        public override Color ImageMarginGradientBegin { get { return Palette.Card; } }
        public override Color ImageMarginGradientMiddle { get { return Palette.Card; } }
        public override Color ImageMarginGradientEnd { get { return Palette.Card; } }
        public override Color MenuBorder { get { return Palette.Border; } }
        public override Color MenuItemBorder { get { return Palette.Hover; } }
        public override Color MenuItemSelected { get { return Palette.Hover; } }
        public override Color MenuItemSelectedGradientBegin { get { return Palette.Hover; } }
        public override Color MenuItemSelectedGradientEnd { get { return Palette.Hover; } }
        public override Color SeparatorDark { get { return Palette.Border; } }
        public override Color SeparatorLight { get { return Palette.Card; } }
        public override Color CheckBackground { get { return Palette.Hover; } }
        public override Color CheckSelectedBackground { get { return Palette.Hover; } }
        public override Color CheckPressedBackground { get { return Palette.Hover; } }
        public override Color ButtonSelectedBorder { get { return Palette.Accent; } }
    }

    class MenuRenderer : ToolStripProfessionalRenderer
    {
        public MenuRenderer() : base(new MenuColors()) { RoundedEdges = false; }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = e.Item.Enabled ? Palette.Text : Palette.Sub;
            base.OnRenderItemText(e);
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = Palette.Text;
            base.OnRenderArrow(e);
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = e.ImageRectangle;
            using (var pen = new Pen(Palette.Accent, 2f))
                g.DrawLines(pen, new[] {
                    new PointF(r.Left + r.Width * 0.2f, r.Top + r.Height * 0.52f),
                    new PointF(r.Left + r.Width * 0.42f, r.Top + r.Height * 0.74f),
                    new PointF(r.Left + r.Width * 0.82f, r.Top + r.Height * 0.28f) });
        }
    }

    // ------------------------------------------------------ Ana uygulama

    class MsgWindow : NativeWindow
    {
        readonly TrayApp app;

        public MsgWindow(TrayApp app)
        {
            this.app = app;
            CreateHandle(new CreateParams { Caption = "MouseBot_MsgWindow" });
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == TrayApp.HotkeyId) app.OnHotkey();
            else if (m.Msg == TrayApp.ShowMsg && TrayApp.ShowMsg != 0) app.ShowSettings(-1);
            else if (m.Msg == TrayApp.ExitMsg && TrayApp.ExitMsg != 0) app.BeginExit();
            base.WndProc(ref m);
        }
    }

    class TrayApp : ApplicationContext
    {
        public const int HotkeyId = 1;
        public static readonly int ShowMsg = Native.RegisterWindowMessage("MouseBot_ShowSettings");
        public static readonly int ExitMsg = Native.RegisterWindowMessage("MouseBot_Exit");

        public readonly Settings S;
        public readonly DateTime StartedAt = DateTime.Now;
        public DateTime PausedUntil = DateTime.MinValue;
        public bool HotkeyRegistered;
        public int LastTab;

        readonly NotifyIcon tray;
        readonly System.Windows.Forms.Timer timer;
        readonly MsgWindow msgWin;
        readonly Dictionary<EngineState, Icon> icons = new Dictionary<EngineState, Icon>();
        ToolStripMenuItem miStatus, miToggle, miResume, miQuick;
        SettingsForm form;
        EngineState? shownState;
        DateTime pauseStartedAt, lastAttempt = DateTime.MinValue;
        uint appliedEs;
        int direction = 1;
        bool hiddenHintShown;

        public TrayApp()
        {
            bool firstRun;
            S = Settings.Load(out firstRun);
            Palette.Apply(S.Theme);
            Lang.Apply(S.Language);

            int iconSize = SystemInformation.SmallIconSize.Width;
            foreach (EngineState st in Enum.GetValues(typeof(EngineState)))
                icons[st] = Gfx.MakeIcon(iconSize, st);

            tray = new NotifyIcon { Icon = icons[EngineState.Active], ContextMenuStrip = BuildMenu(), Text = "MouseBot" };
            tray.MouseClick += (s, e) => { if (e.Button == MouseButtons.Left) ShowSettings(-1); };
            tray.Visible = true;

            msgWin = new MsgWindow(this);
            ApplyHotkey();
            Startup.RefreshPath();

            SystemEvents.PowerModeChanged += OnPowerModeChanged;
            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;

            timer = new System.Windows.Forms.Timer { Interval = 1000 };
            timer.Tick += delegate { Tick(); };
            timer.Start();
            Tick();

            if (firstRun)
            {
                S.Save();
                ShowSettings(-1);
                tray.ShowBalloonTip(6000, Lang.T("ready_t"), Lang.T("ready_b"),
                    ToolTipIcon.Info);
            }
        }

        ContextMenuStrip BuildMenu()
        {
            var menu = new ContextMenuStrip { Renderer = new MenuRenderer(), Font = Lang.UiFont(9.5f), ShowImageMargin = true };
            miStatus = new ToolStripMenuItem("") { Enabled = false };
            miToggle = new ToolStripMenuItem(Lang.T("st_active"), null, delegate { SetEnabled(!S.Enabled); }) { ShortcutKeyDisplayString = Lang.T("hk") };

            var miPause = new ToolStripMenuItem(Lang.T("m_pause"));
            foreach (int m in new[] { 15, 30, 60, 120, 240 })
            {
                int mins = m;
                miPause.DropDownItems.Add(Minutes(mins), null, delegate { PauseFor(mins); });
            }
            miResume = new ToolStripMenuItem(Lang.T("m_resume"), null, delegate { Resume(); });

            miQuick = new ToolStripMenuItem(Lang.T("m_idle"));
            foreach (int m in new[] { 1, 2, 3, 4, 5, 10, 15 })
            {
                int mins = m;
                miQuick.DropDownItems.Add(new ToolStripMenuItem(Minutes(mins), null, delegate
                {
                    S.IdleSeconds = mins * 60;
                    SettingsChanged();
                }) { Tag = mins * 60 });
            }

            var miSettings = new ToolStripMenuItem(Lang.T("m_settings"), null, delegate { ShowSettings(-1); });
            miSettings.Font = new Font(menu.Font, FontStyle.Bold);

            menu.Items.Add(miStatus);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(miToggle);
            menu.Items.Add(miPause);
            menu.Items.Add(miResume);
            menu.Items.Add(miQuick);
            menu.Items.Add(Lang.T("test_now"), null, delegate { Jiggle(); });
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(miSettings);
            menu.Items.Add(Lang.T("m_exit"), null, delegate { ExitApp(); });
            menu.Opening += delegate
            {
                var st = State;
                miStatus.Text = StateTitle(st) + " — " + ShortInfo(st);
                miToggle.Checked = S.Enabled;
                miToggle.Text = S.Enabled ? Lang.T("st_active") : Lang.T("m_inactive");
                miResume.Visible = IsPaused;
                foreach (ToolStripMenuItem it in miQuick.DropDownItems) it.Checked = (int)it.Tag == S.IdleSeconds;
            };
            return menu;
        }

        static string Minutes(int mins)
        {
            if (mins >= 60 && mins % 60 == 0) return Lang.F(mins == 60 ? "hour1" : "hours", mins / 60);
            return Lang.F(mins == 1 ? "min1" : "mins", mins);
        }

        // ---- durum

        public bool IsPaused { get { return PausedUntil > DateTime.Now; } }

        public EngineState State
        {
            get
            {
                if (!S.Enabled) return EngineState.Off;
                if (IsPaused) return EngineState.Paused;
                if (S.PauseOnBattery && SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Offline) return EngineState.OnBattery;
                if (S.ScheduleEnabled && !S.InSchedule(DateTime.Now)) return EngineState.OutsideSchedule;
                return EngineState.Active;
            }
        }

        public float PauseProgress
        {
            get
            {
                double total = (PausedUntil - pauseStartedAt).TotalSeconds;
                return total <= 0 ? 0 : (float)Math.Max(0, Math.Min(1, 1 - (PausedUntil - DateTime.Now).TotalSeconds / total));
            }
        }

        public string StateTitle(EngineState st)
        {
            switch (st)
            {
                case EngineState.Active: return Lang.T("st_active");
                case EngineState.Paused: return Lang.T("st_paused");
                case EngineState.OutsideSchedule: return Lang.T("st_outside");
                case EngineState.OnBattery: return Lang.T("st_battery");
                default: return Lang.T("st_off");
            }
        }

        public string StateDesc(EngineState st)
        {
            switch (st)
            {
                case EngineState.Active:
                    return Lang.F("d_active", Gfx.Words(S.IdleSeconds));
                case EngineState.Paused:
                    return Lang.F("d_paused", PausedUntil.ToString("HH:mm"));
                case EngineState.OutsideSchedule:
                    return Lang.F("d_outside", Settings.HhMm(S.StartMin), Settings.HhMm(S.EndMin));
                case EngineState.OnBattery:
                    return Lang.T("d_battery");
                default:
                    return Lang.T("d_off");
            }
        }

        string ShortInfo(EngineState st)
        {
            if (st == EngineState.Active) return Lang.F("s_next", Gfx.Clock(Math.Max(0, S.IdleSeconds - Native.IdleSeconds())));
            if (st == EngineState.Paused) return Lang.F("s_resumes", PausedUntil.ToString("HH:mm"));
            if (st == EngineState.OutsideSchedule) return Lang.F("s_starts", Settings.HhMm(S.StartMin));
            if (st == EngineState.OnBattery) return Lang.T("s_charger");
            return Lang.T("s_noprot");
        }

        // ---- ana döngü

        void Tick()
        {
            if (PausedUntil != DateTime.MinValue && !IsPaused)
            {
                PausedUntil = DateTime.MinValue;
                Notify(Lang.T("n_resumed_t"), Lang.T("n_resumed_b"));
            }

            var st = State;
            if (st == EngineState.Active && Native.IdleSeconds() >= S.IdleSeconds &&
                (DateTime.Now - lastAttempt).TotalSeconds >= Math.Min(S.IdleSeconds, 60))
            {
                lastAttempt = DateTime.Now;
                Jiggle();
            }

            ApplyKeepAwake(st);

            if (shownState != st) { tray.Icon = icons[st]; shownState = st; }
            string tip = "MouseBot • " + StateTitle(st) + "\n" + ShortInfo(st);
            tray.Text = tip.Length > 63 ? tip.Substring(0, 63) : tip;

            if (form != null && !form.IsDisposed && form.Visible) form.RefreshStatus();
        }

        public bool Jiggle()
        {
            bool ok;
            if (S.Mode == 0) ok = Native.NudgeInvisible();
            else if (S.Mode == 2) ok = Native.TapF15();
            else
            {
                int p = S.Pixels * direction;
                ok = Native.MoveMouse(p, p);
                Thread.Sleep(40);
                ok &= Native.MoveMouse(-p, -p);
                direction = -direction;
            }
            if (ok)
            {
                S.RecordJiggle();
                S.Save();
            }
            return ok;
        }

        void ApplyKeepAwake(EngineState st)
        {
            uint want = Native.ES_CONTINUOUS;
            if (st == EngineState.Active)
            {
                if (S.KeepAwakeApi) want |= Native.ES_SYSTEM_REQUIRED;
                if (S.KeepDisplayOn) want |= Native.ES_DISPLAY_REQUIRED;
            }
            if (want == appliedEs) return;
            Native.SetThreadExecutionState(want);
            appliedEs = want;
        }

        // ---- komutlar

        public void SetEnabled(bool on)
        {
            S.Enabled = on;
            if (on) PausedUntil = DateTime.MinValue;
            SettingsChanged();
        }

        void PauseFor(int minutes)
        {
            pauseStartedAt = DateTime.Now;
            PausedUntil = pauseStartedAt.AddMinutes(minutes);
            Tick();
            Notify(Lang.T("n_paused_t"), Lang.F("n_paused_b", PausedUntil.ToString("HH:mm")));
        }

        public void Resume()
        {
            PausedUntil = DateTime.MinValue;
            Tick();
        }

        public void OnHotkey()
        {
            SetEnabled(!S.Enabled);
            Notify(Lang.T(S.Enabled ? "n_on" : "n_off"), Lang.T("n_hotkey_b"));
        }

        public void ApplyHotkey()
        {
            if (HotkeyRegistered) { Native.UnregisterHotKey(msgWin.Handle, HotkeyId); HotkeyRegistered = false; }
            if (S.HotkeyEnabled)
                HotkeyRegistered = Native.RegisterHotKey(msgWin.Handle, HotkeyId,
                    Native.MOD_CONTROL | Native.MOD_ALT | Native.MOD_NOREPEAT, (uint)Keys.M);
        }

        public void SettingsChanged()
        {
            S.Save();
            if (S.HotkeyEnabled != HotkeyRegistered) ApplyHotkey();
            Tick();
        }

        public void Notify(string title, string text)
        {
            if (S.ShowNotifications) tray.ShowBalloonTip(3000, title, text, ToolTipIcon.Info);
        }

        public void NotifyHiddenOnce()
        {
            if (hiddenHintShown) return;
            hiddenHintShown = true;
            Notify(Lang.T("n_bg_t"), Lang.T("n_bg_b"));
        }

        public void ShowSettings(int tab)
        {
            if (form == null || form.IsDisposed) form = new SettingsForm(this);
            form.SelectTab(tab >= 0 ? tab : LastTab);
            form.LoadValues();
            form.Show();
            form.ActiveControl = null; // yanlışlıkla basılan Boşluk tuşu bir anahtarı değiştirmesin
            if (form.WindowState == FormWindowState.Minimized) form.WindowState = FormWindowState.Normal;
            form.Activate();
        }

        public void RebuildForm()
        {
            bool visible = form != null && !form.IsDisposed && form.Visible;
            Point loc = visible ? form.Location : Point.Empty;
            if (form != null) { form.Dispose(); form = null; }
            Palette.Apply(S.Theme);
            if (tray.ContextMenuStrip != null) tray.ContextMenuStrip.Invalidate();
            if (!visible) return;
            form = new SettingsForm(this) { StartPosition = FormStartPosition.Manual, Location = loc };
            ShowSettings(LastTab);
        }

        public void ApplyLanguage()
        {
            Lang.Apply(S.Language);
            var old = tray.ContextMenuStrip;
            tray.ContextMenuStrip = BuildMenu();
            if (old != null) old.Dispose();
            RebuildForm();
            Tick();
        }

        void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume) appliedEs = 0; // uykudan dönüşte yeniden uygula
        }

        void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (S.Theme == 0 && Palette.SystemIsDark() != Palette.Dark) RebuildForm();
        }

        // Mesaj penceresinin kendi WndProc'u içinde yok edilmemesi için çıkışı bir sonraki döngüye bırak
        public void BeginExit()
        {
            var t = new System.Windows.Forms.Timer { Interval = 10 };
            t.Tick += delegate { t.Stop(); t.Dispose(); ExitApp(); };
            t.Start();
        }

        void ExitApp()
        {
            timer.Stop();
            SystemEvents.PowerModeChanged -= OnPowerModeChanged;
            SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
            if (HotkeyRegistered) Native.UnregisterHotKey(msgWin.Handle, HotkeyId);
            msgWin.DestroyHandle();
            Native.SetThreadExecutionState(Native.ES_CONTINUOUS);
            S.Save();
            tray.Visible = false;
            tray.Dispose();
            if (form != null) form.Dispose();
            ExitThread();
        }
    }

    // ------------------------------------------------------ Ayar penceresi

    class SettingsForm : Form
    {
        const int PageW = 426;
        static readonly int[] DayIdx = { 1, 2, 3, 4, 5, 6, 0 };

        readonly TrayApp app;
        readonly Settings S;
        bool loading;

        LogoBox logo;
        Label lblHeader, lblStateTitle, lblStateDesc, lblIdle, lblToday;
        ToggleSwitch tgMain, tgSchedule, tgBattery, tgApi, tgDisplay, tgStartup, tgNotify, tgHotkey;
        Ring ring;
        TabStrip tabs;
        Panel[] pages;
        NumericUpDown numMin, numSec, numPx, numSH, numSM, numEH, numEM;
        ComboBox cmbMode, cmbTheme, cmbLang;
        Label lblModeHint, lblPx, lblPxUnit, lblHours, lblDays;
        CheckBox[] days;
        Label stToday, stTotal, stLast, stUptime;
        Button btnTest;
        readonly System.Windows.Forms.Timer flash = new System.Windows.Forms.Timer { Interval = 1600 };

        public SettingsForm(TrayApp app)
        {
            this.app = app;
            S = app.S;

            SuspendLayout();
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = Lang.UiFont(9.75f);
            Text = "MouseBot";
            ClientSize = new Size(460, 600);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Palette.Bg;
            ForeColor = Palette.Text;
            Icon = Gfx.MakeIcon(32, EngineState.Active);
            KeyPreview = true;

            BuildHeader();
            BuildStatusCard();

            tabs = new TabStrip { Bounds = new Rectangle(16, 276, 428, 34), BackColor = Palette.Bg, Font = Font };
            tabs.Items = Lang.List("tabs");
            tabs.SelectedChanged += delegate { ShowPage(tabs.Selected); };
            Controls.Add(tabs);

            var content = new Card { Bounds = new Rectangle(16, 316, 428, 226) };
            Controls.Add(content);
            pages = new Panel[4];
            for (int i = 0; i < pages.Length; i++)
            {
                pages[i] = new Panel { Bounds = new Rectangle(1, 1, PageW, 224), BackColor = Palette.Card, Visible = i == 0 };
                content.Controls.Add(pages[i]);
            }
            BuildGeneral(pages[0]);
            BuildSchedule(pages[1]);
            BuildAdvanced(pages[2]);
            BuildStats(pages[3]);

            btnTest = Btn(this, Lang.T("test_now"), 16, 556, 140, 32, true);
            btnTest.Click += delegate
            {
                bool ok = app.Jiggle();
                btnTest.Text = Lang.T(ok ? "sent" : "send_fail");
                flash.Stop(); flash.Start();
                RefreshStatus();
            };
            flash.Tick += delegate { flash.Stop(); btnTest.Text = Lang.T("test_now"); };

            var btnDefaults = Btn(this, Lang.T("defaults"), 166, 556, 120, 32, false);
            btnDefaults.Click += delegate { ResetDefaults(); };
            var btnClose = Btn(this, Lang.T("close"), 334, 556, 110, 32, false);
            btnClose.Click += delegate { HideToTray(); };

            ResumeLayout(false);

            FormClosing += (s, e) =>
            {
                if (e.CloseReason == CloseReason.UserClosing) { e.Cancel = true; HideToTray(); }
            };
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) HideToTray(); };
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            try { int v = Palette.Dark ? 1 : 0; Native.DwmSetWindowAttribute(Handle, 20, ref v, 4); } catch { }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) flash.Dispose();
            base.Dispose(disposing);
        }

        void HideToTray()
        {
            Hide();
            app.NotifyHiddenOnce();
        }

        // ---- bölümler

        void BuildHeader()
        {
            var header = new Panel { Bounds = new Rectangle(0, 0, 460, 84), BackColor = Palette.Card };
            Controls.Add(header);
            Controls.Add(new Panel { Bounds = new Rectangle(0, 84, 460, 1), BackColor = Palette.Border });

            logo = new LogoBox { Bounds = new Rectangle(20, 20, 45, 45), BackColor = Palette.Card };
            header.Controls.Add(logo);
            var title = L(header, "MouseBot", 76, 16, false);
            title.Font = Lang.UiFont(15f, true);
            lblHeader = L(header, "", 78, 50, true);

            tgMain = new ToggleSwitch { Bounds = new Rectangle(384, 29, 56, 28), BackColor = Palette.Card };
            tgMain.CheckedChanged += delegate { if (!loading) app.SetEnabled(tgMain.Checked); };
            header.Controls.Add(tgMain);
        }

        void BuildStatusCard()
        {
            var card = new Card { Bounds = new Rectangle(16, 100, 428, 164) };
            Controls.Add(card);
            ring = new Ring { Bounds = new Rectangle(14, 12, 140, 140), BackColor = Palette.Card };
            card.Controls.Add(ring);

            lblStateTitle = L(card, "", 172, 24, false);
            lblStateTitle.Font = Lang.UiFont(12.5f, true);
            lblStateDesc = new Label { Bounds = new Rectangle(174, 54, 244, 54), BackColor = Palette.Card, ForeColor = Palette.Sub };
            card.Controls.Add(lblStateDesc);
            lblIdle = L(card, "", 174, 110, false);
            lblToday = L(card, "", 174, 132, true);
            lblToday.AutoSize = false; lblToday.AutoEllipsis = true; lblToday.Size = new Size(244, 20);
        }

        void BuildGeneral(Panel p)
        {
            int y = 18;
            L(p, Lang.T("idle_time"), 16, y + 3, false);
            numMin = Num(p, 176, y, 58, 0, 120);
            L(p, Lang.T("u_m"), 238, y + 3, true);
            numSec = Num(p, 266, y, 58, 0, 59);
            L(p, Lang.T("u_s"), 328, y + 3, true);
            numMin.ValueChanged += delegate { ApplyIdle(); };
            numSec.ValueChanged += delegate { ApplyIdle(); };

            y += 38;
            L(p, Lang.T("quick"), 16, y + 4, true);
            int[] presets = { 1, 2, 4, 5, 10 };
            for (int i = 0; i < presets.Length; i++)
            {
                int m = presets[i];
                var b = Btn(p, m.ToString(), 176 + i * 48, y, 44, 27, false);
                b.Font = Lang.UiFont(8.75f);
                b.Click += delegate
                {
                    loading = true; numMin.Value = m; numSec.Value = 0; loading = false;
                    ApplyIdle();
                };
            }

            y += 44;
            L(p, Lang.T("move_type"), 16, y + 3, false);
            cmbMode = Combo(p, 176, y, 234, Lang.List("modes"));
            cmbMode.SelectedIndexChanged += delegate
            {
                if (loading) return;
                S.Mode = cmbMode.SelectedIndex;
                app.SettingsChanged();
                UpdateEnables();
            };

            y += 38;
            lblPx = L(p, Lang.T("move_amount"), 16, y + 3, false);
            numPx = Num(p, 176, y, 58, 1, 50);
            lblPxUnit = L(p, Lang.T("pixels"), 238, y + 3, true);
            numPx.ValueChanged += delegate { if (loading) return; S.Pixels = (int)numPx.Value; app.SettingsChanged(); };

            y += 38;
            lblModeHint = new Label { Bounds = new Rectangle(16, y, 394, 46), BackColor = Palette.Card, ForeColor = Palette.Sub };
            p.Controls.Add(lblModeHint);
        }

        void BuildSchedule(Panel p)
        {
            tgSchedule = ToggleRow(p, Lang.T("sched_only"), 14);
            tgSchedule.CheckedChanged += delegate { if (loading) return; S.ScheduleEnabled = tgSchedule.Checked; app.SettingsChanged(); UpdateEnables(); };

            int y = 54;
            lblHours = L(p, Lang.T("hours_range"), 16, y + 3, false);
            numSH = Num(p, 150, y, 48, 0, 23);
            L(p, ":", 200, y + 2, false);
            numSM = Num(p, 210, y, 48, 0, 59);
            L(p, "–", 264, y + 2, false);
            numEH = Num(p, 280, y, 48, 0, 23);
            L(p, ":", 330, y + 2, false);
            numEM = Num(p, 340, y, 48, 0, 59);
            numSM.Increment = numEM.Increment = 5;
            foreach (var n in new[] { numSH, numSM, numEH, numEM })
                n.ValueChanged += delegate
                {
                    if (loading) return;
                    S.StartMin = (int)numSH.Value * 60 + (int)numSM.Value;
                    S.EndMin = (int)numEH.Value * 60 + (int)numEM.Value;
                    app.SettingsChanged();
                };

            y = 94;
            lblDays = L(p, Lang.T("days_lbl"), 16, y + 5, false);
            string[] dayNames = Lang.List("days");
            days = new CheckBox[7];
            for (int i = 0; i < 7; i++)
            {
                var cb = new CheckBox
                {
                    Text = dayNames[i], Appearance = Appearance.Button, FlatStyle = FlatStyle.Flat,
                    TextAlign = ContentAlignment.MiddleCenter, Bounds = new Rectangle(94 + i * 45, y, 44, 28), Padding = Padding.Empty,
                    Font = Lang.UiFont(8.5f), Cursor = Cursors.Hand, BackColor = Palette.Input
                };
                cb.FlatAppearance.BorderColor = Palette.Border;
                cb.FlatAppearance.CheckedBackColor = Palette.Button;
                cb.FlatAppearance.MouseOverBackColor = Palette.Hover;
                cb.CheckedChanged += delegate
                {
                    StyleDay(cb);
                    if (loading) return;
                    int mask = 0;
                    for (int j = 0; j < 7; j++) if (days[j].Checked) mask |= 1 << DayIdx[j];
                    S.Days = mask;
                    app.SettingsChanged();
                };
                days[i] = cb;
                p.Controls.Add(cb);
            }

            tgBattery = ToggleRow(p, Lang.T("battery"), 138);
            tgBattery.CheckedChanged += delegate { if (loading) return; S.PauseOnBattery = tgBattery.Checked; app.SettingsChanged(); };

            p.Controls.Add(new Label
            {
                Bounds = new Rectangle(16, 176, 394, 44), BackColor = Palette.Card, ForeColor = Palette.Sub,
                Text = Lang.T("sched_hint")
            });
        }

        void BuildAdvanced(Panel p)
        {
            tgApi = ToggleRow(p, Lang.T("opt_api"), 12);
            tgDisplay = ToggleRow(p, Lang.T("opt_display"), 44);
            tgStartup = ToggleRow(p, Lang.T("opt_startup"), 76);
            tgNotify = ToggleRow(p, Lang.T("opt_notify"), 108);
            tgHotkey = ToggleRow(p, Lang.T("opt_hotkey"), 140);

            tgApi.CheckedChanged += delegate { if (loading) return; S.KeepAwakeApi = tgApi.Checked; app.SettingsChanged(); };
            tgDisplay.CheckedChanged += delegate { if (loading) return; S.KeepDisplayOn = tgDisplay.Checked; app.SettingsChanged(); };
            tgNotify.CheckedChanged += delegate { if (loading) return; S.ShowNotifications = tgNotify.Checked; app.SettingsChanged(); };
            tgStartup.CheckedChanged += delegate
            {
                if (loading) return;
                try { Startup.Set(tgStartup.Checked); }
                catch (Exception ex)
                {
                    MessageBox.Show(this, Lang.T("err_startup") + "\n" + ex.Message, "MouseBot", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    loading = true; tgStartup.Checked = Startup.IsEnabled(); loading = false;
                }
            };
            tgHotkey.CheckedChanged += delegate
            {
                if (loading) return;
                S.HotkeyEnabled = tgHotkey.Checked;
                app.SettingsChanged();
                if (S.HotkeyEnabled && !app.HotkeyRegistered)
                    MessageBox.Show(this, Lang.T("err_hotkey"),
                        "MouseBot", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            L(p, Lang.T("theme"), 16, 184, false);
            cmbTheme = Combo(p, 84, 181, 110, Lang.List("themes"));
            L(p, Lang.T("language"), 212, 184, false);
            var langItems = new List<string> { Lang.T("lang_auto") };
            langItems.AddRange(Lang.Names);
            cmbLang = Combo(p, 280, 181, 130, langItems.ToArray());
            cmbLang.SelectedIndexChanged += delegate
            {
                if (loading) return;
                string code = cmbLang.SelectedIndex <= 0 ? "auto" : Lang.Codes[cmbLang.SelectedIndex - 1];
                if (code == S.Language) return;
                S.Language = code;
                app.SettingsChanged();
                BeginInvoke(new MethodInvoker(app.ApplyLanguage));
            };
            cmbTheme.SelectedIndexChanged += delegate
            {
                if (loading || cmbTheme.SelectedIndex == S.Theme) return;
                S.Theme = cmbTheme.SelectedIndex;
                app.SettingsChanged();
                BeginInvoke(new MethodInvoker(app.RebuildForm));
            };
        }

        void BuildStats(Panel p)
        {
            string[] keys = Lang.List("stat_keys");
            var values = new Label[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                L(p, keys[i], 16, 18 + i * 30, true);
                values[i] = L(p, "", 230, 18 + i * 30, false);
                values[i].Font = Lang.UiFont(9.75f, true);
            }
            stToday = values[0]; stTotal = values[1]; stLast = values[2]; stUptime = values[3];

            var reset = Btn(p, Lang.T("reset_stats"), 16, 144, 220, 30, false);
            reset.Click += delegate
            {
                if (MessageBox.Show(this, Lang.T("confirm_stats"), "MouseBot", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                S.TotalJiggles = 0; S.TodayJiggles = 0; S.LastJiggleTicks = 0;
                app.SettingsChanged();
                RefreshStatus();
            };

            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            L(p, "MouseBot v" + ver.Major + "." + ver.Minor + "  •  " + Lang.T("settings_word") + ": %APPDATA%\\MouseBot", 16, 192, true);
        }

        // ---- yardımcılar

        Label L(Control parent, string text, int x, int y, bool sub)
        {
            var l = new Label { Text = text, Left = x, Top = y, AutoSize = true, BackColor = parent.BackColor, ForeColor = sub ? Palette.Sub : Palette.Text };
            parent.Controls.Add(l);
            return l;
        }

        NumericUpDown Num(Control parent, int x, int y, int w, int min, int max)
        {
            var n = new NumericUpDown
            {
                Bounds = new Rectangle(x, y, w, 26), Minimum = min, Maximum = max, TextAlign = HorizontalAlignment.Center,
                BackColor = Palette.Input, ForeColor = Palette.Text, BorderStyle = BorderStyle.FixedSingle
            };
            parent.Controls.Add(n);
            return n;
        }

        ComboBox Combo(Control parent, int x, int y, int w, string[] items)
        {
            var c = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, Bounds = new Rectangle(x, y, w, 26),
                BackColor = Palette.Input, ForeColor = Palette.Text
            };
            c.Items.AddRange(items);
            parent.Controls.Add(c);
            return c;
        }

        Button Btn(Control parent, string text, int x, int y, int w, int h, bool primary)
        {
            var b = new Button
            {
                Text = text, Bounds = new Rectangle(x, y, w, h), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                BackColor = primary ? Palette.Button : (parent == this ? Palette.Card : Palette.Input),
                ForeColor = primary ? Color.White : Palette.Text
            };
            b.FlatAppearance.BorderColor = primary ? Palette.Button : Palette.Border;
            b.FlatAppearance.MouseOverBackColor = primary ? Palette.ButtonHover : Palette.Hover;
            b.FlatAppearance.MouseDownBackColor = primary ? Palette.ButtonHover : Palette.Border;
            parent.Controls.Add(b);
            return b;
        }

        ToggleSwitch ToggleRow(Control parent, string text, int y)
        {
            var label = L(parent, text, 16, y + 3, false);
            var t = new ToggleSwitch { Bounds = new Rectangle(PageW - 16 - 44, y, 44, 24), BackColor = parent.BackColor };
            label.Cursor = Cursors.Hand;
            label.Click += delegate { if (t.Enabled) t.Checked = !t.Checked; };
            parent.Controls.Add(t);
            return t;
        }

        void StyleDay(CheckBox cb)
        {
            cb.ForeColor = cb.Checked ? Color.White : Palette.Text;
            cb.FlatAppearance.BorderColor = cb.Checked ? Palette.Button : Palette.Border;
        }

        void ShowPage(int i)
        {
            for (int j = 0; j < pages.Length; j++) pages[j].Visible = j == i;
            app.LastTab = i;
        }

        public void SelectTab(int i)
        {
            tabs.Selected = i;
            ShowPage(tabs.Selected);
        }

        void ApplyIdle()
        {
            if (loading) return;
            if (numMin.Value == 0 && numSec.Value < 10)
            {
                loading = true; numSec.Value = 10; loading = false;
            }
            S.IdleSeconds = (int)numMin.Value * 60 + (int)numSec.Value;
            app.SettingsChanged();
        }

        void UpdateEnables()
        {
            bool px = cmbMode.SelectedIndex == 1;
            numPx.Enabled = lblPx.Enabled = lblPxUnit.Enabled = px;
            lblModeHint.Text = Lang.T("hint" + Math.Max(0, cmbMode.SelectedIndex));
            bool sch = tgSchedule.Checked;
            foreach (var n in new Control[] { numSH, numSM, numEH, numEM, lblHours, lblDays }) n.Enabled = sch;
            foreach (var d in days) d.Enabled = sch;
        }

        void ResetDefaults()
        {
            if (MessageBox.Show(this, Lang.T("confirm_defaults"),
                "MouseBot", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            int oldTheme = S.Theme;
            S.ResetDefaults();
            app.PausedUntil = DateTime.MinValue;
            app.SettingsChanged();
            if (S.Theme != oldTheme) { BeginInvoke(new MethodInvoker(app.RebuildForm)); return; }
            LoadValues();
        }

        public void LoadValues()
        {
            loading = true;
            tgMain.Checked = S.Enabled;
            numMin.Value = Math.Min(120, S.IdleSeconds / 60);
            numSec.Value = S.IdleSeconds % 60;
            cmbMode.SelectedIndex = S.Mode;
            numPx.Value = S.Pixels;
            tgSchedule.Checked = S.ScheduleEnabled;
            numSH.Value = S.StartMin / 60; numSM.Value = S.StartMin % 60;
            numEH.Value = S.EndMin / 60; numEM.Value = S.EndMin % 60;
            for (int i = 0; i < 7; i++) { days[i].Checked = (S.Days & (1 << DayIdx[i])) != 0; StyleDay(days[i]); }
            tgBattery.Checked = S.PauseOnBattery;
            tgApi.Checked = S.KeepAwakeApi;
            tgDisplay.Checked = S.KeepDisplayOn;
            tgStartup.Checked = Startup.IsEnabled();
            tgNotify.Checked = S.ShowNotifications;
            tgHotkey.Checked = S.HotkeyEnabled;
            cmbTheme.SelectedIndex = S.Theme;
            cmbLang.SelectedIndex = S.Language == "auto" ? 0 : Array.IndexOf(Lang.Codes, S.Language) + 1;
            loading = false;
            UpdateEnables();
            RefreshStatus();
        }

        public void RefreshStatus()
        {
            var st = app.State;
            Color sc = Palette.StateColor(st);
            double idle = Native.IdleSeconds();

            if (logo.State != st) { logo.State = st; logo.Invalidate(); }
            lblHeader.Text = app.StateTitle(st) + (st == EngineState.Active ? "  •  " + Lang.T("hdr_protect") : "");
            lblHeader.ForeColor = sc;
            if (tgMain.Checked != S.Enabled) { loading = true; tgMain.Checked = S.Enabled; loading = false; }

            ring.RingColor = sc;
            switch (st)
            {
                case EngineState.Active:
                    ring.Value = (float)Math.Min(1, idle / S.IdleSeconds);
                    ring.Center = Gfx.Clock(S.IdleSeconds - idle);
                    ring.Caption = Lang.T("r_next");
                    break;
                case EngineState.Paused:
                    ring.Value = app.PauseProgress;
                    ring.Center = Gfx.Clock((app.PausedUntil - DateTime.Now).TotalSeconds);
                    ring.Caption = Lang.T("r_resume");
                    break;
                case EngineState.OutsideSchedule:
                    ring.Value = 0; ring.Center = Settings.HhMm(S.StartMin); ring.Caption = Lang.T("r_start");
                    break;
                case EngineState.OnBattery:
                    ring.Value = 0; ring.Center = Lang.T("r_battery_c"); ring.Caption = Lang.T("r_battery");
                    break;
                default:
                    ring.Value = 0; ring.Center = Lang.T("st_off"); ring.Caption = "";
                    break;
            }
            ring.Invalidate();

            lblStateTitle.Text = app.StateTitle(st);
            lblStateTitle.ForeColor = sc;
            lblStateDesc.Text = app.StateDesc(st);
            lblIdle.Text = Lang.F("idle_lbl", Gfx.Clock(idle));
            lblToday.Text = Lang.F("today_lbl", S.TodayCount) +
                (S.LastJiggle.HasValue ? "  •  " + Lang.F("last_short", S.LastJiggle.Value.ToString("HH:mm")) : "");

            stToday.Text = S.TodayCount.ToString();
            stTotal.Text = S.TotalJiggles.ToString();
            stLast.Text = S.LastJiggle.HasValue ? S.LastJiggle.Value.ToString(S.LastJiggle.Value.Date == DateTime.Today ? "HH:mm:ss" : "dd.MM.yyyy HH:mm") : "—";
            stUptime.Text = Gfx.Words((int)(DateTime.Now - app.StartedAt).TotalSeconds);
        }
    }

    // --------------------------------------------------------------- Giriş

    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 2 && args[0] == "--make-icon")
            {
                Gfx.WriteIco(args[1]);
                return;
            }

            bool created;
            using (var mutex = new Mutex(true, "MouseBot_SingleInstance_Mutex", out created))
            {
                if (!created)
                {
                    // Zaten çalışıyor: mevcut pencereyi öne getir
                    Native.PostMessage((IntPtr)0xFFFF, TrayApp.ShowMsg, IntPtr.Zero, IntPtr.Zero);
                    return;
                }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                Application.ThreadException += (s, e) => LogError(e.Exception);
                Application.Run(new TrayApp());
            }
        }

        static void LogError(Exception ex)
        {
            try
            {
                Directory.CreateDirectory(Settings.Dir);
                File.AppendAllText(Path.Combine(Settings.Dir, "error.log"), DateTime.Now.ToString("s") + "  " + ex + Environment.NewLine);
            }
            catch { }
        }
    }
}
