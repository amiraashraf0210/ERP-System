namespace ERP.Core.Models
{
    public class RecipeItem
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public int RawMaterialId { get; set; }
        public double Quantity { get; set; }
        public string? Notes { get; set; }

        // Navigation
        public ProductionRecipe Recipe { get; set; } = null!;
        public RawMaterial RawMaterial { get; set; } = null!;
    }
}
