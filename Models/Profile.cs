using System.ComponentModel.DataAnnotations;

namespace DinamikCvSitesi.Models
{
    public class Profile
    {
        [Key]
        public int ProfileID { get; set; }

        [Required(ErrorMessage = "Ad ve soyad zorunludur")]
        [StringLength(100)]
        public string FullName { get; set; }

        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(500)]
        public string? Summary { get; set; }

        [StringLength(200)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(200)]
        public string? ImagePath { get; set; }

        [StringLength(100)]
        public string? LinkedIn { get; set; }

        [StringLength(100)]
        public string? GitHub { get; set; }

        [StringLength(100)]
        public string? Twitter { get; set; }

        [StringLength(2000)]
        public string? AboutMe { get; set; }
    }
} 