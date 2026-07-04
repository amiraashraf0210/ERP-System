namespace ERP.Core.Models
{
    public class Importer
    {
        public int Id { get; set; }
        public int Code { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Manager { get; set; }
        public string? Period { get; set; }
        public int? GroupId { get; set; }
        public string? Tel { get; set; }
        public string? Mobile { get; set; }
        public string? Fax { get; set; }
        public string? Notes { get; set; }

        // Navigation
        public ImporterGroup? Group { get; set; }
        public ICollection<BuyBill> BuyBills { get; set; } = new List<BuyBill>();
        public ICollection<ImporterInstallment> Installments { get; set; } = new List<ImporterInstallment>();
        public ICollection<Good> Goods { get; set; } = new List<Good>();
    }
}
