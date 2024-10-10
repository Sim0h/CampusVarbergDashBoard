using CampusVarbergDashBoard.Models;
using CampusVarbergDashBoard.Repository;
using CampusVarbergDashBoard.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

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

        [HttpGet("/GeoData/heatmap-data")]
        public async Task<IActionResult> GetHeatmapData()
        {
            var applicants = await _applicantRepo.GetApplicantsLocAsync();

            // Bygg mappingar från Postnummer till Ort och vice versa
            var postnummerOrtMapping = applicants
                .Select(a => new
                {
                    Postnummer = CleanPostnummer(a.Postnummer),
                    Ort = CleanOrt(a.Ort)
                })
                .Where(a => !string.IsNullOrWhiteSpace(a.Postnummer) && !string.IsNullOrWhiteSpace(a.Ort))
                .GroupBy(a => a.Postnummer)
                .ToDictionary(g => g.Key, g => g.Select(a => a.Ort).Distinct().ToList());

            var ortPostnummerMapping = applicants
                .Select(a => new
                {
                    Ort = CleanOrt(a.Ort),
                    Postnummer = CleanPostnummer(a.Postnummer)
                })
                .Where(a => !string.IsNullOrWhiteSpace(a.Ort) && !string.IsNullOrWhiteSpace(a.Postnummer))
                .GroupBy(a => a.Ort)
                .ToDictionary(g => g.Key, g => g.Select(a => a.Postnummer).Distinct().ToList());

            var cleanedApplicants = new List<Applicant>();

            foreach (var applicant in applicants)
            {
                string cleanedOrt = CleanOrt(applicant.Ort);
                string cleanedPostnummer = CleanPostnummer(applicant.Postnummer);

                bool ortIsNumeric = IsNumeric(cleanedOrt);
                bool postnummerIsNumeric = IsNumeric(cleanedPostnummer);

                // Kontrollera om Ort och Postnummer kan vara omvända
                if (ortIsNumeric && !postnummerIsNumeric)
                {
                    // Byt plats på dem
                    var temp = cleanedOrt;
                    cleanedOrt = cleanedPostnummer;
                    cleanedPostnummer = temp;
                }

                // Om Ort är ogiltig, försök hämta från Postnummer
                if (string.IsNullOrWhiteSpace(cleanedOrt) || IsLikelyInvalidOrt(cleanedOrt))
                {
                    if (!string.IsNullOrWhiteSpace(cleanedPostnummer) && postnummerOrtMapping.TryGetValue(cleanedPostnummer, out var ortList))
                    {
                        cleanedOrt = ortList.First();
                    }
                    else
                    {
                        cleanedOrt = "Okänd Ort";
                    }
                }

                // Om Postnummer är ogiltigt, försök hämta från Ort
                if (string.IsNullOrWhiteSpace(cleanedPostnummer))
                {
                    if (!string.IsNullOrWhiteSpace(cleanedOrt) && ortPostnummerMapping.TryGetValue(cleanedOrt, out var postnummerList))
                    {
                        cleanedPostnummer = postnummerList.First();
                    }
                    else
                    {
                        cleanedPostnummer = "Okänt Postnummer";
                    }
                }

                // Validera koordinater
                if (!IsValidCoordinate(applicant.Latitude, applicant.Longitud))
                {
                    // Om koordinater saknas, kontrollera om orten är okänd
                    if (cleanedOrt == "Okänd Ort" || cleanedPostnummer == "Okänt Postnummer")
                    {
                        // Tilldela standardkoordinater
                        applicant.Latitude = 56.66;
                        applicant.Longitud = 12.86;
                    }
                    else
                    {
                        // Hoppa över om vi inte har några koordinater
                        continue;
                    }
                }

                // Uppdatera applicant-objektet med rengjorda värden
                applicant.Ort = cleanedOrt;
                applicant.Postnummer = cleanedPostnummer;

                cleanedApplicants.Add(applicant);
            }

            // Gruppera och aggregera data
            var coordinatesData = cleanedApplicants
                .GroupBy(a => new { a.Ort, a.Postnummer, a.Latitude, a.Longitud })
                .Select(g => new
                {
                    ort = g.Key.Ort,
                    postnummer = g.Key.Postnummer,
                    latitude = g.Key.Latitude.Value,
                    longitude = g.Key.Longitud.Value,
                    antalSokande = g.Count()
                })
                .ToList();

            if (coordinatesData == null || coordinatesData.Count == 0)
            {
                return BadRequest("Ingen giltig data hittades.");
            }

            return Ok(new { coordinates = coordinatesData });
        }

        // Hjälpfunktioner

        private string CleanOrt(string ort)
        {
            if (string.IsNullOrWhiteSpace(ort))
                return null;

            ort = ort.Trim().ToLower();

            return CapitalizeFirstLetter(ort);
        }

        private string CleanPostnummer(string postnummer)
        {
            if (string.IsNullOrWhiteSpace(postnummer))
                return null;

            
            postnummer = postnummer.Replace(" ", "").Trim();

            
            if (postnummer.Length == 5 && IsNumeric(postnummer))
            {
                return postnummer;
            }
            else
            {
                return null; 
            }
        }

        private bool IsNumeric(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            return text.All(char.IsDigit);
        }

        private bool IsLikelyInvalidOrt(string ort)
        {
            return IsNumeric(ort) || ContainsInvalidCharacters(ort);
        }

        private bool ContainsInvalidCharacters(string text)
        {
            return !text.All(c => char.IsLetter(c) || char.IsWhiteSpace(c) || c == '-');
        }

        private string CapitalizeFirstLetter(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            return char.ToUpper(text[0]) + text.Substring(1);
        }

        private bool IsValidCoordinate(double? latitude, double? longitude)
        {
            if (!latitude.HasValue || !longitude.HasValue)
                return false;

            double lat = latitude.Value;
            double lng = longitude.Value;

            return lat >= -90 && lat <= 90 && lng >= -180 && lng <= 180;
        }

        [HttpGet]
        public IActionResult Map()
        {
            return View();
        }
    }
}