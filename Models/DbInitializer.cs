using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DinamikCvSitesi.Models
{
    public static class DbInitializer
    {
        // Şifre hashleme metodu
        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        public static void Initialize(CVContext context)
        {
            try
            {
                Console.WriteLine("Veritabanı oluşturuluyor...");
                context.Database.EnsureCreated();

                // Admin kullanıcısı kontrolü
                if (!context.Admins.Any())
                {
                    Console.WriteLine("Admin hesapları oluşturuluyor...");
                    
                    // Ana admin hesabı (güvenli şifreyle)
                    context.Admins.Add(new Admin
                    {
                        Username = "admin",
                        Password = HashPassword("Admin123!"), // Şifrelenmiş güvenli şifre
                        Email = "admin@example.com"
                    });

                    // Test admin hesabı (kolay hatırlanabilir şifreyle)
                    context.Admins.Add(new Admin
                    {
                        Username = "test",
                        Password = HashPassword("Test123!"), // Şifrelenmiş test şifresi
                        Email = "test@example.com"
                    });

                    context.SaveChanges();
                    Console.WriteLine("Admin hesapları başarıyla oluşturuldu.");
                }

                // Varsayılan profil kontrolü
                if (!context.Profiles.Any())
                {
                    Console.WriteLine("Profil oluşturuluyor...");
                    
                    try
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
                            Twitter = string.Empty, // Null değil boş string
                            AboutMe = "Ben bir yazılım geliştiriciyim ve yeni teknolojileri öğrenmeyi seviyorum.",
                            ImagePath = "/images/profile-placeholder.jpg" // Varsayılan resim yolu
                        });

                        context.SaveChanges();
                        Console.WriteLine("Profil başarıyla oluşturuldu.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Profil oluşturulurken hata: {ex.Message}");
                        if (ex.InnerException != null)
                        {
                            Console.WriteLine($"İç hata: {ex.InnerException.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Veritabanı işlemlerinde hata: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"İç hata: {ex.InnerException.Message}");
                }
            }
        }
    }
} 