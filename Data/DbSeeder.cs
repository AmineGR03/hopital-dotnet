using gestion_hopital.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace gestion_hopital.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Créer les rôles s'ils n'existent pas
            string[] roles = { "admin", "doctor", "receptionist" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Créer l'utilisateur admin
            var adminUser = await userManager.FindByEmailAsync("admin@hopital.com");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin@hopital.com",
                    Email = "admin@hopital.com",
                    EmailConfirmed = true,
                    Role = "admin"
                };
                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "admin");
                    Console.WriteLine("✓ Admin créé : admin@hopital.com / Admin123!");
                }
            }

            // Créer l'utilisateur réceptionniste
            var receptionistUser = await userManager.FindByEmailAsync("receptionist@hopital.com");
            if (receptionistUser == null)
            {
                receptionistUser = new ApplicationUser
                {
                    UserName = "receptionist@hopital.com",
                    Email = "receptionist@hopital.com",
                    EmailConfirmed = true,
                    Role = "receptionist"
                };
                var result = await userManager.CreateAsync(receptionistUser, "Receptionist123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(receptionistUser, "receptionist");
                    Console.WriteLine("✓ Réceptionniste créé : receptionist@hopital.com / Receptionist123!");
                }
            }

            // Créer des médecins
            if (!await context.Doctors.AnyAsync())
            {
                var doctor1 = new Doctor
                {
                    Nom = "Martin",
                    Prenom = "Jean",
                    Specialite = "Cardiologie",
                    Tel = "0123456789",
                    Adresse = "123 Rue de la Santé, Paris"
                };
                context.Doctors.Add(doctor1);

                var doctor2 = new Doctor
                {
                    Nom = "Dubois",
                    Prenom = "Marie",
                    Specialite = "Pédiatrie",
                    Tel = "0123456790",
                    Adresse = "456 Avenue des Enfants, Lyon"
                };
                context.Doctors.Add(doctor2);

                var doctor3 = new Doctor
                {
                    Nom = "Bernard",
                    Prenom = "Pierre",
                    Specialite = "Neurologie",
                    Tel = "0123456791",
                    Adresse = "789 Boulevard Neurologique, Marseille"
                };
                context.Doctors.Add(doctor3);

                await context.SaveChangesAsync();
                Console.WriteLine("✓ 3 médecins créés");
            }

            // Créer l'utilisateur médecin et l'associer à un médecin
            var doctorUser = await userManager.FindByEmailAsync("doctor@hopital.com");
            if (doctorUser == null)
            {
                var doctor = await context.Doctors.FirstOrDefaultAsync();
                if (doctor != null)
                {
                    doctorUser = new ApplicationUser
                    {
                        UserName = "doctor@hopital.com",
                        Email = "doctor@hopital.com",
                        EmailConfirmed = true,
                        Role = "doctor",
                        DoctorId = doctor.Id
                    };
                    var result = await userManager.CreateAsync(doctorUser, "Doctor123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(doctorUser, "doctor");
                        doctor.UtilisateurId = doctorUser.Id;
                        await context.SaveChangesAsync();
                        Console.WriteLine($"✓ Médecin créé : doctor@hopital.com / Doctor123! (associé au Dr. {doctor.NomComplet})");
                    }
                }
            }

            // Créer un deuxième utilisateur médecin et l'associer au deuxième médecin
            var doctorUser2 = await userManager.FindByEmailAsync("doctor2@hopital.com");
            if (doctorUser2 == null)
            {
                var doctors = await context.Doctors.ToListAsync();
                if (doctors.Count > 1)
                {
                    var doctor2 = doctors[1]; // Dr. Marie Dubois
                    doctorUser2 = new ApplicationUser
                    {
                        UserName = "doctor2@hopital.com",
                        Email = "doctor2@hopital.com",
                        EmailConfirmed = true,
                        Role = "doctor",
                        DoctorId = doctor2.Id
                    };
                    var result = await userManager.CreateAsync(doctorUser2, "Doctor2123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(doctorUser2, "doctor");
                        doctor2.UtilisateurId = doctorUser2.Id;
                        await context.SaveChangesAsync();
                        Console.WriteLine($"✓ Médecin 2 créé : doctor2@hopital.com / Doctor2123! (associé au Dr. {doctor2.NomComplet})");
                    }
                }
            }

            // Créer des patients
            if (!await context.Patients.AnyAsync())
            {
                var patient1 = new Patient
                {
                    Nom = "Dupont",
                    Prenom = "Sophie",
                    DateNaissance = new DateTime(1985, 5, 15),
                    Sexe = "F",
                    Tel = "0612345678",
                    Adresse = "10 Rue de la Paix, Paris"
                };
                context.Patients.Add(patient1);

                var patient2 = new Patient
                {
                    Nom = "Lefebvre",
                    Prenom = "Marc",
                    DateNaissance = new DateTime(1990, 8, 22),
                    Sexe = "M",
                    Tel = "0612345679",
                    Adresse = "20 Avenue de la République, Lyon"
                };
                context.Patients.Add(patient2);

                var patient3 = new Patient
                {
                    Nom = "Moreau",
                    Prenom = "Julie",
                    DateNaissance = new DateTime(1978, 12, 3),
                    Sexe = "F",
                    Tel = "0612345680",
                    Adresse = "30 Boulevard de la Liberté, Marseille"
                };
                context.Patients.Add(patient3);

                var patient4 = new Patient
                {
                    Nom = "Garcia",
                    Prenom = "Lucas",
                    DateNaissance = new DateTime(2000, 3, 10),
                    Sexe = "M",
                    Tel = "0612345681",
                    Adresse = "40 Rue des Fleurs, Nice"
                };
                context.Patients.Add(patient4);

                await context.SaveChangesAsync();
                Console.WriteLine("✓ 4 patients créés");
            }

            // Créer des historiques médicaux
            var patients = await context.Patients.ToListAsync();
            foreach (var patient in patients.Take(2))
            {
                if (patient.HistoriqueMedical == null)
                {
                    var historique = new HistoriqueMedical
                    {
                        PatientId = patient.Id,
                        GroupeSanguin = new[] { "A+", "B+", "O+", "AB+" }[patients.IndexOf(patient) % 4],
                        Allergies = new[] { "Aucune", "Pénicilline", "Aspirine", "Aucune" }[patients.IndexOf(patient) % 4]
                    };
                    context.HistoriquesMedicaux.Add(historique);
                }
            }
            await context.SaveChangesAsync();
            Console.WriteLine("✓ Historiques médicaux créés");

            // Créer des diagnostics passés
            var historiques = await context.HistoriquesMedicaux.ToListAsync();
            if (historiques.Any() && !await context.DiagnosticsPasses.AnyAsync())
            {
                foreach (var historique in historiques)
                {
                    var diagnostic1 = new DiagnosticPasse
                    {
                        HistoriqueMedicalId = historique.Id,
                        Date = DateTime.Now.AddMonths(-6),
                        Condition = "Hypertension artérielle",
                        Notes = "Traitement prescrit, suivi régulier nécessaire"
                    };
                    context.DiagnosticsPasses.Add(diagnostic1);

                    var diagnostic2 = new DiagnosticPasse
                    {
                        HistoriqueMedicalId = historique.Id,
                        Date = DateTime.Now.AddMonths(-3),
                        Condition = "Diabète type 2",
                        Notes = "Contrôle glycémique en cours"
                    };
                    context.DiagnosticsPasses.Add(diagnostic2);
                }
                await context.SaveChangesAsync();
                Console.WriteLine("✓ Diagnostics passés créés");
            }

            // Créer des rendez-vous
            if (!await context.Appointments.AnyAsync())
            {
                var doctors = await context.Doctors.ToListAsync();
                var appointments = new List<Appointment>
                {
                    new Appointment
                    {
                        PatientId = patients[0].Id,
                        DoctorId = doctors[0].Id,
                        DateRdv = DateTime.Now.AddDays(5),
                        Motif = "Consultation de routine",
                        Status = "Planifié"
                    },
                    new Appointment
                    {
                        PatientId = patients[1].Id,
                        DoctorId = doctors[1].Id,
                        DateRdv = DateTime.Now.AddDays(7),
                        Motif = "Examen annuel",
                        Status = "Confirmé"
                    },
                    new Appointment
                    {
                        PatientId = patients[2].Id,
                        DoctorId = doctors[0].Id,
                        DateRdv = DateTime.Now.AddDays(-2),
                        Motif = "Suivi post-opératoire",
                        Status = "Terminé"
                    },
                    new Appointment
                    {
                        PatientId = patients[3].Id,
                        DoctorId = doctors[2].Id,
                        DateRdv = DateTime.Now.AddDays(10),
                        Motif = "Consultation spécialisée",
                        Status = "Planifié"
                    }
                };
                context.Appointments.AddRange(appointments);
                await context.SaveChangesAsync();
                Console.WriteLine("✓ 4 rendez-vous créés");
            }

            // Créer des prescriptions
            if (!await context.Prescriptions.AnyAsync())
            {
                var doctors = await context.Doctors.ToListAsync();
                var prescriptions = new List<Prescription>
                {
                    new Prescription
                    {
                        PatientId = patients[0].Id,
                        DoctorId = doctors[0].Id,
                        Date = DateTime.Now.AddDays(-10),
                        Medicaments = "Paracétamol 500mg, Ibuprofène 400mg",
                        ValidityDays = 30,
                        Instructions = "Prendre 2 comprimés de paracétamol toutes les 6 heures, 1 comprimé d'ibuprofène matin et soir"
                    },
                    new Prescription
                    {
                        PatientId = patients[1].Id,
                        DoctorId = doctors[1].Id,
                        Date = DateTime.Now.AddDays(-5),
                        Medicaments = "Amoxicilline 500mg, Probiotiques",
                        ValidityDays = 15,
                        Instructions = "Prendre 1 comprimé d'amoxicilline 3 fois par jour pendant 7 jours, probiotiques le matin"
                    },
                    new Prescription
                    {
                        PatientId = patients[2].Id,
                        DoctorId = doctors[0].Id,
                        Date = DateTime.Now.AddDays(-3),
                        Medicaments = "Métoprolol 50mg, Aspirine 100mg",
                        ValidityDays = 60,
                        Instructions = "Prendre 1 comprimé de métoprolol matin et soir, 1 comprimé d'aspirine le matin"
                    }
                };
                context.Prescriptions.AddRange(prescriptions);
                await context.SaveChangesAsync();
                Console.WriteLine("✓ 3 prescriptions créées");
            }

            // Créer des relations many-to-many Doctor-Patient
            var doctorPatients = await context.Doctors
                .Include(d => d.Patients)
                .ToListAsync();
            
            if (doctorPatients.Any(d => d.Patients.Count == 0))
            {
                var allDoctors = await context.Doctors.ToListAsync();
                var allPatients = await context.Patients.ToListAsync();
                
                if (allDoctors.Count > 0 && allPatients.Count > 0)
                {
                    // Associer chaque médecin à quelques patients
                    allDoctors[0].Patients.Add(allPatients[0]);
                    allDoctors[0].Patients.Add(allPatients[2]);
                    
                    allDoctors[1].Patients.Add(allPatients[1]);
                    allDoctors[1].Patients.Add(allPatients[3]);
                    
                    allDoctors[2].Patients.Add(allPatients[0]);
                    allDoctors[2].Patients.Add(allPatients[1]);
                    
                    await context.SaveChangesAsync();
                    Console.WriteLine("✓ Relations médecin-patient créées");
                }
            }

            Console.WriteLine("\n=== SEEDING TERMINÉ ===");
            Console.WriteLine("\n=== IDENTIFIANTS DE CONNEXION ===");
            Console.WriteLine("\n🔑 ADMINISTRATEUR:");
            Console.WriteLine("   Email: admin@hopital.com");
            Console.WriteLine("   Mot de passe: Admin123!");
            Console.WriteLine("   Rôle: admin (accès complet)");
            
            Console.WriteLine("\n🔑 MÉDECIN 1:");
            Console.WriteLine("   Email: doctor@hopital.com");
            Console.WriteLine("   Mot de passe: Doctor123!");
            Console.WriteLine("   Rôle: doctor (gestion patients, rendez-vous, prescriptions)");
            
            Console.WriteLine("\n🔑 MÉDECIN 2:");
            Console.WriteLine("   Email: doctor2@hopital.com");
            Console.WriteLine("   Mot de passe: Doctor2123!");
            Console.WriteLine("   Rôle: doctor (gestion patients, rendez-vous, prescriptions)");
            
            Console.WriteLine("\n🔑 RÉCEPTIONNISTE:");
            Console.WriteLine("   Email: receptionist@hopital.com");
            Console.WriteLine("   Mot de passe: Receptionist123!");
            Console.WriteLine("   Rôle: receptionist (gestion médecins, patients, rendez-vous)");
            Console.WriteLine("\n================================\n");
        }
    }
}


