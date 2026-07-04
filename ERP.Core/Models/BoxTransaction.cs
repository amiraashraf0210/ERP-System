namespace ERP.Core.Models
{
    public class BoxTransaction
    {
        public int Id { get; set; }
        public bool Out { get; set; }       // true = outgoing, false = incoming
        public double Value { get; set; }
        public string? Notes { get; set; }
        public DateTime Date { get; set; }
        public DateTime Time { get; set; }
        public int No { get; set; }
        public short BoxNo { get; set; }
        public string? CustName { get; set; }

        // Fiscal Year
        public int FiscalYearId { get; set; }

        // Foreign keys for traceability — nullable because not all transactions originate from bills
        public int? SellBillId { get; set; }
        public int? BuyBillId  { get; set; }

        // Navigation
        public FiscalYear? FiscalYear { get; set; }
    }

    public class Expense
    {
        public int Id { get; set; }
        public float Value { get; set; }
        public string? Detail { get; set; }
        public string? Notes { get; set; }
        public DateTime Date { get; set; }
        public int? CostId { get; set; }           // نوع المصروف
        public Cost? Cost { get; set; }
        public int? BoxTransactionId { get; set; } // مرتبط بحركة الخزينة
        public int? AccountFrom { get; set; }
        public bool MainActive { get; set; }
        public bool DisFrom { get; set; }

        // Fiscal Year
        public int FiscalYearId { get; set; }

        // Navigation
        public FiscalYear? FiscalYear { get; set; }
    }

    public class Income
    {
        public int Id { get; set; }
        public float Value { get; set; }
        public string? Detail { get; set; }
        public string? Notes { get; set; }
        public DateTime Date { get; set; }
        public int? CostId { get; set; }           // نوع الإيراد
        public Cost? Cost { get; set; }
        public int? BoxTransactionId { get; set; } // مرتبط بحركة الخزينة
        public int? AccountFrom { get; set; }
        public bool MainActive { get; set; }

        // Fiscal Year
        public int FiscalYearId { get; set; }

        // Navigation
        public FiscalYear? FiscalYear { get; set; }
    }
}
