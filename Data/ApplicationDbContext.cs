using gestion_hopital.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace gestion_hopital.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<HistoriqueMedical> HistoriquesMedicaux { get; set; }
        public DbSet<DiagnosticPasse> DiagnosticsPasses { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configuration de la relation many-to-many Doctor-Patient
            builder.Entity<Doctor>()
                .HasMany(d => d.Patients)
                .WithMany(p => p.Doctors)
                .UsingEntity(j => j.ToTable("DoctorPatients"));

            // Configuration de HistoriqueMedical (one-to-one avec Patient)
            builder.Entity<HistoriqueMedical>()
                .HasOne(h => h.Patient)
                .WithOne(p => p.HistoriqueMedical)
                .HasForeignKey<HistoriqueMedical>(h => h.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuration de DiagnosticPasse (many-to-one avec HistoriqueMedical)
            builder.Entity<DiagnosticPasse>()
                .HasOne(d => d.HistoriqueMedical)
                .WithMany(h => h.DiagnosticsPasses)
                .HasForeignKey(d => d.HistoriqueMedicalId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuration de Appointment
            builder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuration de Prescription
            builder.Entity<Prescription>()
                .HasOne(p => p.Patient)
                .WithMany(p => p.Prescriptions)
                .HasForeignKey(p => p.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Prescription>()
                .HasOne(p => p.Doctor)
                .WithMany(d => d.Prescriptions)
                .HasForeignKey(p => p.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuration de Doctor avec ApplicationUser
            builder.Entity<Doctor>()
                .HasOne(d => d.Utilisateur)
                .WithOne(u => u.Doctor)
                .HasForeignKey<Doctor>(d => d.UtilisateurId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
