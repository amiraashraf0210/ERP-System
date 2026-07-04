namespace ERP.Core.Models
{
    public class RestockingOrder
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public int GoodId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public DateTime? ProcessDate { get; set; }
        public double Quantity { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Processed, Cancelled
        public string? Notes { get; set; }
        public int? ProcessedByUserId { get; set; }

        // Navigation
        public Good Good { get; set; } = null!;
        public User? ProcessedBy { get; set; }
    }
}
