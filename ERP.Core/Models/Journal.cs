namespace ERP.Core.Models
{
    public class Journal
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public DateTime Time { get; set; }
        public string? T1 { get; set; }
        public string? T2 { get; set; }
        public int? SellNo { get; set; }
        public float Value { get; set; }
        public float Pay { get; set; }
        public float Reset { get; set; }
        public float Spend { get; set; }
        public float Remind { get; set; }
        public string? Notes { get; set; }
        public string? Main { get; set; }
    }
}
