namespace ERP.Core.Models
{
    public class BuyBill
    {
        public int Id { get; set; }
        public int ImporterId { get; set; }
        public string Code { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public float Asked { get; set; }
        public float Paid { get; set; }
        public string? Notes { get; set; }
        public float DisPercent { get; set; }

        // Fiscal Year
        public int FiscalYearId { get; set; }

        // Navigation
        public Importer? Importer { get; set; }
        public FiscalYear? FiscalYear { get; set; }
        public ICollection<ImporterInstallment> Items { get; set; } = new List<ImporterInstallment>();
    }
}
