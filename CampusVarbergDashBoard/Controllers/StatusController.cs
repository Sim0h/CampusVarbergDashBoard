using CampusVarbergDashBoard.Repository;
using CampusVarbergDashBoard.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CampusVarbergDashBoard.Controllers
{
    public class StatusController : Controller
    {
        private readonly IApplicantRepository _applicantRepository;
        private readonly IYearRepository _yearRepository;
        public StatusController(IApplicantRepository applicantRepository, IYearRepository yearRepository)
        {
            _applicantRepository = applicantRepository;
            _yearRepository = yearRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var statusDistributions = await _applicantRepository.GetStatusDistributionAsync();

            List<StatusChartData> chartData = new List<StatusChartData>
            {
                new StatusChartData { Status = "Inlämnad Ansökan", Count = statusDistributions.Sum(s => s.InlämnadCount) },
                new StatusChartData { Status = "Behöriga", Count = statusDistributions.Sum(s => s.BehörigCount) },
                new StatusChartData { Status = "Erbjudna plats", Count = statusDistributions.Sum(s => s.ErbjudenPlatsTackatJaCount) }
               
            };

            ViewBag.dataSource = chartData;
            return View();
        }


    }

    public class StatusChartData
    {
        public string Status { get; set; }
        public int Count { get; set; }
    }
}
