namespace ERP.Core.Models
{
    public class Unit
    {
        public int Id { get; set; }
        public int Code { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public ICollection<Good> Goods { get; set; } = new List<Good>();
    }

    public class GoodGroup
    {
        public int Id { get; set; }
        public int Code { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public ICollection<Good> Goods { get; set; } = new List<Good>();
    }

    public class CustomerGroup
    {
        public int Id { get; set; }
        public int Code { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    }

    public class ImporterGroup
    {
        public int Id { get; set; }
        public int Code { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public ICollection<Importer> Importers { get; set; } = new List<Importer>();
    }

    public class GoodModel
    {
        public int Id { get; set; }
        public int Code { get; set; }
        public string ModelName { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public ICollection<Good> Goods { get; set; } = new List<Good>();
    }

    public class Market
    {
        public int Id { get; set; }
        public int Code { get; set; }
        public string MarketName { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public ICollection<Good> Goods { get; set; } = new List<Good>();
    }

    public class GoodColor
    {
        public int Id { get; set; }
        public int Code { get; set; }
        public string ColorName { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public ICollection<Good> Goods { get; set; } = new List<Good>();
    }

    public class Car
    {
        public int Id { get; set; }
        public int Code { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? TelMobile { get; set; }
        public string? Address { get; set; }
        public string? Notes { get; set; }
    }

    public class Bank
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string? AccountNo { get; set; }
        public string? Notes { get; set; }
    }

    public class Cost
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string CostName { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class Trader
    {
        public int Id { get; set; }
        public int Code { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Tel { get; set; }
        public string? Mobile { get; set; }
        public string? Fax { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
    }

    public class Place
    {
        public int Id { get; set; }
        public string PlaceName { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
