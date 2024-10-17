using CampusVarbergDashBoard.FilterData;
using CampusVarbergDashBoard.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CampusVarbergDashBoard.Repository
{
    public class YearRepository : IYearRepository
    {

        private readonly string _connectionString;

        public YearRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
      
        public async Task<IEnumerable<string>> GetSpecificTermAsync(string term, int year)
        {
            using (var connection = GetConnection())
            {
                DateTime startDate;
                DateTime endDate;

                if (term == "Hösttermin")
                {
                    startDate = new DateTime(year, 1, 1);
                    endDate = new DateTime(year, 6, 30);
                }
                else if (term == "Vårtermin")
                {
                    startDate = new DateTime(year, 7, 1);
                    endDate = new DateTime(year, 12, 31);
                }
                else
                {
                    throw new ArgumentException("Invalid term specified. Valid terms are 'Vårtermin' and 'Hösttermin'.");
                }

                string query = @"
                SELECT CONVERT(varchar, Inlämnad, 23) AS Termin
                FROM dbo.ExcelData
                WHERE Inlämnad IS NOT NULL AND Inlämnad BETWEEN @StartDate AND @EndDate";

                var result = await connection.QueryAsync<string>(query, new { StartDate = startDate, EndDate = endDate });
              
                return result;
            }
        }
              
        public async Task<IEnumerable<Applicant>> GetApplicantsAsync()
        {
            using (var connection = GetConnection())
            {
                string query = "SELECT * FROM dbo.ExcelData";
                var applicants = await connection.QueryAsync<Applicant>(query);
                return applicants;
            }
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
