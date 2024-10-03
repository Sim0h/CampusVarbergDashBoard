using CampusVarbergDashBoard.FilterData;
using CampusVarbergDashBoard.Models;

namespace CampusVarbergDashBoard.Repository
{
    public interface IApplicantRepository
    {

        //Implementera filter för status Erbjuden_plats(tackat ja) , Erbjuden_plats(tackat nej) , Reservplats , Ej antagen
        //Filter för Behörighet, ja eller nej

        Task<CompetenceDistribution> GetCompetenceDistributionAsync();
        Task<StatusDistribution> GetStatusDistributionAsync();

        //Define function that shows how many people applied for all educations
        Task<TotalApplicants> GetTotalApplicantsAsync();
        
        Task<GenderDistribution> GetGenderDistributionAsync();
        //Define function to get gender split by Utbildning
        Task<GenderDistribution> GetGenderDistributionByEducationAsync(string education, string term);
        Task<IEnumerable<string>> GetAllEducationsAsync();
        Task<IEnumerable<string>> GetAllTermsAsync();




        Task UpdateApplicantCoordinatesAsync(Applicant applicant);

    }
}
