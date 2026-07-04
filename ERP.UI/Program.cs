using ERP.Data;
using ERP.UI.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.UI
{
    internal static class Program
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        [STAThread]
        static void Main()
        {
            // Global exception handlers to capture silent crashes
            Application.ThreadException += (s, e) =>
            {
                string log = $"[{DateTime.Now}] ThreadException: {e.Exception}\n";
                File.AppendAllText(@"d:\MySystem\erp_error.log", log);
                MessageBox.Show(e.Exception.ToString(), "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                string log = $"[{DateTime.Now}] UnhandledException: {e.ExceptionObject}\n";
                File.AppendAllText(@"d:\MySystem\erp_error.log", log);
            };

            ApplicationConfiguration.Initialize();

            // Show database setup screen on first run (no saved connection string)
            if (!ConnectionManager.IsConfigured)
            {
                var setup = new Forms.SetupConnectionForm();
                if (setup.ShowDialog() != DialogResult.OK)
                {
                    // Fall back to LocalDB default if user dismisses setup
                    ConnectionManager.Save(ConnectionManager.Build(
                        "(localdb)\\MSSQLLocalDB", "ERPSystem", true));
                }
            }

            // Enforce license activation before proceeding
            if (!LicenseManager.IsLicensed)
            {
                var licForm = new Forms.LicenseActivationForm();
                if (licForm.ShowDialog() != DialogResult.OK)
                {
                    MessageBox.Show("البرنامج غير مفعّل. يرجى التواصل مع المورد.",
                        "غير مفعّل", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                // Warn user if license expires within 14 days
                var lic = LicenseManager.LoadLicense();
                if (lic != null && lic.DaysLeft <= 14 && lic.DaysLeft >= 0)
                    MessageBox.Show(
                        $"⚠ تنبيه: سينتهي ترخيصك خلال {lic.DaysLeft} يوم ({lic.ExpiryDate:dd/MM/yyyy})\nيرجى التجديد.",
                        "تجديد الترخيص", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            string connStr = ConnectionManager.Load()
                ?? ConnectionManager.Build("(localdb)\\MSSQLLocalDB", "ERPSystem", true);

            var services = new ServiceCollection();
            ConfigureServices(services, connStr);
            ServiceProvider = services.BuildServiceProvider();

            // Ensure the database schema exists and seed required lookup data
            using (var scope = ServiceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Check if production tables and monthly inventories exist, if not recreate database
                try
                {
                    var hasProductionTables = db.ProductionRecipes.Any();
                    var hasInventoryTable = db.MonthlyInventories.Any(); // check new table
                }
                catch
                {
                    // Drop and recreate database if any structure is missing
                    db.Database.EnsureDeleted();
                }

                db.Database.EnsureCreated();
                Forms.DatabaseSetup.SeedInitialData(db);
                // Demo seeders commented out to start with a clean database
                // Forms.SeedDemoData.Seed(db);
                // Forms.SeedDemoData.SeedMissing(db);
                // Forms.SeedDemoData.SeedProductionOnly(db);
            }

            Application.Run(new Forms.LoginForm());
        }

        private static void ConfigureServices(ServiceCollection services, string connStr)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connStr),
                ServiceLifetime.Transient);
        }
    }
}
