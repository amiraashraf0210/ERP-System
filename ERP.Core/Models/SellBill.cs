namespace ERP.Core.Models
{
    public class SellBill
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int Code { get; set; }
        public DateTime Date { get; set; }
        public DateTime Time { get; set; }
        public double Asked { get; set; }
        public double Paid { get; set; }
        public string? Notes { get; set; }
        public string? TxtNote { get; set; }
        public float DisPercent { get; set; }
        public bool Zero { get; set; }
        public int ZeroNum { get; set; }
        public int UserId { get; set; }
        public int? SellerId { get; set; }
        public int StoreNo { get; set; }

        // Bill type: 1=main, 2,3,4=extra stores
        public int BillType { get; set; } = 1;

        // Fiscal Year
        public int FiscalYearId { get; set; }

        // Navigation
        public Customer? Customer { get; set; }
        public User? User { get; set; }
        public FiscalYear? FiscalYear { get; set; }
        public ICollection<CustomerInstallment> Items { get; set; } = new List<CustomerInstallment>();
    }
}
