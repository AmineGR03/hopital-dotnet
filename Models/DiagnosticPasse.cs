using System.ComponentModel.DataAnnotations;

namespace gestion_hopital.Models
{
    public class DiagnosticPasse
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "La date est requise")]
        [DataType(DataType.Date)]
        [Display(Name = "Date")]
        public DateTime Date { get; set; }
        
        [Required(ErrorMessage = "La condition est requise")]
        [StringLength(200, ErrorMessage = "La condition ne peut pas dépasser 200 caractères")]
        [Display(Name = "Condition")]
        public string Condition { get; set; } = string.Empty;
        
        [StringLength(1000, ErrorMessage = "Les notes ne peuvent pas dépasser 1000 caractères")]
        [Display(Name = "Notes")]
        public string Notes { get; set; } = string.Empty;
        
        [Required]
        public int HistoriqueMedicalId { get; set; }
        public HistoriqueMedical? HistoriqueMedical { get; set; }
    }
}



