using Newtonsoft.Json.Linq;
using System.Web;

namespace CampusVarbergDashBoard.Services
{
    public class GeoCodingService
    {
        private readonly string _googleMapsApiKey = "";

        //för att hämta long och lat via API längrefram för att skickas till databas
        public async Task<(double Latitude, double Longitude)> GetCoordinatesAsync(string postalCode, string City)
        {
            string url = $"https://maps.googleapis.com/maps/api/geocode/json?address={HttpUtility.UrlEncode(postalCode)},+{HttpUtility.UrlEncode(City)}&key={_googleMapsApiKey}";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "CampusVarbergDashBoard");

                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var jsonObject = JArray.Parse(json).FirstOrDefault();

                    if (jsonObject != null)
                    {
                        double latitude = jsonObject["lat"]?.Value<double>() ?? 0;
                        double longitude = jsonObject["lon"]?.Value<double>() ?? 0;

                        return (latitude, longitude);
                    }


                }
            }
            return (0, 0);
        }

        //placeholder för data för heatmap
        public (double Latitude, double Longitude) GetInMemoryCoordinates(string postalCode, string city)
        {

            return city.ToLower() switch
            {
                "stockholm" => (59.3293, 18.0686),
                "göteborg" => (57.7089, 11.9746),
                "malmö" => (55.604981, 13.003822),
                "uppsala" => (59.8586, 17.6389),
                "västerås" => (59.6162, 16.5528),
                "örebro" => (59.2741, 15.2066),
                "linköping" => (58.4108, 15.6214),
                "helsingborg" => (56.0465, 12.6945),
                "jönköping" => (57.7815, 14.1562),
                "norrköping" => (58.5869, 16.2037),
                _ => (0, 0)
            };
        }


    }
}