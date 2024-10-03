using CampusVarbergDashBoard.FilterData;
using CampusVarbergDashBoard.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CampusVarbergDashBoard.Repository
{
    public class ApplicantRepository : IApplicantRepository
    {
        private readonly string _connectionString;

        public ApplicantRepository(string connectionString)
        {
            _connectionString = connectionString;
        }



        //Hämtar alla ansökande
        public async Task<TotalApplicants> GetTotalApplicantsAsync()
        {
            using (var connection = GetConnection())
            {
                string query = "SELECT COUNT(ID) FROM dbo.ExcelData";
                int totalApplicantsCount = await connection.ExecuteScalarAsync<int>(query);

                return new TotalApplicants
                {
                    TotalApplicantsCount = totalApplicantsCount
                };
            }
        }

        //Hämtar alla utbildningar
        public async Task<IEnumerable<EducationDistribution>> GetAllEducationsAsync()
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                    string query = @"
                        SELECT Utbildning AS Name, COUNT(*) AS NumberOfApplicants
                            FROM Applicants
                            GROUP BY Utbildning";

                return await connection.QueryAsync<EducationDistribution>(query);
            }
        }

        //Hämtar alla terminer
        //On hold då terminer inte definerats som kolumn i Databasen
        public Task<IEnumerable<string>> GetAllTermsAsync()
        {
            using (var connection = GetConnection())
            {
                string query = "SELECT DISTINCT Termin FROM dbo.ExcelData";
                return connection.QueryAsync<string>(query);
            }
        }

        public async Task<CompetenceDistribution> GetCompetenceDistributionAsync()
        {

            using (var connection = GetConnection())
            {
                await connection.OpenAsync(); // Ensure the connection is opened

                string query = @"
                        SELECT
                            SUM(CASE WHEN Behörig = 'Ja' THEN 1 ELSE 0 END) AS CompetenceCount,
                            SUM(CASE WHEN Behörig = 'Nej' THEN 1 ELSE 0 END) AS NonCompetenceCount
                        FROM dbo.ExcelData";

                return await connection.QueryFirstOrDefaultAsync<CompetenceDistribution>(query);
            }
        }



        //Hämtar könsfördelning för alla ansökande
        public async Task<GenderDistribution> GetGenderDistributionAsync()
        {
            using (var connection = GetConnection())
            {
                string query = @"
                                SELECT
                                    SUM(CASE WHEN Kön = 'Man' THEN 1 ELSE 0 END) AS MaleCount,
                                    SUM(CASE WHEN Kön = 'Kvinna' THEN 1 ELSE 0 END) AS FemaleCount 
                                 FROM dbo.ExcelData";

                return await connection.QueryFirstOrDefaultAsync<GenderDistribution>(query);

            }
        }

        //Hämtar könsfördelning för en specifik utbildning
        public async Task<GenderDistribution> GetGenderDistributionByEducationAsync(string education, string term)
        {
            using (var connection = GetConnection())
            {
                string query = @"
                                SELECT
                                    SUM(CASE WHEN Kön = 'Man' THEN 1 ELSE 0 END) AS MaleCount,
                                    SUM(CASE WHEN Kön = 'Kvinna' THEN 1 ELSE 0 END) AS FemaleCount 
                                FROM dbo.ExcelData
                                WHERE Utbildning = @Education AND Termin = @Term";

                return await connection.QueryFirstOrDefaultAsync<GenderDistribution>(query, new { Education = education, Term = term });
            }

        }

        public async Task<StatusDistribution> GetStatusDistributionAsync()
        {
            using (var connection = GetConnection())
            {
                string query = @"
                                SELECT
                                    SUM(CASE WHEN Status = 'Erbjuden plats (tackat ja)' THEN 1 ELSE 0 END) AS OfferedPlaceAcceptedCount,
                                    SUM(CASE WHEN Status = 'Erbjuden plats (tackat nej)' THEN 1 ELSE 0 END) AS OfferedPlaceDeclinedCount,
                                    SUM(CASE WHEN Status = 'Reservplats' THEN 1 ELSE 0 END) AS ReservePlaceCount,
                                    SUM(CASE WHEN Status = 'Ej antagen' THEN 1 ELSE 0 END) AS NotAcceptedCount
                                FROM dbo.ExcelData";

                return await connection.QueryFirstOrDefaultAsync<StatusDistribution>(query);
            }
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
        //plockar ut relevant data ifrån databas för att få location för inmemory 
        public async Task<IEnumerable<Applicant>> GetApplicantsLocAsync()
        {
            using (var connection = GetConnection())
            {
                string query = @"
                    SELECT 
                        ID,
                        Postnummer, 
                        Ort, 
                        Latitude, 
                        Longitud
                    FROM dbo.ExcelData";

                // Hämta alla relevanta fält som en lista för karta
                return (await connection.QueryAsync<Applicant>(query)).ToList();
            }
        }

        public async Task UpdateApplicantCoordinatesAsync(Applicant applicant)
        {
            using (var connection = GetConnection())
            {
                string updateQuery = "UPDATE dbo.ExcelData SET Latitude = @Latitude, Longitude = @Longitude WHERE Postnummer = @Postnummer AND Ort = @Ort";
                await connection.ExecuteAsync(updateQuery, new { applicant.Latitude, applicant.Longitud, applicant.Postnummer, applicant.Ort });
            }
        }


        public async Task<IEnumerable<Applicant>> GetMenAsync()
        {
            using (var connection = GetConnection())
            {
                string query = "SELECT * FROM dbo.ExcelData WHERE Kön = 'Man'";
                return await connection.QueryAsync<Applicant>(query);
            }
        }

        public async Task<IEnumerable<Applicant>> GetWomenAsync()
        {
            using(var connection = GetConnection())
            {
                string query = "SELECT * FROM dbo.ExcelData WHERE Kön = 'Kvinna'";
                return await connection.QueryAsync<Applicant>(query);
            }
        }
    }
}