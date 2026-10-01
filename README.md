# MouseBot

A small system tray app that keeps Windows from going to sleep, turning off the screen, or locking by nudging the cursor very slightly whenever the mouse has been idle for a set amount of time.

## Features

- Adjustable idle time (default 4 minutes)
- Three movement types: invisible, small mouse movement, F15 key signal
- Prevents sleep and display shutdown via the Windows power API
- Lets a laptop sleep when its lid is closed (can be turned off under Settings > Advanced)
- Working hours / day scheduling, pauses while on battery
- Teams hours: opens Microsoft Teams when the chosen hours start and closes it when they end (waits until a meeting is over); set under Settings > Teams
- Temporary pause, `Ctrl+Alt+M` hotkey
- Light / dark theme (follows the system)
- 7 languages: Türkçe, English, Deutsch, Español, Français, Русский, 中文
- Setup wizard that doesn't require administrator rights; can be uninstalled from the Windows "Apps" list, keeping or deleting your settings

## Installation

1. Download [`dist/MouseBotSetup.exe`](dist/MouseBotSetup.exe) (**Download raw file** on the file page), or clone the repository and open the `dist` folder.
2. Run `MouseBotSetup.exe`. The wizard steps:
   1. **Welcome**
   2. **Language**: the language of both setup and the app (7 languages). It can be changed later under Settings > Advanced > Language.
   3. **Install folder**: `%LOCALAPPDATA%\Programs\MouseBot` by default (skipped when updating)
   4. **Preferences**: idle time, movement type, keep the display on, let the laptop sleep when the lid is closed, pause on battery, keep Teams open only on weekdays 09:00–18:00 (shown only when Teams is installed) (skipped when updating; existing settings are kept)
   5. **Additional tasks**: start with Windows, desktop shortcut
   6. **Ready to install**: a summary of your choices
   7. **Finish**: optionally launches MouseBot

No administrator rights are required. MouseBot appears in the Start menu and in the system tray (the mouse icon at the bottom right). When updating, setup opens in the app's current language and closes the running MouseBot on its own. Installs made with the older (v2.1 and earlier) installer are taken over in place.

Silent install: `MouseBotSetup.exe /VERYSILENT` (MouseBot starts in the tray afterwards).

> Because the installer is not digitally signed, Windows SmartScreen may show a "Windows protected your PC" warning. Click **More info > Run anyway** to continue.

### Uninstalling

Uninstall from Windows **Settings > Apps**. The uninstaller asks whether to **keep your settings** (recommended; statistics are kept too and a reinstall continues where you left off) or **delete them**. A silent uninstall (`unins000.exe /VERYSILENT`) keeps the settings.

## Building

Requirements (only on the build machine): Inno Setup 6.5+ (`winget install JRSoftware.InnoSetup`). The app itself is compiled with the .NET Framework 4 compiler that ships with Windows.

```bat
build.bat
```

Steps and outputs:

1. `MouseBot.exe`: the application
2. `installer\images\`: setup wizard images, drawn from the app's own logo (`MouseBot.exe --make-wizard-images`)
3. `dist\MouseBotSetup.exe`: the installer, compiled from `installer\MouseBot.iss`. The version comes from `AppInfo.cs`.

## Files

| File | Description |
|---|---|
| `MouseBot.cs` | Application, UI and translations |
| `installer/MouseBot.iss` | Install / uninstall wizard (Inno Setup) |
| `installer/Languages/` | Chinese (Simplified) setup translation, not bundled with Inno Setup |
| `AppInfo.cs` | Application version info |
| `app.manifest` | DPI and visual style settings |
| `MouseBot.ico` | Application icon |
| `build.bat` | Build script |
| `dist/MouseBotSetup.exe` | Ready-to-use installer |

Settings are stored in `%APPDATA%\MouseBot\settings.ini`.
