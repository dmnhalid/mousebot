@echo off
REM MouseBot derleme
REM  1) MouseBot.exe: Windows ile gelen .NET Framework derleyicisiyle (ek SDK gerekmez)
REM  2) Kurulum sihirbazi gorselleri: uygulamanin kendi logosundan (installer\images)
REM  3) dist\MouseBotSetup.exe: Inno Setup 6.5+ ile (kurmak icin: winget install JRSoftware.InnoSetup)
setlocal
cd /d "%~dp0"
set CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe
if not exist "%CSC%" set CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe
set OPTS=/nologo /target:winexe /optimize+ /codepage:65001 /r:System.Windows.Forms.dll /r:System.Drawing.dll

set ISCC=%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe
if not exist "%ISCC%" set ISCC=%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe
if not exist "%ISCC%" set ISCC=%ProgramFiles%\Inno Setup 6\ISCC.exe
if not exist "%ISCC%" (
  echo Inno Setup 6 bulunamadi. Kurmak icin: winget install JRSoftware.InnoSetup
  goto :fail
)

REM Calisan kopya exe'yi kilitler; once kapat
taskkill /im MouseBot.exe /f >nul 2>&1

if not exist MouseBot.ico (
  echo Ikon uretiliyor...
  "%CSC%" %OPTS% /out:MouseBot.exe MouseBot.cs AppInfo.cs || goto :fail
  .\MouseBot.exe --make-icon MouseBot.ico || goto :fail
)

"%CSC%" %OPTS% /win32icon:MouseBot.ico /win32manifest:app.manifest /out:MouseBot.exe MouseBot.cs AppInfo.cs || goto :fail
echo Derleme basarili: MouseBot.exe

.\MouseBot.exe --make-wizard-images installer\images || goto :fail

"%ISCC%" /Q installer\MouseBot.iss || goto :fail
echo Derleme basarili: dist\MouseBotSetup.exe
exit /b 0

:fail
echo Derleme HATASI
pause
exit /b 1
