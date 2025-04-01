using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DinamikCvSitesi.Models
{
    public class Experience
    {
        [Key]
        public int ExperienceID { get; set; }

        [Required(ErrorMessage = "Şirket adı zorunludur")]
        [StringLength(200)]
        public string Company { get; set; }

        [Required(ErrorMessage = "Pozisyon adı zorunludur")]
        [StringLength(200)]
        public string Position { get; set; }

        [StringLength(200)]
        public string Location { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? EndDate { get; set; }

        public bool IsCurrentlyWorking { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        public int? ProfileID { get; set; }
        
        [ForeignKey("ProfileID")]
        public virtual Profile Profile { get; set; }
    }
} 