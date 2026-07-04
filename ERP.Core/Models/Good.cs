namespace ERP.Core.Models
{
    public class Good
    {
        public int Id { get; set; }
        public int? ImporterId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int? GroupId { get; set; }
        public int? ModelId { get; set; }
        public int? MarketId { get; set; }
        public string? Size { get; set; }
        public int? ColorId { get; set; }
        public int? UnitId { get; set; }
        public int UnitValue { get; set; } = 1;
        public double SellPrice { get; set; }
        public float Percent { get; set; }
        public double SellPriceSP { get; set; }
        public double HalfPrice { get; set; }
        public double CustPrice { get; set; }
        public float BuyPrice { get; set; }
        public float BuyAverage { get; set; }
        public string? Notes { get; set; }
        public DateTime? LastBuy { get; set; }
        public float LastBuyValue { get; set; }
        public int MinStock { get; set; }
        public int MaxStock { get; set; }
        public int? StoreId { get; set; }
        public bool Diff { get; set; }
        public string? GoodsSpeedNo { get; set; }
        public DateTime? DayOfRegister { get; set; }
        public float DiscountValue { get; set; }
        public int QtyBig { get; set; }

        // Price tiers
        public int? Paper1 { get; set; } public float Price1 { get; set; }
        public int? Paper2 { get; set; } public float Price2 { get; set; }
        public int? Paper3 { get; set; } public float Price3 { get; set; }
        public int? Paper4 { get; set; } public float Price4 { get; set; }
        public int? Paper5 { get; set; } public float Price5 { get; set; }

        // Offer period
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public float PresentPrice { get; set; }
        public float TagHez { get; set; }
        public float BorsaPrice { get; set; }

        // Navigation
        public Importer? Importer { get; set; }
        public GoodGroup? Group { get; set; }
        public GoodModel? Model { get; set; }
        public Market? Market { get; set; }
        public GoodColor? Color { get; set; }
        public Unit? Unit { get; set; }
        public Store? Store { get; set; }
        public ICollection<Movement> Movements { get; set; } = new List<Movement>();
    }
}
