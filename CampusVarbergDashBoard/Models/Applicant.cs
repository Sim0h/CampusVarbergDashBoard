using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace CampusVarbergDashBoard.Models
{
    public class Applicant
    {
        public string Utbildning { get; set; }
        public DateTime Födelsedatum { get; set; }
        public DateTime Registrerad { get; set; }
        public DateTime Inlämnad { get; set; }
        public string Behörig { get; set; }
        public string SenAnmälan { get; set; }
        public string Status { get; set; }
        public string ErbjudenPlats { get; set; }
        public string Urval { get; set; }
        public double Longitud { get; set; }
        public double Latitude { get; set; }
        public int Ålder { get; set; }
    }
}
