using CampusVarbergDashBoard.Models;
using CampusVarbergDashBoard.Repository;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace CampusVarbergDashBoard.Controllers
{
    public class FilterVyController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IApplicantRepository _applicantRepo;
        private readonly IConfiguration _configuration;

        public FilterVyController(ILogger<HomeController> logger, IApplicantRepository applicantRepo, IConfiguration configuration)
        {
            _applicantRepo = applicantRepo;
            _logger = logger;
            _configuration = configuration;
        }
        public async Task<IActionResult> Index()
        {
            var applicants = await _applicantRepo.GetAllApplicantsAsync();
            return View(applicants);
        }


        public async Task<IEnumerable<Applicant>> GetApplicantsByEducationAsync(string educationFilter)
        {
            using (var connection = GetConnection())
            {
                string query = @"
                SELECT * FROM dbo.ExcelData
                WHERE Utbildning LIKE '%' + @EducationFilter + '%'";

                var parameters = new DynamicParameters();
                parameters.Add("@EducationFilter", educationFilter);

                return (await connection.QueryAsync<Applicant>(query, parameters)).ToList();
            }
        }




        private SqlConnection GetConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        }

    }
}
