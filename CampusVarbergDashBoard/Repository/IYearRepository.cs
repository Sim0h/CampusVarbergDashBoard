using CampusVarbergDashBoard.FilterData;
using CampusVarbergDashBoard.Models;

namespace CampusVarbergDashBoard.Repository
{
	public interface IYearRepository
	{
		Task<IEnumerable<YearDistribution>> GetAllYearsAsync(int year);
		Task<IEnumerable<string>> GetSpecificTermAsync(string term, int year);
		Task<IEnumerable<Applicant>> GetApplicantsByEducationAsync(string education);
		Task<IEnumerable<Applicant>> GetApplicantsAsync();

    }
}
