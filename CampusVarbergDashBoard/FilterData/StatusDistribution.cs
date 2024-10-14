namespace CampusVarbergDashBoard.FilterData
{
    public class StatusDistribution
    {
        public DateTime? Registrerad { get; set; } 
        public DateTime? Inlämnad { get; set; }
        public DateTime? Erbjuden_Plats_Datum { get; set; }
        public int RegistreradCount { get; set; }
        public int InlämnadCount { get; set; }
        public int Erbjuden_Plats_DatumCount { get; set; }
        public int Year { get; set; }

    }
}
