using CampusVarbergDashBoard.FilterData;
using CampusVarbergDashBoard.Models;

namespace CampusVarbergDashBoard.Repository
{
    public interface IApplicantRepository
    {
        Task<IEnumerable<Applicant>> GetAllApplicantsAsync();
        Task<CompetenceDistribution> GetCompetenceDistributionAsync();
        Task<IEnumerable<StatusDistribution>> GetStatusDistributionAsync(int? year);

        //Define function that shows how many people applied for all educations
        Task<TotalApplicants> GetTotalApplicantsAsync();

        Task<GenderDistribution> GetGenderDistributionAsync();

        //Define function to get gender split by Utbildning
        Task<GenderDistribution> GetGenderDistributionByEducationAsync(string education, string term);
        Task<IEnumerable<EducationDistribution>> GetAllEducationsAsync();
        Task<IEnumerable<string>> GetAllTermsAsync();
        //Filter by men only
        Task<IEnumerable<Applicant>> GetMenAsync();
        //Filter by female only
        Task<IEnumerable<Applicant>> GetWomenAsync();
        //function for age Distribution in the database
        Task<IEnumerable<AgeDistribution>> GetAgeDistributionAsync();





        Task<IEnumerable<Applicant>> GetApplicantsLocAsync();
        Task UpdateApplicantCoordinatesAsync(Applicant applicant);

    }
}
