using System.Security.Cryptography;
using System.Text;

namespace ERP.UI.Helpers
{
    /// <summary>
    /// Manages the database connection string.
    /// The string is encrypted with Windows DPAPI and stored in AppData,
    /// keeping it invisible to end users.
    /// </summary>
    public static class ConnectionManager
    {
        // Stored at %AppData%\ERPSystem\conn.dat — encrypted, not human-readable
        private static string ConfigPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ERPSystem", "conn.dat");

        private static readonly byte[] _entropy = Encoding.UTF8.GetBytes("ERP$y$t3m@2025#Secure");

        /// <summary>Returns true when a saved connection string exists on disk.</summary>
        public static bool IsConfigured => File.Exists(ConfigPath);

        /// <summary>Decrypts and returns the saved connection string, or null if not found.</summary>
        public static string? Load()
        {
            if (!IsConfigured) return null;
            try
            {
                var encrypted = File.ReadAllBytes(ConfigPath);
                var decrypted = ProtectedData.Unprotect(encrypted, _entropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(decrypted);
            }
            catch { return null; }
        }

        /// <summary>Encrypts and saves the connection string to disk.</summary>
        public static void Save(string connectionString)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
            var data      = Encoding.UTF8.GetBytes(connectionString);
            var encrypted = ProtectedData.Protect(data, _entropy, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(ConfigPath, encrypted);
        }

        /// <summary>Deletes the saved connection string, forcing setup on next launch.</summary>
        public static void Reset()
        {
            if (File.Exists(ConfigPath)) File.Delete(ConfigPath);
        }

        /// <summary>
        /// Builds a SQL Server connection string from its components.
        /// Use windowsAuth=true for trusted connections; supply user/password for SQL auth.
        /// </summary>
        public static string Build(string server, string database, bool windowsAuth,
                                   string user = "", string password = "")
        {
            return windowsAuth
                ? $"Server={server};Database={database};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;"
                : $"Server={server};Database={database};User Id={user};Password={password};MultipleActiveResultSets=true;TrustServerCertificate=True;";
        }

        /// <summary>Opens a test connection and returns success status with a message.</summary>
        public static (bool success, string message) TestConnection(string connStr)
        {
            try
            {
                using var conn = new Microsoft.Data.SqlClient.SqlConnection(connStr);
                conn.Open();
                return (true, "✅ تم الاتصال بنجاح");
            }
            catch (Exception ex)
            {
                return (false, $"❌ فشل الاتصال: {ex.Message}");
            }
        }
    }
}
