using System.ComponentModel.DataAnnotations;

namespace gestion_hopital.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Le nom est requis")]
        [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
        [Display(Name = "Nom")]
        public string Nom { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Le prénom est requis")]
        [StringLength(100, ErrorMessage = "Le prénom ne peut pas dépasser 100 caractères")]
        [Display(Name = "Prénom")]
        public string Prenom { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "La spécialité est requise")]
        [StringLength(100, ErrorMessage = "La spécialité ne peut pas dépasser 100 caractères")]
        [Display(Name = "Spécialité")]
        public string Specialite { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Le téléphone est requis")]
        [StringLength(20, ErrorMessage = "Le téléphone ne peut pas dépasser 20 caractères")]
        [Display(Name = "Téléphone")]
        public string Tel { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "L'adresse est requise")]
        [StringLength(200, ErrorMessage = "L'adresse ne peut pas dépasser 200 caractères")]
        [Display(Name = "Adresse")]
        public string Adresse { get; set; } = string.Empty;
        
        // Relation avec Utilisateur
        public string? UtilisateurId { get; set; }
        public ApplicationUser? Utilisateur { get; set; }
        
        // Relations
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
        public ICollection<Patient> Patients { get; set; } = new List<Patient>();
        
        [Display(Name = "Nom complet")]
        public string NomComplet => $"{Prenom} {Nom}";
    }
}



