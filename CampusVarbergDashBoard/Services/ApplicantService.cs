using CampusVarbergDashBoard.Repository;
using System.Globalization;

namespace CampusVarbergDashBoard.Services
{
    public class ApplicantService
    {
        private readonly IApplicantRepository _applicantRepo;
        private readonly GeoCodingService _geoCodingService;

        public ApplicantService(IApplicantRepository applicantRepo, GeoCodingService geoCodingService)
        {
            _applicantRepo = applicantRepo;
            _geoCodingService = geoCodingService;
        }

        public async Task UpdateApplicantsCoordinatesAsync()
        {
            var applicants = await _applicantRepo.GetApplicantsWithoutCoordinatesAsync();

            foreach (var applicant in applicants)
            {
                // Rensa och validera postnummer och ort
                string postalCode = CleanPostnummer(applicant.Postnummer);
                string city = CleanOrt(applicant.Ort);

                if (!string.IsNullOrWhiteSpace(postalCode) && !string.IsNullOrWhiteSpace(city))
                {
                    var (latitude, longitude) = await _geoCodingService.GetCoordinatesAsync(postalCode, city);

                    if (latitude != 0 && longitude != 0)
                    {
                        applicant.Latitude = latitude;
                        applicant.Longitud = longitude;

                        await _applicantRepo.UpdateApplicantCoordinatesAsync(applicant);

                        // Skriv ut uppdaterad sökande
                        Console.WriteLine($"Uppdaterad sökande - ID: {applicant.ID}, Ort: {city}, Postnummer: {postalCode}, Latitude: {latitude}, Longitud: {longitude}");
                    }
                    else
                    {
                        Console.WriteLine($"Kunde inte hämta koordinater för sökande ID {applicant.ID} med postnummer {postalCode} och ort {city}");
                    }
                }
                else
                {
                    Console.WriteLine($"Ogiltigt postnummer eller ort för sökande ID {applicant.ID}");
                }
            }
        }

        private string CleanOrt(string ort)
        {
            if (string.IsNullOrWhiteSpace(ort))
                return null;

            ort = ort.Trim().ToLowerInvariant();

            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(ort);
        }

        private string CleanPostnummer(string postnummer)
        {
            if (string.IsNullOrWhiteSpace(postnummer))
                return null;

            // Ta bort alla mellanslag
            postnummer = postnummer.Replace(" ", "").Trim();

            // Kontrollera om postnumret består av exakt 5 siffror
            if (postnummer.Length == 5 && postnummer.All(char.IsDigit))
            {
                return postnummer;
            }
            else
            {
                return null; // Ogiltigt postnummer
            }
        }

        public async Task UpdateApplicantsAgeAsync()
        {
            var applicants = await _applicantRepo.GetApplicantsWithoutAgeAsync();

            foreach (var applicant in applicants)
            {
                if (applicant.Födelsedatum != default(DateTime) && applicant.Registrerad != default(DateTime))
                {
                    int age = CalculateAge(applicant.Födelsedatum, applicant.Registrerad);
                    await _applicantRepo.UpdateApplicantAgeAsync(applicant.ID, age);

                    
                    Console.WriteLine($"Uppdaterad sökande - ID: {applicant.ID}, Ålder: {age}");
                }
                else
                {
                    Console.WriteLine($"Ogiltigt födelsedatum eller registreringsdatum för sökande ID {applicant.ID}");
                }
            }
        }

        private int CalculateAge(DateTime birthDate, DateTime registrationDate)
        {
            int age = registrationDate.Year - birthDate.Year;
            if (registrationDate < birthDate.AddYears(age))
            {
                age--;
            }
            return age;
        }


    }
}