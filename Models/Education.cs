using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DinamikCvSitesi.Models
{
    public class Education
    {
        [Key]
        public int EducationID { get; set; }

        [Required(ErrorMessage = "Okul adı zorunludur")]
        [StringLength(200)]
        public string School { get; set; }

        [StringLength(200)]
        public string Degree { get; set; }

        [StringLength(200)]
        public string FieldOfStudy { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? EndDate { get; set; }

        public bool IsCurrentlyStudying { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public int? ProfileID { get; set; }
        
        [ForeignKey("ProfileID")]
        public virtual Profile Profile { get; set; }
    }
} 