# DinamikCvSitesi - Dinamik CV/Resume Web Sitesi

Bu proje kişisel CV/Resume sitesi olarak tasarlanmış dinamik bir ASP.NET Core MVC uygulamasıdır. Kullanıcılar site üzerinden kişisel bilgilerini, deneyimlerini, eğitimlerini, yeteneklerini ve sertifikalarını yönetebilirler.

## Güvenlik Geliştirmeleri

Projede aşağıdaki güvenlik önlemleri uygulanmıştır:

### Program.cs'deki Güvenlik Yapılandırmaları

- **Çerez Güvenliği**: HttpOnly, SameSite ve Secure politikaları ortama bağlı olarak yapılandırıldı.
- **Oturum Yönetimi**: 30 dakikalık zaman aşımı süresi ile güvenli oturum yönetimi.
- **XSS Koruması**: X-XSS-Protection başlıkları eklendi.
- **Content Type Sniffing Koruması**: X-Content-Type-Options nosniff başlıkları eklendi.
- **Clickjacking Koruması**: X-Frame-Options başlıkları eklendi.
- **Antiforgery Token Koruması**: CSRF saldırılarına karşı tüm formlar korundu.
- **SSL/HTTPS Yapılandırması**: 
  - Geliştirme ortamında HTTP izni verildi
  - Üretim ortamında HTTPS zorunlu kılındı ve HSTS eklendi.
- **Content Security Policy**: Üretim ortamında script, style, font ve resim kaynakları için güvenlik politikaları tanımlandı.

### Kimlik Doğrulama ve Yetkilendirme

- **Cookie Tabanlı Kimlik Doğrulama**: Güvenli cookie ayarları ile kimlik doğrulama.
- **Rol Tabanlı Yetkilendirme**: Admin rolü için özel erişim kontrolleri.
- **Şifre Güvenliği**: SHA-256 ile şifre hashleme.

### Diğer Güvenlik Önlemleri

- **Dosya Yükleme Güvenliği**: Profil resmi yüklemede güvenlik kontrolleri.
- **Giriş/Çıkış İşlemleri**: Güvenli oturum açma ve kapatma süreçleri.
- **Validator Kontrolleri**: Tüm formlar için sunucu taraflı doğrulama.
- **Hata Yönetimi**: Güvenli hata yakalama ve loglama.

## Admin Paneli Giriş Bilgileri

Admin paneline erişim için aşağıdaki kullanıcı bilgilerini kullanabilirsiniz:

**Varsayılan Admin:**
- Kullanıcı Adı: admin
- Şifre: Admin123!

**Test Kullanıcısı:**
- Kullanıcı Adı: test
- Şifre: test123

## Proje Özellikleri

- **Anasayfa**: Kişisel özet bilgileri ve yetenekler.
- **Resume/CV Sayfası**: Eğitim, deneyim, yetenek ve sertifika bilgilerinin görüntülenmesi.
- **Hakkımda Sayfası**: Detaylı kişisel bilgiler.
- **İletişim Sayfası**: İletişim bilgileri.
- **Admin Paneli**:
  - Profil bilgilerini düzenleme
  - Eğitim bilgilerini yönetme
  - Deneyim bilgilerini yönetme
  - Yetenekleri yönetme
  - Sertifikaları yönetme

## Teknik Bilgiler

- **Framework**: ASP.NET Core 8.0 MVC
- **Veritabanı**: SQLite
- **ORM**: Entity Framework Core
- **Frontend**: Bootstrap 5, CSS, JavaScript
- **Deployment**: Herhangi bir web sunucusunda çalışabilir

## Kurulum

1. Repoyu klonlayın
2. `dotnet restore` komutu ile paketleri yükleyin
3. `dotnet run` komutu ile uygulamayı çalıştırın
4. Tarayıcınızdan `http://localhost:5000` adresine gidin

## Geliştirme Notları

Geliştirme ortamında çalışırken:
- SSL gerektiren güvenlik önlemleri otomatik olarak devre dışı bırakılır
- Üretim ortamına geçişte tüm güvenlik önlemleri aktif hale gelir 