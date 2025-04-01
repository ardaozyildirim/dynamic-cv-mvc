using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DinamikCvSitesi.Models
{
    public class Certificate
    {
        [Key]
        public int CertificateID { get; set; }

        [Required(ErrorMessage = "Sertifika adı zorunludur")]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(200)]
        public string IssuingOrganization { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? IssueDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? ExpirationDate { get; set; }

        [StringLength(200)]
        public string CredentialID { get; set; }

        [StringLength(200)]
        public string CredentialURL { get; set; }

        public int? ProfileID { get; set; }
        
        [ForeignKey("ProfileID")]
        public virtual Profile Profile { get; set; }
    }
} 