namespace ERP.Core.Models
{
    public class RootAccount
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int? MainAccount { get; set; }
        public string? TypeAccount { get; set; }
        public bool AccountAnalyses { get; set; }
        public bool AccountType { get; set; }
        public string? Notes { get; set; }
    }

    public class Root
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public bool AccountType { get; set; }
    }
}
