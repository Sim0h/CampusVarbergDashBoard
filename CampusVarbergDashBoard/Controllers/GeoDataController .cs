using CampusVarbergDashBoard.FilterData;
using CampusVarbergDashBoard.Models;
using CampusVarbergDashBoard.Repository;
using CampusVarbergDashBoard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CampusVarbergDashBoard.Controllers
{
    public class GeoDataController : Controller
    {
        private readonly IApplicantRepository _applicantRepo;
        private readonly GeoCodingService _geoCodingService;
        private readonly ILogger<GeoDataController> _logger;

        public GeoDataController(IApplicantRepository applicantRepo, GeoCodingService geoCodingService, ILogger<GeoDataController> logger)
        {
            _applicantRepo = applicantRepo;
            _geoCodingService = geoCodingService;
            _logger = logger;
        }

        [HttpGet("/GeoData/heatmap-data")]
        public async Task<IActionResult> GetHeatmapData(
            string ort,
            string postnummer,
            int? year,
            string term,
            string kön,
            string behörig,
            string utbildning)
        {
            _logger.LogInformation("Received GetHeatmapData request with parameters: ort={Ort}, postnummer={Postnummer}, year={Year}, term={Term}, kön={Kön}, behörig={Behörig}, utbildning={Utbildning}",
                ort, postnummer, year, term, kön, behörig, utbildning);

            try
            {
                // Rensa och formatera postnummer
                if (!string.IsNullOrEmpty(postnummer))
                {
                    postnummer = postnummer.Replace(" ", "").Trim();
                }

                // Mappa 'Alla Orter', 'Alla Postnummer', 'Alla Kön', 'Alla Behörig', 'Alla Utbildningar' till null
                ort = (ort == "Alla Orter") ? null : ort;
                postnummer = (postnummer == "Alla Postnummer") ? null : postnummer;
                kön = (kön == "Alla Kön") ? null : kön;
                behörig = (behörig == "Alla Behörig") ? null : behörig;
                utbildning = (utbildning == "Alla Utbildningar") ? null : utbildning;

                _logger.LogInformation("Mapped parameters to: ort={Ort}, postnummer={Postnummer}, year={Year}, term={Term}, kön={Kön}, behörig={Behörig}, utbildning={Utbildning}",
                    ort, postnummer, year, term, kön, behörig, utbildning);

                // Hämta sökande baserat på filtreringsparametrar
                var applicants = await _applicantRepo.GetApplicantsLocAsync(ort, postnummer, year, term, kön, behörig, utbildning);

                _logger.LogInformation("Retrieved {Count} applicants from repository.", applicants.Count());

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
                        var temp = cleanedOrt;
                        cleanedOrt = cleanedPostnummer;
                        cleanedPostnummer = temp;
                        _logger.LogDebug("Swapped Ort and Postnummer for applicant.");
                    }

                    // Om Ort är ogiltig, försök hämta från Postnummer
                    if (string.IsNullOrWhiteSpace(cleanedOrt) || IsLikelyInvalidOrt(cleanedOrt))
                    {
                        if (!string.IsNullOrWhiteSpace(cleanedPostnummer) && postnummerOrtMapping.TryGetValue(cleanedPostnummer, out var ortList))
                        {
                            cleanedOrt = ortList.First();
                            _logger.LogDebug("Set Ort from postnummerOrtMapping for applicant.");
                        }
                        else
                        {
                            cleanedOrt = "Okänd Ort";
                            _logger.LogDebug("Set Ort to 'Okänd Ort' for applicant.");
                        }
                    }

                    // Om Postnummer är ogiltigt, försök hämta från Ort
                    if (string.IsNullOrWhiteSpace(cleanedPostnummer))
                    {
                        if (!string.IsNullOrWhiteSpace(cleanedOrt) && ortPostnummerMapping.TryGetValue(cleanedOrt, out var postnummerList))
                        {
                            cleanedPostnummer = postnummerList.First();
                            _logger.LogDebug("Set Postnummer from ortPostnummerMapping for applicant.");
                        }
                        else
                        {
                            cleanedPostnummer = "Okänt Postnummer";
                            _logger.LogDebug("Set Postnummer to 'Okänt Postnummer' for applicant.");
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
                            _logger.LogDebug("Assigned default coordinates to applicant.");
                        }
                        else
                        {
                            _logger.LogDebug("Skipped applicant due to invalid coordinates.");
                            continue;
                        }
                    }

                    // Uppdatera applicant-objektet med rätt värden
                    applicant.Ort = cleanedOrt;
                    applicant.Postnummer = cleanedPostnummer;

                    cleanedApplicants.Add(applicant);
                }

                _logger.LogInformation("Cleaned applicants count: {Count}", cleanedApplicants.Count());

                // Gruppér och sammanställ data för heatmap
                var coordinatesData = cleanedApplicants
                    .GroupBy(a => new { a.Ort, a.Postnummer, a.Latitude, a.Longitud })
                    .Select(g => new
                    {
                        ort = g.Key.Ort,
                        postnummer = g.Key.Postnummer,
                        latitude = g.Key.Latitude.Value, // Latitude är nullable och validerad
                        longitude = g.Key.Longitud.Value, // Longitud är nullable och validerad
                        antalSokande = g.Count()
                    })
                    .ToList();

                _logger.LogInformation("Prepared coordinatesData with {Count} entries.", coordinatesData.Count());

                // Extrahera unika utbildningar och år
                var uniqueUtbildningar = new List<string> { "Alla Utbildningar" }
                    .Concat(cleanedApplicants
                        .Select(a => a.Utbildning)
                        .Where(u => !string.IsNullOrWhiteSpace(u) && !u.StartsWith("INSTÄLLD"))
                        .Select(u => RemoveLocation(u))
                        .Distinct()
                        .OrderBy(u => u))
                    .ToList();

                var uniqueYears = new List<string> { "Alla År" }
                    .Concat(cleanedApplicants
                        .Select(a => a.Inlämnad.Year.ToString())
                        .Where(y => int.Parse(y) > 2000)
                        .Distinct()
                        .OrderByDescending(y => y))
                    .ToList();

                // Extrahera unika orter, postnummer, terminer, kön och behörig
                var uniqueOrter = new List<string> { "Alla Orter" }
                    .Concat(cleanedApplicants
                        .Select(a => a.Ort)
                        .Where(o => !string.IsNullOrWhiteSpace(o))
                        .Distinct()
                        .OrderBy(o => o))
                    .ToList();

                var uniquePostnummer = new List<string> { "Alla Postnummer" }
                    .Concat(cleanedApplicants
                        .Select(a => a.Postnummer)
                        .Where(pn => !string.IsNullOrWhiteSpace(pn))
                        .Distinct()
                        .OrderBy(pn => pn))
                    .ToList();

                var uniqueTerms = new List<string> { "Alla Termin" }
                    .Concat(cleanedApplicants
                        .Select(a => DetermineTerm(a.Inlämnad))
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .Distinct()
                        .OrderBy(t => t))
                    .ToList();

                var uniqueGenders = new List<string> { "Alla Kön" }
                    .Concat(cleanedApplicants
                        .Select(a => a.Kön)
                        .Where(g => !string.IsNullOrWhiteSpace(g))
                        .Distinct()
                        .OrderBy(g => g))
                    .ToList();

                var uniqueCompetences = new List<string> { "Alla Behörig" }
                    .Concat(cleanedApplicants
                        .Select(a => a.Behörig)
                        .Where(c => !string.IsNullOrWhiteSpace(c))
                        .Distinct()
                        .OrderBy(c => c))
                    .ToList();

                _logger.LogInformation("Prepared unique lists: utbildningar={UtbildningarCount}, orter={OrterCount}, postnummer={PostnummerCount}, years={YearsCount}, terms={TermsCount}, genders={GendersCount}, competences={CompetencesCount}",
                    uniqueUtbildningar.Count, uniqueOrter.Count, uniquePostnummer.Count, uniqueYears.Count, uniqueTerms.Count, uniqueGenders.Count, uniqueCompetences.Count);

                if (coordinatesData.Count == 0)
                {
                    _logger.LogWarning("No valid coordinates data found.");
                    // Returnera tomma listor istället för BadRequest
                    return Ok(new
                    {
                        coordinates = new List<object>(),
                        utbildningar = uniqueUtbildningar,
                        orter = uniqueOrter,
                        postnummer = uniquePostnummer,
                        years = uniqueYears,
                        terms = uniqueTerms,
                        genders = uniqueGenders,
                        competences = uniqueCompetences
                    });
                }

                return Ok(new
                {
                    coordinates = coordinatesData,
                    utbildningar = uniqueUtbildningar,
                    orter = uniqueOrter,
                    postnummer = uniquePostnummer,
                    years = uniqueYears,
                    terms = uniqueTerms,
                    genders = uniqueGenders,
                    competences = uniqueCompetences
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in GetHeatmapData.");
                // Returnera ett 500-fel med en giltig JSON-struktur
                return StatusCode(500, new { error = "Ett internt fel uppstod." });
            }
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

        private string RemoveLocation(string utbildning)
        {
            if (string.IsNullOrWhiteSpace(utbildning))
                return utbildning;

            // Ta bort allt efter kommatecken, inklusive kommatecknet
            var parts = utbildning.Split(',');
            return parts[0].Trim();
        }

        // Hjälpfunktion för att bestämma term baserat på Inlämnad-datum
        private string DetermineTerm(DateTime inlämnad)
        {
            var month = inlämnad.Month;
            if (month >= 1 && month <= 6)
                return "Hösttermin";
            else
                return "Vårtermin";
        }

        [HttpGet]
        public IActionResult Map()
        {
            return View();
        }
    }
}