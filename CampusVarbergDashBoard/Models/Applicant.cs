using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace CampusVarbergDashBoard.Models
{
    public class Applicant
    {
        public string Utbildning { get; set; }
        public DateTime Födelsedatum { get; }
        public string Kön { get; set; }
        public string Postnummer { get; set; }
        public string Ort { get; set; }
        public string Land { get; set; }
        public DateTime Registrerad { get; }
        public DateTime Inlämnad { get; }
        public string Behörig { get; set; }
        public bool SenAnmalan { get; set; }
        public string Status { get; set; }
        public bool ErbjudenPlats { get; set; }
        public string Urval { get; set; }
        public double Longitud { get; set; }
        public double Latitude { get; set; }
        public int Ålder { get; set; }


    }
}
