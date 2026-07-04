namespace ERP.Core.Models
{
    public class BankLoan
    {
        public int      Id          { get; set; }
        public int      BankId      { get; set; }
        public string   LoanCode    { get; set; } = string.Empty;
        public DateTime LoanDate    { get; set; } = DateTime.Today;
        public double   Amount      { get; set; }
        public double   InterestRate{ get; set; }
        public string   LoanType    { get; set; } = "Loan";  // Loan | Deposit
        public string   Status      { get; set; } = "Active"; // Active | Settled
        public string?  Notes       { get; set; }

        public Bank? Bank { get; set; }
        public ICollection<LoanPayment> Payments { get; set; } = new List<LoanPayment>();

        public double TotalPaid   => Payments.Sum(p => p.Amount);
        public double Remaining   => Amount - TotalPaid;
    }

    public class LoanPayment
    {
        public int      Id          { get; set; }
        public int      LoanId      { get; set; }
        public DateTime PayDate     { get; set; } = DateTime.Today;
        public double   Amount      { get; set; }
        public string?  Notes       { get; set; }
        public int?     BoxTransactionId { get; set; }

        public BankLoan? Loan { get; set; }
        public BoxTransaction? BoxTransaction { get; set; }
    }
}
