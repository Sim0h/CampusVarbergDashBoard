using CampusVarbergDashBoard.FilterData;
using CampusVarbergDashBoard.Models;

namespace CampusVarbergDashBoard.Repository
{
	public interface IYearRepository
	{
		Task<IEnumerable<string>> GetSpecificTermAsync(string term, int year);
	
		Task<IEnumerable<Applicant>> GetApplicantsAsync();

    }
}
