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

        //public async Task<IEnumerable<string>> GetAllTermsAsync(DateTime startDate, DateTime endDate)
        //{
        //	using (var connection = GetConnection())
        //	{
        //		string query = @"
        //						SELECT Inlämnad	AS YEAR
        //						FROM dbo.ExcelData
        //						WHERE Inlämnad BETWEEN @StartDate AND @EndDate";
        //		return await connection.QueryAsync<string>(query, new { StartDate = startDate, EndDate = endDate });
        //	}
        //}

        public async Task<IEnumerable<YearDistribution>> GetAllYearsAsync(int year)
        {
            using (var connection = GetConnection())
            {
                string query = @"
                        SELECT Inlämnad AS Year
                        FROM dbo.ExcelData
                        WHERE YEAR(Inlämnad) = @Year
                        ORDER BY Inlämnad DESC";

                return await connection.QueryAsync<YearDistribution>(query, new { Year = year });
            }
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
                WHERE Inlämnad BETWEEN @StartDate AND @EndDate";

                var result = await connection.QueryAsync<string>(query, new { StartDate = startDate, EndDate = endDate });
              
                return result;
            }
        }

        public async Task<IEnumerable<Applicant>> GetApplicantsByEducationAsync(string education)
        {
            using (var connection = GetConnection())
            {
                string query = "SELECT * FROM dbo.ExcelData WHERE Utbildning LIKE '%' + @Education + '%'";
                return await connection.QueryAsync<Applicant>(query, new { Education = education });
            }
        }

        public async Task<IEnumerable<Applicant>> GetApplicantsAsync()
        {
            using (var connection = GetConnection())
            {
                string query = "SELECT * FROM dbo.ExcelData";
                var applicants = await connection.QueryAsync<Applicant>(query);
                Console.WriteLine($"Retrieved {applicants.Count()} applicants.");
                return applicants;
            }
        }

        public Task<IEnumerable<string>> GetSpecificYearAsync(string year)
        {
            throw new NotImplementedException();
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
