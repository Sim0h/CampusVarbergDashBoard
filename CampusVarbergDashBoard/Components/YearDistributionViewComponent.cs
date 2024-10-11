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
				CompetenceDistribution = GetCompetenceDistribution(filteredApplicants),
				AgeDistribution = await GetAgeFilterDistributionAsync(filteredApplicants)
			};

			return View(viewModel);
		}

		private async Task<IEnumerable<Applicant>> GetFilteredApplicantsAsync(string utbildning, string kön, string år, string termin)
		{
			var applicants = await _IYearRepository.GetApplicantsAsync();
			applicants = ApplyEducationFilter(applicants, utbildning);
			applicants = ApplyGenderFilter(applicants, kön);
			applicants = ApplyYearFilter(applicants, år, 2016, DateTime.Now.Year);
			applicants = CalculateApplicantAges(applicants);
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
        private async Task<IEnumerable<AgeDistribution>> GetAgeFilterDistributionAsync(IEnumerable<Applicant> applicants)
        {
            var ageGroups = new List<string>
			{
			"Under 20", "20-25", "26-30", "31-35", "36-40", "41-45", "46-50", "51-55", "56-60", "Över 60"
			};

            var ageDistribution = ageGroups.Select(ageGroup => new AgeDistribution
            {
                AgeRange = ageGroup,
                Count = applicants.Count(a =>
                {
                    var ageGroupValue = GetAgeGroup(a.Ålder);
                    return ageGroupValue == ageGroup;
                })
            });

            return ageDistribution;
        }

        private string GetAgeGroup(int age)
		{
			if (age < 20) return "Under 20";
			if (age <= 25) return "20-25";
			if (age <= 30) return "26-30";
			if (age <= 35) return "31-35";
			if (age <= 40) return "36-40";
			if (age <= 45) return "41-45";
			if (age <= 50) return "46-50";
			if (age <= 55) return "51-55";
			if (age <= 60) return "56-60";
			return "Över 60";
		}
        private IEnumerable<Applicant> CalculateApplicantAges(IEnumerable<Applicant> applicants)
        {
            foreach (var applicant in applicants)
            {
                if (applicant.Födelsedatum != null)
                {
                    applicant.Ålder = DateTime.Now.Year - applicant.Födelsedatum.Date.Year;
                    if (applicant.Födelsedatum > DateTime.Now.AddYears(-applicant.Ålder))
                    {
                        applicant.Ålder--;
                    }
                }
            }
            return applicants;
        }


    }

}

