# MouseBot

Fare belirli bir süre hareketsiz kaldığında imleci otomatik olarak çok az hareket ettirerek Windows'un uykuya geçmesini, ekranın kapanmasını veya kilitlenmesini engelleyen küçük bir sistem tepsisi uygulaması.

## Özellikler

- Ayarlanabilir hareketsizlik süresi (varsayılan 4 dakika)
- Üç hareket türü: görünmez, küçük fare hareketi, F15 tuş sinyali
- Windows güç API'si ile uyku ve ekran kapanmasını engelleme
- Çalışma saati / gün zamanlaması, pildeyken beklemeye geçme
- Geçici duraklatma, `Ctrl+Alt+M` kısayolu
- Açık / koyu tema (sistemi takip eder)
- 7 dil: Türkçe, English, Deutsch, Español, Français, Русский, 中文
- Yönetici izni gerektirmeyen kurulum sihirbazı; Windows "Uygulamalar" listesinden kaldırılabilir

## Kurulum

[Releases](../../releases) sayfasından `MouseBotSetup.exe` dosyasını indirip çalıştırın.

## Derleme

Ek bir SDK gerekmez; Windows ile gelen .NET Framework 4 derleyicisi kullanılır.

```bat
build.bat
```

Çıktılar:

- `MouseBot.exe`: uygulama
- `MouseBotSetup.exe`: uygulamayı içinde taşıyan kurulum sihirbazı

## Dosyalar

| Dosya | Açıklama |
|---|---|
| `MouseBot.cs` | Uygulama, arayüz ve çeviriler |
| `Setup.cs` | Kurulum / kaldırma sihirbazı |
| `AppInfo.cs` | Uygulama sürüm bilgisi |
| `app.manifest` | DPI ve görsel stil ayarları |
| `MouseBot.ico` | Uygulama ikonu |
| `build.bat` | Derleme betiği |

Ayarlar `%APPDATA%\MouseBot\settings.ini` dosyasında saklanır.
