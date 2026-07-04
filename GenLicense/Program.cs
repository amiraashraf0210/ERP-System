using System;
using System.Security.Cryptography;
using System.Text;

string SecretKey   = "ERP@L1c3ns3#S3cr3t$2025!";
string companyName = "My Company";
DateTime expiry    = DateTime.Today.AddYears(2);
int maxUsers       = 10;

string payload    = $"{companyName.Trim().ToUpper()}|{expiry:yyyyMMdd}|{maxUsers}";
using var hmac    = new HMACSHA256(Encoding.UTF8.GetBytes(SecretKey));
string hash       = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload))).ToLower();
string shortHash  = hash[..16].ToUpper();
string key        = $"{shortHash[..4]}-{shortHash[4..8]}-{shortHash[8..12]}-{shortHash[12..16]}";
string encoded    = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload))
                    .Replace("+","-").Replace("/","_").Replace("=","");
string fullKey    = $"{key}#{encoded}";

Console.WriteLine("=== License Key ===");
Console.WriteLine(fullKey);
Console.WriteLine("===================");
Console.WriteLine($"الشركة: {companyName}");
Console.WriteLine($"الانتهاء: {expiry:dd/MM/yyyy}");
Console.WriteLine($"المستخدمين: {maxUsers}");
