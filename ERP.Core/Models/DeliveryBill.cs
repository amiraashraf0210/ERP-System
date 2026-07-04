namespace ERP.Core.Models
{
    public class DeliveryBill
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int Code { get; set; }
        public DateTime Date { get; set; }
        public DateTime Time { get; set; }
        public double Asked { get; set; }
        public double Paid { get; set; }
        public string? Notes { get; set; }
        public string? TxtNote { get; set; }
        public float DisPercent { get; set; }
        public bool Zero { get; set; }
        public int ZeroNum { get; set; }
        public int? UserId { get; set; }
        public int? SellerId { get; set; }
        public int StoreNo { get; set; }

        public ICollection<DeliveryMovement> Movements { get; set; } = new List<DeliveryMovement>();
    }

    public class DeliveryMovement
    {
        public int Id { get; set; }
        public int GoodId { get; set; }
        public double Quantity { get; set; }
        public DateTime Date { get; set; }
        public bool IsBill { get; set; }
        public bool Out { get; set; }
        public string? BillNo { get; set; }
        public string? OrderNo { get; set; }
        public float SellPrice { get; set; }
        public float BuyPrice { get; set; }
        public string? Notes { get; set; }
        public int StoreNo { get; set; }
        public int? ImporterNo { get; set; }
        public bool Move { get; set; }
        public int? StoreNo2 { get; set; }
        public int? Tag { get; set; }
        public string? ResetNo { get; set; }
        public int? SellerId { get; set; }
        public bool Type { get; set; }
        public int? ImporterId { get; set; }
        public int? CustomerId { get; set; }
        public float DisbyGood { get; set; }
        public string? IdGCode { get; set; }
        public short? PointerBox { get; set; }

        public Good? Good { get; set; }
    }
}
