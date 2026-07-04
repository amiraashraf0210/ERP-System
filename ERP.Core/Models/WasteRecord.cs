namespace ERP.Core.Models
{
    public class WasteRecord
    {
        public int Id { get; set; }
        public int ProductionOrderId { get; set; }
        public int RawMaterialId { get; set; }
        public double Quantity { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime WasteDate { get; set; } = DateTime.Now;
        public string? Notes { get; set; }

        // Navigation
        public ProductionOrder ProductionOrder { get; set; } = null!;
        public RawMaterial RawMaterial { get; set; } = null!;
    }
}
