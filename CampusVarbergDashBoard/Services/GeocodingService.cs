using Newtonsoft.Json.Linq;
using System.Web;
using System.Globalization;
using System.Net;

namespace CampusVarbergDashBoard.Services
{
    public class GeoCodingService
    {
        //AIzaSyBsYLhNMRJ0I1u1hesreIN6mYEBxEmG3VA
        private readonly string _googleMapsApiKey = "";

        // Hämtar latitud och longitud från Google Geolocation API
        public async Task<(double Latitude, double Longitude)> GetCoordinatesAsync(string postalCode, string city)
        {
            string url = $"https://maps.googleapis.com/maps/api/geocode/json?address={HttpUtility.UrlEncode(postalCode)},+{HttpUtility.UrlEncode(city)}&key={_googleMapsApiKey}";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "CampusVarbergDashBoard");

                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var jsonObject = JObject.Parse(json);
                    if (jsonObject["results"] is JArray resultsArray && resultsArray.Count > 0)
                    {
                        var location = resultsArray.FirstOrDefault()?["geometry"]?["location"];
                        if (location != null)
                        {
                            double latitude = Math.Round(location["lat"]?.Value<double>() ?? 0, 4);
                            double longitude = Math.Round(location["lng"]?.Value<double>() ?? 0, 4);

                            Console.WriteLine($"{latitude},{longitude}");
                            return (latitude, longitude);
                        }
                    }
                }
            }
            return (0, 0);
        }
    }
}