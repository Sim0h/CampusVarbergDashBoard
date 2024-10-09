using CampusVarbergDashBoard.FilterData;
using CampusVarbergDashBoard.Models;

namespace CampusVarbergDashBoard.Repository
{
	public interface IYearRepository
	{
		Task<IEnumerable<YearDistribution>> GetAllYearsAsync(int year);
		Task<IEnumerable<string>> GetSpecificYearAsync(string year);
		Task<IEnumerable<string>> GetSpecificTermAsync(string term);
		Task<IEnumerable<Applicant>> GetApplicantsByEducationAsync(string education);
		Task<IEnumerable<Applicant>> GetApplicantsAsync();

    }
}
