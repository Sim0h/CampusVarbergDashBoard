using CampusVarbergDashBoard.FilterData;
using CampusVarbergDashBoard.Models;
using CampusVarbergDashBoard.Repository;
using Microsoft.AspNetCore.Mvc;

namespace CampusVarbergDashBoard.Components
{
    public class YearDistributionViewComponent : ViewComponent
    {
        private readonly IYearRepository _IYearRepository;
        private readonly IApplicantRepository _IApplicantRepository;

        public YearDistributionViewComponent(IYearRepository IYearRepository, IApplicantRepository applicantRepository)
        {
            _IYearRepository = IYearRepository;
            _IApplicantRepository = applicantRepository;
        }



        public async Task<IViewComponentResult> InvokeAsync(string utbildning, string kön, string år, string termin)
        {
            // Get all applicants initially
           IEnumerable<Applicant> filteredApplicants = await _IYearRepository.GetApplicantsAsync();
    Console.WriteLine($"Initial applicants count: {filteredApplicants.Count()}");

    // Apply filters
    filteredApplicants = ApplyGenderFilter(filteredApplicants, kön);
    Console.WriteLine($"After gender filter: {filteredApplicants.Count()}");

    filteredApplicants = ApplyEducationFilter(filteredApplicants, utbildning);
    Console.WriteLine($"After education filter: {filteredApplicants.Count()}");

    filteredApplicants = ApplyYearFilter(filteredApplicants, år);
    Console.WriteLine($"After year filter: {filteredApplicants.Count()}");

    filteredApplicants = await ApplyTermFilter(filteredApplicants, termin);
    Console.WriteLine($"After term filter: {filteredApplicants.Count()}");

            // Transform filtered applicants into YearDistribution objects
            var yearDistributions = filteredApplicants.Select(a => new YearDistribution
            {
                Year = a.Inlämnad,
                Utbildning = a.Utbildning,
                Kön = a.Kön
            });

            return View(yearDistributions);
        }

        private IEnumerable<Applicant> ApplyYearFilter(IEnumerable<Applicant> applicants, string år)
        {
            if (!string.IsNullOrEmpty(år))
            {
                int year = int.Parse(år);
                return applicants.Where(a => a.Inlämnad.Year == year);
            }
            return applicants;
        }
        private IEnumerable<Applicant> ApplyGenderFilter(IEnumerable<Applicant> applicants, string kön)
        {
            if (!string.IsNullOrEmpty(kön) && kön != "Alla Kön")
            {
                return applicants.Where(a => a.Kön == kön);
            }
            return applicants;
        }
        private IEnumerable<Applicant> ApplyEducationFilter(IEnumerable<Applicant> applicants, string utbildning)
        {
            if (!string.IsNullOrEmpty(utbildning) && utbildning != "Alla YH Utbildningar")
            {
                return applicants.Where(a => a.Utbildning.Contains(utbildning, StringComparison.OrdinalIgnoreCase));
            }
            return applicants;
        }

        private async Task<IEnumerable<Applicant>> ApplyTermFilter(IEnumerable<Applicant> applicants, string termin)
        {
            if (!string.IsNullOrEmpty(termin) && termin != "Alla terminer")
            {
                var termDates = await _IYearRepository.GetSpecificTermAsync(termin);
                Console.WriteLine($"Term dates for {termin}: {string.Join(", ", termDates)}");

                // Log each term date individually
                foreach (var termDate in termDates)
                {
                    Console.WriteLine($"Term date: {termDate}");
                }

                var filteredApplicants = new List<Applicant>();

                foreach (var applicant in applicants)
                {
                    var applicantDate = applicant.Inlämnad.ToString("yyyy-MM-dd");
                    if (termDates.Contains(applicantDate))
                    {
                        filteredApplicants.Add(applicant);
                    }
                    else
                    {
                        Console.WriteLine($"Applicant {applicant.ID} with date {applicantDate} does not match any term date.");
                    }
                }

                Console.WriteLine($"Filtered applicants count for term {termin}: {filteredApplicants.Count()}");

                return filteredApplicants;
            }
            return applicants;
        }

    }
}

