namespace ERP.Core.Models
{
    public class Store
    {
        public int Id { get; set; }
        public int Code { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string? Person { get; set; }
        public string? Details { get; set; }
        public string? Notes { get; set; }

        public ICollection<Good> Goods { get; set; } = new List<Good>();
        public ICollection<StoreMovement> StoreMovements { get; set; } = new List<StoreMovement>();
    }
}
