using Microsoft.AspNetCore.Mvc;
using DinamikCvSitesi.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Antiforgery;

namespace DinamikCvSitesi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly CVContext _context;
        private readonly ILogger<AdminController> _logger;
        private readonly IHostEnvironment _environment;
        private readonly IAntiforgery _antiforgery;

        public AdminController(CVContext context, ILogger<AdminController> logger, IHostEnvironment environment, IAntiforgery antiforgery)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
            _antiforgery = antiforgery;
        }

        // Admin giriş sayfası - Giriş için authorize gerekmiyor
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, bool rememberMe = false)
        {
            try
            {
                // Kullanıcı adını kontrol et
                var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Username == username);

                if (admin != null && VerifyPassword(password, admin.Password))
                {
                    // Claims based identity oluştur
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, admin.AdminID.ToString()),
                        new Claim(ClaimTypes.Name, admin.Username),
                        new Claim(ClaimTypes.Role, "Admin")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, "DinamikCvSitesiCookie");

                    // Cookie ayarları
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = rememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                    };

                    // Kullanıcıyı giriş yaptır
                    await HttpContext.SignInAsync("DinamikCvSitesiCookie", new ClaimsPrincipal(claimsIdentity), authProperties);

                    _logger.LogInformation($"Admin user {username} logged in at {DateTime.UtcNow}");
                    return RedirectToAction("Index", "Admin");
                }

                _logger.LogWarning($"Failed login attempt for username {username} at {DateTime.UtcNow}");
                ViewBag.Error = "Kullanıcı adı veya şifre hatalı!";
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during login for user {username}");
                ViewBag.Error = "Giriş yapılırken bir hata oluştu.";
                return View();
            }
        }

        // Admin çıkış işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Authentication cookie'yi temizle
                await HttpContext.SignOutAsync("DinamikCvSitesiCookie");
                
                _logger.LogInformation($"Admin user {User.Identity.Name} logged out at {DateTime.UtcNow}");
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return RedirectToAction("Login");
            }
        }

        // Erişim reddedildi sayfası
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Admin girişi kontrolü için helper method
        private bool IsAdminLoggedIn()
        {
            return User.Identity.IsAuthenticated || HttpContext.Session.GetInt32("AdminId") != null;
        }

        // Admin dashboard sayfası
        public async Task<IActionResult> Index()
        {
            try
            {
                // Dashboard için gerekli sayım istatistiklerini topla
                ViewBag.EducationCount = await _context.Educations.CountAsync(e => e.ProfileID == 1);
                ViewBag.ExperienceCount = await _context.Experiences.CountAsync(e => e.ProfileID == 1);
                ViewBag.SkillCount = await _context.Skills.CountAsync(s => s.ProfileID == 1);
                ViewBag.CertificateCount = await _context.Certificates.CountAsync(c => c.ProfileID == 1);

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard data");
                ViewBag.Error = "Veriler yüklenirken bir hata oluştu.";
                return View();
            }
        }

        #region Profil Yönetimi
        
        // Profil düzenleme sayfası
        public async Task<IActionResult> EditProfile()
        {
            try
            {
                var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.ProfileID == 1);
                if (profile == null)
                {
                    _logger.LogWarning("Profile not found during edit request");
                    return NotFound();
                }

                return View(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving profile for editing");
                ViewBag.Error = "Profil verileri yüklenirken bir hata oluştu.";
                return View(new Profile());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(Profile profile, IFormFile profileImage)
        {
            try 
            {
                var existingProfile = await _context.Profiles.FindAsync(profile.ProfileID);

                if (existingProfile == null)
                {
                    _logger.LogWarning($"Profile with ID {profile.ProfileID} not found during update");
                    ViewBag.Error = "Profil bulunamadı!";
                    return View(profile);
                }

                // Sadece gönderilen değerleri güncelle, boşsa mevcut değerleri koru
                if (!string.IsNullOrWhiteSpace(profile.FullName))
                    existingProfile.FullName = profile.FullName;
                
                if (!string.IsNullOrWhiteSpace(profile.Title))
                    existingProfile.Title = profile.Title;
                
                if (!string.IsNullOrWhiteSpace(profile.Summary))
                    existingProfile.Summary = profile.Summary;
                
                if (!string.IsNullOrWhiteSpace(profile.Email))
                    existingProfile.Email = profile.Email;
                
                if (!string.IsNullOrWhiteSpace(profile.Phone))
                    existingProfile.Phone = profile.Phone;
                
                if (!string.IsNullOrWhiteSpace(profile.Address))
                    existingProfile.Address = profile.Address;
                
                if (!string.IsNullOrWhiteSpace(profile.LinkedIn))
                    existingProfile.LinkedIn = profile.LinkedIn;
                
                if (!string.IsNullOrWhiteSpace(profile.GitHub))
                    existingProfile.GitHub = profile.GitHub;
                
                if (!string.IsNullOrWhiteSpace(profile.Twitter))
                    existingProfile.Twitter = profile.Twitter;
                
                if (!string.IsNullOrWhiteSpace(profile.AboutMe))
                    existingProfile.AboutMe = profile.AboutMe;

                // Profil resmi yükleme işlemi
                if (profileImage != null && profileImage.Length > 0)
                {
                    // Validate file extension
                    var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(profileImage.FileName).ToLowerInvariant();
                    
                    if (!validExtensions.Contains(extension))
                    {
                        ViewBag.Error = "Lütfen geçerli bir resim dosyası (.jpg, .jpeg, .png, .gif) yükleyin.";
                        return View(existingProfile);
                    }
                    
                    // Validate file size (max 5MB)
                    if (profileImage.Length > 5 * 1024 * 1024)
                    {
                        ViewBag.Error = "Dosya boyutu 5MB'dan küçük olmalıdır.";
                        return View(existingProfile);
                    }
                    
                    try
                    {
                        // Dosya yükleme işlemleri
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(profileImage.FileName);
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                        
                        // Klasör yoksa oluştur
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }
                        
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await profileImage.CopyToAsync(stream);
                        }

                        existingProfile.ImagePath = "/images/" + fileName;
                        _logger.LogInformation($"Profile image uploaded successfully: {fileName}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading profile image");
                        ViewBag.Error = "Profil resmi yüklenirken bir hata oluştu: " + ex.Message;
                        return View(existingProfile);
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Profile updated successfully: {existingProfile.ProfileID}");
                ViewBag.Success = "Profil başarıyla güncellendi!";
                
                // Güncel verileri göster
                return View(existingProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating profile {profile.ProfileID}");
                ViewBag.Error = "Profil güncellenirken bir hata oluştu: " + ex.Message;
                return View(profile);
            }
        }
        
        #endregion

        #region Eğitim Yönetimi
        
        public async Task<IActionResult> Educations()
        {
            try
            {
                var educations = await _context.Educations.Where(e => e.ProfileID == 1).ToListAsync();
                return View(educations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving education list");
                ViewBag.Error = "Eğitim bilgileri yüklenirken bir hata oluştu.";
                return View(new List<Education>());
            }
        }

        public IActionResult AddEducation()
        {
            return View(new Education { ProfileID = 1 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEducation(Education education)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    education.ProfileID = 1;
                    _context.Educations.Add(education);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Education added successfully: {education.EducationID}");
                    return RedirectToAction("Educations");
                }
                return View(education);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding education record");
                ViewBag.Error = "Eğitim bilgisi eklenirken bir hata oluştu.";
                return View(education);
            }
        }

        public async Task<IActionResult> EditEducation(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login");
            }

            var education = await _context.Educations.FindAsync(id);
            if (education == null)
            {
                return NotFound();
            }

            return View(education);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEducation(Education education)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                education.ProfileID = 1;
                _context.Entry(education).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToAction("Educations");
            }

            return View(education);
        }

        public async Task<IActionResult> DeleteEducation(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login");
            }

            var education = await _context.Educations.FindAsync(id);
            if (education != null)
            {
                _context.Educations.Remove(education);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Educations");
        }
        
        #endregion

        #region Deneyim Yönetimi
        
        public async Task<IActionResult> Experiences()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login");
            }

            var experiences = await _context.Experiences.Where(e => e.ProfileID == 1).ToListAsync();
            return View(experiences);
        }

        public IActionResult AddExperience()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login");
            }

            return View(new Experience { ProfileID = 1 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddExperience(Experience experience)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                experience.ProfileID = 1;
                _context.Experiences.Add(experience);
                await _context.SaveChangesAsync();
                return RedirectToAction("Experiences");
            }

            return View(experience);
        }

        public async Task<IActionResult> EditExperience(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login");
            }

            var experience = await _context.Experiences.FindAsync(id);
            if (experience == null)
            {
                return NotFound();
            }

            return View(experience);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditExperience(Experience experience)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                experience.ProfileID = 1;
                _context.Entry(experience).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToAction("Experiences");
            }

            return View(experience);
        }

        public async Task<IActionResult> DeleteExperience(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login");
            }

            var experience = await _context.Experiences.FindAsync(id);
            if (experience != null)
            {
                _context.Experiences.Remove(experience);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Experiences");
        }
        
        #endregion

        #region Yetenek Yönetimi
        
        public async Task<IActionResult> Skills()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login");
            }

            var skills = await _context.Skills.Where(s => s.ProfileID == 1).ToListAsync();
            return View(skills);
        }

        public IActionResult AddSkill()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login");
            }

            return View(new Skill { ProfileID = 1 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSkill(Skill skill)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                skill.ProfileID = 1;
                _context.Skills.Add(skill);
                await _context.SaveChangesAsync();
                return RedirectToAction("Skills");
            }

            return View(skill);
        }

        public async Task<IActionResult> EditSkill(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login");
            }

            var skill = await _context.Skills.FindAsync(id);
            if (skill == null)
            {
                return NotFound();
            }

            return View(skill);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSkill(Skill skill)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                skill.ProfileID = 1;
                _context.Entry(skill).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToAction("Skills");
            }

            return View(skill);
        }

        public async Task<IActionResult> DeleteSkill(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login");
            }

            var skill = await _context.Skills.FindAsync(id);
            if (skill != null)
            {
                _context.Skills.Remove(skill);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Skills");
        }
        
        #endregion

        #region Sertifika Yönetimi
        
        public async Task<IActionResult> Certificates()
        {
            try
            {
                var certificates = await _context.Certificates.Where(c => c.ProfileID == 1).ToListAsync();
                return View(certificates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving certificate list");
                ViewBag.Error = "Sertifika bilgileri yüklenirken bir hata oluştu.";
                return View(new List<Certificate>());
            }
        }

        public IActionResult AddCertificate()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login");
            }

            return View(new Certificate { ProfileID = 1 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCertificate(Certificate certificate)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                certificate.ProfileID = 1;
                _context.Certificates.Add(certificate);
                await _context.SaveChangesAsync();
                return RedirectToAction("Certificates");
            }

            return View(certificate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCertificate(Certificate certificate)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                certificate.ProfileID = 1;
                _context.Entry(certificate).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToAction("Certificates");
            }

            return View(certificate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCertificate(int id)
        {
            try
            {
                var certificate = await _context.Certificates.FindAsync(id);
                if (certificate != null)
                {
                    _context.Certificates.Remove(certificate);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Certificate deleted successfully: {id}");
                }
                else
                {
                    _logger.LogWarning($"Certificate not found during delete request: {id}");
                }

                return RedirectToAction("Certificates");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting certificate {id}");
                TempData["Error"] = "Sertifika silinirken bir hata oluştu.";
                return RedirectToAction("Certificates");
            }
        }
        
        #endregion

        #region Yardımcı Metodlar
        
        // Şifre hashleme metodu
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
        
        // Şifre doğrulama metodu
        private bool VerifyPassword(string password, string hashedPassword)
        {
            // Eğer hash kullanılmıyorsa geçici olarak doğrudan karşılaştırma yap
            // Not: Gerçek sistemlerde düz metin şifre kullanılmamalıdır
            if (password == hashedPassword)
            {
                return true;
            }
            
            var hashedInput = HashPassword(password);
            return hashedInput == hashedPassword;
        }
        
        #endregion
    }
} 