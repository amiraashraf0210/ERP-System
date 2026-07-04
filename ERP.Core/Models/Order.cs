namespace ERP.Core.Models
{
    public class Order
    {
        public int Code { get; set; }
        public bool Book { get; set; }
        public bool Delivery { get; set; }
        public string? CashName { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public string? CustName { get; set; }
        public string? CustTel { get; set; }
        public string? CustAddress { get; set; }
        public string? Detail { get; set; }
        public float Total { get; set; }
        public float Pay { get; set; }
        public bool Paid { get; set; }
        public bool NotPaid { get; set; }
        public int Discount { get; set; }
        public string? ResetNo { get; set; }
        public bool CkDeal { get; set; }

        public ICollection<OrderDetail> Details { get; set; } = new List<OrderDetail>();
    }

    public class OrderDetail
    {
        public int Id { get; set; }
        public int OrderCode { get; set; }
        public string? ItemCode { get; set; }
        public string? Items { get; set; }
        public int Quantity { get; set; }
        public float Price { get; set; }
        public float Total { get; set; }
        public DateTime Date { get; set; }
        public string? ResetNo { get; set; }

        public Order? Order { get; set; }
    }
}
