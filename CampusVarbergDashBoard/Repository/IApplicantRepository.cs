using CampusVarbergDashBoard.FilterData;
using CampusVarbergDashBoard.Models;
using Syncfusion.EJ2.Linq;

namespace CampusVarbergDashBoard.Repository
{
    public interface IApplicantRepository
    {
        Task<IEnumerable<Applicant>> GetAllApplicantsAsync();
        Task<CompetenceDistribution> GetCompetenceDistributionAsync();
        Task<IEnumerable<StatusDistribution>> GetStatusDistributionAsync();

        //Define function that shows how many people applied for all educations
        Task<TotalApplicants> GetTotalApplicantsAsync();

        Task<GenderDistribution> GetGenderDistributionAsync();

        Task<IEnumerable<EducationDistribution>> GetAllEducationsAsync();
        Task<IEnumerable<string>> GetAllTermsAsync();
        //function for age Distribution in the database
        Task<IEnumerable<AgeDistribution>> GetAgeDistributionAsync();






        Task<IEnumerable<Applicant>> GetApplicantsLocAsync();
        Task UpdateApplicantCoordinatesAsync(Applicant applicant);
        Task<IEnumerable<Applicant>> GetApplicantsWithoutCoordinatesAsync();
        Task<IEnumerable<Applicant>> GetApplicantsWithoutAgeAsync();
        Task UpdateApplicantAgeAsync(int applicantId, int age);


    }
}
