using CampusVarbergDashBoard.FilterData;
using CampusVarbergDashBoard.Models;
using CampusVarbergDashBoard.Repository;
using CampusVarbergDashBoard.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

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
            var filteredApplicants = await GetFilteredApplicantsAsync(utbildning, kön, år, termin);

            var viewModel = new FilterViewModel
            {
                YearDistributions = GetYearDistributions(filteredApplicants),
                GenderDistribution = GetGenderDistribution(filteredApplicants),
                CompetenceDistribution = GetCompetenceDistribution(filteredApplicants)
            };

            return View(viewModel);
        }

        private async Task<IEnumerable<Applicant>> GetFilteredApplicantsAsync(string utbildning, string kön, string år, string termin)
        {
            var applicants = await _IYearRepository.GetApplicantsAsync();
            applicants = ApplyEducationFilter(applicants, utbildning);
            applicants = ApplyGenderFilter(applicants, kön);
            applicants = ApplyYearFilter(applicants, år, 2016, DateTime.Now.Year);
            return await ApplyTermFilter(applicants, termin);
        }

        private IEnumerable<YearDistribution> GetYearDistributions(IEnumerable<Applicant> applicants)
        {
            return applicants.Select(a => new YearDistribution
            {
                Year = a.Inlämnad,
                Utbildning = a.Utbildning,
                Kön = a.Kön,
                Termin = a.Inlämnad
            });
        }

        private GenderDistribution GetGenderDistribution(IEnumerable<Applicant> applicants)
        {
            return new GenderDistribution
            {
                MaleCount = applicants.Count(a => a.Kön == "Man"),
                FemaleCount = applicants.Count(a => a.Kön == "Kvinna")
            };
        }

        private CompetenceDistribution GetCompetenceDistribution(IEnumerable<Applicant> applicants)
        {
            return new CompetenceDistribution
            {
                CompetenceCount = applicants.Count(a => a.Behörig == "Ja"),
                NonCompetenceCount = applicants.Count(a => a.Behörig == "Nej")
            };
        }


        private IEnumerable<Applicant> ApplyYearFilter(IEnumerable<Applicant> applicants, string år, int startYear, int endYear)
        {
            if (string.IsNullOrEmpty(år) || år == "Alla år")
            {
                return applicants.Where(a => a.Inlämnad.Year >= startYear && a.Inlämnad.Year <= endYear);
            }
            else
            {
                int year = int.Parse(år);
                return applicants.Where(a => a.Inlämnad.Year == year);
            }
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
                var termDates = new List<string>();
                for (int year = 2016; year <= DateTime.Now.Year; year++)
                {
                    var specificTermDates = await _IYearRepository.GetSpecificTermAsync(termin, year);
                    termDates.AddRange(specificTermDates);
                }

                var filteredApplicants = applicants.Where(a => termDates.Contains(a.Inlämnad.ToString("yyyy-MM-dd"))).ToList();
                return filteredApplicants;
            }
            return applicants;
        }

    }

}

