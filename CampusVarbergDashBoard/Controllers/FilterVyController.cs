using CampusVarbergDashBoard.FilterData;
using CampusVarbergDashBoard.Models;
using CampusVarbergDashBoard.Repository;
using CampusVarbergDashBoard.ViewModels;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace CampusVarbergDashBoard.Controllers
{

	public class FilterVyController : Controller
	{

		private readonly ILogger<HomeController> _logger;
		private readonly IApplicantRepository _applicantRepo;
		private readonly IConfiguration _configuration;
		private readonly IYearRepository _yearRepository;

		public FilterVyController(ILogger<HomeController> logger, IApplicantRepository applicantRepo, IConfiguration configuration, IYearRepository yearRepository)
		{
			_logger = logger;
			_applicantRepo = applicantRepo;
			_configuration = configuration;
			_yearRepository = yearRepository;
		}

		[HttpGet]
		public async Task<IActionResult> Index(string utbildning, string kön, string år, string termin)
		{
			var filteredApplicants = await GetFilteredApplicantsAsync(utbildning, kön, år, termin);

			var viewModel = new FilterViewModel
			{
				YearDistributions = GetYearDistributions(filteredApplicants),
				GenderDistribution = GetGenderDistribution(filteredApplicants),
				CompetenceDistribution = GetCompetenceDistribution(filteredApplicants),
				AgeDistribution = await GetAgeFilterDistributionAsync(filteredApplicants),
				OfferedSpotDistribution = GetOfferedSpotDistribution(filteredApplicants),
				LateApplicationDistribution = GetLateApplicationDistribution(filteredApplicants),
				Applicants = filteredApplicants.ToList(),
				AcceptedOfferDistribution = GetAcceptedOfferDistribution(filteredApplicants),


				SelectedUtbildning = utbildning,
				SelectedKön = kön,
				SelectedÅr = år,
				SelectedTermin = termin


			};

			return View(viewModel);
		}

		private async Task<IEnumerable<Applicant>> GetFilteredApplicantsAsync(string utbildning, string kön, string år, string termin)
		{
			var applicants = await _yearRepository.GetApplicantsAsync();

			applicants = OfferedSpotFilter(applicants);

			applicants = AcceptedApplication(applicants);

			if (!string.IsNullOrEmpty(utbildning) && utbildning != "Alla YH Utbildningar")
			{
				applicants = ApplyEducationFilter(applicants, utbildning);


			}

			if (!string.IsNullOrEmpty(kön) && kön != "Alla Kön")
			{
				applicants = ApplyGenderFilter(applicants, kön);

			}

			if (!string.IsNullOrEmpty(år) && år != "Alla år")
			{
				applicants = ApplyYearFilter(applicants, år, int.Parse(år), int.Parse(år));

			}

			if (!string.IsNullOrEmpty(termin) && termin != "Alla terminer")
			{
				applicants = await ApplyTermFilter(applicants, termin);

			}

			return applicants;
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

		private LateApplicationDistribution GetLateApplicationDistribution(IEnumerable<Applicant> applicants)
		{
			return new LateApplicationDistribution
			{
				LateApplicationCount = applicants.Count(a => a.Sen_anmälan == "Ja"),
				NotLateApplicationCount = applicants.Count(a => a.Sen_anmälan == "Nej")
			};
		}

		private OfferedSpotDistribution GetOfferedSpotDistribution(IEnumerable<Applicant> applicants)
		{
			return new OfferedSpotDistribution
			{
				OfferedSpotCount = applicants.Count(a => a.ErbjudenPlats == "Erbjuden plats"),
				ReserveCount = applicants.Count(a => a.ErbjudenPlats == "Reserv"),
				NotOfferedSpotCount = applicants.Count(a => a.ErbjudenPlats == "Ej erbjuden plats")
			};
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

		private IEnumerable<Applicant> OfferedSpotFilter(IEnumerable<Applicant> applicants) //filter för erbjuden plats, klumpar tackat ja + inskriven
		{

			return applicants.Select(a =>
			{
				if (a.Status != null && a.Status.Contains("Erbjuden plats", StringComparison.OrdinalIgnoreCase))
				{
					a.ErbjudenPlats = "Erbjuden plats";
				}
				else if (a.Status != null && (
				a.Status.Contains("Tackat ja", StringComparison.OrdinalIgnoreCase) ||
				a.Status.Contains("inskriven", StringComparison.OrdinalIgnoreCase)))
				{
					a.ErbjudenPlats = "Erbjuden plats";
				}
				else if (a.Status != null && a.Status.Contains("Reserv"))
				{
					a.ErbjudenPlats = "Reserv";
				}
				else
				{
					a.ErbjudenPlats = "Ej erbjuden plats";
				}
				return a;
			});
		}

		private IEnumerable<Applicant> LateApplicantion(IEnumerable<Applicant> applicants)
		{
			return applicants.Select(a =>
			{
				if (a.Sen_anmälan != null && a.Sen_anmälan.Contains("Ja", StringComparison.OrdinalIgnoreCase))
				{
					a.Sen_anmälan = "Ja";
				}
				else if (a.Sen_anmälan != null && a.Sen_anmälan.Contains("Nej", StringComparison.OrdinalIgnoreCase))
				{
					a.Sen_anmälan = "Nej";
				}
				return a;
			});
		}

		private IEnumerable<Applicant> AcceptedApplication(IEnumerable<Applicant> applicants) // filterar status på om man tackat ja / inskriven eller ej.
		{
			return applicants.Select(a =>
			{
				if (a.Status != null &&
				a.Status.Contains(("Tackat ja"), StringComparison.OrdinalIgnoreCase))
				{
					a.Status = "Tackat Ja";
				}
				else if (a.Status != null &&
				a.Status.Contains(("inskriven"), StringComparison.OrdinalIgnoreCase))
				{
					a.Status = "Tackat Ja";
				}
				else if (a.Status != null && a.Status.Contains("Tackat Nej", StringComparison.OrdinalIgnoreCase))
				{
					a.Status = "Tackat Nej";
				}

				return a;
			});
		}

		private AcceptedOfferDistribution GetAcceptedOfferDistribution(IEnumerable<Applicant> applicants)
		{
			return new AcceptedOfferDistribution
			{
				AcceptedOfferCount = applicants.Count(a => a.Status == "Tackat Ja"),
				DeclinedOfferCount = applicants.Count(a => a.Status == "Tackat Nej")
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
				var educationNameMapping = GetEducationNameMapping();

				if (educationNameMapping.ContainsKey(utbildning))
				{
					var relatedNames = educationNameMapping[utbildning];

					return applicants.Where(a =>
						relatedNames.Any(oldName => a.Utbildning.Contains(oldName, StringComparison.OrdinalIgnoreCase)) ||
						a.Utbildning.Contains(utbildning, StringComparison.OrdinalIgnoreCase));
				}
				else
				{
					return applicants.Where(a => a.Utbildning.Contains(utbildning, StringComparison.OrdinalIgnoreCase));
				}
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
					var specificTermDates = await _yearRepository.GetSpecificTermAsync(termin, year);
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

			// Calculate ages for all applicants
			applicants = CalculateApplicantAges(applicants);

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


		private SqlConnection GetConnection()
		{
			return new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
		}

		private Dictionary<string, List<string>> GetEducationNameMapping() // Mapping för att kunna filtrera på olika namn för samma utbildning
		{
			return new Dictionary<string, List<string>>
			{
				{ "Medicinsk vårdadministratör", new List<string> { "Medicinsk Sekreterare", "Medicinsk Sekreterare / koordinator" } },
				{ "Digital analytiker", new List<string>() },
				{ "Elkonstruktör", new List<string>(){"Elingenjör/Elkonstruktör" } },
				{ "Vatten- och biogastekniker", new List<string>(){ "Drifttekniker - biogas och vattenrening", "Drifttekniker biogas och vattenrening, Varberg" } },
				{ "VVS-ingenjör - energi och teknik", new List<string>(){ "VVS-ingenjör, Varberg" } },

			};
		}


	}

    public class FilterVyController : Controller
    {

        private readonly ILogger<HomeController> _logger;
        private readonly IApplicantRepository _applicantRepo;
        private readonly IConfiguration _configuration;
        private readonly IYearRepository _yearRepository;

        public FilterVyController(ILogger<HomeController> logger, IApplicantRepository applicantRepo, IConfiguration configuration, IYearRepository yearRepository)
        {
            _logger = logger;
            _applicantRepo = applicantRepo;
            _configuration = configuration;
            _yearRepository = yearRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string utbildning, string kön, string år, string termin)
        {
            var filteredApplicants = await GetFilteredApplicantsAsync(utbildning, kön, år, termin);

            var viewModel = new FilterViewModel
            {
                YearDistributions = GetYearDistributions(filteredApplicants),
                GenderDistribution = GetGenderDistribution(filteredApplicants),
                CompetenceDistribution = GetCompetenceDistribution(filteredApplicants),
                AgeDistribution = await GetAgeFilterDistributionAsync(filteredApplicants),

            };

            return View(viewModel);
        }

        private async Task<IEnumerable<Applicant>> GetFilteredApplicantsAsync(string utbildning, string kön, string år, string termin)
        {
            var applicants = await _yearRepository.GetApplicantsAsync();

            if (!string.IsNullOrEmpty(utbildning) && utbildning != "Alla YH Utbildningar")
            {
                applicants = ApplyEducationFilter(applicants, utbildning);
                Console.WriteLine($"After Education Filter: {applicants.Count()} applicants");
            }

            if (!string.IsNullOrEmpty(kön) && kön != "Alla Kön")
            {
                applicants = ApplyGenderFilter(applicants, kön);
                Console.WriteLine($"After Gender Filter: {applicants.Count()} applicants");
            }

            if (!string.IsNullOrEmpty(år) && år != "Alla år")
            {
                applicants = ApplyYearFilter(applicants, år, int.Parse(år), int.Parse(år));
                Console.WriteLine($"After Year Filter: {applicants.Count()} applicants");
            }

            if (!string.IsNullOrEmpty(termin) && termin != "Alla terminer")
            {
                applicants = await ApplyTermFilter(applicants, termin);
                Console.WriteLine($"After Term Filter: {applicants.Count()} applicants");
            }

            return applicants;
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
                    var specificTermDates = await _yearRepository.GetSpecificTermAsync(termin, year);
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

            // Calculate ages for all applicants
            applicants = CalculateApplicantAges(applicants);

            var ageDistribution = ageGroups.Select(ageGroup => new AgeDistribution
            {
                AgeRange = ageGroup,
                Count = applicants.Count(a =>
                {
                    var ageGroupValue = GetAgeGroup(a.Ålder);
                    return ageGroupValue == ageGroup;
                })
            });

            // Log the age distribution for debugging
            foreach (var ageDist in ageDistribution)
            {
                Console.WriteLine($"Age Group: {ageDist.AgeRange}, Count: {ageDist.Count}");
            }

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



        private SqlConnection GetConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        }

    }
}
