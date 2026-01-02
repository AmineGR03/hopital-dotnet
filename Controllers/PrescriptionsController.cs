using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using gestion_hopital.Data;
using gestion_hopital.Models;

namespace gestion_hopital.Controllers
{
    [Authorize(Roles = "admin,doctor")]
    public class PrescriptionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PrescriptionsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
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

        // GET: Prescriptions
        public async Task<IActionResult> Index()
        {
            var currentDoctorId = await GetCurrentDoctorIdAsync();
            
            var query = _context.Prescriptions
                .Include(p => p.Patient)
                .Include(p => p.Doctor)
                .AsQueryable();

            // Si c'est un médecin, ne montrer que ses prescriptions
            if (currentDoctorId.HasValue)
            {
                query = query.Where(p => p.DoctorId == currentDoctorId.Value);
            }

            var prescriptions = await query
                .OrderByDescending(p => p.Date)
                .ToListAsync();
            
            return View(prescriptions);
        }

        // GET: Prescriptions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prescription = await _context.Prescriptions
                .Include(p => p.Patient)
                .Include(p => p.Doctor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (prescription == null)
            {
                return NotFound();
            }

            return View(prescription);
        }

        // GET: Prescriptions/Create
        public async Task<IActionResult> Create()
        {
            var currentDoctorId = await GetCurrentDoctorIdAsync();
            
            if (User.IsInRole("admin"))
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

        // POST: Prescriptions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PatientId,Medicaments,Date,ValidityDays,Instructions")] Prescription prescription)
        {
            var currentDoctorId = await GetCurrentDoctorIdAsync();
            
            // Si c'est un médecin, forcer automatiquement l'utilisation de son ID
            if (currentDoctorId.HasValue)
            {
                prescription.DoctorId = currentDoctorId.Value;
            }
            else if (User.IsInRole("admin"))
            {
                // Pour l'admin, le DoctorId doit être fourni dans le formulaire
                // On le récupère depuis le formulaire si nécessaire
                var doctorIdFromForm = Request.Form["DoctorId"].FirstOrDefault();
                if (!string.IsNullOrEmpty(doctorIdFromForm) && int.TryParse(doctorIdFromForm, out int doctorId))
                {
                    prescription.DoctorId = doctorId;
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(prescription);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            if (User.IsInRole("admin"))
            {
                ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "NomComplet", prescription.DoctorId);
            }
            else if (currentDoctorId.HasValue)
            {
                ViewData["DoctorId"] = new SelectList(_context.Doctors.Where(d => d.Id == currentDoctorId.Value), "Id", "NomComplet", currentDoctorId.Value);
                ViewData["CurrentDoctorId"] = currentDoctorId.Value;
            }
            
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "NomComplet", prescription.PatientId);
            ViewData["IsDoctor"] = User.IsInRole("doctor");
            return View(prescription);
        }

        // GET: Prescriptions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription == null)
            {
                return NotFound();
            }

            var currentDoctorId = await GetCurrentDoctorIdAsync();
            
            // Si c'est un médecin, vérifier qu'il peut modifier cette prescription
            if (currentDoctorId.HasValue && prescription.DoctorId != currentDoctorId.Value)
            {
                return Forbid();
            }

            if (User.IsInRole("admin"))
            {
                ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "NomComplet", prescription.DoctorId);
            }
            else if (currentDoctorId.HasValue)
            {
                ViewData["DoctorId"] = new SelectList(_context.Doctors.Where(d => d.Id == currentDoctorId.Value), "Id", "NomComplet", currentDoctorId.Value);
                ViewData["CurrentDoctorId"] = currentDoctorId.Value;
            }
            
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "NomComplet", prescription.PatientId);
            ViewData["IsDoctor"] = User.IsInRole("doctor");
            return View(prescription);
        }

        // POST: Prescriptions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PatientId,Medicaments,Date,ValidityDays,Instructions")] Prescription prescription)
        {
            if (id != prescription.Id)
            {
                return NotFound();
            }

            var currentDoctorId = await GetCurrentDoctorIdAsync();
            
            // Vérifier que le médecin peut modifier cette prescription et forcer son ID
            if (currentDoctorId.HasValue)
            {
                var existingPrescription = await _context.Prescriptions.FindAsync(id);
                if (existingPrescription != null && existingPrescription.DoctorId != currentDoctorId.Value)
                {
                    return Forbid();
                }
                // Forcer automatiquement l'utilisation de son ID
                prescription.DoctorId = currentDoctorId.Value;
            }
            else if (User.IsInRole("admin"))
            {
                // Pour l'admin, le DoctorId doit être fourni dans le formulaire
                var doctorIdFromForm = Request.Form["DoctorId"].FirstOrDefault();
                if (!string.IsNullOrEmpty(doctorIdFromForm) && int.TryParse(doctorIdFromForm, out int doctorId))
                {
                    prescription.DoctorId = doctorId;
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(prescription);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PrescriptionExists(prescription.Id))
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
            
            if (User.IsInRole("admin"))
            {
                ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "NomComplet", prescription.DoctorId);
            }
            else if (currentDoctorId.HasValue)
            {
                ViewData["DoctorId"] = new SelectList(_context.Doctors.Where(d => d.Id == currentDoctorId.Value), "Id", "NomComplet", currentDoctorId.Value);
                ViewData["CurrentDoctorId"] = currentDoctorId.Value;
            }
            
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "NomComplet", prescription.PatientId);
            ViewData["IsDoctor"] = User.IsInRole("doctor");
            return View(prescription);
        }

        // GET: Prescriptions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prescription = await _context.Prescriptions
                .Include(p => p.Patient)
                .Include(p => p.Doctor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (prescription == null)
            {
                return NotFound();
            }

            var currentDoctorId = await GetCurrentDoctorIdAsync();
            // Si c'est un médecin, vérifier qu'il peut supprimer cette prescription
            if (currentDoctorId.HasValue && prescription.DoctorId != currentDoctorId.Value)
            {
                return Forbid();
            }

            return View(prescription);
        }

        // POST: Prescriptions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription != null)
            {
                var currentDoctorId = await GetCurrentDoctorIdAsync();
                // Si c'est un médecin, vérifier qu'il peut supprimer cette prescription
                if (currentDoctorId.HasValue && prescription.DoctorId != currentDoctorId.Value)
                {
                    return Forbid();
                }

                _context.Prescriptions.Remove(prescription);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PrescriptionExists(int id)
        {
            return _context.Prescriptions.Any(e => e.Id == id);
        }
    }
}

