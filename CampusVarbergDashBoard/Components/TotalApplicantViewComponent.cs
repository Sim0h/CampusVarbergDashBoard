using CampusVarbergDashBoard.Repository;
using Microsoft.AspNetCore.Mvc;

namespace CampusVarbergDashBoard.Components
{
    public class TotalApplicantsViewComponent : ViewComponent
    {
        private readonly IApplicantRepository _applicantRepo;

        public TotalApplicantsViewComponent(IApplicantRepository applicantRepo)
        {
            _applicantRepo = applicantRepo;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var totalApplicants = await _applicantRepo.GetTotalApplicantsAsync();
            return View(totalApplicants);
        }

    }
}
