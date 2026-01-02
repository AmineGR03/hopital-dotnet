using System.ComponentModel.DataAnnotations;

namespace gestion_hopital.Models
{
    public class Patient
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
        
        [Required(ErrorMessage = "La date de naissance est requise")]
        [DataType(DataType.Date)]
        [Display(Name = "Date de naissance")]
        public DateTime DateNaissance { get; set; }
        
        [Required(ErrorMessage = "Le sexe est requis")]
        [StringLength(10, ErrorMessage = "Le sexe ne peut pas dépasser 10 caractères")]
        [Display(Name = "Sexe")]
        public string Sexe { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Le téléphone est requis")]
        [StringLength(20, ErrorMessage = "Le téléphone ne peut pas dépasser 20 caractères")]
        [Display(Name = "Téléphone")]
        public string Tel { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "L'adresse est requise")]
        [StringLength(200, ErrorMessage = "L'adresse ne peut pas dépasser 200 caractères")]
        [Display(Name = "Adresse")]
        public string Adresse { get; set; } = string.Empty;
        
        // Relations
        public HistoriqueMedical? HistoriqueMedical { get; set; }
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
        
        [Display(Name = "Nom complet")]
        public string NomComplet => $"{Prenom} {Nom}";
        
        [Display(Name = "Âge")]
        public int Age => DateTime.Now.Year - DateNaissance.Year - (DateTime.Now.DayOfYear < DateNaissance.DayOfYear ? 1 : 0);
    }
}



