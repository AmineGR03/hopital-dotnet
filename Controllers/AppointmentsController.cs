using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using gestion_hopital.Data;
using gestion_hopital.Models;

namespace gestion_hopital.Controllers
{
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<int?> GetCurrentDoctorIdAsync()
        {
            if (User.IsInRole("doctor"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null && user.DoctorId.HasValue)
                {
                    return user.DoctorId.Value;
                }
            }
            return null;
        }

        // GET: Appointments
        public async Task<IActionResult> Index()
        {
            var currentDoctorId = await GetCurrentDoctorIdAsync();
            
            var query = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .AsQueryable();

            // Si c'est un médecin, ne montrer que ses rendez-vous
            if (currentDoctorId.HasValue)
            {
                query = query.Where(a => a.DoctorId == currentDoctorId.Value);
            }

            var appointments = await query
                .OrderByDescending(a => a.DateRdv)
                .ToListAsync();
            
            return View(appointments);
        }

        // GET: Appointments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // GET: Appointments/Create
        [Authorize(Roles = "admin,receptionist,doctor")]
        public async Task<IActionResult> Create()
        {
            var currentDoctorId = await GetCurrentDoctorIdAsync();
            
            if (User.IsInRole("admin") || User.IsInRole("receptionist"))
            {
                ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "NomComplet");
            }
            else if (currentDoctorId.HasValue)
            {
                // Pour les médecins, pré-remplir avec leur ID
                ViewData["DoctorId"] = new SelectList(_context.Doctors.Where(d => d.Id == currentDoctorId.Value), "Id", "NomComplet", currentDoctorId.Value);
                ViewData["CurrentDoctorId"] = currentDoctorId.Value;
            }
            
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "NomComplet");
            ViewData["IsDoctor"] = User.IsInRole("doctor");
            return View();
        }

        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin,receptionist,doctor")]
        public async Task<IActionResult> Create([Bind("PatientId,DateRdv,Motif,Status")] Appointment appointment)
        {
            var currentDoctorId = await GetCurrentDoctorIdAsync();
            
            // Si c'est un médecin, forcer automatiquement l'utilisation de son ID
            if (currentDoctorId.HasValue)
            {
                appointment.DoctorId = currentDoctorId.Value;
            }
            else if (User.IsInRole("admin") || User.IsInRole("receptionist"))
            {
                // Pour l'admin/réceptionniste, le DoctorId doit être fourni dans le formulaire
                var doctorIdFromForm = Request.Form["DoctorId"].FirstOrDefault();
                if (!string.IsNullOrEmpty(doctorIdFromForm) && int.TryParse(doctorIdFromForm, out int doctorId))
                {
                    appointment.DoctorId = doctorId;
                }
            }

            // Check for appointment conflicts (same doctor at the same time)
            var appointmentDurationMinutes = 30; // Standard appointment duration in minutes
            var startTime = appointment.DateRdv;
            var endTime = startTime.AddMinutes(appointmentDurationMinutes);
            var timeWindowStart = startTime.AddMinutes(-appointmentDurationMinutes);
            var timeWindowEnd = endTime;

            // Load potential conflicting appointments for the doctor
            var potentialConflicts = await _context.Appointments
                .Where(a => a.DoctorId == appointment.DoctorId 
                    && a.Status != "Annulé"
                    && a.DateRdv >= timeWindowStart 
                    && a.DateRdv < timeWindowEnd)
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .ToListAsync();

            // Check for actual overlap in memory (since we can't use Add() in SQL)
            var conflictingAppointment = potentialConflicts
                .Where(a => 
                {
                    var existingEnd = a.DateRdv.AddMinutes(appointmentDurationMinutes);
                    // Check if appointments overlap: newStart < existingEnd AND newEnd > existingStart
                    return startTime < existingEnd && endTime > a.DateRdv;
                })
                .FirstOrDefault();

            if (conflictingAppointment != null)
            {
                ModelState.AddModelError("DateRdv", 
                    $"Un conflit de rendez-vous existe. Le médecin {conflictingAppointment.Doctor?.NomComplet} a déjà un rendez-vous prévu le {conflictingAppointment.DateRdv:dd/MM/yyyy à HH:mm} avec {conflictingAppointment.Patient?.NomComplet}.");
            }

            // Also check if the same patient has an appointment at the same time
            var potentialPatientConflicts = await _context.Appointments
                .Where(a => a.PatientId == appointment.PatientId 
                    && a.Status != "Annulé"
                    && a.DateRdv >= timeWindowStart 
                    && a.DateRdv < timeWindowEnd)
                .Include(a => a.Doctor)
                .ToListAsync();

            var conflictingPatientAppointment = potentialPatientConflicts
                .Where(a => 
                {
                    var existingEnd = a.DateRdv.AddMinutes(appointmentDurationMinutes);
                    return startTime < existingEnd && endTime > a.DateRdv;
                })
                .FirstOrDefault();

            if (conflictingPatientAppointment != null)
            {
                ModelState.AddModelError("DateRdv", 
                    $"Le patient a déjà un rendez-vous prévu le {conflictingPatientAppointment.DateRdv:dd/MM/yyyy à HH:mm} avec {conflictingPatientAppointment.Doctor?.NomComplet}.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "NomComplet", appointment.DoctorId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "NomComplet", appointment.PatientId);
            return View(appointment);
        }

        // GET: Appointments/Edit/5
        [Authorize(Roles = "admin,receptionist,doctor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            var currentDoctorId = await GetCurrentDoctorIdAsync();
            
            // Si c'est un médecin, vérifier qu'il peut modifier ce rendez-vous
            if (currentDoctorId.HasValue && appointment.DoctorId != currentDoctorId.Value)
            {
                return Forbid();
            }

            if (User.IsInRole("admin") || User.IsInRole("receptionist"))
            {
                ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "NomComplet", appointment.DoctorId);
            }
            else if (currentDoctorId.HasValue)
            {
                ViewData["DoctorId"] = new SelectList(_context.Doctors.Where(d => d.Id == currentDoctorId.Value), "Id", "NomComplet", currentDoctorId.Value);
                ViewData["CurrentDoctorId"] = currentDoctorId.Value;
            }
            
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "NomComplet", appointment.PatientId);
            ViewData["IsDoctor"] = User.IsInRole("doctor");
            return View(appointment);
        }

        // POST: Appointments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin,receptionist,doctor")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PatientId,DateRdv,Motif,Status")] Appointment appointment)
        {
            if (id != appointment.Id)
            {
                return NotFound();
            }

            var currentDoctorId = await GetCurrentDoctorIdAsync();
            
            // Vérifier que le médecin peut modifier ce rendez-vous et forcer son ID
            if (currentDoctorId.HasValue)
            {
                var existingAppointment = await _context.Appointments.FindAsync(id);
                if (existingAppointment != null && existingAppointment.DoctorId != currentDoctorId.Value)
                {
                    return Forbid();
                }
                // Forcer automatiquement l'utilisation de son ID
                appointment.DoctorId = currentDoctorId.Value;
            }
            else if (User.IsInRole("admin") || User.IsInRole("receptionist"))
            {
                // Pour l'admin/réceptionniste, le DoctorId doit être fourni dans le formulaire
                var doctorIdFromForm = Request.Form["DoctorId"].FirstOrDefault();
                if (!string.IsNullOrEmpty(doctorIdFromForm) && int.TryParse(doctorIdFromForm, out int doctorId))
                {
                    appointment.DoctorId = doctorId;
                }
            }

            // Check for appointment conflicts (same doctor at the same time)
            var appointmentDurationMinutes = 30; // Standard appointment duration in minutes
            var startTime = appointment.DateRdv;
            var endTime = startTime.AddMinutes(appointmentDurationMinutes);
            var timeWindowStart = startTime.AddMinutes(-appointmentDurationMinutes);
            var timeWindowEnd = endTime;

            // Load potential conflicting appointments for the doctor (excluding current appointment)
            var potentialConflicts = await _context.Appointments
                .Where(a => a.DoctorId == appointment.DoctorId 
                    && a.Id != appointment.Id // Exclude current appointment when editing
                    && a.Status != "Annulé"
                    && a.DateRdv >= timeWindowStart 
                    && a.DateRdv < timeWindowEnd)
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .ToListAsync();

            // Check for actual overlap in memory (since we can't use Add() in SQL)
            var conflictingAppointment = potentialConflicts
                .Where(a => 
                {
                    var existingEnd = a.DateRdv.AddMinutes(appointmentDurationMinutes);
                    // Check if appointments overlap: newStart < existingEnd AND newEnd > existingStart
                    return startTime < existingEnd && endTime > a.DateRdv;
                })
                .FirstOrDefault();

            if (conflictingAppointment != null)
            {
                ModelState.AddModelError("DateRdv", 
                    $"Un conflit de rendez-vous existe. Le médecin {conflictingAppointment.Doctor?.NomComplet} a déjà un rendez-vous prévu le {conflictingAppointment.DateRdv:dd/MM/yyyy à HH:mm} avec {conflictingAppointment.Patient?.NomComplet}.");
            }

            // Also check if the same patient has an appointment at the same time
            var potentialPatientConflicts = await _context.Appointments
                .Where(a => a.PatientId == appointment.PatientId 
                    && a.Id != appointment.Id
                    && a.Status != "Annulé"
                    && a.DateRdv >= timeWindowStart 
                    && a.DateRdv < timeWindowEnd)
                .Include(a => a.Doctor)
                .ToListAsync();

            var conflictingPatientAppointment = potentialPatientConflicts
                .Where(a => 
                {
                    var existingEnd = a.DateRdv.AddMinutes(appointmentDurationMinutes);
                    return startTime < existingEnd && endTime > a.DateRdv;
                })
                .FirstOrDefault();

            if (conflictingPatientAppointment != null)
            {
                ModelState.AddModelError("DateRdv", 
                    $"Le patient a déjà un rendez-vous prévu le {conflictingPatientAppointment.DateRdv:dd/MM/yyyy à HH:mm} avec {conflictingPatientAppointment.Doctor?.NomComplet}.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(appointment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppointmentExists(appointment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "NomComplet", appointment.DoctorId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "NomComplet", appointment.PatientId);
            return View(appointment);
        }

        // GET: Appointments/Delete/5
        [Authorize(Roles = "admin,receptionist,doctor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (appointment == null)
            {
                return NotFound();
            }

            var currentDoctorId = await GetCurrentDoctorIdAsync();
            // Si c'est un médecin, vérifier qu'il peut supprimer ce rendez-vous
            if (currentDoctorId.HasValue && appointment.DoctorId != currentDoctorId.Value)
            {
                return Forbid();
            }

            return View(appointment);
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin,receptionist,doctor")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                var currentDoctorId = await GetCurrentDoctorIdAsync();
                // Si c'est un médecin, vérifier qu'il peut supprimer ce rendez-vous
                if (currentDoctorId.HasValue && appointment.DoctorId != currentDoctorId.Value)
                {
                    return Forbid();
                }

                _context.Appointments.Remove(appointment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }
    }
}

