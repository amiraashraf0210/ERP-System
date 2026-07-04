namespace ERP.Core.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Pass { get; set; } = string.Empty;
        public string? Tel { get; set; }
        public string? Mobile { get; set; }
        public string? Address { get; set; }

        // Permissions - 120 boolean flags stored as JSON string for flexibility
        public string? Permissions { get; set; }

        // Navigation
        public ICollection<SellBill> SellBills { get; set; } = new List<SellBill>();
    }
}
