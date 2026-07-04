using ERP.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace ERP.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Company
        public DbSet<AppData> AppData => Set<AppData>();

        // Security
        public DbSet<User> Users => Set<User>();

        // Customers & Importers
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<CustomerGroup> CustomerGroups => Set<CustomerGroup>();
        public DbSet<Importer> Importers => Set<Importer>();
        public DbSet<ImporterGroup> ImporterGroups => Set<ImporterGroup>();

        // Goods
        public DbSet<Good> Goods => Set<Good>();
        public DbSet<GoodGroup> GoodGroups => Set<GoodGroup>();
        public DbSet<GoodModel> GoodModels => Set<GoodModel>();
        public DbSet<Market> Markets => Set<Market>();
        public DbSet<GoodColor> Colors => Set<GoodColor>();
        public DbSet<Unit> Units => Set<Unit>();

        // Bills
        public DbSet<SellBill> SellBills => Set<SellBill>();
        public DbSet<BuyBill> BuyBills => Set<BuyBill>();
        public DbSet<CustomerInstallment> CustomerInstallments => Set<CustomerInstallment>();
        public DbSet<ImporterInstallment> ImporterInstallments => Set<ImporterInstallment>();

        // Stores & Movements
        public DbSet<Store> Stores => Set<Store>();
        public DbSet<StoreMovement> StoreMovements => Set<StoreMovement>();
        public DbSet<Movement> Movements => Set<Movement>();

        // Finance
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Journal> Journal => Set<Journal>();
        public DbSet<RootAccount> RootAccounts => Set<RootAccount>();
        public DbSet<Root> Roots => Set<Root>();
        public DbSet<BoxTransaction> BoxTransactions => Set<BoxTransaction>();
        public DbSet<Expense> Expenses => Set<Expense>();
        public DbSet<Income> Incomes => Set<Income>();
        public DbSet<Bank> Banks => Set<Bank>();
        public DbSet<Cost> Costs => Set<Cost>();

        // Orders & Delivery
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
        public DbSet<DeliveryBill> DeliveryBills => Set<DeliveryBill>();
        public DbSet<DeliveryMovement> DeliveryMovements => Set<DeliveryMovement>();

        // HR
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<PayrollRecord> PayrollRecords => Set<PayrollRecord>();
        public DbSet<EmployeeAttendance> EmployeeAttendances => Set<EmployeeAttendance>();

        // Lookups
        public DbSet<Car> Cars => Set<Car>();
        public DbSet<Trader> Traders => Set<Trader>();
        public DbSet<Place> Places => Set<Place>();
        public DbSet<FiscalYear> FiscalYears => Set<FiscalYear>();
        public DbSet<BankLoan> BankLoans => Set<BankLoan>();
        public DbSet<LoanPayment> LoanPayments => Set<LoanPayment>();

        // Production
        public DbSet<RawMaterial> RawMaterials => Set<RawMaterial>();
        public DbSet<ProductionRecipe> ProductionRecipes => Set<ProductionRecipe>();
        public DbSet<RecipeItem> RecipeItems => Set<RecipeItem>();
        public DbSet<ProductionOrder> ProductionOrders => Set<ProductionOrder>();
        public DbSet<ProductionMaterial> ProductionMaterials => Set<ProductionMaterial>();
        public DbSet<WasteRecord> WasteRecords => Set<WasteRecord>();
        public DbSet<RestockingOrder> RestockingOrders => Set<RestockingOrder>();
        public DbSet<RawMaterialMovement> RawMaterialMovements => Set<RawMaterialMovement>();
        public DbSet<MonthlyInventory> MonthlyInventories => Set<MonthlyInventory>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // AppData
            modelBuilder.Entity<AppData>().ToTable("AppData");

            // User
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<User>().Property(u => u.Permissions).HasColumnType("nvarchar(max)");

            // Customer
            modelBuilder.Entity<Customer>().ToTable("Customers");
            modelBuilder.Entity<Customer>().HasOne(c => c.Group)
                .WithMany(g => g.Customers).HasForeignKey(c => c.GroupId);

            // Importer
            modelBuilder.Entity<Importer>().ToTable("Importers");
            modelBuilder.Entity<Importer>().HasOne(i => i.Group)
                .WithMany(g => g.Importers).HasForeignKey(i => i.GroupId);

            // Good
            modelBuilder.Entity<Good>().ToTable("Goods");
            modelBuilder.Entity<Good>().HasOne(g => g.Importer)
                .WithMany(i => i.Goods).HasForeignKey(g => g.ImporterId);
            modelBuilder.Entity<Good>().HasOne(g => g.Group)
                .WithMany(gg => gg.Goods).HasForeignKey(g => g.GroupId);
            modelBuilder.Entity<Good>().HasOne(g => g.Model)
                .WithMany(m => m.Goods).HasForeignKey(g => g.ModelId);
            modelBuilder.Entity<Good>().HasOne(g => g.Market)
                .WithMany(m => m.Goods).HasForeignKey(g => g.MarketId);
            modelBuilder.Entity<Good>().HasOne(g => g.Color)
                .WithMany(c => c.Goods).HasForeignKey(g => g.ColorId);
            modelBuilder.Entity<Good>().HasOne(g => g.Unit)
                .WithMany(u => u.Goods).HasForeignKey(g => g.UnitId);
            modelBuilder.Entity<Good>().HasOne(g => g.Store)
                .WithMany(s => s.Goods).HasForeignKey(g => g.StoreId);

            // SellBill
            modelBuilder.Entity<SellBill>().ToTable("SellBills");
            modelBuilder.Entity<SellBill>().HasOne(b => b.Customer)
                .WithMany(c => c.SellBills).HasForeignKey(b => b.CustomerId);
            modelBuilder.Entity<SellBill>().HasOne(b => b.FiscalYear)
                .WithMany().HasForeignKey(b => b.FiscalYearId).OnDelete(DeleteBehavior.Restrict);

            // BuyBill
            modelBuilder.Entity<BuyBill>().ToTable("BuyBills");
            modelBuilder.Entity<BuyBill>().HasOne(b => b.Importer)
                .WithMany(i => i.BuyBills).HasForeignKey(b => b.ImporterId);
            modelBuilder.Entity<BuyBill>().HasOne(b => b.FiscalYear)
                .WithMany().HasForeignKey(b => b.FiscalYearId).OnDelete(DeleteBehavior.Restrict);

            // CustomerInstallment (bill details)
            modelBuilder.Entity<CustomerInstallment>().ToTable("CustomerInstallments");
            modelBuilder.Entity<CustomerInstallment>().HasOne(ci => ci.Bill)
                .WithMany(b => b.Items).HasForeignKey(ci => ci.BillId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CustomerInstallment>().HasOne(ci => ci.Customer)
                .WithMany(c => c.Installments).HasForeignKey(ci => ci.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // ImporterInstallment (buy bill details)
            modelBuilder.Entity<ImporterInstallment>().ToTable("ImporterInstallments");
            modelBuilder.Entity<ImporterInstallment>().HasOne(ii => ii.Bill)
                .WithMany(b => b.Items).HasForeignKey(ii => ii.BillId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ImporterInstallment>().HasOne(ii => ii.Importer)
                .WithMany(i => i.Installments).HasForeignKey(ii => ii.ImporterId)
                .OnDelete(DeleteBehavior.Restrict);

            // StoreMovement
            modelBuilder.Entity<StoreMovement>().ToTable("StoreMovements");
            modelBuilder.Entity<StoreMovement>().HasOne(sm => sm.StoreFromNav)
                .WithMany().HasForeignKey(sm => sm.StoreFrom).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<StoreMovement>().HasOne(sm => sm.StoreToNav)
                .WithMany(s => s.StoreMovements).HasForeignKey(sm => sm.StoreTo).OnDelete(DeleteBehavior.Restrict);

            // Movement
            modelBuilder.Entity<Movement>().ToTable("Movements");
            modelBuilder.Entity<Movement>().HasOne(m => m.Good)
                .WithMany(g => g.Movements).HasForeignKey(m => m.GoodId);
            modelBuilder.Entity<Movement>().HasOne(m => m.FiscalYear)
                .WithMany().HasForeignKey(m => m.FiscalYearId).OnDelete(DeleteBehavior.Restrict);

            // Order - Code is PK (not auto-incremented)
            modelBuilder.Entity<Order>().ToTable("Orders").HasKey(o => o.Code);
            modelBuilder.Entity<Order>().Property(o => o.Code).ValueGeneratedNever();
            modelBuilder.Entity<OrderDetail>().ToTable("OrderDetails");
            modelBuilder.Entity<OrderDetail>().HasOne(od => od.Order)
                .WithMany(o => o.Details).HasForeignKey(od => od.OrderCode);

            // DeliveryBill
            modelBuilder.Entity<DeliveryBill>().ToTable("DeliveryBills");
            modelBuilder.Entity<DeliveryMovement>().ToTable("DeliveryMovements");
            modelBuilder.Entity<DeliveryMovement>().HasOne(dm => dm.Good)
                .WithMany().HasForeignKey(dm => dm.GoodId);

            // Finance tables
            modelBuilder.Entity<Journal>().ToTable("Journal");
            modelBuilder.Entity<BoxTransaction>().ToTable("BoxTransactions");
            modelBuilder.Entity<BoxTransaction>().HasOne(bt => bt.FiscalYear)
                .WithMany().HasForeignKey(bt => bt.FiscalYearId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Expense>().ToTable("Expenses");
            modelBuilder.Entity<Expense>().HasOne(e => e.Cost).WithMany().HasForeignKey(e => e.CostId).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Expense>().HasOne(e => e.FiscalYear)
                .WithMany().HasForeignKey(e => e.FiscalYearId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Income>().ToTable("Incomes");
            modelBuilder.Entity<Income>().HasOne(i => i.Cost).WithMany().HasForeignKey(i => i.CostId).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Income>().HasOne(i => i.FiscalYear)
                .WithMany().HasForeignKey(i => i.FiscalYearId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Cost>().ToTable("Costs");

            // Production tables
            modelBuilder.Entity<RawMaterial>().ToTable("RawMaterials");
            modelBuilder.Entity<RawMaterial>().HasOne(r => r.Unit)
                .WithMany().HasForeignKey(r => r.UnitId).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<RawMaterial>().HasOne(r => r.Store)
                .WithMany().HasForeignKey(r => r.StoreId).OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ProductionRecipe>().ToTable("ProductionRecipes");
            modelBuilder.Entity<ProductionRecipe>().HasOne(pr => pr.Good)
                .WithMany().HasForeignKey(pr => pr.GoodId);

            modelBuilder.Entity<RecipeItem>().ToTable("RecipeItems");
            modelBuilder.Entity<RecipeItem>().HasOne(ri => ri.Recipe)
                .WithMany(pr => pr.RecipeItems).HasForeignKey(ri => ri.RecipeId);
            modelBuilder.Entity<RecipeItem>().HasOne(ri => ri.RawMaterial)
                .WithMany(rm => rm.RecipeItems).HasForeignKey(ri => ri.RawMaterialId);

            modelBuilder.Entity<ProductionOrder>().ToTable("ProductionOrders");
            modelBuilder.Entity<ProductionOrder>().HasOne(po => po.Customer)
                .WithMany().HasForeignKey(po => po.CustomerId).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<ProductionOrder>().HasOne(po => po.CreatedBy)
                .WithMany().HasForeignKey(po => po.CreatedByUserId).OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ProductionMaterial>().ToTable("ProductionMaterials");
            modelBuilder.Entity<ProductionMaterial>().HasOne(pm => pm.ProductionOrder)
                .WithMany(po => po.Materials).HasForeignKey(pm => pm.ProductionOrderId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ProductionMaterial>().HasOne(pm => pm.RawMaterial)
                .WithMany().HasForeignKey(pm => pm.RawMaterialId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WasteRecord>().ToTable("WasteRecords");
            modelBuilder.Entity<WasteRecord>().HasOne(wr => wr.RawMaterial)
                .WithMany().HasForeignKey(wr => wr.RawMaterialId);

            modelBuilder.Entity<RestockingOrder>().ToTable("RestockingOrders");
            modelBuilder.Entity<RestockingOrder>().HasOne(ro => ro.Good)
                .WithMany().HasForeignKey(ro => ro.GoodId);
            modelBuilder.Entity<RestockingOrder>().HasOne(ro => ro.ProcessedBy)
                .WithMany().HasForeignKey(ro => ro.ProcessedByUserId).OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<RawMaterialMovement>().ToTable("RawMaterialMovements");
            modelBuilder.Entity<RawMaterialMovement>().HasOne(rmm => rmm.RawMaterial)
                .WithMany(rm => rm.Movements).HasForeignKey(rmm => rmm.RawMaterialId);

            modelBuilder.Entity<FiscalYear>().ToTable("FiscalYears");

            // HR - Payroll
            modelBuilder.Entity<PayrollRecord>().ToTable("PayrollRecords");
            modelBuilder.Entity<PayrollRecord>().HasOne(p => p.Employee).WithMany().HasForeignKey(p => p.EmployeeId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PayrollRecord>().HasOne(p => p.BoxTransaction).WithMany().HasForeignKey(p => p.BoxTransactionId).OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<EmployeeAttendance>().ToTable("EmployeeAttendances");
            modelBuilder.Entity<EmployeeAttendance>().HasOne(a => a.Employee).WithMany().HasForeignKey(a => a.EmployeeId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BankLoan>().ToTable("BankLoans");
            modelBuilder.Entity<BankLoan>().HasOne(l => l.Bank).WithMany().HasForeignKey(l => l.BankId);
            modelBuilder.Entity<LoanPayment>().ToTable("LoanPayments");
            modelBuilder.Entity<LoanPayment>().HasOne(p => p.Loan).WithMany(l => l.Payments).HasForeignKey(p => p.LoanId);
            modelBuilder.Entity<LoanPayment>().HasOne(p => p.BoxTransaction).WithMany().HasForeignKey(p => p.BoxTransactionId).OnDelete(DeleteBehavior.SetNull);

            // MonthlyInventory
            modelBuilder.Entity<MonthlyInventory>().ToTable("MonthlyInventories");
            modelBuilder.Entity<MonthlyInventory>().HasOne(mi => mi.Good)
                .WithMany().HasForeignKey(mi => mi.GoodId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
