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

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<IEnumerable<Applicant>> GetAllApplicantsAsync()
        {

            using (var connection = GetConnection())
            {
                string query = "SELECT Utbildning, Födelsedatum, Kön, Postnummer, Ort, Land, Registrerad, Inlämnad, Behörig, Sen_Anmälan, Status, Erbjuden_Plats_Datum, Urval, Longitud, Latitude from dbo.ExcelData";
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

    }
}