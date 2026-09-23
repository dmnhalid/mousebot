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

1. [`dist/MouseBotSetup.exe`](dist/MouseBotSetup.exe) dosyasını indirin (dosya sayfasında **Download raw file**) ya da depoyu klonlayıp `dist` klasörünü açın.
2. `MouseBotSetup.exe`'yi çalıştırın ve **İleri > Kur > Bitir** ile kurulumu tamamlayın.

Yönetici izni gerekmez. Kurulumdan sonra MouseBot Başlat menüsünde ve sistem tepsisinde (sağ alttaki fare simgesi) yer alır; Windows **Ayarlar > Uygulamalar** bölümünden kaldırılabilir.

> Kurulum dosyası dijital olarak imzalı olmadığı için Windows SmartScreen "Windows bilgisayarınızı korudu" uyarısı gösterebilir. **Ek bilgi > Yine de çalıştır** ile devam edebilirsiniz.

## Derleme

Ek bir SDK gerekmez; Windows ile gelen .NET Framework 4 derleyicisi kullanılır.

```bat
build.bat
```

Çıktılar:

- `MouseBot.exe`: uygulama
- `MouseBotSetup.exe`: uygulamayı içinde taşıyan kurulum sihirbazı (ayrıca `dist/` klasörüne kopyalanır)

## Dosyalar

| Dosya | Açıklama |
|---|---|
| `MouseBot.cs` | Uygulama, arayüz ve çeviriler |
| `Setup.cs` | Kurulum / kaldırma sihirbazı |
| `AppInfo.cs` | Uygulama sürüm bilgisi |
| `app.manifest` | DPI ve görsel stil ayarları |
| `MouseBot.ico` | Uygulama ikonu |
| `build.bat` | Derleme betiği |
| `dist/MouseBotSetup.exe` | Hazır kurulum dosyası |

Ayarlar `%APPDATA%\MouseBot\settings.ini` dosyasında saklanır.
