namespace ERP.Core.Models
{
    public class RawMaterial
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int? UnitId { get; set; }
        public double BuyPrice { get; set; }
        public double CurrentStock { get; set; }
        public int MinStock { get; set; }
        public int? StoreId { get; set; }
        public string? Notes { get; set; }
        public DateTime? LastPurchase { get; set; }

        // Navigation
        public Unit? Unit { get; set; }
        public Store? Store { get; set; }
        public ICollection<RecipeItem> RecipeItems { get; set; } = new List<RecipeItem>();
        public ICollection<RawMaterialMovement> Movements { get; set; } = new List<RawMaterialMovement>();
    }
}
