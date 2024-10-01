using CampusVarbergDashBoard.Models;
using CampusVarbergDashBoard.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CampusVarbergDashBoard.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly IApplicantRepository _applicantRepo;
        public HomeController(ILogger<HomeController> logger, IApplicantRepository applicantRepo)
		{
            _applicantRepo = applicantRepo;
            _logger = logger;
		}

		public async Task <IActionResult> Index()
		{
            var applicants = await _applicantRepo.GetAllApplicantsAsync();
            return View(applicants);
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
}
