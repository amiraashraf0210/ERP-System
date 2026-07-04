namespace ERP.Core.Models
{
    public class ProductionRecipe
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int GoodId { get; set; }
        public double OutputQuantity { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation
        public Good Good { get; set; } = null!;
        public ICollection<RecipeItem> RecipeItems { get; set; } = new List<RecipeItem>();
        public ICollection<ProductionOrder> ProductionOrders { get; set; } = new List<ProductionOrder>();
    }
}
