using CampusVarbergDashBoard.Repository;
using Microsoft.AspNetCore.Mvc;

namespace CampusVarbergDashBoard.Controllers
{
    public class StatusController : Controller
    {
        private readonly IApplicantRepository _applicantRepository;
        public StatusController(IApplicantRepository applicantRepository)
        {
            _applicantRepository = applicantRepository;
        }


        public async Task<IActionResult> Index(int? year)
        {
            var statusDistribution = await _applicantRepository.GetStatusDistributionAsync(year);
            if (!statusDistribution.Any())
            {
                // Log or debug here
                Console.WriteLine("No data returned from GetStatusDistributionAsync");
            }
            return View(statusDistribution);
        }




    }
}
