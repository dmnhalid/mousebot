// MouseBot kurulum sihirbazı (MouseBotSetup.exe)
// MouseBot.exe bu dosyaya gömülü kaynak olarak eklenir; ortak kod (tema, diller, çizim) MouseBot.cs'ten gelir.
// Kaldırma: kurulum klasöründeki uninstall.exe /uninstall

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

[assembly: AssemblyTitle("MouseBot Setup")]
[assembly: AssemblyDescription("MouseBot kurulum sihirbazı")]
[assembly: AssemblyProduct("MouseBot")]
[assembly: AssemblyCopyright("MouseBot")]
[assembly: AssemblyVersion("2.1.0.0")]
[assembly: AssemblyFileVersion("2.1.0.0")]

namespace MouseBot
{
    static class Installer
    {
        const string UninstallKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\MouseBot";
        const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

        static string StartMenuLnk { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "MouseBot.lnk"); } }
        static string DesktopLnk { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "MouseBot.lnk"); } }
        static string SettingsFile { get { return Path.Combine(Settings.Dir, "settings.ini"); } }

        public static string Version
        {
            get { var v = Assembly.GetExecutingAssembly().GetName().Version; return v.Major + "." + v.Minor + "." + v.Build; }
        }

        public static string ExistingDir()
        {
            try
            {
                using (var k = Registry.CurrentUser.OpenSubKey(UninstallKey))
                {
                    var v = k == null ? null : k.GetValue("InstallLocation") as string;
                    return string.IsNullOrEmpty(v) ? null : v;
                }
            }
            catch { return null; }
        }

        public static string DefaultDir()
        {
            return ExistingDir() ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "MouseBot");
        }

        public static string SavedLanguage()
        {
            try
            {
                if (!File.Exists(SettingsFile)) return null;
                foreach (var line in File.ReadAllLines(SettingsFile))
                    if (line.StartsWith("Language=")) return line.Substring(9).Trim();
            }
            catch { }
            return null;
        }

        // Önce uygulamadan kibarca çıkmasını iste, olmazsa sonlandır
        public static void CloseRunning()
        {
            int msg = Native.RegisterWindowMessage("MouseBot_Exit");
            if (msg != 0) Native.PostMessage((IntPtr)0xFFFF, msg, IntPtr.Zero, IntPtr.Zero);
            for (int i = 0; i < 30 && Process.GetProcessesByName("MouseBot").Length > 0; i++) Thread.Sleep(100);
            foreach (var p in Process.GetProcessesByName("MouseBot"))
            {
                try { p.Kill(); p.WaitForExit(3000); } catch { }
                finally { p.Dispose(); }
            }
        }

        public static void CopyFiles(string dir)
        {
            Directory.CreateDirectory(dir);
            using (var src = Assembly.GetExecutingAssembly().GetManifestResourceStream("MouseBot.payload.exe"))
            using (var dst = File.Create(Path.Combine(dir, "MouseBot.exe")))
                src.CopyTo(dst);
            string self = Path.GetFullPath(Application.ExecutablePath);
            string un = Path.GetFullPath(Path.Combine(dir, "uninstall.exe"));
            if (!string.Equals(self, un, StringComparison.OrdinalIgnoreCase)) File.Copy(self, un, true);
        }

        public static void CreateShortcuts(string dir, bool desktop)
        {
            string exe = Path.Combine(dir, "MouseBot.exe");
            CreateShortcut(StartMenuLnk, exe);
            if (desktop) CreateShortcut(DesktopLnk, exe);
            else TryDelete(DesktopLnk);
        }

        static void CreateShortcut(string lnk, string target)
        {
            var t = Type.GetTypeFromProgID("WScript.Shell");
            object shell = Activator.CreateInstance(t);
            try
            {
                object sc = t.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, shell, new object[] { lnk });
                var st = sc.GetType();
                st.InvokeMember("TargetPath", BindingFlags.SetProperty, null, sc, new object[] { target });
                st.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, sc, new object[] { Path.GetDirectoryName(target) });
                st.InvokeMember("IconLocation", BindingFlags.SetProperty, null, sc, new object[] { target + ",0" });
                st.InvokeMember("Description", BindingFlags.SetProperty, null, sc, new object[] { "MouseBot" });
                st.InvokeMember("Save", BindingFlags.InvokeMethod, null, sc, null);
                Marshal.ReleaseComObject(sc);
            }
            finally { Marshal.ReleaseComObject(shell); }
        }

        public static void Register(string dir, bool startup, string language)
        {
            string exe = Path.Combine(dir, "MouseBot.exe");
            string un = Path.Combine(dir, "uninstall.exe");
            long size = 0;
            foreach (var f in new[] { exe, un }) if (File.Exists(f)) size += new FileInfo(f).Length;

            using (var k = Registry.CurrentUser.CreateSubKey(UninstallKey))
            {
                k.SetValue("DisplayName", "MouseBot");
                k.SetValue("DisplayVersion", Version);
                k.SetValue("Publisher", "MouseBot");
                k.SetValue("DisplayIcon", exe + ",0");
                k.SetValue("InstallLocation", dir);
                k.SetValue("InstallDate", DateTime.Today.ToString("yyyyMMdd"));
                k.SetValue("UninstallString", "\"" + un + "\" /uninstall");
                k.SetValue("QuietUninstallString", "\"" + un + "\" /uninstall /quiet");
                k.SetValue("NoModify", 1, RegistryValueKind.DWord);
                k.SetValue("NoRepair", 1, RegistryValueKind.DWord);
                k.SetValue("EstimatedSize", (int)(size / 1024), RegistryValueKind.DWord);
            }

            using (var k = Registry.CurrentUser.OpenSubKey(RunKey, true))
            {
                if (k != null)
                {
                    if (startup) k.SetValue("MouseBot", "\"" + exe + "\"");
                    else k.DeleteValue("MouseBot", false);
                }
            }

            WriteLanguage(language);
        }

        // Sihirbazda seçilen dili uygulamaya aktar; yeni kurulumda uygulama ilk açılışta karşılama penceresini gösterir
        static void WriteLanguage(string code)
        {
            Directory.CreateDirectory(Settings.Dir);
            if (File.Exists(SettingsFile))
            {
                var lines = new List<string>();
                foreach (var line in File.ReadAllLines(SettingsFile))
                    if (!line.StartsWith("Language=")) lines.Add(line);
                lines.Add("Language=" + code);
                File.WriteAllLines(SettingsFile, lines);
            }
            else File.WriteAllLines(SettingsFile, new[] { "Language=" + code, "ShowWelcome=1" });
        }

        public static void Uninstall()
        {
            string dir = ExistingDir();
            if (dir == null && Path.GetFileName(Application.ExecutablePath).Equals("uninstall.exe", StringComparison.OrdinalIgnoreCase))
                dir = Path.GetDirectoryName(Application.ExecutablePath);

            CloseRunning();
            try
            {
                using (var k = Registry.CurrentUser.OpenSubKey(RunKey, true))
                    if (k != null) k.DeleteValue("MouseBot", false);
            }
            catch { }
            TryDelete(StartMenuLnk);
            TryDelete(DesktopLnk);
            try { Registry.CurrentUser.DeleteSubKeyTree(UninstallKey, false); } catch { }
            try { if (Directory.Exists(Settings.Dir)) Directory.Delete(Settings.Dir, true); } catch { }

            if (dir == null) return;
            // Çalışan uninstall.exe kendini silemez: kısa bir gecikmeden sonra yalnızca bizim dosyalarımızı
            // ve (boşsa) klasörü sil. rmdir /s kullanılmaz; klasördeki başka dosyalara dokunulmaz.
            string cmd = "/c ping 127.0.0.1 -n 3 > nul & del /f /q \"" + Path.Combine(dir, "MouseBot.exe") + "\" \"" +
                         Path.Combine(dir, "uninstall.exe") + "\" & rmdir \"" + dir + "\"";
            Process.Start(new ProcessStartInfo("cmd.exe", cmd) { CreateNoWindow = true, UseShellExecute = false, WindowStyle = ProcessWindowStyle.Hidden });
        }

        static void TryDelete(string file)
        {
            try { if (File.Exists(file)) File.Delete(file); } catch { }
        }
    }

    // ------------------------------------------------------ Sihirbaz kontrolleri

    class SetupSidebar : Control
    {
        public int Step;

        public SetupSidebar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            using (var br = new LinearGradientBrush(ClientRectangle, Palette.Hex(0x064E3B), Palette.Hex(0x0F766E), 90f))
                g.FillRectangle(br, ClientRectangle);

            float s = Width * 0.42f;
            Gfx.DrawLogo(g, (Width - s) / 2, Height * 0.08f, s, EngineState.Active);

            var center = TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding;
            int y = (int)(Height * 0.08f + s + 12);
            using (var f = Lang.UiFont(15f, true))
                TextRenderer.DrawText(g, "MouseBot", f, new Rectangle(0, y, Width, 32), Color.White, center);
            TextRenderer.DrawText(g, "v" + Installer.Version, Font, new Rectangle(0, y + 32, Width, 20), Color.FromArgb(170, 255, 255, 255), center);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            string[] steps = Lang.List("steps");
            int sy = y + 76, rowH = (int)(Font.Height * 1.9f), d = (int)(Font.Height * 1.2f);
            for (int i = 0; i < steps.Length; i++)
            {
                int ry = sy + i * rowH;
                bool done = i < Step, cur = i == Step;
                Color c = cur ? Color.White : done ? Palette.Hex(0x6EE7B7) : Color.FromArgb(130, 255, 255, 255);
                var circle = new Rectangle(22, ry, d, d);
                if (cur) using (var b = new SolidBrush(Color.White)) g.FillEllipse(b, circle);
                else using (var p = new Pen(c, 1.5f)) g.DrawEllipse(p, circle);
                TextRenderer.DrawText(g, done ? "✓" : (i + 1).ToString(), Font, circle, cur ? Palette.Hex(0x065F46) : c,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                using (var f = new Font(Font, cur ? FontStyle.Bold : FontStyle.Regular))
                    TextRenderer.DrawText(g, steps[i], f, new Rectangle(22 + d + 10, ry, Width - d - 40, d), c,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
            }
        }
    }

    class ProgressLine : Control
    {
        public float Value;

        public ProgressLine()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(BackColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new RectangleF(0, 0, Width - 1, Height - 1);
            using (var p = Gfx.RoundRect(r, r.Height / 2))
            using (var b = new SolidBrush(Palette.Track)) g.FillPath(b, p);
            if (Value > 0)
                using (var p = Gfx.RoundRect(new RectangleF(0, 0, Math.Max(r.Height, r.Width * Math.Min(1, Value)), r.Height), r.Height / 2))
                using (var b = new SolidBrush(Palette.Accent)) g.FillPath(b, p);
        }
    }

    // ------------------------------------------------------ Sihirbaz penceresi

    class SetupForm : Form
    {
        const int SideW = 200, PageX = SideW, PageW = 440, PageH = 380, Pad = 32;
        const int TextW = PageW - Pad * 2;

        readonly bool upgrade;
        int page;
        SetupSidebar side;
        Panel[] pages;
        Button btnBack, btnNext, btnCancel, btnBrowse;
        Label wHead, wBody, wUpgrade, wLangLbl, oHead, oFolder, oDesktop, oStartup, oNote, iHead, iStep, fHead, fBody, fLaunch;
        ComboBox cmbLang;
        TextBox txtDir;
        ToggleSwitch tgDesktop, tgStartup, tgLaunch;
        ProgressLine progress;
        string installedExe;

        public SetupForm()
        {
            upgrade = Installer.ExistingDir() != null;

            SuspendLayout();
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = Lang.UiFont(9.75f);
            ClientSize = new Size(SideW + PageW, PageH + 60);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Palette.Bg;
            ForeColor = Palette.Text;
            Icon = Gfx.MakeIcon(32, EngineState.Active);

            side = new SetupSidebar { Bounds = new Rectangle(0, 0, SideW, PageH + 60), Font = Font };
            Controls.Add(side);

            pages = new Panel[4];
            for (int i = 0; i < 4; i++)
            {
                pages[i] = new Panel { Bounds = new Rectangle(PageX, 0, PageW, PageH), BackColor = Palette.Card, Visible = i == 0 };
                Controls.Add(pages[i]);
            }
            Controls.Add(new Panel { Bounds = new Rectangle(PageX, PageH, PageW, 1), BackColor = Palette.Border });

            BuildWelcome(pages[0]);
            BuildOptions(pages[1]);
            BuildInstalling(pages[2]);
            BuildFinish(pages[3]);

            int by = PageH + 14;
            btnCancel = Btn(PageX + PageW - 16 - 96, by, 96, false);
            btnNext = Btn(PageX + PageW - 16 - 96 - 10 - 116, by, 116, true);
            btnBack = Btn(PageX + PageW - 16 - 96 - 10 - 116 - 6 - 96, by, 96, false);
            btnCancel.Click += delegate { Close(); };
            btnNext.Click += delegate { Next(); };
            btnBack.Click += delegate { ShowPage(page - 1); };
            AcceptButton = btnNext;

            ResumeLayout(false);
            ApplyTexts();
            ShowPage(0);

            FormClosing += (s, e) =>
            {
                if (page == 2) { e.Cancel = true; return; }
                if (page < 2 && e.CloseReason == CloseReason.UserClosing &&
                    MessageBox.Show(this, Lang.T("c_cancel"), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    e.Cancel = true;
            };
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            try { int v = Palette.Dark ? 1 : 0; Native.DwmSetWindowAttribute(Handle, 20, ref v, 4); } catch { }
        }

        // ---- sayfalar

        void BuildWelcome(Panel p)
        {
            wHead = Head(p);
            wBody = Body(p, 84, 160);
            wLangLbl = Lbl(p, Pad, 262, false);
            cmbLang = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat,
                Bounds = new Rectangle(Pad + 150, 258, TextW - 150, 26), BackColor = Palette.Input, ForeColor = Palette.Text
            };
            cmbLang.Items.AddRange(Lang.Names);
            cmbLang.SelectedIndex = Array.IndexOf(Lang.Codes, Lang.Code);
            cmbLang.SelectedIndexChanged += delegate
            {
                Lang.Apply(Lang.Codes[cmbLang.SelectedIndex]);
                ApplyTexts();
            };
            p.Controls.Add(cmbLang);
            wUpgrade = Body(p, 306, 44);
            wUpgrade.ForeColor = Palette.Amber;
            wUpgrade.Visible = upgrade;
        }

        void BuildOptions(Panel p)
        {
            oHead = Head(p);
            oFolder = Lbl(p, Pad, 84, true);
            txtDir = new TextBox
            {
                Bounds = new Rectangle(Pad, 108, TextW - 110, 26), Text = Installer.DefaultDir(),
                BackColor = Palette.Input, ForeColor = Palette.Text, BorderStyle = BorderStyle.FixedSingle
            };
            p.Controls.Add(txtDir);
            btnBrowse = new Button { Bounds = new Rectangle(Pad + TextW - 102, 106, 102, 28) };
            StyleButton(btnBrowse, false, Palette.Input);
            btnBrowse.Click += delegate { Browse(); };
            p.Controls.Add(btnBrowse);

            tgDesktop = Toggle(p, 170, true, out oDesktop);
            tgStartup = Toggle(p, 208, true, out oStartup);
            oNote = Body(p, 262, 44);
            oNote.ForeColor = Palette.Sub;
        }

        void BuildInstalling(Panel p)
        {
            iHead = Head(p);
            iStep = Lbl(p, Pad, 110, true);
            progress = new ProgressLine { Bounds = new Rectangle(Pad, 140, TextW, 8), BackColor = Palette.Card };
            p.Controls.Add(progress);
        }

        void BuildFinish(Panel p)
        {
            fHead = Head(p);
            fBody = Body(p, 84, 100);
            tgLaunch = Toggle(p, 200, true, out fLaunch);
        }

        void ApplyTexts()
        {
            Text = Lang.T("setup_title");
            wHead.Text = Lang.T("w_head");
            wBody.Text = Lang.T("w_body").Replace("\\n", "\n");
            wLangLbl.Text = Lang.T("language") + " / Language";
            wUpgrade.Text = Lang.T("w_upgrade");
            oHead.Text = Lang.T("o_head");
            oFolder.Text = Lang.T("o_folder");
            btnBrowse.Text = Lang.T("o_browse");
            oDesktop.Text = Lang.T("o_desktop");
            oStartup.Text = Lang.T("opt_startup");
            oNote.Text = Lang.T("o_note");
            iHead.Text = Lang.T("i_head");
            fHead.Text = Lang.T("f_head");
            fBody.Text = Lang.T("f_body");
            fLaunch.Text = Lang.T("f_launch");
            btnBack.Text = Lang.T("b_back");
            btnCancel.Text = Lang.T("b_cancel");
            UpdateButtons();
            side.Invalidate();
        }

        void ShowPage(int i)
        {
            page = i;
            for (int j = 0; j < pages.Length; j++) pages[j].Visible = j == i;
            side.Step = i;
            side.Invalidate();
            UpdateButtons();
            ActiveControl = btnNext;
        }

        void UpdateButtons()
        {
            btnBack.Visible = page == 1;
            btnCancel.Visible = page < 2;
            btnNext.Visible = page != 2;
            btnNext.Text = Lang.T(page == 1 ? "b_install" : page == 3 ? "b_finish" : "b_next");
        }

        void Next()
        {
            if (page == 0) ShowPage(1);
            else if (page == 1) StartInstall();
            else if (page == 3)
            {
                if (tgLaunch.Checked && installedExe != null)
                    try { Process.Start(installedExe); } catch { }
                page = 4; // kapatırken onay sorma
                Close();
            }
        }

        void Browse()
        {
            using (var dlg = new FolderBrowserDialog { SelectedPath = txtDir.Text, ShowNewFolderButton = true })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                string path = dlg.SelectedPath;
                if (!Path.GetFileName(path.TrimEnd('\\')).Equals("MouseBot", StringComparison.OrdinalIgnoreCase))
                    path = Path.Combine(path, "MouseBot");
                txtDir.Text = path;
            }
        }

        // ---- kurulum

        void StartInstall()
        {
            string dir;
            try
            {
                dir = Path.GetFullPath(txtDir.Text.Trim());
                if (!Path.IsPathRooted(dir) || dir.Length < 4) throw new ArgumentException();
            }
            catch
            {
                MessageBox.Show(this, Lang.T("o_badpath"), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ShowPage(2);
            bool desktop = tgDesktop.Checked, startup = tgStartup.Checked;
            string lang = Lang.Code;
            var steps = new List<KeyValuePair<string, Action>>
            {
                new KeyValuePair<string, Action>("i_close_app", Installer.CloseRunning),
                new KeyValuePair<string, Action>("i_copy", () => Installer.CopyFiles(dir)),
                new KeyValuePair<string, Action>("i_shortcuts", () => Installer.CreateShortcuts(dir, desktop)),
                new KeyValuePair<string, Action>("i_register", () => Installer.Register(dir, startup, lang)),
            };

            int n = 0;
            var timer = new System.Windows.Forms.Timer { Interval = 250 };
            timer.Tick += delegate
            {
                if (n == steps.Count)
                {
                    timer.Stop(); timer.Dispose();
                    installedExe = Path.Combine(dir, "MouseBot.exe");
                    ShowPage(3);
                    return;
                }
                iStep.Text = Lang.T(steps[n].Key) + "...";
                iStep.Refresh();
                try { steps[n].Value(); }
                catch (Exception ex)
                {
                    timer.Stop(); timer.Dispose();
                    MessageBox.Show(this, Lang.T("e_install") + "\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    progress.Value = 0; progress.Invalidate();
                    ShowPage(1);
                    return;
                }
                n++;
                progress.Value = (float)n / steps.Count;
                progress.Invalidate();
            };
            timer.Start();
        }

        // ---- yardımcılar

        Label Head(Panel p)
        {
            var l = new Label { Bounds = new Rectangle(Pad, 32, TextW, 36), Font = Lang.UiFont(15f, true), BackColor = p.BackColor, ForeColor = Palette.Text };
            p.Controls.Add(l);
            return l;
        }

        Label Body(Panel p, int y, int h)
        {
            var l = new Label { Bounds = new Rectangle(Pad, y, TextW, h), BackColor = p.BackColor, ForeColor = Palette.Text };
            p.Controls.Add(l);
            return l;
        }

        Label Lbl(Panel p, int x, int y, bool sub)
        {
            var l = new Label { Left = x, Top = y, AutoSize = true, BackColor = p.BackColor, ForeColor = sub ? Palette.Sub : Palette.Text };
            p.Controls.Add(l);
            return l;
        }

        ToggleSwitch Toggle(Panel p, int y, bool on, out Label label)
        {
            var t = new ToggleSwitch { Bounds = new Rectangle(Pad + TextW - 44, y, 44, 24), BackColor = p.BackColor, Checked = on };
            var l = new Label { Bounds = new Rectangle(Pad, y + 3, TextW - 56, 22), BackColor = p.BackColor, ForeColor = Palette.Text, Cursor = Cursors.Hand };
            l.Click += delegate { t.Checked = !t.Checked; };
            p.Controls.Add(l);
            p.Controls.Add(t);
            label = l;
            return t;
        }

        Button Btn(int x, int y, int w, bool primary)
        {
            var b = new Button { Bounds = new Rectangle(x, y, w, 32) };
            StyleButton(b, primary, Palette.Card);
            Controls.Add(b);
            return b;
        }

        static void StyleButton(Button b, bool primary, Color bg)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.Cursor = Cursors.Hand;
            b.UseVisualStyleBackColor = false;
            b.BackColor = primary ? Palette.Button : bg;
            b.ForeColor = primary ? Color.White : Palette.Text;
            b.FlatAppearance.BorderColor = primary ? Palette.Button : Palette.Border;
            b.FlatAppearance.MouseOverBackColor = primary ? Palette.ButtonHover : Palette.Hover;
            b.FlatAppearance.MouseDownBackColor = primary ? Palette.ButtonHover : Palette.Border;
        }
    }

    static class SetupProgram
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Palette.Apply(0);
            Lang.Apply(Installer.SavedLanguage() ?? "auto");

            bool uninstall = false, quiet = false;
            foreach (var a in args)
            {
                if (a.Equals("/uninstall", StringComparison.OrdinalIgnoreCase)) uninstall = true;
                if (a.Equals("/quiet", StringComparison.OrdinalIgnoreCase)) quiet = true;
            }

            if (uninstall)
            {
                if (!quiet && MessageBox.Show(Lang.T("u_confirm"), Lang.T("u_title"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                try
                {
                    Installer.Uninstall();
                    if (!quiet) MessageBox.Show(Lang.T("u_done"), Lang.T("u_title"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    if (!quiet) MessageBox.Show(ex.Message, Lang.T("u_title"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }

            using (var mutex = new Mutex(false, "MouseBot_Setup_Mutex"))
            {
                if (!mutex.WaitOne(0)) return;
                Application.Run(new SetupForm());
            }
        }
    }
}
