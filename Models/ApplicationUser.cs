using Microsoft.AspNetCore.Identity;

namespace gestion_hopital.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Role { get; set; } = string.Empty; // admin, doctor, receptionist
        public int? DoctorId { get; set; } // Requis uniquement si role = 'doctor'
        public Doctor? Doctor { get; set; }
    }
}



