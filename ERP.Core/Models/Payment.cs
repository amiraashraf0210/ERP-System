namespace ERP.Core.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int? BillIdReceipt { get; set; }
        public int? BillIdExchange { get; set; }
        public int? DisPones { get; set; }
        public bool Type { get; set; }  // true=receipt, false=exchange
        public DateTime Date { get; set; }
        public float Total { get; set; }
        public string? Notes { get; set; }
        public int? From { get; set; }
        public int? To { get; set; }
        public int? UserId { get; set; }
        public string? ResetNo { get; set; }
        public int? MandobId { get; set; }
    }
}
