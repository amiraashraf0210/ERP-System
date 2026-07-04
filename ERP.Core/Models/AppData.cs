namespace ERP.Core.Models
{
    public class AppData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Active { get; set; }
        public string? Tel { get; set; }
        public string? Fax { get; set; }
        public string? Address { get; set; }
        public string? Logo { get; set; }
    }
}
