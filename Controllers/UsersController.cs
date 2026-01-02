using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using gestion_hopital.Data;
using gestion_hopital.Models;

namespace gestion_hopital.Controllers
{
    [Authorize(Roles = "admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .Include(u => u.Doctor)
                .ToListAsync();

            var userViewModels = new List<UserViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    UserName = user.UserName ?? "",
                    Role = user.Role,
                    Roles = roles.ToList(),
                    DoctorName = user.Doctor?.NomComplet,
                    DoctorId = user.DoctorId
                });
            }

            return View(userViewModels);
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.Users
                .Include(u => u.Doctor)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var viewModel = new UserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "",
                UserName = user.UserName ?? "",
                Role = user.Role,
                Roles = roles.ToList(),
                DoctorName = user.Doctor?.NomComplet,
                DoctorId = user.DoctorId
            };

            return View(viewModel);
        }

        // GET: Users/Create
        public async Task<IActionResult> Create()
        {
            ViewData["Doctors"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Doctors.ToListAsync(), "Id", "NomComplet");
            ViewData["Roles"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _roleManager.Roles.ToListAsync(), "Name", "Name");
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Email,Password,Role,DoctorId")] CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = true,
                    Role = model.Role,
                    DoctorId = model.DoctorId
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Assign role
                    if (!string.IsNullOrEmpty(model.Role))
                    {
                        await _userManager.AddToRoleAsync(user, model.Role);
                    }

                    // Update doctor if assigned
                    if (model.DoctorId.HasValue)
                    {
                        var doctor = await _context.Doctors.FindAsync(model.DoctorId.Value);
                        if (doctor != null)
                        {
                            doctor.UtilisateurId = user.Id;
                            await _context.SaveChangesAsync();
                        }
                    }

                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ViewData["Doctors"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Doctors.ToListAsync(), "Id", "NomComplet", model.DoctorId);
            ViewData["Roles"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _roleManager.Roles.ToListAsync(), "Name", "Name", model.Role);
            return View(model);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "",
                Role = user.Role,
                DoctorId = user.DoctorId
            };

            ViewData["Doctors"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Doctors.ToListAsync(), "Id", "NomComplet", user.DoctorId);
            ViewData["Roles"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _roleManager.Roles.ToListAsync(), "Name", "Name", user.Role);
            return View(model);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Email,Role,DoctorId")] EditUserViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return NotFound();
                }

                user.Email = model.Email;
                user.UserName = model.Email;
                user.Role = model.Role;

                // Update roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!string.IsNullOrEmpty(model.Role))
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                }

                // Update doctor association
                var oldDoctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UtilisateurId == user.Id);
                if (oldDoctor != null && oldDoctor.Id != model.DoctorId)
                {
                    oldDoctor.UtilisateurId = null;
                }

                if (model.DoctorId.HasValue)
                {
                    var doctor = await _context.Doctors.FindAsync(model.DoctorId.Value);
                    if (doctor != null)
                    {
                        doctor.UtilisateurId = user.Id;
                        user.DoctorId = model.DoctorId;
                    }
                }
                else
                {
                    user.DoctorId = null;
                }

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ViewData["Doctors"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Doctors.ToListAsync(), "Id", "NomComplet", model.DoctorId);
            ViewData["Roles"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _roleManager.Roles.ToListAsync(), "Name", "Name", model.Role);
            return View(model);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.Users
                .Include(u => u.Doctor)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var viewModel = new UserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "",
                UserName = user.UserName ?? "",
                Role = user.Role,
                Roles = roles.ToList(),
                DoctorName = user.Doctor?.NomComplet,
                DoctorId = user.DoctorId
            };

            return View(viewModel);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                // Remove doctor association
                if (user.DoctorId.HasValue)
                {
                    var doctor = await _context.Doctors.FindAsync(user.DoctorId.Value);
                    if (doctor != null)
                    {
                        doctor.UtilisateurId = null;
                        await _context.SaveChangesAsync();
                    }
                }

                await _userManager.DeleteAsync(user);
            }

            return RedirectToAction(nameof(Index));
        }
    }

    // View Models
    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public string? DoctorName { get; set; }
        public int? DoctorId { get; set; }
    }

    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "L'email n'est pas valide")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est requis")]
        [StringLength(100, ErrorMessage = "Le {0} doit contenir au moins {2} caractères.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le rôle est requis")]
        [Display(Name = "Rôle")]
        public string Role { get; set; } = string.Empty;

        [Display(Name = "Médecin associé")]
        public int? DoctorId { get; set; }
    }

    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "L'email n'est pas valide")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le rôle est requis")]
        [Display(Name = "Rôle")]
        public string Role { get; set; } = string.Empty;

        [Display(Name = "Médecin associé")]
        public int? DoctorId { get; set; }
    }
}

