using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DinamikCvSitesi.Models
{
    public class Skill
    {
        [Key]
        public int SkillID { get; set; }

        [Required(ErrorMessage = "Yetenek adı zorunludur")]
        [StringLength(100)]
        public string Name { get; set; }

        [Range(1, 100, ErrorMessage = "Seviye 1-100 arasında olmalıdır")]
        public int Level { get; set; }

        [StringLength(50)]
        public string Category { get; set; }

        public int? ProfileID { get; set; }
        
        [ForeignKey("ProfileID")]
        public virtual Profile Profile { get; set; }
    }
} 