namespace ERP.Core.Models
{
    public class Customer
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
        public int? MandobId { get; set; }

        // Navigation
        public CustomerGroup? Group { get; set; }
        public ICollection<SellBill> SellBills { get; set; } = new List<SellBill>();
        public ICollection<CustomerInstallment> Installments { get; set; } = new List<CustomerInstallment>();
    }
}
