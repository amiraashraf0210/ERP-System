namespace ERP.Core.Models
{
    public class StoreMovement
    {
        public int Id { get; set; }
        public int Code { get; set; }
        public DateTime Date { get; set; }
        public int StoreFrom { get; set; }
        public int StoreTo { get; set; }
        public string? Notes { get; set; }
        public float Value { get; set; }
        public int? Reference { get; set; }

        public Store? StoreFromNav { get; set; }
        public Store? StoreToNav { get; set; }
    }
}
