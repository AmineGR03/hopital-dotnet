using System.Diagnostics;
using gestion_hopital.Data;
using gestion_hopital.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace gestion_hopital.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return View();
            }

            var viewModel = new DashboardViewModel();

            if (User.IsInRole("admin"))
            {
                viewModel.TotalDoctors = await _context.Doctors.CountAsync();
                viewModel.TotalPatients = await _context.Patients.CountAsync();
                viewModel.TotalAppointments = await _context.Appointments.CountAsync();
                viewModel.TotalPrescriptions = await _context.Prescriptions.CountAsync();
                viewModel.TodayAppointments = await _context.Appointments
                    .Where(a => a.DateRdv.Date == DateTime.Today)
                    .CountAsync();
                viewModel.UpcomingAppointments = await _context.Appointments
                    .Where(a => a.DateRdv > DateTime.Now && a.Status != "Annulé")
                    .CountAsync();
            }
            else if (User.IsInRole("doctor"))
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UtilisateurId == user!.Id);
                
                if (doctor != null)
                {
                    viewModel.TotalPatients = await _context.Patients
                        .Where(p => p.Doctors.Any(d => d.Id == doctor.Id))
                        .CountAsync();
                    viewModel.TotalAppointments = await _context.Appointments
                        .Where(a => a.DoctorId == doctor.Id)
                        .CountAsync();
                    viewModel.TotalPrescriptions = await _context.Prescriptions
                        .Where(p => p.DoctorId == doctor.Id)
                        .CountAsync();
                    viewModel.TodayAppointments = await _context.Appointments
                        .Where(a => a.DoctorId == doctor.Id && a.DateRdv.Date == DateTime.Today)
                        .CountAsync();
                    viewModel.UpcomingAppointments = await _context.Appointments
                        .Where(a => a.DoctorId == doctor.Id && a.DateRdv > DateTime.Now && a.Status != "Annulé")
                        .CountAsync();
                }
            }
            else if (User.IsInRole("receptionist"))
            {
                viewModel.TotalDoctors = await _context.Doctors.CountAsync();
                viewModel.TotalPatients = await _context.Patients.CountAsync();
                viewModel.TotalAppointments = await _context.Appointments.CountAsync();
                viewModel.TodayAppointments = await _context.Appointments
                    .Where(a => a.DateRdv.Date == DateTime.Today)
                    .CountAsync();
                viewModel.UpcomingAppointments = await _context.Appointments
                    .Where(a => a.DateRdv > DateTime.Now && a.Status != "Annulé")
                    .CountAsync();
            }

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class DashboardViewModel
    {
        public int TotalDoctors { get; set; }
        public int TotalPatients { get; set; }
        public int TotalAppointments { get; set; }
        public int TotalPrescriptions { get; set; }
        public int TodayAppointments { get; set; }
        public int UpcomingAppointments { get; set; }
    }
}
