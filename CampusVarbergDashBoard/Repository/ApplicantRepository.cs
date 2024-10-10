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
        public async Task<IEnumerable<string>> GetAllEducationsAsync()
        {
            using (var connection = GetConnection())
            {
                string query = "SELECT DISTINCT Utbildning FROM dbo.ExcelData";
                return await connection.QueryAsync<string>(query);
            }
        }

        //Hämtar alla terminer
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

                return (await connection.QueryAsync<Applicant>(query)).ToList();
            }
        }

        public async Task UpdateApplicantCoordinatesAsync(Applicant applicant)
        {
            using (var connection = GetConnection())
            {
                string checkQuery = "SELECT Latitude, Longitud FROM dbo.ExcelData WHERE ID = @ID";
                var existingCoordinates = await connection.QueryFirstOrDefaultAsync<Applicant>(checkQuery, new { ID = applicant.ID });

                if (existingCoordinates != null && (existingCoordinates.Latitude.HasValue && existingCoordinates.Longitud.HasValue))
                {

                    return;
                }
                string updateQuery = "UPDATE dbo.ExcelData SET Latitude = @Latitude, Longitud = @Longitud WHERE ID = @ID";
                await connection.ExecuteAsync(updateQuery, new { Latitude = applicant.Latitude, Longitud = applicant.Longitud, ID = applicant.ID });
            }
        }
        public async Task<IEnumerable<Applicant>> GetApplicantsWithoutCoordinatesAsync()
        {
            using (var connection = GetConnection())
            {
                string query = @"
            SELECT 
                ID,
                Postnummer, 
                Ort
            FROM dbo.ExcelData
            WHERE Latitude IS NULL OR Longitud IS NULL";

                return (await connection.QueryAsync<Applicant>(query)).ToList();
            }
        }
    }
}