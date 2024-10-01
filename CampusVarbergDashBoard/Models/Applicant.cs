using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace CampusVarbergDashBoard.Models
{
    public class Applicant
    {
        public string Utbildning { get; set; }
        public DateTime Fodelsedatum { get; set; }
        public string Kon { get; set; }
        public string Postnummer { get; set; }
        public string Ort { get; set; }
        public string Land { get; set; }
        public DateTime Registrerad { get; set; }
        public DateTime Inlamnad { get; set; }
        public bool Behorig { get; set; }
        public bool SenAnmalan { get; set; }
        public string Status { get; set; }
        public bool ErbjudenPlats { get; set; }
        public string Urval { get; set; }
        public double Longitud { get; set; }
        public double Latitude { get; set; }
        public int Alder { get; set; }

    }
}
