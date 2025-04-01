using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DinamikCvSitesi.Models;
using Microsoft.EntityFrameworkCore;

namespace DinamikCvSitesi.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly CVContext _context;

    public HomeController(ILogger<HomeController> logger, CVContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Varsayılan profili al (1 numaralı profil)
        var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.ProfileID == 1);
        
        if (profile == null)
        {
            // Eğer profil yoksa, yeni bir profil oluştur
            profile = new Profile
            {
                FullName = "Adınız Soyadınız",
                Title = "Ünvanınız",
                Summary = "Kısa özgeçmişiniz",
                Email = "ornek@email.com",
                Phone = "555-1234567",
                Address = "İstanbul, Türkiye",
                LinkedIn = "https://linkedin.com/in/ornek",
                GitHub = "https://github.com/ornek",
                Twitter = "https://twitter.com/ornek",
                ImagePath = "",
                AboutMe = "Hakkımda bilgisi buraya eklenecek."
            };
            
            _context.Profiles.Add(profile);
            await _context.SaveChangesAsync();
        }
        
        return View(profile);
    }

    public async Task<IActionResult> About()
    {
        var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.ProfileID == 1);
        return View(profile);
    }

    public async Task<IActionResult> Resume()
    {
        var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.ProfileID == 1);
        
        if (profile != null)
        {
            // İlgili profil için eğitim, deneyim, yetenek ve sertifikaları getir
            var educations = await _context.Educations.Where(e => e.ProfileID == profile.ProfileID).ToListAsync();
            var experiences = await _context.Experiences.Where(e => e.ProfileID == profile.ProfileID).ToListAsync();
            var skills = await _context.Skills.Where(s => s.ProfileID == profile.ProfileID).ToListAsync();
            var certificates = await _context.Certificates.Where(c => c.ProfileID == profile.ProfileID).ToListAsync();
            
            ViewBag.Educations = educations;
            ViewBag.Experiences = experiences;
            ViewBag.Skills = skills;
            ViewBag.Certificates = certificates;
        }
        
        return View(profile);
    }

    public IActionResult Contact()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
