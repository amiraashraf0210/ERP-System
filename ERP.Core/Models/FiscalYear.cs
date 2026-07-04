namespace ERP.Core.Models
{
    public class FiscalYear
    {
        public int      Id        { get; set; }
        public int      Year      { get; set; }       // e.g. 2026
        public DateTime StartDate { get; set; }
        public DateTime EndDate   { get; set; }
        public bool     IsClosed  { get; set; } = false;
        public string?  Notes     { get; set; }
    }
}
