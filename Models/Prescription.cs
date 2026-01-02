using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gestion_hopital.Models
{
    public class Prescription
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Le patient est requis")]
        [Display(Name = "Patient")]
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }
        
        [Required(ErrorMessage = "Le médecin est requis")]
        [Display(Name = "Médecin")]
        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; }
        
        [Required(ErrorMessage = "Les médicaments sont requis")]
        [Display(Name = "Médicaments")]
        public string Medicaments { get; set; } = string.Empty; // JSON array ou séparé par virgule
        
        [Required(ErrorMessage = "La date est requise")]
        [DataType(DataType.Date)]
        [Display(Name = "Date")]
        public DateTime Date { get; set; }
        
        [Required(ErrorMessage = "La durée de validité est requise")]
        [Range(1, 365, ErrorMessage = "La validité doit être entre 1 et 365 jours")]
        [Display(Name = "Validité (jours)")]
        public int ValidityDays { get; set; }
        
        [StringLength(1000, ErrorMessage = "Les instructions ne peuvent pas dépasser 1000 caractères")]
        [Display(Name = "Instructions")]
        public string Instructions { get; set; } = string.Empty;
        
        [Display(Name = "Date d'expiration")]
        public DateTime DateExpiration => Date.AddDays(ValidityDays);
    }
}



