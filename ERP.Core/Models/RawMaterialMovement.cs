namespace ERP.Core.Models
{
    public class RawMaterialMovement
    {
        public int Id { get; set; }
        public int RawMaterialId { get; set; }
        public double Quantity { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public bool IsPurchase { get; set; }
        public bool IsProduction { get; set; }
        public bool Out { get; set; }
        public string? ReferenceNo { get; set; }
        public string? Notes { get; set; }
        public double UnitPrice { get; set; }

        // Navigation
        public RawMaterial RawMaterial { get; set; } = null!;
    }
}
