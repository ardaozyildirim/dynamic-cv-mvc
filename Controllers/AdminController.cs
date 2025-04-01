using Microsoft.AspNetCore.Mvc;
using DinamikCvSitesi.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace DinamikCvSitesi.Controllers
{
    public class AdminController : Controller
    {
        private readonly CVContext _context;

        public AdminController(CVContext context)
        {
            _context = context;
        }

        // Admin giriş sayfası
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, bool rememberMe = true)
        {
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Username == username && a.Password == password);

            if (admin != null)
            {
                // Claims based identity oluştur
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, admin.AdminID.ToString()),
                    new Claim(ClaimTypes.Name, admin.Username),
                    new Claim(ClaimTypes.Role, "Admin")
                };

                var claimsIdentity = new ClaimsIdentity(claims, "DinamikCvSitesiCookie");

                // Kalıcı giriş için cookie ayarları
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = rememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
                };

                // Kullanıcıyı giriş yaptır
                await HttpContext.SignInAsync("DinamikCvSitesiCookie", new ClaimsPrincipal(claimsIdentity), authProperties);

                // Session'a da admin bilgilerini kaydet (eski kodla uyumluluk için)
                HttpContext.Session.SetInt32("AdminId", admin.AdminID);
                HttpContext.Session.SetString("AdminUsername", admin.Username);
                
                return RedirectToAction("Index", "Admin");
            }

            ViewBag.Error = "Kullanıcı adı veya şifre hatalı!";
            return View();
        }

        // Admin çıkış işlemi
        public async Task<IActionResult> Logout()
        {
            // Authentication cookie'yi temizle
            await HttpContext.SignOutAsync("DinamikCvSitesiCookie");
            
            // Session'dan admin bilgilerini temizle
            HttpContext.Session.Remove("AdminId");
            HttpContext.Session.Remove("AdminUsername");
            
            return RedirectToAction("Login");
        }

        // Admin girişi kontrolü için helper method
        private bool IsAdminLoggedIn()
        {
            return User.Identity.IsAuthenticated || HttpContext.Session.GetInt32("AdminId") != null;
        }

        // Admin dashboard sayfası - giriş kontrolü
        public async Task<IActionResult> Index()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login");
            }

            // Dashboard için gerekli sayım istatistiklerini topla
            ViewBag.EducationCount = await _context.Educations.CountAsync(e => e.ProfileID == 1);
            ViewBag.ExperienceCount = await _context.Experiences.CountAsync(e => e.ProfileID == 1);
            ViewBag.SkillCount = await _context.Skills.CountAsync(s => s.ProfileID == 1);
            ViewBag.CertificateCount = await _context.Certificates.CountAsync(c => c.ProfileID == 1);

            return View();
        }

        #region Profil Yönetimi
        
        // Profil düzenleme sayfası
        public async Task<IActionResult> EditProfile()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login");
            }

            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.ProfileID == 1);
            return View(profile);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(Profile profile, IFormFile profileImage)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login");
            }

            try 
            {
                var existingProfile = await _context.Profiles.FindAsync(profile.ProfileID);

                if (existingProfile == null)
                {
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
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Error = "Profil resmi yüklenirken bir hata oluştu: " + ex.Message;
                        return View(existingProfile);
                    }
                }

                await _context.SaveChangesAsync();
                ViewBag.Success = "Profil başarıyla güncellendi!";
                
                // Güncel verileri göster
                return View(existingProfile);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Profil güncellenirken bir hata oluştu: " + ex.Message;
                return View(profile);
            }
        }
        
        #endregion

        #region Eğitim Yönetimi
        
        public async Task<IActionResult> Educations()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login");
            }

            var educations = await _context.Educations.Where(e => e.ProfileID == 1).ToListAsync();
            return View(educations);
        }

        public IActionResult AddEducation()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login");
            }

            return View(new Education { ProfileID = 1 });
        }

        [HttpPost]
        public async Task<IActionResult> AddEducation(Education education)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                education.ProfileID = 1;
                _context.Educations.Add(education);
                await _context.SaveChangesAsync();
                return RedirectToAction("Educations");
            }

            return View(education);
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
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login");
            }

            var certificates = await _context.Certificates.Where(c => c.ProfileID == 1).ToListAsync();
            return View(certificates);
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

        public async Task<IActionResult> EditCertificate(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login");
            }

            var certificate = await _context.Certificates.FindAsync(id);
            if (certificate == null)
            {
                return NotFound();
            }

            return View(certificate);
        }

        [HttpPost]
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

        public async Task<IActionResult> DeleteCertificate(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login");
            }

            var certificate = await _context.Certificates.FindAsync(id);
            if (certificate != null)
            {
                _context.Certificates.Remove(certificate);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Certificates");
        }
        
        #endregion
    }
} 