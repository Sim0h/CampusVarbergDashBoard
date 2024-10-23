using CampusVarbergDashBoard.FilterData;
using CampusVarbergDashBoard.Models;
using CampusVarbergDashBoard.Repository;
using CampusVarbergDashBoard.Services;
using CampusVarbergDashBoard.ViewModels;
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
            var totalApplicants = await _applicantRepo.GetTotalApplicantsAsync();
            var educationDistribution = await _applicantRepo.GetAllEducationsAsync();
            var competenceDistribution = await _applicantRepo.GetCompetenceDistributionAsync();
            var ageDistribution = await _applicantRepo.GetAgeDistributionAsync();
            var genderDistribution = await _applicantRepo.GetGenderDistributionAsync();
            var statusDistribution = await _applicantRepo.GetStatusDistributionAsync();

            List<StatusChartData> chartData = new List<StatusChartData>
            {
                new StatusChartData { Status = "Inlämnad Ansökan", Count = statusDistribution.Sum(s => s.InlämnadCount) },
                new StatusChartData { Status = "Behöriga", Count = statusDistribution.Sum(s => s.BehörigCount) },
                new StatusChartData { Status = "Erbjudna plats", Count = statusDistribution.Sum(s => s.ErbjudenPlatsTackatJaCount) }

            };

            ViewBag.dataSource = chartData;

            var viewModel = new DashboardViewModel
            {
                TotalApplicants = totalApplicants,
                EducationDistribution = educationDistribution,
                CompetenceDistribution = competenceDistribution,
                AgeDistribution = ageDistribution,
                GenderDistribution = genderDistribution,
                StatusDistribution = statusDistribution
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

		public IActionResult Kontakt()
		{
			return View();
		}

        public IActionResult Settings()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
    public class StatusChartData
    {
        public string Status { get; set; }
        public int Count { get; set; }
    }
}
