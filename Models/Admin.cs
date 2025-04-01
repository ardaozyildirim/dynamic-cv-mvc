using System.ComponentModel.DataAnnotations;

namespace DinamikCvSitesi.Models
{
    public class Admin
    {
        [Key]
        public int AdminID { get; set; }

        [Required(ErrorMessage = "Kullanıcı adı zorunludur")]
        [StringLength(50)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur")]
        [StringLength(100)]
        public string Password { get; set; }

        public string Email { get; set; }
    }
} 