namespace ERP.Core.Models
{
    public class Movement
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
        public float BorsaPrice { get; set; }
        public int ChickenCount { get; set; }
        public string? CustName { get; set; }
        public int? MandobId { get; set; }

        // Fiscal Year
        public int FiscalYearId { get; set; }

        // Navigation
        public Good? Good { get; set; }
        public FiscalYear? FiscalYear { get; set; }
    }
}
