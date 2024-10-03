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
            var applicants = await _applicantRepo.GetAllApplicantsAsync();

            var coordinatesData = new List<object>();

            foreach (var applicant in applicants)
            {
                // Kontrollera om koordinater redan finns
                if (applicant.Latitude == 0 && applicant.Longitud == 0)
                {
                    var coordinates = await _geoCodingService.GetCoordinatesAsync(applicant.Postnummer, applicant.Ort);

                    // Uppdatera Applicant-objektet med koordinater
                    applicant.Latitude = coordinates.Latitude;
                    applicant.Longitud = coordinates.Longitude;

                    // Spara de nya koordinaterna till databasen
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

            return Ok(coordinatesData);
        }

        [HttpGet("coordinates/memory")]
        public async Task<IActionResult> GetApplicantsCoordinatesMemory()
        {
            var applicants = await _applicantRepo.GetAllApplicantsAsync();

            var coordinatesData = new List<object>();

            foreach (var applicant in applicants)
            {
                // Kontrollera om koordinater redan finns
                if (applicant.Latitude == 0 && applicant.Longitud == 0)
                {
                    // Använd in-memory-kod för att hämta koordinater
                    var coordinates = _geoCodingService.GetInMemoryCoordinates(applicant.Postnummer, applicant.Ort);

                    // Uppdatera Applicant-objektet med de in-memory-genererade koordinaterna
                    applicant.Latitude = coordinates.Latitude;
                    applicant.Longitud = coordinates.Longitude;

                    // Om du vill spara dessa koordinater till databasen, avkommentera nästa rad
                    // await _applicantRepository.UpdateApplicantCoordinatesAsync(applicant);
                }

                // Lägg till koordinaterna i resultatet
                coordinatesData.Add(new
                {
                    applicant.Postnummer,
                    applicant.Ort,
                    applicant.Latitude,
                    applicant.Longitud
                });
            }

            return Ok(coordinatesData);
        }

        //heatmap med filter API 
        //[HttpGet("heatmap-data")]
        //public async Task<IActionResult> GetHeatmapData()
        //{
        //    var applicants = await _applicantRepo.GetAllApplicantsAsync();

        //    var heatmapData = applicants
        //        .Where(a => a.Latitude != 0 && a.Longitud != 0)
        //        .Select(a => new { a.Latitude, a.Longitud });

        //    return Ok(heatmapData);
        //}

        [HttpGet("heatmap-data")]
        public async Task<IActionResult> GetHeatmapData()
        {
            var applicants = await _applicantRepo.GetAllApplicantsAsync();

            var coordinatesData = new List<object>();

            foreach (var applicant in applicants)
            {
                // Kontrollera om koordinater redan finns, annars använd in-memory koordinater
                if (applicant.Latitude == 0 && applicant.Longitud == 0)
                {
                    // Använd in-memory-kod för att hämta koordinater
                    var coordinates = _geoCodingService.GetInMemoryCoordinates(applicant.Postnummer, applicant.Ort);

                    // Uppdatera Applicant-objektet med de in-memory-genererade koordinaterna
                    applicant.Latitude = coordinates.Latitude;
                    applicant.Longitud = coordinates.Longitude;

                    // Lägg till koordinaterna i resultatet
                    coordinatesData.Add(new
                    {
                        applicant.Latitude,
                        applicant.Longitud
                    });
                }
            }

            return Ok(coordinatesData);
        }

        [HttpGet("location-data")]
        public async Task<IActionResult> GetLocationData()
        {
            var applicants = await _applicantRepo.GetAllApplicantsAsync();

            var locationData = applicants.Select(a => new
            {
                a.Postnummer,
                a.Ort
            });

            return Ok(locationData);
        }
        [HttpGet]
        public IActionResult Map()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}