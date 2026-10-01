; MouseBot — kurulum betiği (Inno Setup 6.5+)
; Derlemek için: build.bat (önce MouseBot.exe'yi ve sihirbaz görsellerini üretir, sonra bu betiği derler)
;
; Sihirbaz: Hoş geldiniz → Dil seçimi → Kurulum klasörü → Tercihler → Ek görevler → Hazır → Son
; - 7 dil; seçilen dil uygulamanın arayüz dili olur (%APPDATA%\MouseBot\settings.ini, Language=)
; - Güncellemede sihirbaz uygulamanın mevcut dilinde açılır; klasör ve tercih adımları atlanır, ayarlar korunur
; - Kullanıcı bazlı kurulum: yönetici izni gerekmez (varsayılan %LOCALAPPDATA%\Programs\MouseBot)
; - Eski (v2.1 ve öncesi) kendi sihirbazıyla yapılmış kurulum aynı klasörde devralınır ve kaydı temizlenir
; - Kaldırma: onay kutusu yerine "Kaldırma Seçenekleri" sayfası (ayarları koru / sil)

#define MyAppName "MouseBot"
#define MyAppPublisher "Halid DUMAN"
#define MyAppExeName "MouseBot.exe"
#define MyAppGuid "1C452B30-C661-48E3-8CC5-B049B6FEEA41"
#define VerFull GetVersionNumbersString(AddBackslash(SourcePath) + "..\" + MyAppExeName)
#define MyAppVersion Copy(VerFull, 1, RPos(".", VerFull) - 1)

[Setup]
AppId={{{#MyAppGuid}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
VersionInfoVersion={#VerFull}
VersionInfoProductName={#MyAppName}
VersionInfoDescription={#MyAppName} Setup

; Kullanıcı bazlı kurulum (yönetici izni istemez). Klasör seçilebilir; güncellemede önceki klasör kullanılır
PrivilegesRequired=lowest
DefaultDirName={code:GetDefaultDir}
DisableDirPage=auto
DisableProgramGroupPage=yes
UsePreviousAppDir=yes
DisableWelcomePage=no
MinVersion=10.0

OutputDir=..\dist
OutputBaseFilename=MouseBotSetup
SetupIconFile=..\MouseBot.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}
WizardStyle=modern
WizardImageFile=images\wizard_164.bmp,images\wizard_192.bmp,images\wizard_246.bmp,images\wizard_273.bmp,images\wizard_328.bmp,images\wizard_355.bmp,images\wizard_410.bmp
WizardSmallImageFile=images\wizard_small_55.bmp,images\wizard_small_64.bmp,images\wizard_small_83.bmp,images\wizard_small_92.bmp,images\wizard_small_110.bmp,images\wizard_small_119.bmp,images\wizard_small_138.bmp
Compression=lzma2/ultra64
SolidCompression=yes

; Çalışan MouseBot'u kurulum kendisi kapatır (önce kibarca, olmazsa zorla); Windows'un "uygulamaları kapat" sorusu gerekmez
CloseApplications=no
RestartApplications=no

; Dil ayrı bir pencerede değil sihirbazın "Dil Seçimi" adımında seçilir. Kurulum Windows dilinde başlar;
; güncellemede uygulamanın mevcut dilinde yeniden açılır. Farklı dil seçilip İleri'ye basılınca kurulum
; /LANG=... /LANGCHOSEN ile yeniden açılır ve bir sonraki adımdan devam eder (Inno Setup sihirbaz dilini açıkken değiştiremez).
ShowLanguageDialog=no
LanguageDetectionMethod=uilanguage

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
; Inno Setup'ın kurulu sürümünde yok; depodaki kopya kullanılır (jrsoftware/issrc, Files\Languages)
Name: "chinesesimplified"; MessagesFile: "Languages\ChineseSimplified.isl"

; Inno Setup'ın hazır Türkçe çevirisindeki yazım hataları ve çeviri kokan ifadeler,
; Windows'un kendi Türkçe terimleriyle (Sihirbaz, İleri, Geri, Son, Gözat) düzeltildi
[Messages]
turkish.SetupAppTitle=Kurulum
turkish.SetupWindowTitle=%1 Kurulumu
turkish.UninstallAppTitle=Kaldır
turkish.UninstallAppFullTitle=%1 Kaldırma
turkish.ExitSetupTitle=Kurulumdan Çık
turkish.ExitSetupMessage=Kurulum henüz tamamlanmadı. Şimdi çıkarsanız program kurulmayacak.%n%nKurulumu daha sonra yeniden çalıştırarak tamamlayabilirsiniz.%n%nKurulumdan çıkmak istiyor musunuz?
turkish.ButtonBack=< &Geri
turkish.ButtonNext=İ&leri >
turkish.ButtonInstall=&Kur
turkish.ButtonFinish=&Son
turkish.ButtonBrowse=&Gözat...
turkish.ButtonWizardBrowse=Göza&t...
turkish.ButtonNewFolder=Ye&ni Klasör
turkish.SelectLanguageTitle=Kurulum Dilini Seçin
turkish.BrowseDialogTitle=Klasöre Gözat
turkish.BrowseDialogLabel=Listeden bir klasör seçip Tamam'a tıklayın.
turkish.WelcomeLabel1=[name] Kurulum Sihirbazına Hoş Geldiniz
turkish.ClickNext=Devam etmek için İleri'ye, çıkmak için İptal'e tıklayın.
turkish.WizardSelectDir=Kurulum Klasörünü Seçin
turkish.SelectDirDesc=[name] nereye kurulsun?
turkish.SelectDirLabel3=[name] aşağıdaki klasöre kurulacak.
turkish.SelectDirBrowseLabel=Devam etmek için İleri'ye tıklayın. Farklı bir klasör seçmek için Gözat'a tıklayın.
turkish.DiskSpaceMBLabel=En az [mb] MB boş disk alanı gerekir.
turkish.DirExistsTitle=Klasör Zaten Var
turkish.DirExists=%n%n%1%n%nklasörü zaten var. Yine de bu klasöre kurmak istiyor musunuz?
turkish.WizardSelectTasks=Ek Görevleri Seçin
turkish.SelectTasksDesc=Hangi ek görevler yapılsın?
turkish.SelectTasksLabel2=[name] kurulurken yapılmasını istediğiniz ek görevleri seçip İleri'ye tıklayın.
turkish.WizardReady=Kuruluma Hazır
turkish.ReadyLabel1=[name] bilgisayarınıza kurulmaya hazır.
turkish.ReadyLabel2a=Kurulumu başlatmak için Kur'a, seçimlerinizi gözden geçirmek veya değiştirmek için Geri'ye tıklayın.
turkish.ReadyLabel2b=Kurulumu başlatmak için Kur'a tıklayın.
turkish.ReadyMemoDir=Kurulum klasörü:
turkish.ReadyMemoTasks=Ek görevler:
turkish.WizardPreparing=Kuruluma Hazırlanıyor
turkish.PreparingDesc=[name] bilgisayarınıza kurulmaya hazırlanıyor.
turkish.WizardInstalling=Kuruluyor
turkish.InstallingLabel=[name] bilgisayarınıza kurulurken lütfen bekleyin.
turkish.FinishedHeadingLabel=[name] Kurulumu Tamamlandı
turkish.ClickFinish=Kurulumdan çıkmak için Son'a tıklayın.
turkish.StatusClosingApplications=Uygulamalar kapatılıyor...
turkish.StatusCreateDirs=Klasörler oluşturuluyor...
turkish.StatusExtractFiles=Dosyalar kopyalanıyor...
turkish.StatusCreateIcons=Kısayollar oluşturuluyor...
turkish.StatusCreateRegistryEntries=Kayıt defteri girdileri oluşturuluyor...
turkish.StatusRunProgram=Kurulum tamamlanıyor...
turkish.ConfirmUninstall=%1 uygulamasını ve tüm bileşenlerini kaldırmak istediğinizden emin misiniz?
turkish.UninstallStatusLabel=%1 bilgisayarınızdan kaldırılırken lütfen bekleyin.
turkish.UninstalledAll=%1 bilgisayarınızdan başarıyla kaldırıldı.
turkish.WizardUninstalling=Kaldırma Durumu
turkish.StatusUninstalling=%1 kaldırılıyor...

turkish.WelcomeLabel2=Bu sihirbaz, [name/ver] uygulamasını bilgisayarınıza kuracaktır.%n%nMouseBot, saatin yanında fare simgesi olarak çalışır. Fare bir süre hareketsiz kaldığında imleci çok hafifçe oynatarak bilgisayarın uykuya geçmesini, ekranın kapanmasını ve kilitlenmesini engeller.%n%nKurulum için yönetici izni gerekmez; bilgisayarınıza başka bir program kurulmaz.
turkish.FinishedLabel=[name] bilgisayarınıza kuruldu.%n%nUygulamaya saatin yanındaki fare simgesinden ulaşabilirsiniz. Simge görünmüyorsa ^ okuna tıklayıp görev çubuğuna sürükleyin. Tercihlerinizi daha sonra uygulamadaki Ayarlar penceresinden değiştirebilirsiniz.
english.WelcomeLabel2=This wizard will install [name/ver] on your computer.%n%nMouseBot runs as the mouse icon next to the clock. When the mouse has been idle for a while, it nudges the cursor very slightly to keep the computer from sleeping, turning off the screen or locking.%n%nNo administrator rights are required and no other programs are installed.
english.FinishedLabel=[name] has been installed on your computer.%n%nYou can reach the app from the mouse icon next to the clock. If you can't see it, click the ^ arrow and drag it onto the taskbar. You can change your preferences later in the app's Settings window.
german.WelcomeLabel2=Dieser Assistent installiert [name/ver] auf Ihrem Computer.%n%nMouseBot läuft als Maussymbol neben der Uhr. Wenn die Maus eine Weile nicht bewegt wurde, bewegt es den Mauszeiger ganz leicht und verhindert so Energiesparmodus, Abschalten des Bildschirms und Bildschirmsperre.%n%nEs sind keine Administratorrechte erforderlich, und es werden keine weiteren Programme installiert.
german.FinishedLabel=[name] wurde auf Ihrem Computer installiert.%n%nSie erreichen die App über das Maussymbol neben der Uhr. Falls es nicht sichtbar ist, klicken Sie auf den Pfeil ^ und ziehen Sie es in die Taskleiste. Ihre Einstellungen können Sie später im Einstellungsfenster der App ändern.
spanish.WelcomeLabel2=Este asistente instalará [name/ver] en tu equipo.%n%nMouseBot se ejecuta como el icono del ratón junto al reloj. Cuando el ratón lleva un rato inactivo, mueve el cursor muy ligeramente para evitar que el equipo se suspenda, apague la pantalla o se bloquee.%n%nNo se necesitan permisos de administrador y no se instala ningún otro programa.
spanish.FinishedLabel=[name] se ha instalado en tu equipo.%n%nPuedes abrir la aplicación desde el icono del ratón junto al reloj. Si no lo ves, haz clic en la flecha ^ y arrástralo a la barra de tareas. Puedes cambiar tus preferencias más adelante en la ventana de Configuración.
french.WelcomeLabel2=Cet assistant va installer [name/ver] sur votre ordinateur.%n%nMouseBot s'exécute sous forme d'icône de souris à côté de l'horloge. Lorsque la souris reste inactive un moment, il déplace très légèrement le curseur pour empêcher la mise en veille, l'extinction de l'écran et le verrouillage.%n%nAucun droit d'administrateur n'est nécessaire et aucun autre programme n'est installé.
french.FinishedLabel=[name] a été installé sur votre ordinateur.%n%nVous pouvez ouvrir l'application depuis l'icône de souris à côté de l'horloge. Si elle n'apparaît pas, cliquez sur la flèche ^ et glissez-la dans la barre des tâches. Vous pourrez modifier vos préférences plus tard dans la fenêtre Paramètres.
russian.WelcomeLabel2=Мастер установит [name/ver] на ваш компьютер.%n%nMouseBot работает как значок мыши рядом с часами. Когда мышь некоторое время не используется, он слегка сдвигает курсор, чтобы компьютер не уснул, экран не погас и не заблокировался.%n%nПрава администратора не требуются, другие программы не устанавливаются.
russian.FinishedLabel=[name] установлен на ваш компьютер.%n%nОткрыть программу можно через значок мыши рядом с часами. Если его не видно, нажмите стрелку ^ и перетащите значок на панель задач. Изменить параметры можно позже в окне настроек программы.
chinesesimplified.WelcomeLabel2=本向导将在您的电脑上安装 [name/ver]。%n%nMouseBot 以鼠标图标的形式在时钟旁运行。当鼠标闲置一段时间后，它会轻微移动光标，防止电脑进入睡眠、关闭屏幕或锁定。%n%n无需管理员权限，也不会安装其他程序。
chinesesimplified.FinishedLabel=[name] 已安装到您的电脑上。%n%n您可以通过时钟旁的鼠标图标打开程序。如果看不到图标，请点击 ^ 箭头并将其拖到任务栏上。之后可以在程序的设置窗口中更改首选项。

[CustomMessages]
turkish.StartupTask=Windows açılışında başlat (önerilir)
turkish.DesktopIconTask=Masaüstüne kısayol oluştur
turkish.LaunchApp=%1 uygulamasını başlat
turkish.ShortcutComment=Fare hareketsizken bilgisayarın uykuya geçmesini engeller
turkish.LangTitle=Dil Seçimi
turkish.LangDesc=Kurulum ve uygulama hangi dilde olsun?
turkish.LangIntro=Kurulum sihirbazı ve MouseBot seçtiğiniz dilde görüntülenecek.
turkish.LangNote=Dili daha sonra uygulamada Ayarlar > Gelişmiş > Dil bölümünden değiştirebilirsiniz.
turkish.PrefsTitle=Tercihler
turkish.PrefsDesc=MouseBot'un ilk ayarları. Hepsini daha sonra Ayarlar penceresinden değiştirebilirsiniz.
turkish.PrefsIdle=Fare bu kadar süre hareketsiz kalınca hareket ettirilsin:
turkish.PrefsMode=Hareket türü:
turkish.PrefsMode0=Görünmez hareket
turkish.PrefsMode1=Küçük fare hareketi
turkish.PrefsMode2=Tuş sinyali (F15)
turkish.PrefsDisplay=Ekranın kapanmasını engelle
turkish.PrefsLid=Kapak kapanınca uykuya geçmesine izin ver (dizüstü bilgisayarlar)
turkish.PrefsTeams=Teams'i yalnızca hafta içi 09:00–18:00 arası açık tut
turkish.PrefsBattery=Pil ile çalışırken beklemeye al
turkish.PrefsLocal=Ayarlarınız yalnızca bu bilgisayarda saklanır.
turkish.Minutes=%1 dakika
turkish.MemoLanguage=Dil:
turkish.MemoPrefs=Tercihler:
turkish.MemoIdle=Hareketsizlik süresi:
turkish.MemoMode=Hareket türü:
turkish.MemoKeep=Mevcut ayarlarınız korunacak.
turkish.UninstallOptionsTitle=Kaldırma Seçenekleri
turkish.UninstallOptionsDesc=Ayarlarınızla ne yapılacağını seçin.
turkish.UninstallIntro=%1 bilgisayarınızdan kaldırılacak.
turkish.UninstallKeep=Ayarlarımı koru (önerilen)
turkish.UninstallKeepHint=Ayarlarınız ve istatistikleriniz bu bilgisayarda kalır; yeniden kurduğunuzda kaldığınız yerden devam edersiniz.
turkish.UninstallAll=Ayarlarımı ve istatistiklerimi sil
turkish.UninstallAllHint=Ayarlar, hareket istatistikleri ve hata günlüğü silinir.
turkish.UninstallWarning=Silinen veriler geri alınamaz.
turkish.UninstallNoData=Bu bilgisayarda MouseBot'a ait kayıtlı ayar bulunmuyor.
turkish.UninstallButton=&Kaldır

english.StartupTask=Start with Windows (recommended)
english.DesktopIconTask=Create a desktop shortcut
english.LaunchApp=Launch %1
english.ShortcutComment=Keeps the computer awake while the mouse is idle
english.LangTitle=Language
english.LangDesc=Which language should setup and the app use?
english.LangIntro=The setup wizard and MouseBot will be shown in the language you choose.
english.LangNote=You can change the language later in the app under Settings > Advanced > Language.
english.PrefsTitle=Preferences
english.PrefsDesc=Initial settings of MouseBot. You can change all of them later in the Settings window.
english.PrefsIdle=Move the mouse after it has been idle for:
english.PrefsMode=Movement type:
english.PrefsMode0=Invisible movement
english.PrefsMode1=Small mouse movement
english.PrefsMode2=Key signal (F15)
english.PrefsDisplay=Keep the display on
english.PrefsLid=Let the laptop sleep when the lid is closed
english.PrefsTeams=Keep Teams open only on weekdays 09:00–18:00
english.PrefsBattery=Pause while on battery
english.PrefsLocal=Your settings are stored only on this computer.
english.Minutes=%1 minutes
english.MemoLanguage=Language:
english.MemoPrefs=Preferences:
english.MemoIdle=Idle time:
english.MemoMode=Movement type:
english.MemoKeep=Your existing settings will be kept.
english.UninstallOptionsTitle=Uninstall Options
english.UninstallOptionsDesc=Choose what to do with your settings.
english.UninstallIntro=%1 will be removed from your computer.
english.UninstallKeep=Keep my settings (recommended)
english.UninstallKeepHint=Your settings and statistics stay on this computer; if you reinstall, you continue where you left off.
english.UninstallAll=Delete my settings and statistics
english.UninstallAllHint=Settings, movement statistics and the error log are deleted.
english.UninstallWarning=Deleted data cannot be recovered.
english.UninstallNoData=There are no saved MouseBot settings on this computer.
english.UninstallButton=&Uninstall

german.StartupTask=Mit Windows starten (empfohlen)
german.DesktopIconTask=Desktopverknüpfung erstellen
german.LaunchApp=%1 starten
german.ShortcutComment=Verhindert den Energiesparmodus, solange die Maus nicht bewegt wird
german.LangTitle=Sprache
german.LangDesc=In welcher Sprache sollen Setup und App angezeigt werden?
german.LangIntro=Der Setup-Assistent und MouseBot werden in der gewählten Sprache angezeigt.
german.LangNote=Sie können die Sprache später in der App unter Einstellungen > Erweitert > Sprache ändern.
german.PrefsTitle=Einstellungen
german.PrefsDesc=Anfangseinstellungen von MouseBot. Alle lassen sich später im Einstellungsfenster ändern.
german.PrefsIdle=Maus bewegen, wenn sie so lange nicht benutzt wurde:
german.PrefsMode=Bewegungsart:
german.PrefsMode0=Unsichtbare Bewegung
german.PrefsMode1=Kleine Mausbewegung
german.PrefsMode2=Tastensignal (F15)
german.PrefsDisplay=Bildschirm eingeschaltet lassen
german.PrefsLid=Beim Zuklappen Energiesparen zulassen (Laptops)
german.PrefsTeams=Teams nur werktags 09:00–18:00 geöffnet lassen
german.PrefsBattery=Im Akkubetrieb pausieren
german.PrefsLocal=Ihre Einstellungen werden nur auf diesem Computer gespeichert.
german.Minutes=%1 Minuten
german.MemoLanguage=Sprache:
german.MemoPrefs=Einstellungen:
german.MemoIdle=Inaktivitätsdauer:
german.MemoMode=Bewegungsart:
german.MemoKeep=Ihre vorhandenen Einstellungen bleiben erhalten.
german.UninstallOptionsTitle=Deinstallationsoptionen
german.UninstallOptionsDesc=Wählen Sie, was mit Ihren Einstellungen geschehen soll.
german.UninstallIntro=%1 wird von Ihrem Computer entfernt.
german.UninstallKeep=Einstellungen behalten (empfohlen)
german.UninstallKeepHint=Einstellungen und Statistik bleiben auf diesem Computer; nach einer Neuinstallation machen Sie dort weiter, wo Sie aufgehört haben.
german.UninstallAll=Einstellungen und Statistik löschen
german.UninstallAllHint=Einstellungen, Bewegungsstatistik und Fehlerprotokoll werden gelöscht.
german.UninstallWarning=Gelöschte Daten können nicht wiederhergestellt werden.
german.UninstallNoData=Auf diesem Computer sind keine MouseBot-Einstellungen gespeichert.
german.UninstallButton=&Deinstallieren

spanish.StartupTask=Iniciar con Windows (recomendado)
spanish.DesktopIconTask=Crear acceso directo en el escritorio
spanish.LaunchApp=Iniciar %1
spanish.ShortcutComment=Evita que el equipo se suspenda mientras el ratón está inactivo
spanish.LangTitle=Idioma
spanish.LangDesc=¿En qué idioma deben mostrarse la instalación y la aplicación?
spanish.LangIntro=El asistente de instalación y MouseBot se mostrarán en el idioma que elijas.
spanish.LangNote=Puedes cambiar el idioma más adelante en la aplicación, en Configuración > Avanzado > Idioma.
spanish.PrefsTitle=Preferencias
spanish.PrefsDesc=Configuración inicial de MouseBot. Puedes cambiarla más adelante en la ventana de Configuración.
spanish.PrefsIdle=Mover el ratón cuando lleve este tiempo inactivo:
spanish.PrefsMode=Tipo de movimiento:
spanish.PrefsMode0=Movimiento invisible
spanish.PrefsMode1=Pequeño movimiento
spanish.PrefsMode2=Señal de tecla (F15)
spanish.PrefsDisplay=Mantener la pantalla encendida
spanish.PrefsLid=Permitir suspender al cerrar la tapa (portátiles)
spanish.PrefsTeams=Mantener Teams abierto solo entre semana de 09:00 a 18:00
spanish.PrefsBattery=Pausar con batería
spanish.PrefsLocal=Tu configuración se guarda solo en este equipo.
spanish.Minutes=%1 minutos
spanish.MemoLanguage=Idioma:
spanish.MemoPrefs=Preferencias:
spanish.MemoIdle=Tiempo de inactividad:
spanish.MemoMode=Tipo de movimiento:
spanish.MemoKeep=Se conservará tu configuración actual.
spanish.UninstallOptionsTitle=Opciones de desinstalación
spanish.UninstallOptionsDesc=Elige qué hacer con tu configuración.
spanish.UninstallIntro=%1 se quitará de tu equipo.
spanish.UninstallKeep=Conservar mi configuración (recomendado)
spanish.UninstallKeepHint=Tu configuración y estadísticas se quedan en este equipo; si vuelves a instalar, continúas donde lo dejaste.
spanish.UninstallAll=Eliminar mi configuración y estadísticas
spanish.UninstallAllHint=Se eliminan la configuración, las estadísticas de movimiento y el registro de errores.
spanish.UninstallWarning=Los datos eliminados no se pueden recuperar.
spanish.UninstallNoData=No hay configuración de MouseBot guardada en este equipo.
spanish.UninstallButton=&Desinstalar

french.StartupTask=Lancer au démarrage de Windows (recommandé)
french.DesktopIconTask=Créer un raccourci sur le bureau
french.LaunchApp=Lancer %1
french.ShortcutComment=Empêche la mise en veille lorsque la souris est inactive
french.LangTitle=Langue
french.LangDesc=Dans quelle langue afficher l'installation et l'application ?
french.LangIntro=L'assistant d'installation et MouseBot s'afficheront dans la langue choisie.
french.LangNote=Vous pourrez changer la langue plus tard dans l'application, sous Paramètres > Avancé > Langue.
french.PrefsTitle=Préférences
french.PrefsDesc=Réglages initiaux de MouseBot. Vous pourrez tous les modifier plus tard dans la fenêtre Paramètres.
french.PrefsIdle=Déplacer la souris après cette durée d'inactivité :
french.PrefsMode=Type de mouvement :
french.PrefsMode0=Mouvement invisible
french.PrefsMode1=Petit mouvement
french.PrefsMode2=Signal de touche (F15)
french.PrefsDisplay=Garder l'écran allumé
french.PrefsLid=Autoriser la veille capot fermé (portables)
french.PrefsTeams=Garder Teams ouvert uniquement en semaine de 09:00 à 18:00
french.PrefsBattery=Pause sur batterie
french.PrefsLocal=Vos réglages sont enregistrés uniquement sur cet ordinateur.
french.Minutes=%1 minutes
french.MemoLanguage=Langue :
french.MemoPrefs=Préférences :
french.MemoIdle=Délai d'inactivité :
french.MemoMode=Type de mouvement :
french.MemoKeep=Vos réglages actuels seront conservés.
french.UninstallOptionsTitle=Options de désinstallation
french.UninstallOptionsDesc=Choisissez quoi faire de vos réglages.
french.UninstallIntro=%1 va être supprimé de votre ordinateur.
french.UninstallKeep=Conserver mes réglages (recommandé)
french.UninstallKeepHint=Vos réglages et statistiques restent sur cet ordinateur ; en cas de réinstallation, vous reprenez là où vous vous étiez arrêté.
french.UninstallAll=Supprimer mes réglages et statistiques
french.UninstallAllHint=Les réglages, les statistiques de mouvement et le journal d'erreurs sont supprimés.
french.UninstallWarning=Les données supprimées ne peuvent pas être récupérées.
french.UninstallNoData=Aucun réglage de MouseBot n'est enregistré sur cet ordinateur.
french.UninstallButton=&Désinstaller

russian.StartupTask=Запускать вместе с Windows (рекомендуется)
russian.DesktopIconTask=Создать ярлык на рабочем столе
russian.LaunchApp=Запустить %1
russian.ShortcutComment=Не даёт компьютеру уснуть, пока мышь не используется
russian.LangTitle=Язык
russian.LangDesc=На каком языке показывать установку и программу?
russian.LangIntro=Мастер установки и MouseBot будут отображаться на выбранном языке.
russian.LangNote=Язык можно изменить позже в программе: Настройки > Расширенные > Язык.
russian.PrefsTitle=Параметры
russian.PrefsDesc=Начальные настройки MouseBot. Все их можно изменить позже в окне настроек.
russian.PrefsIdle=Сдвигать мышь после такого времени бездействия:
russian.PrefsMode=Тип движения:
russian.PrefsMode0=Невидимое движение
russian.PrefsMode1=Небольшое движение
russian.PrefsMode2=Сигнал клавиши (F15)
russian.PrefsDisplay=Не выключать экран
russian.PrefsLid=Разрешать сон при закрытой крышке (ноутбуки)
russian.PrefsTeams=Держать Teams открытым только по будням 09:00–18:00
russian.PrefsBattery=Приостанавливать при работе от батареи
russian.PrefsLocal=Настройки хранятся только на этом компьютере.
russian.Minutes=%1 мин
russian.MemoLanguage=Язык:
russian.MemoPrefs=Параметры:
russian.MemoIdle=Время бездействия:
russian.MemoMode=Тип движения:
russian.MemoKeep=Текущие настройки будут сохранены.
russian.UninstallOptionsTitle=Параметры удаления
russian.UninstallOptionsDesc=Выберите, что сделать с вашими настройками.
russian.UninstallIntro=%1 будет удалён с компьютера.
russian.UninstallKeep=Сохранить мои настройки (рекомендуется)
russian.UninstallKeepHint=Настройки и статистика останутся на компьютере; после повторной установки вы продолжите с того же места.
russian.UninstallAll=Удалить мои настройки и статистику
russian.UninstallAllHint=Будут удалены настройки, статистика движений и журнал ошибок.
russian.UninstallWarning=Удалённые данные восстановить нельзя.
russian.UninstallNoData=На этом компьютере нет сохранённых настроек MouseBot.
russian.UninstallButton=&Удалить

chinesesimplified.StartupTask=开机时自动启动（推荐）
chinesesimplified.DesktopIconTask=创建桌面快捷方式
chinesesimplified.LaunchApp=启动 %1
chinesesimplified.ShortcutComment=鼠标闲置时防止电脑进入睡眠
chinesesimplified.LangTitle=语言
chinesesimplified.LangDesc=安装程序和应用使用哪种语言？
chinesesimplified.LangIntro=安装向导和 MouseBot 将以您选择的语言显示。
chinesesimplified.LangNote=之后可以在程序的“设置 > 高级 > 语言”中更改语言。
chinesesimplified.PrefsTitle=首选项
chinesesimplified.PrefsDesc=MouseBot 的初始设置。之后都可以在设置窗口中更改。
chinesesimplified.PrefsIdle=鼠标闲置达到此时间后移动：
chinesesimplified.PrefsMode=移动方式：
chinesesimplified.PrefsMode0=隐形移动
chinesesimplified.PrefsMode1=轻微移动鼠标
chinesesimplified.PrefsMode2=按键信号 (F15)
chinesesimplified.PrefsDisplay=保持屏幕常亮
chinesesimplified.PrefsLid=合上盖子时允许睡眠（笔记本电脑）
chinesesimplified.PrefsTeams=仅在工作日 09:00–18:00 保持 Teams 打开
chinesesimplified.PrefsBattery=使用电池时暂停
chinesesimplified.PrefsLocal=您的设置仅保存在这台电脑上。
chinesesimplified.Minutes=%1 分钟
chinesesimplified.MemoLanguage=语言：
chinesesimplified.MemoPrefs=首选项：
chinesesimplified.MemoIdle=闲置时间：
chinesesimplified.MemoMode=移动方式：
chinesesimplified.MemoKeep=将保留您现有的设置。
chinesesimplified.UninstallOptionsTitle=卸载选项
chinesesimplified.UninstallOptionsDesc=选择如何处理您的设置。
chinesesimplified.UninstallIntro=将从您的电脑上移除 %1。
chinesesimplified.UninstallKeep=保留我的设置（推荐）
chinesesimplified.UninstallKeepHint=您的设置和统计数据会保留在这台电脑上；重新安装后可以继续使用。
chinesesimplified.UninstallAll=删除我的设置和统计数据
chinesesimplified.UninstallAllHint=将删除设置、移动统计和错误日志。
chinesesimplified.UninstallWarning=删除的数据无法恢复。
chinesesimplified.UninstallNoData=这台电脑上没有保存 MouseBot 的设置。
chinesesimplified.UninstallButton=卸载(&U)

[Tasks]
Name: "startup"; Description: "{cm:StartupTask}"
Name: "desktopicon"; Description: "{cm:DesktopIconTask}"; Flags: unchecked

[InstallDelete]
; Eski (v2.1 ve öncesi) sihirbazın kaldırıcısı; artık Inno Setup'ın kaldırıcısı kullanılır
Type: files; Name: "{app}\uninstall.exe"

[UninstallDelete]
; Eski sürümden devralınan klasörü Inno Setup kendisi oluşturmadığı için silmez; boş kaldıysa kaldırılır
Type: dirifempty; Name: "{app}"

[Files]
Source: "..\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{userprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Comment: "{cm:ShortcutComment}"
Name: "{userdesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Comment: "{cm:ShortcutComment}"; Tasks: desktopicon

[Registry]
; Uygulamanın kendi "Windows açılışında otomatik başlat" ayarıyla aynı kayıt (Ayarlar'dan değiştirilebilir)
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "MouseBot"; \
    ValueData: """{app}\{#MyAppExeName}"""; Tasks: startup; Flags: uninsdeletevalue

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchApp,{#MyAppName}}"; Flags: nowait postinstall skipifsilent
; Sessiz (toplu) kurulumda uygulama tepside simge olarak başlar
Filename: "{app}\{#MyAppExeName}"; Flags: nowait; Check: WizardSilent

[Code]
const
  RunKey = 'Software\Microsoft\Windows\CurrentVersion\Run';
  OldUninstallKey = 'Software\Microsoft\Windows\CurrentVersion\Uninstall\MouseBot';
  LangCount = 7;

var
  LanguagePage: TWizardPage;
  LangRadios: array of TNewRadioButton;
  Relaunching: Boolean;
  ResumingAfterLanguage: Boolean;
  Configured: Boolean;
  PrefsPage: TWizardPage;
  IdleCombo: TNewComboBox;
  ModeCombo: TNewComboBox;
  DisplayCheck: TNewCheckBox;
  LidCheck: TNewCheckBox;
  BatteryCheck: TNewCheckBox;
  TeamsCheck: TNewCheckBox;

function RegisterWindowMessage(lpString: String): Cardinal;
  external 'RegisterWindowMessageW@user32.dll stdcall';

// ---------- Ortak yardımcılar ----------

function HasParam(Name: String): Boolean;
var
  I: Integer;
begin
  Result := False;
  for I := 1 to ParamCount do
    if CompareText(ParamStr(I), Name) = 0 then
      Result := True;
end;

function UserDataDir(): String;
begin
  Result := ExpandConstant('{userappdata}\MouseBot');
end;

function ConfigPath(): String;
begin
  Result := UserDataDir() + '\settings.ini';
end;

// Uygulama kapanırken ayarlarını kaydeder ve güç isteğini bırakır; bu yüzden önce kibarca çıkması istenir,
// 3 saniyede kapanmazsa sonlandırılır
procedure CloseRunningApp();
var
  Msg: Cardinal;
  I, ResultCode: Integer;
begin
  if FindWindowByWindowName('MouseBot_MsgWindow') <> 0 then
  begin
    Msg := RegisterWindowMessage('MouseBot_Exit');
    if Msg <> 0 then
      PostBroadcastMessage(Msg, 0, 0);
    for I := 1 to 30 do
    begin
      if FindWindowByWindowName('MouseBot_MsgWindow') = 0 then
        Break;
      Sleep(100);
    end;
    // Pencere kapandıktan sonra ayarların yazılması için kısa bir süre tanınır
    Sleep(500);
  end;
  Exec(ExpandConstant('{sys}\taskkill.exe'), '/F /IM {#MyAppExeName}', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
end;

// Setup kendi exe'sini doğrudan başlatamıyor ("Erişim engellendi"); cmd üzerinden açılır
function Relaunch(Params: String): Boolean;
var
  ResultCode: Integer;
begin
  Result := Exec(ExpandConstant('{cmd}'), '/c start "" "' + ExpandConstant('{srcexe}') + '" ' + Params,
    '', SW_HIDE, ewNoWait, ResultCode);
  if not Result then
    Log('Kurulum yeniden açılamadı: ' + SysErrorMessage(ResultCode));
end;

// ---------- Diller: Inno Setup adı / uygulama kodu / görünen ad ----------

function InnoLangName(I: Integer): String;
begin
  case I of
    0: Result := 'turkish';
    1: Result := 'english';
    2: Result := 'german';
    3: Result := 'spanish';
    4: Result := 'french';
    5: Result := 'russian';
  else
    Result := 'chinesesimplified';
  end;
end;

function AppLangCode(I: Integer): String;
begin
  case I of
    0: Result := 'tr';
    1: Result := 'en';
    2: Result := 'de';
    3: Result := 'es';
    4: Result := 'fr';
    5: Result := 'ru';
  else
    Result := 'zh';
  end;
end;

function LangDisplayName(I: Integer): String;
begin
  case I of
    0: Result := 'Türkçe';
    1: Result := 'English';
    2: Result := 'Deutsch';
    3: Result := 'Español';
    4: Result := 'Français';
    5: Result := 'Русский';
  else
    Result := '中文（简体）';
  end;
end;

function IndexOfInnoLang(Name: String): Integer;
var
  I: Integer;
begin
  Result := 1;
  for I := 0 to LangCount - 1 do
    if InnoLangName(I) = Name then
      Result := I;
end;

function IndexOfAppLang(Code: String): Integer;
var
  I: Integer;
begin
  Result := -1;
  for I := 0 to LangCount - 1 do
    if AppLangCode(I) = Code then
      Result := I;
end;

// ---------- settings.ini ----------

// Anahtarın değeri; dosya veya anahtar yoksa Found = False
function ReadSetting(Key: String; var Found: Boolean): String;
var
  Lines: TArrayOfString;
  I: Integer;
begin
  Result := '';
  Found := False;
  if not LoadStringsFromFile(ConfigPath(), Lines) then
    Exit;
  for I := 0 to GetArrayLength(Lines) - 1 do
    if CompareText(Copy(Lines[I], 1, Length(Key) + 1), Key + '=') = 0 then
    begin
      Result := Trim(Copy(Lines[I], Length(Key) + 2, MaxInt));
      Found := True;
    end;
end;

// Uygulama en az bir kez açılıp ayarlarını kaydettiyse (güncelleme) tercihler sorulmaz; mevcut ayarlar korunur.
// ShowWelcome yalnızca kurulumun yazdığı, uygulamanın henüz açılmadığı dosyada bulunur.
function IsAlreadyConfigured(): Boolean;
var
  Found: Boolean;
begin
  ReadSetting('ShowWelcome', Found);
  Result := FileExists(ConfigPath()) and not Found;
end;

function BoolStr(B: Boolean): String;
begin
  if B then
    Result := 'True'
  else
    Result := 'False';
end;

function IdleMinutes(Index: Integer): Integer;
begin
  case Index of
    0: Result := 1;
    1: Result := 2;
    2: Result := 3;
    3: Result := 4;
    4: Result := 5;
    5: Result := 10;
  else
    Result := 15;
  end;
end;

// Yeni kurulumda tercihler ve dil yazılır; uygulama ilk açılışta karşılama penceresini gösterir.
// Güncellemede yalnızca dil güncellenir ("Sistem dili" seçiliyse ve dil değiştirilmediyse ya da kurulum sessizse o da korunur).
procedure WriteSettings();
var
  Lines, Kept: TArrayOfString;
  Code, Existing: String;
  Found: Boolean;
  I, N: Integer;
begin
  ForceDirectories(UserDataDir());
  Code := AppLangCode(IndexOfInnoLang(ActiveLanguage()));

  if not Configured then
  begin
    SetArrayLength(Lines, 7);
    Lines[0] := 'Language=' + Code;
    Lines[1] := 'IdleSeconds=' + IntToStr(IdleMinutes(IdleCombo.ItemIndex) * 60);
    Lines[2] := 'Mode=' + IntToStr(ModeCombo.ItemIndex);
    Lines[3] := 'KeepDisplayOn=' + BoolStr(DisplayCheck.Checked);
    Lines[4] := 'SleepOnLidClose=' + BoolStr(LidCheck.Checked);
    Lines[5] := 'PauseOnBattery=' + BoolStr(BatteryCheck.Checked);
    Lines[6] := 'TeamsSchedule=' + BoolStr(TeamsCheck.Visible and TeamsCheck.Checked);
    if not WizardSilent then
    begin
      SetArrayLength(Lines, 8);
      Lines[7] := 'ShowWelcome=1';
    end;
    SaveStringsToFile(ConfigPath(), Lines, False);
    Exit;
  end;

  // Sessiz güncellemede sihirbaz uygulamanın dilinde açılmadığı için (Windows dilinde) dil korunur
  if WizardSilent then
    Exit;
  Existing := ReadSetting('Language', Found);
  if Found and (Existing = Code) then
    Exit;
  if Found and (Existing = 'auto') and not HasParam('/LANGCHOSEN') then
    Exit;
  if not LoadStringsFromFile(ConfigPath(), Lines) then
    Exit;
  N := 0;
  SetArrayLength(Kept, GetArrayLength(Lines) + 1);
  for I := 0 to GetArrayLength(Lines) - 1 do
    if CompareText(Copy(Lines[I], 1, 9), 'Language=') <> 0 then
    begin
      Kept[N] := Lines[I];
      N := N + 1;
    end;
  Kept[N] := 'Language=' + Code;
  SetArrayLength(Kept, N + 1);
  SaveStringsToFile(ConfigPath(), Kept, False);
end;

// ---------- Eski sürüm (kendi sihirbazıyla kurulmuş v2.1 ve öncesi) ----------

function OldInstallDir(): String;
begin
  if not RegQueryStringValue(HKCU, OldUninstallKey, 'InstallLocation', Result) then
    Result := '';
end;

function GetDefaultDir(Param: String): String;
begin
  Result := OldInstallDir();
  if Result = '' then
    Result := ExpandConstant('{localappdata}\Programs\MouseBot');
end;

// ---------- Başlangıç ----------

// Güncellemede sihirbaz, uygulamanın kullandığı dilde yeniden açılır (kullanıcı Dil adımında değiştirebilir)
function InitializeSetup(): Boolean;
var
  Code, Wanted: String;
  Found: Boolean;
  I: Integer;
begin
  Result := True;
  if WizardSilent or HasParam('/LANGCHOSEN') or HasParam('/LANGAUTO') then
    Exit;
  Code := ReadSetting('Language', Found);
  I := IndexOfAppLang(Code);
  if I < 0 then
    Exit;
  Wanted := InnoLangName(I);
  if (Wanted <> ActiveLanguage()) and Relaunch('/LANG=' + Wanted + ' /LANGAUTO') then
    Result := False;
end;

// ---------- Dil seçimi adımı ----------

function SelectedLanguage(): String;
var
  I: Integer;
begin
  Result := ActiveLanguage();
  for I := 0 to LangCount - 1 do
    if LangRadios[I].Checked then
      Result := InnoLangName(I);
end;

function AddStatic(Page: TWizardPage; Top, Height: Integer; Caption: String): TNewStaticText;
begin
  Result := TNewStaticText.Create(Page);
  Result.Parent := Page.Surface;
  Result.AutoSize := False;
  Result.WordWrap := True;
  Result.Left := 0;
  Result.Top := Top;
  Result.Width := Page.SurfaceWidth;
  Result.Height := Height;
  Result.Caption := Caption;
end;

procedure CreateLanguagePage();
var
  Note: TNewStaticText;
  I: Integer;
begin
  LanguagePage := CreateCustomPage(wpWelcome, CustomMessage('LangTitle'), CustomMessage('LangDesc'));
  AddStatic(LanguagePage, 0, ScaleY(30), CustomMessage('LangIntro'));

  // Her seçenek kendi dilinde yazılır; kullanıcı hangi dili okuyorsa onu bulur
  SetArrayLength(LangRadios, LangCount);
  for I := 0 to LangCount - 1 do
  begin
    LangRadios[I] := TNewRadioButton.Create(LanguagePage);
    LangRadios[I].Parent := LanguagePage.Surface;
    LangRadios[I].Caption := LangDisplayName(I);
    LangRadios[I].Left := ScaleX(8);
    LangRadios[I].Top := ScaleY(34) + I * ScaleY(22);
    LangRadios[I].Width := LanguagePage.SurfaceWidth - ScaleX(8);
    LangRadios[I].Height := ScaleY(18);
    LangRadios[I].Checked := InnoLangName(I) = ActiveLanguage();
  end;

  Note := AddStatic(LanguagePage, ScaleY(34) + LangCount * ScaleY(22) + ScaleY(8), ScaleY(30), CustomMessage('LangNote'));
  Note.Font.Color := clGray;
end;

// Dil değişikliği için yeniden açılırken "Kurulumdan çıkılsın mı?" sorulmaz
procedure CancelButtonClick(CurPageID: Integer; var Cancel, Confirm: Boolean);
begin
  if Relaunching then
    Confirm := False;
end;

// ---------- Tercihler adımı ----------

function AddCombo(Top, Width: Integer): TNewComboBox;
begin
  Result := TNewComboBox.Create(PrefsPage);
  Result.Parent := PrefsPage.Surface;
  Result.Style := csDropDownList;
  Result.Left := 0;
  Result.Top := Top;
  Result.Width := Width;
end;

function AddCheck(Top: Integer; Caption: String; Checked: Boolean): TNewCheckBox;
begin
  Result := TNewCheckBox.Create(PrefsPage);
  Result.Parent := PrefsPage.Surface;
  Result.Caption := Caption;
  Result.Left := 0;
  Result.Top := Top;
  Result.Width := PrefsPage.SurfaceWidth;
  Result.Height := ScaleY(18);
  Result.Checked := Checked;
end;

// Yeni Teams (MSIX) çalıştırma kısayolu veya klasik Teams güncelleyicisi
function TeamsInstalled(): Boolean;
begin
  Result := FileExists(ExpandConstant('{localappdata}\Microsoft\WindowsApps\ms-teams.exe')) or
    FileExists(ExpandConstant('{localappdata}\Microsoft\Teams\Update.exe'));
end;

procedure CreatePrefsPage();
var
  Note: TNewStaticText;
  I: Integer;
begin
  PrefsPage := CreateCustomPage(wpSelectDir, CustomMessage('PrefsTitle'), CustomMessage('PrefsDesc'));

  AddStatic(PrefsPage, 0, ScaleY(16), CustomMessage('PrefsIdle'));
  IdleCombo := AddCombo(ScaleY(18), ScaleX(140));
  for I := 0 to 6 do
    IdleCombo.Items.Add(FmtMessage(CustomMessage('Minutes'), [IntToStr(IdleMinutes(I))]));
  IdleCombo.ItemIndex := 3; // 4 dakika (uygulamanın varsayılanı)

  AddStatic(PrefsPage, ScaleY(52), ScaleY(16), CustomMessage('PrefsMode'));
  ModeCombo := AddCombo(ScaleY(70), ScaleX(240));
  ModeCombo.Items.Add(CustomMessage('PrefsMode0'));
  ModeCombo.Items.Add(CustomMessage('PrefsMode1'));
  ModeCombo.Items.Add(CustomMessage('PrefsMode2'));
  ModeCombo.ItemIndex := 1;

  DisplayCheck := AddCheck(ScaleY(110), CustomMessage('PrefsDisplay'), True);
  LidCheck := AddCheck(ScaleY(134), CustomMessage('PrefsLid'), True);
  BatteryCheck := AddCheck(ScaleY(158), CustomMessage('PrefsBattery'), False);
  // Teams kurulu değilse seçenek gösterilmez; saatler ve günler uygulamadaki Teams sekmesinden değiştirilir
  TeamsCheck := AddCheck(ScaleY(182), CustomMessage('PrefsTeams'), False);
  TeamsCheck.Visible := TeamsInstalled();

  Note := AddStatic(PrefsPage, ScaleY(212), ScaleY(16), CustomMessage('PrefsLocal'));
  Note.Font.Color := clGray;
end;

procedure InitializeWizard();
begin
  // Dil değiştirilerek yeniden açıldıysa Hoş geldiniz ve Dil adımları geçilir (Geri ile dönülebilir)
  ResumingAfterLanguage := HasParam('/LANGCHOSEN');
  Configured := IsAlreadyConfigured();
  CreateLanguagePage();
  CreatePrefsPage();
end;

function ShouldSkipPage(PageID: Integer): Boolean;
begin
  Result := False;
  if (PageID = PrefsPage.ID) and Configured then
    Result := True;
  // Eski sürümün klasörü devralınır; klasör sorulmaz
  if (PageID = wpSelectDir) and (OldInstallDir() <> '') then
    Result := True;
  if ResumingAfterLanguage and (PageID = wpWelcome) then
    Result := True;
end;

procedure CurPageChanged(CurPageID: Integer);
begin
  // Dil değiştirilerek yeniden açıldıysa Dil adımı kendiliğinden geçilir;
  // sayfa atlanmadığı için sonraki adımda Geri ile Dil ve Hoş geldiniz adımlarına dönülebilir
  if ResumingAfterLanguage and (CurPageID = LanguagePage.ID) then
  begin
    ResumingAfterLanguage := False;
    WizardForm.NextButton.OnClick(WizardForm.NextButton);
    Exit;
  end;

  // Sayfa açıldığında hiçbir alan seçili/odaklı görünmez; odak İleri/Kur/Son düğmesindedir
  if WizardForm.NextButton.Visible and WizardForm.NextButton.Enabled then
    WizardForm.ActiveControl := WizardForm.NextButton;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;
  // Farklı dil seçildiyse kurulum o dille yeniden açılır ve bu pencere sorusuz kapanır
  if (CurPageID = LanguagePage.ID) and (SelectedLanguage() <> ActiveLanguage()) then
  begin
    if Relaunch('/LANG=' + SelectedLanguage() + ' /LANGCHOSEN') then
    begin
      Relaunching := True;
      WizardForm.Close;
    end;
    Result := False;
  end;
end;

// "Kuruluma Hazır" sayfasındaki özet
function UpdateReadyMemo(Space, NewLine, MemoUserInfoInfo, MemoDirInfo, MemoTypeInfo,
  MemoComponentsInfo, MemoGroupInfo, MemoTasksInfo: String): String;
begin
  Result := CustomMessage('MemoLanguage') + NewLine + Space + LangDisplayName(IndexOfInnoLang(ActiveLanguage())) + NewLine + NewLine;
  if MemoDirInfo <> '' then
    Result := Result + MemoDirInfo + NewLine + NewLine;
  Result := Result + CustomMessage('MemoPrefs') + NewLine;
  if Configured then
    Result := Result + Space + CustomMessage('MemoKeep') + NewLine
  else
  begin
    Result := Result + Space + CustomMessage('MemoIdle') + ' ' + IdleCombo.Text + NewLine;
    Result := Result + Space + CustomMessage('MemoMode') + ' ' + ModeCombo.Text + NewLine;
    if DisplayCheck.Checked then
      Result := Result + Space + DisplayCheck.Caption + NewLine;
    if LidCheck.Checked then
      Result := Result + Space + LidCheck.Caption + NewLine;
    if BatteryCheck.Checked then
      Result := Result + Space + BatteryCheck.Caption + NewLine;
    if TeamsCheck.Visible and TeamsCheck.Checked then
      Result := Result + Space + TeamsCheck.Caption + NewLine;
  end;
  if MemoTasksInfo <> '' then
    Result := Result + NewLine + MemoTasksInfo;
end;

// ---------- Kurulum adımları ----------

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  CloseRunningApp();
  Result := '';
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep <> ssPostInstall then
    Exit;

  WriteSettings();

  // Seçilmeyen görevlerin önceki kurulumdan kalan izleri kaldırılır
  if not WizardIsTaskSelected('startup') then
    RegDeleteValue(HKCU, RunKey, 'MouseBot');
  if not WizardIsTaskSelected('desktopicon') then
    DeleteFile(ExpandConstant('{userdesktop}\{#MyAppName}.lnk'));

  // Eski sihirbazın "Uygulamalar" kaydı; artık bu kurulumun kaydı kullanılır
  RegDeleteKeyIncludingSubkeys(HKCU, OldUninstallKey);
end;

// ---------- Kaldırma: onay kutusu yerine "Kaldırma Seçenekleri" sayfası ----------

var
  UninstallRadioKeep: TNewRadioButton;
  UninstallRadioAll: TNewRadioButton;
  UninstallWarning: TNewStaticText;
  DeleteAll: Boolean;

// Standart "kaldırmak istediğinizden emin misiniz?" kutusu yerine kaldırıcı, seçenek sayfasıyla yeniden açılır.
// Yönetici/toplu kaldırma (/SILENT, /VERYSILENT) olduğu gibi çalışır ve kullanıcı ayarlarını korur.
function InitializeUninstall(): Boolean;
var
  ResultCode: Integer;
begin
  Result := True;
  if not UninstallSilent and not HasParam('/OPTIONS') then
    if Exec(ExpandConstant('{uninstallexe}'), '/SILENT /OPTIONS', '', SW_SHOW, ewNoWait, ResultCode) then
      Result := False;
end;

// Klasördeki dosyaların toplam boyutu (bayt); klasör yoksa veya boşsa -1
function DataSize(): Integer;
var
  FindRec: TFindRec;
begin
  Result := -1;
  if FindFirst(UserDataDir() + '\*', FindRec) then
  try
    repeat
      if FindRec.Attributes and FILE_ATTRIBUTE_DIRECTORY = 0 then
      begin
        if Result < 0 then
          Result := 0;
        Result := Result + FindRec.SizeLow;
      end;
    until not FindNext(FindRec);
  finally
    FindClose(FindRec);
  end;
end;

function FormatSize(Bytes: Integer): String;
begin
  if Bytes < 1024 * 1024 then
    Result := IntToStr((Bytes + 1023) div 1024) + ' KB'
  else
    Result := IntToStr(Bytes div (1024 * 1024)) + ',' + IntToStr((Bytes mod (1024 * 1024)) * 10 div (1024 * 1024)) + ' MB';
end;

function AddText(Parent: TNewNotebookPage; Left, Top, Width, Height: Integer; Caption: String): TNewStaticText;
begin
  Result := TNewStaticText.Create(UninstallProgressForm);
  Result.Parent := Parent;
  Result.AutoSize := False;
  Result.WordWrap := True;
  Result.Left := Left;
  Result.Top := Top;
  Result.Width := Width;
  Result.Height := Height;
  Result.Caption := Caption;
end;

function AddRadio(Parent: TNewNotebookPage; Left, Top, Width: Integer; Caption: String): TNewRadioButton;
begin
  Result := TNewRadioButton.Create(UninstallProgressForm);
  Result.Parent := Parent;
  Result.Caption := Caption;
  Result.Left := Left;
  Result.Top := Top;
  Result.Width := Width;
  Result.Height := ScaleY(18);
end;

// Silme seçiliyse uyarı görünür
procedure UpdateUninstallOptions(Sender: TObject);
begin
  UninstallWarning.Visible := UninstallRadioAll.Checked;
end;

procedure InitializeUninstallProgressForm();
var
  Page: TNewNotebookPage;
  Hint: TNewStaticText;
  UninstallButton: TNewButton;
  PageNameLabel, PageDescriptionLabel: String;
  CancelEnabled: Boolean;
  CancelModalResult: Integer;
  Left, Width, Top, Size: Integer;
begin
  if not HasParam('/OPTIONS') then
    Exit;

  Size := DataSize();

  Page := TNewNotebookPage.Create(UninstallProgressForm);
  Page.Notebook := UninstallProgressForm.InnerNotebook;
  Page.Parent := UninstallProgressForm.InnerNotebook;
  Page.Align := alClient;

  // İçerik, sayfa başlığıyla aynı hizadan başlar
  Left := ScaleX(20);
  Width := UninstallProgressForm.InnerNotebook.Width - Left - ScaleX(20);
  Top := ScaleY(4);

  AddText(Page, Left, Top, Width, ScaleY(16), FmtMessage(CustomMessage('UninstallIntro'), ['{#MyAppName}']));
  Top := Top + ScaleY(28);

  if Size < 0 then
  begin
    // Silinecek ayar yoksa yalnızca bilgi gösterilir
    Hint := AddText(Page, Left, Top, Width, ScaleY(16), CustomMessage('UninstallNoData'));
    Hint.Font.Color := clGray;
    UninstallRadioKeep := nil;
  end
  else
  begin
    UninstallRadioKeep := AddRadio(Page, Left, Top, Width, CustomMessage('UninstallKeep'));
    UninstallRadioKeep.Font.Style := [fsBold];
    UninstallRadioKeep.Checked := True;
    UninstallRadioKeep.OnClick := @UpdateUninstallOptions;
    Hint := AddText(Page, Left + ScaleX(18), Top + ScaleY(20), Width - ScaleX(18), ScaleY(30), CustomMessage('UninstallKeepHint'));
    Hint.Font.Color := clGray;
    Top := Top + ScaleY(58);

    UninstallRadioAll := AddRadio(Page, Left, Top, Width, CustomMessage('UninstallAll') + '  (' + FormatSize(Size) + ')');
    UninstallRadioAll.OnClick := @UpdateUninstallOptions;
    Hint := AddText(Page, Left + ScaleX(18), Top + ScaleY(20), Width - ScaleX(18), ScaleY(16), CustomMessage('UninstallAllHint'));
    Hint.Font.Color := clGray;
    Top := Top + ScaleY(46);

    UninstallWarning := AddText(Page, Left, Top, Width, ScaleY(16), '⚠  ' + CustomMessage('UninstallWarning'));
    UninstallWarning.Font.Color := $000A60B2;
    UpdateUninstallOptions(nil);
  end;

  // Seçenek sayfasını göster, "Kaldır" / "İptal" beklenir
  UninstallProgressForm.InnerNotebook.ActivePage := Page;
  PageNameLabel := UninstallProgressForm.PageNameLabel.Caption;
  PageDescriptionLabel := UninstallProgressForm.PageDescriptionLabel.Caption;
  UninstallProgressForm.PageNameLabel.Caption := CustomMessage('UninstallOptionsTitle');
  UninstallProgressForm.PageDescriptionLabel.Caption := CustomMessage('UninstallOptionsDesc');

  UninstallButton := TNewButton.Create(UninstallProgressForm);
  UninstallButton.Parent := UninstallProgressForm;
  UninstallButton.Width := UninstallProgressForm.CancelButton.Width;
  UninstallButton.Height := UninstallProgressForm.CancelButton.Height;
  UninstallButton.Left := UninstallProgressForm.CancelButton.Left - UninstallButton.Width - ScaleX(10);
  UninstallButton.Top := UninstallProgressForm.CancelButton.Top;
  UninstallButton.Caption := CustomMessage('UninstallButton');
  UninstallButton.ModalResult := mrOk;
  UninstallButton.Default := True;
  // Açılışta hiçbir seçenek odaklı görünmez
  UninstallProgressForm.ActiveControl := UninstallButton;

  CancelEnabled := UninstallProgressForm.CancelButton.Enabled;
  CancelModalResult := UninstallProgressForm.CancelButton.ModalResult;
  UninstallProgressForm.CancelButton.Enabled := True;
  UninstallProgressForm.CancelButton.ModalResult := mrCancel;

  if UninstallProgressForm.ShowModal = mrCancel then
    Abort;

  DeleteAll := (UninstallRadioKeep <> nil) and UninstallRadioAll.Checked;

  // Kaldırma ilerleme sayfasına dön
  UninstallButton.Visible := False;
  UninstallProgressForm.CancelButton.Enabled := CancelEnabled;
  UninstallProgressForm.CancelButton.ModalResult := CancelModalResult;
  UninstallProgressForm.PageNameLabel.Caption := PageNameLabel;
  UninstallProgressForm.PageDescriptionLabel.Caption := PageDescriptionLabel;
  UninstallProgressForm.InnerNotebook.ActivePage := UninstallProgressForm.InstallingPage;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then
  begin
    CloseRunningApp();
    // Başlangıç kaydını uygulama da (Ayarlar'dan) oluşturabildiği için her durumda silinir
    RegDeleteValue(HKCU, RunKey, 'MouseBot');
  end;
  if (CurUninstallStep = usPostUninstall) and DeleteAll then
    DelTree(UserDataDir(), True, True, True);
end;
