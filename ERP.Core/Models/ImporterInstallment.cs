namespace ERP.Core.Models
{
    public class ImporterInstallment
    {
        public int Id { get; set; }
        public int BillId { get; set; }
        public int ImporterId { get; set; }
        public DateTime Date { get; set; }
        public int GoodId { get; set; }
        public float Quantity { get; set; }
        public float Price { get; set; }
        public float Total { get; set; }
        public float Pay { get; set; }
        public int? MandubId { get; set; }
        public string? Notes { get; set; }
        public float Discount { get; set; }
        public float Tax { get; set; }
        public int StoreId { get; set; }
        public int? SellerId { get; set; }
        public float DisMoney { get; set; }
        public float DisPercent { get; set; }
        public float DisPerItem { get; set; }
        public float TaxPerGoods { get; set; }
        public int? ReNo { get; set; }
        public float ValueTax { get; set; }
        public float SellGoods { get; set; }
        public bool Back { get; set; }
        public double CustPaid { get; set; }
        public int? PayNo { get; set; }
        public int? Reference { get; set; }
        public int ChickenCount { get; set; }

        // Navigation
        public BuyBill? Bill { get; set; }
        public Importer? Importer { get; set; }
        public Good? Good { get; set; }
    }
}
