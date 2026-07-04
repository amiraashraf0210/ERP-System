using ERP.Core.Models;
using ERP.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.UI.Forms
{
    /// <summary>
    /// First-run setup: creates admin user and company data
    /// </summary>
    public static class DatabaseSetup
    {
        public static void SeedInitialData(AppDbContext db)
        {
            // Create admin user if no users exist
            if (!db.Users.Any())
            {
                db.Users.Add(new User
                {
                    Name = "admin",
                    Pass = "admin",
                    Permissions = BuildAllPermissions()
                });
            }

            // Create default company info
            if (!db.AppData.Any())
            {
                db.AppData.Add(new AppData
                {
                    Name = "الشركة",
                    Active = "1",
                    Tel = "",
                    Fax = "",
                    Address = ""
                });
            }

            // Default store
            if (!db.Stores.Any())
            {
                db.Stores.Add(new Store { Code = 1, StoreName = "المخزن الرئيسي" });
            }

            db.SaveChanges();
        }

        private static string BuildAllPermissions()
        {
            // All 120 permissions = true by default for admin
            var perms = new Dictionary<string, bool>();
            for (int i = 0; i <= 120; i++)
                perms[$"a{i}"] = true;
            return System.Text.Json.JsonSerializer.Serialize(perms);
        }
    }
}
