namespace ERP.Core.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public int Code { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Tel { get; set; }
        public string? Mobile { get; set; }
        public string? Job { get; set; }
        public string? Salary { get; set; }
        public bool ByDay { get; set; }
        public bool ByMonth { get; set; }
        public string? Notes { get; set; }
    }
}
