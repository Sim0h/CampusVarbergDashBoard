﻿using CampusVarbergDashBoard.FilterData;
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
		public async Task<IEnumerable<EducationDistribution>> GetAllEducationsAsync()
		{
			using (var connection = GetConnection())
			{
				await connection.OpenAsync();

				string query = @"
                        SELECT Utbildning AS Name, COUNT(*) AS NumberOfApplicants
                        FROM dbo.ExcelData
						WHERE Utbildning NOT LIKE '%INSTÄLLD%'
                        GROUP BY Utbildning"; // under from stod det Applicants, har ingen tabell vid namn Applicants :) lade också till WHERE för att filtrera bort Inställda utbildningar

				return await connection.QueryAsync<EducationDistribution>(query);
			}
		}

		//Hämtar alla terminer
		//On hold då terminer inte definerats som kolumn i Databasen
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

		
		//plockar ut relevant data ifrån databas för att få location för inmemory 
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

				// Hämta alla relevanta fält som en lista för karta
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


		public async Task<IEnumerable<Applicant>> GetMenAsync()
		{
			using (var connection = GetConnection())
			{
				string query = "SELECT * FROM dbo.ExcelData WHERE Kön = 'Man'";
				return await connection.QueryAsync<Applicant>(query);
			}
		}

		public async Task<IEnumerable<Applicant>> GetWomenAsync()
		{
			using (var connection = GetConnection())
			{
				string query = "SELECT * FROM dbo.ExcelData WHERE Kön = 'Kvinna'";
				return await connection.QueryAsync<Applicant>(query);
			}
		}

        public async Task<IEnumerable<AgeDistribution>> GetAgeDistributionAsync()
        {
            using(var connection = GetConnection())
			{
				await connection.OpenAsync();
				string query = "SELECT Födelsedatum FROM dbo.ExcelData WHERE Födelsedatum is NOT NULL";
				var birthdates = await connection.QueryAsync<DateTime>(query);

				var ageDistribution = birthdates.Select(b => DateTime.Now.Year - b.Year)
					.GroupBy(age => GetAgeRange(age))
					.Select(g => new AgeDistribution
					{
						AgeRange = g.Key,
						Count = g.Count()
					}).ToList();

				return ageDistribution;
            }
        }

		private string GetAgeRange(int age)
		{
			if(age < 20) return "Under 20";
			if(age <= 25) return "20-25";
            if (age <= 30) return "26-30";
            if (age <= 35) return "31-35";
            if (age <= 40) return "36-40";
            if (age <= 45) return "41-45";
            if (age <= 50) return "46-50";
            if (age <= 55) return "51-55";
            if (age <= 60) return "56-60";
            return "Över 60";

        }
    }
}