using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gestion_hopital.Models
{
    public class Appointment
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
        
        [Required(ErrorMessage = "La date du rendez-vous est requise")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Date du rendez-vous")]
        public DateTime DateRdv { get; set; }
        
        [Required(ErrorMessage = "Le motif est requis")]
        [StringLength(500, ErrorMessage = "Le motif ne peut pas dépasser 500 caractères")]
        [Display(Name = "Motif")]
        public string Motif { get; set; } = string.Empty;
        
        [StringLength(50, ErrorMessage = "Le statut ne peut pas dépasser 50 caractères")]
        [Display(Name = "Statut")]
        public string Status { get; set; } = "Planifié"; // Planifié, Confirmé, Annulé, Terminé
    }
}



