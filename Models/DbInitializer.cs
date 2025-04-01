using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace DinamikCvSitesi.Models
{
    public static class DbInitializer
    {
        public static void Initialize(CVContext context)
        {
            try
            {
                Console.WriteLine("Veritabanı oluşturuluyor...");
                context.Database.EnsureCreated();

                // Admin kullanıcısı kontrolü
                if (!context.Admins.Any())
                {
                    Console.WriteLine("Admin oluşturuluyor...");
                    context.Admins.Add(new Admin
                    {
                        Username = "admin",
                        Password = "123456", // Gerçek projede şifre hashlenmeli
                        Email = "admin@example.com"
                    });

                    context.SaveChanges();
                    Console.WriteLine("Admin başarıyla oluşturuldu.");
                }

                // Varsayılan profil kontrolü
                if (!context.Profiles.Any())
                {
                    Console.WriteLine("Profil oluşturuluyor...");
                    context.Profiles.Add(new Profile
                    {
                        FullName = "Ad Soyad",
                        Title = "Yazılım Geliştirici",
                        Summary = "Yazılım geliştirme konusunda deneyimli, problem çözme becerileri yüksek bir yazılım mühendisi.",
                        Email = "ornek@email.com",
                        Phone = "+90 555 123 45 67",
                        Address = "İstanbul, Türkiye",
                        LinkedIn = "https://linkedin.com/in/adsoyad",
                        GitHub = "https://github.com/adsoyad",
                        Twitter = "", // Explicitly set empty string instead of null
                        AboutMe = "Ben bir yazılım geliştiriciyim ve yeni teknolojileri öğrenmeyi seviyorum.",
                        ImagePath = "/images/profile-placeholder.jpg" // Set default image path
                    });

                    context.SaveChanges();
                    Console.WriteLine("Profil başarıyla oluşturuldu.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"İç hata: {ex.InnerException.Message}");
                }
            }
        }
    }
} 