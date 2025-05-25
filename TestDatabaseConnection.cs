using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

namespace InventoryTracker.Test
{
    public class TestDatabaseConnection
    {
        public static async Task Main(string[] args)
        {
            // Note: Replace YOUR_PASSWORD with actual password
            var connectionString = "Server=heccdbs.database.windows.net,1433;Database=TestApps;User ID=hecc_admin;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;MultipleActiveResultSets=true";
            
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                
                Console.WriteLine("✅ Successfully connected to TestApps database!");
                
                // Test basic table access
                using var command = new SqlCommand("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME IN ('CustomerList', 'RfidTag')", connection);
                var tableCount = await command.ExecuteScalarAsync();
                
                Console.WriteLine($"✅ Found {tableCount} expected tables in TestApps database");
                
                // Test CustomerList table structure
                using var customerListCommand = new SqlCommand(@"
                    SELECT COLUMN_NAME, DATA_TYPE 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'CustomerList' 
                    ORDER BY ORDINAL_POSITION", connection);
                
                using var reader = await customerListCommand.ExecuteReaderAsync();
                Console.WriteLine("\n📋 CustomerList table structure:");
                while (await reader.ReadAsync())
                {
                    Console.WriteLine($"  - {reader["COLUMN_NAME"]} ({reader["DATA_TYPE"]})");
                }
                reader.Close();
                
                // Test RfidTag table structure  
                using var rfidTagCommand = new SqlCommand(@"
                    SELECT COLUMN_NAME, DATA_TYPE 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'RfidTag' 
                    ORDER BY ORDINAL_POSITION", connection);
                
                using var rfidReader = await rfidTagCommand.ExecuteReaderAsync();
                Console.WriteLine("\n🏷️  RfidTag table structure:");
                while (await rfidReader.ReadAsync())
                {
                    Console.WriteLine($"  - {rfidReader["COLUMN_NAME"]} ({rfidReader["DATA_TYPE"]})");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Database connection failed: {ex.Message}");
                Console.WriteLine("\n🔧 Please ensure:");
                Console.WriteLine("1. Replace YOUR_PASSWORD with actual database password");
                Console.WriteLine("2. Firewall allows connection from your IP");
                Console.WriteLine("3. Database credentials are correct");
                Environment.Exit(1);
            }
        }
    }
}
