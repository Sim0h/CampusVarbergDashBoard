using CampusVarbergDashBoard.Models;
using CampusVarbergDashBoard.Repository;
using CampusVarbergDashBoard.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CampusVarbergDashBoard.Controllers
{

    public class GeoDataController : Controller
    {
        private readonly IApplicantRepository _applicantRepo;
        private readonly GeoCodingService _geoCodingService;

        public GeoDataController(IApplicantRepository applicantRepo, GeoCodingService geoCodingService)
        {
            _applicantRepo = applicantRepo;
            _geoCodingService = geoCodingService;
        }

        [HttpGet("coordinates")]
        public async Task<IActionResult> GetApplicantsCoordinates()
        {
            // Hämta totalantalet ansökande
            var totalApplicants = await _applicantRepo.GetTotalApplicantsAsync();

            // Hämta alla relevanta ansökandes data
            var applicants = await _applicantRepo.GetApplicantsLocAsync();

            var coordinatesData = new List<object>();

            foreach (var applicant in applicants)
            {
                if (applicant.Latitude == 0 && applicant.Longitud == 0)
                {
                    var coordinates = await _geoCodingService.GetCoordinatesAsync(applicant.Postnummer, applicant.Ort);

                    applicant.Latitude = coordinates.Latitude;
                    applicant.Longitud = coordinates.Longitude;

                    await _applicantRepo.UpdateApplicantCoordinatesAsync(applicant);
                }

                coordinatesData.Add(new
                {
                    applicant.Postnummer,
                    applicant.Ort,
                    applicant.Latitude,
                    applicant.Longitud
                });
            }

            // Returnera koordinater och totalantalet ansökande
            return Ok(new { TotalApplicants = totalApplicants.TotalApplicantsCount, Coordinates = coordinatesData });
        }

        [HttpGet("coordinates/memory")]
        public async Task<IActionResult> GetApplicantsCoordinatesMemory()
        {
            // Hämta totalantalet ansökande
            var totalApplicants = await _applicantRepo.GetTotalApplicantsAsync();

            // Hämta alla relevanta ansökandes data
            var applicants = await _applicantRepo.GetApplicantsLocAsync();

            var coordinatesData = new List<object>();

            foreach (var applicant in applicants)
            {
                if (applicant.Latitude == 0 && applicant.Longitud == 0)
                {
                    var coordinates = _geoCodingService.GetInMemoryCoordinates(applicant.Postnummer, applicant.Ort);

                    applicant.Latitude = coordinates.Latitude;
                    applicant.Longitud = coordinates.Longitude;

                    // Om du vill spara dessa koordinater till databasen, avkommentera nästa rad
                    // await _applicantRepo.UpdateApplicantCoordinatesAsync(applicant);
                }

                coordinatesData.Add(new
                {
                    applicant.Postnummer,
                    applicant.Ort,
                    applicant.Latitude,
                    applicant.Longitud
                });
            }

            // Returnera koordinater och totalantalet ansökande
            return Ok(new { TotalApplicants = totalApplicants.TotalApplicantsCount, Coordinates = coordinatesData });
        }


        [HttpGet("/GeoData/heatmap-data")]
        public async Task<IActionResult> GetHeatmapData()
        {
            var totalApplicants = await _applicantRepo.GetTotalApplicantsAsync();
            var applicants = await _applicantRepo.GetApplicantsLocAsync();

            var coordinatesData = new List<object>();

            foreach (var applicant in applicants)
            {
                // Använd in-memory koordinater
                var coordinates = _geoCodingService.GetInMemoryCoordinates(applicant.Postnummer, applicant.Ort);

                // Kontrollera att koordinater är giltiga
                if (coordinates.Latitude != 0 && coordinates.Longitude != 0)
                {
                    coordinatesData.Add(new
                    {
                        Latitude = coordinates.Latitude,
                        Longitud = coordinates.Longitude
                    });
                }
            }

            // Returnera endast giltiga koordinater
            return Ok(new { TotalApplicants = totalApplicants.TotalApplicantsCount, Coordinates = coordinatesData });
        }

        [HttpGet("location-data")]
        public async Task<IActionResult> GetLocationData()
        {
            // Hämta totalantalet ansökande
            var totalApplicants = await _applicantRepo.GetTotalApplicantsAsync();

            // Hämta alla relevanta ansökandes data
            var applicants = await _applicantRepo.GetApplicantsLocAsync();

            var locationData = applicants.Select(a => new
            {
                a.Postnummer,
                a.Ort
            });

            // Returnera platsdata och totalantalet ansökande
            return Ok(new { TotalApplicants = totalApplicants.TotalApplicantsCount, LocationData = locationData });
        }

        [HttpGet("total-applicants")]
        public async Task<IActionResult> GetTotalApplicants()
        {
            var totalApplicants = await _applicantRepo.GetTotalApplicantsAsync();
            return Ok(totalApplicants);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet("/GeoData/applicant-stats")]
        public async Task<IActionResult> GetApplicantStats()
        {
            var applicants = await _applicantRepo.GetApplicantsLocAsync();

            // Separera felaktiga postnummer/orter
            var invalidStats = applicants
                .Where(a => IsNumeric(a.Ort) || !IsNumeric(a.Postnummer))
                .GroupBy(a => new { a.Postnummer, a.Ort })
                .Select(group => new
                {
                    Postnummer = group.Key.Postnummer,
                    Ort = group.Key.Ort,
                    AntalSokande = group.Count()
                })
                .ToList();

            // Separera giltiga postnummer/orter
            var validStats = applicants
                .Where(a => !IsNumeric(a.Ort) && IsNumeric(a.Postnummer) && !string.IsNullOrWhiteSpace(a.Ort))
                .GroupBy(a => new { a.Postnummer, a.Ort })
                .Select(group => new
                {
                    Postnummer = group.Key.Postnummer,
                    Ort = group.Key.Ort,
                    AntalSokande = group.Count()
                })
                .ToList();

            return Ok(new { ValidStats = validStats, InvalidStats = invalidStats });
        }


        private bool IsNumeric(string text)
        {
            var cleanedText = text.Replace(" ", "");
            return text.All(char.IsDigit);
        }

        [HttpGet]
        public IActionResult Map()
        {
            return View();
        }
    }
}