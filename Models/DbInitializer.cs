using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace DinamikCvSitesi.Models
{
    public static class DbInitializer
    {
        public static void Initialize(CVContext context)
        {
            context.Database.EnsureCreated();

            // Admin kullanıcısı kontrolü
            if (!context.Admins.Any())
            {
                context.Admins.Add(new Admin
                {
                    Username = "admin",
                    Password = "123456", // Gerçek projede şifre hashlenmeli
                    Email = "admin@example.com"
                });

                context.SaveChanges();
            }

            // Varsayılan profil kontrolü
            if (!context.Profiles.Any())
            {
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
                    AboutMe = "Ben bir yazılım geliştiriciyim ve yeni teknolojileri öğrenmeyi seviyorum."
                });

                context.SaveChanges();
            }
        }
    }
} 