# MouseBot

A small system tray app that keeps Windows from going to sleep, turning off the screen, or locking by nudging the cursor very slightly whenever the mouse has been idle for a set amount of time.

## Features

- Adjustable idle time (default 4 minutes)
- Three movement types: invisible, small mouse movement, F15 key signal
- Prevents sleep and display shutdown via the Windows power API
- Working hours / day scheduling, pauses while on battery
- Temporary pause, `Ctrl+Alt+M` hotkey
- Light / dark theme (follows the system)
- 7 languages: Türkçe, English, Deutsch, Español, Français, Русский, 中文
- Setup wizard that doesn't require administrator rights; can be uninstalled from the Windows "Apps" list

## Installation

1. Download [`dist/MouseBotSetup.exe`](dist/MouseBotSetup.exe) (**Download raw file** on the file page), or clone the repository and open the `dist` folder.
2. Run `MouseBotSetup.exe` and complete the setup with **Next > Install > Finish**.

No administrator rights are required. After installation, MouseBot appears in the Start menu and in the system tray (the mouse icon at the bottom right); it can be uninstalled from Windows **Settings > Apps**.

> Because the installer is not digitally signed, Windows SmartScreen may show a "Windows protected your PC" warning. Click **More info > Run anyway** to continue.

## Building

No additional SDK is needed; it uses the .NET Framework 4 compiler that ships with Windows.

```bat
build.bat
```

Outputs:

- `MouseBot.exe`: the application
- `MouseBotSetup.exe`: setup wizard that bundles the application (also copied to the `dist/` folder)

## Files

| File | Description |
|---|---|
| `MouseBot.cs` | Application, UI and translations |
| `Setup.cs` | Install / uninstall wizard |
| `AppInfo.cs` | Application version info |
| `app.manifest` | DPI and visual style settings |
| `MouseBot.ico` | Application icon |
| `build.bat` | Build script |
| `dist/MouseBotSetup.exe` | Ready-to-use installer |

Settings are stored in `%APPDATA%\MouseBot\settings.ini`.
