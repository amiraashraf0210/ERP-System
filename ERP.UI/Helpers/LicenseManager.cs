using System.Security.Cryptography;
using System.Text;

namespace ERP.UI.Helpers
{
    /// <summary>
    /// Handles license key generation, validation, and persistence.
    /// Keys are HMAC-SHA256 signed to prevent forgery.
    /// </summary>
    public static class LicenseManager
    {
        // Stored in ProgramData so all Windows users on the same machine share one license file.
        // The file is encrypted with LocalMachine DPAPI, so it cannot be copied to another PC.
        private static string LicensePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "ERPSystem", "license.dat");

        // !! Keep this secret — changing it invalidates all existing keys !!
        private const string SecretKey = "ERP@L1c3ns3#S3cr3t$2025!";

        /// <summary>
        /// Generates a signed license key for a company.
        /// Pass null for expiryDate to create a permanent (lifetime) license.
        /// </summary>
        public static string GenerateKey(string companyName, DateTime? expiryDate = null, int maxUsers = 5)
        {
            // "99991231" is the sentinel value for a permanent license
            string expStr  = expiryDate.HasValue ? expiryDate.Value.ToString("yyyyMMdd") : "99991231";
            string payload = $"{companyName.Trim().ToUpper()}|{expStr}|{maxUsers}";
            string hash    = ComputeHash(payload);
            string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload))
                             .Replace("+", "-").Replace("/", "_").Replace("=", "");
            string shortHash = hash[..16].ToUpper();
            string key = $"{shortHash[..4]}-{shortHash[4..8]}-{shortHash[8..12]}-{shortHash[12..16]}";
            return $"{key}#{encoded}";
        }

        /// <summary>
        /// Validates a full license key string.
        /// Returns null if the key is invalid or tampered with.
        /// </summary>
        public static LicenseInfo? ValidateKey(string fullKey)
        {
            try
            {
                var parts = fullKey.Split('#');
                if (parts.Length != 2) return null;

                string key     = parts[0];
                string encoded = parts[1];
                string payload = Encoding.UTF8.GetString(
                    Convert.FromBase64String(encoded.Replace("-", "+").Replace("_", "/") + "=="));

                // Verify HMAC signature
                string expectedHash = ComputeHash(payload)[..16].ToUpper();
                string expectedKey  = $"{expectedHash[..4]}-{expectedHash[4..8]}-{expectedHash[8..12]}-{expectedHash[12..16]}";
                if (key != expectedKey) return null;

                // Decode payload fields: COMPANY|YYYYMMDD|MAXUSERS
                var dataParts = payload.Split('|');
                if (dataParts.Length != 3) return null;

                return new LicenseInfo
                {
                    CompanyName = dataParts[0],
                    ExpiryDate  = DateTime.ParseExact(dataParts[1], "yyyyMMdd", null),
                    MaxUsers    = int.Parse(dataParts[2]),
                    IsValid     = true
                };
            }
            catch { return null; }
        }

        /// <summary>
        /// Encrypts and persists the license key to the user's AppData folder.
        /// Uses DPAPI (LocalMachine scope) so the file is machine-bound.
        /// </summary>
        public static void SaveLicense(string fullKey)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LicensePath)!);
            var data      = Encoding.UTF8.GetBytes(fullKey);
            var encrypted = ProtectedData.Protect(data,
                Encoding.UTF8.GetBytes(SecretKey[..8]), DataProtectionScope.LocalMachine);
            File.WriteAllBytes(LicensePath, encrypted);
        }

        /// <summary>
        /// Loads and decrypts the saved license, then validates it.
        /// Returns null if no license file exists or decryption fails.
        /// </summary>
        public static LicenseInfo? LoadLicense()
        {
            if (!File.Exists(LicensePath)) return null;
            try
            {
                var encrypted = File.ReadAllBytes(LicensePath);
                var data      = ProtectedData.Unprotect(encrypted,
                    Encoding.UTF8.GetBytes(SecretKey[..8]), DataProtectionScope.LocalMachine);
                return ValidateKey(Encoding.UTF8.GetString(data));
            }
            catch { return null; }
        }

        /// <summary>True when a valid, non-expired license is present on this machine.</summary>
        public static bool IsLicensed => LoadLicense() is { IsValid: true } lic && !lic.IsExpired;

        private static string ComputeHash(string input)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SecretKey));
            return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(input))).ToLower();
        }
    }

    public class LicenseInfo
    {
        public string   CompanyName { get; set; } = "";
        public DateTime ExpiryDate  { get; set; }
        public int      MaxUsers    { get; set; }
        public bool     IsValid     { get; set; }

        /// <summary>True for time-limited licenses that have passed their expiry date.</summary>
        public bool IsPermanent => ExpiryDate.Year == 9999;
        public bool IsExpired   => !IsPermanent && ExpiryDate < DateTime.Today;
        public int  DaysLeft    => IsPermanent ? int.MaxValue : (ExpiryDate - DateTime.Today).Days;
    }
}
