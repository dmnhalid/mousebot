@echo off
REM MouseBot derleme - Windows ile gelen .NET Framework derleyicisini kullanir
REM Cikti: MouseBot.exe (uygulama) ve MouseBotSetup.exe (kurulum sihirbazi, uygulamayi icinde tasir)
setlocal
cd /d "%~dp0"
set CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe
if not exist "%CSC%" set CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe
set OPTS=/nologo /target:winexe /optimize+ /codepage:65001 /r:System.Windows.Forms.dll /r:System.Drawing.dll

REM Calisan kopya exe'yi kilitler; once kapat
taskkill /im MouseBot.exe /f >nul 2>&1

if not exist MouseBot.ico (
  echo Ikon uretiliyor...
  "%CSC%" %OPTS% /out:MouseBot.exe MouseBot.cs AppInfo.cs || goto :fail
  .\MouseBot.exe --make-icon MouseBot.ico || goto :fail
)

"%CSC%" %OPTS% /win32icon:MouseBot.ico /win32manifest:app.manifest /out:MouseBot.exe MouseBot.cs AppInfo.cs || goto :fail
echo Derleme basarili: MouseBot.exe

"%CSC%" %OPTS% /win32icon:MouseBot.ico /win32manifest:app.manifest /main:MouseBot.SetupProgram /resource:MouseBot.exe,MouseBot.payload.exe /out:MouseBotSetup.exe MouseBot.cs Setup.cs || goto :fail
echo Derleme basarili: MouseBotSetup.exe
exit /b 0

:fail
echo Derleme HATASI
pause
exit /b 1
