using System.ComponentModel.DataAnnotations;

namespace gestion_hopital.Models
{
    public class HistoriqueMedical
    {
        public int Id { get; set; }
        
        [StringLength(10, ErrorMessage = "Le groupe sanguin ne peut pas dépasser 10 caractères")]
        [Display(Name = "Groupe sanguin")]
        public string GroupeSanguin { get; set; } = string.Empty;
        
        [Display(Name = "Allergies")]
        public string Allergies { get; set; } = string.Empty; // JSON ou séparé par virgule
        
        public int? PatientId { get; set; }
        public Patient? Patient { get; set; }
        
        public ICollection<DiagnosticPasse> DiagnosticsPasses { get; set; } = new List<DiagnosticPasse>();
    }
}



