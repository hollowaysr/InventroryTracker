using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

class TestConnection
{
    static async Task Main(string[] args)
    {
        // Connection string to TestApps database
        string connectionString = "Server=heccdbs.database.windows.net,1433;Database=TestApps;User ID=hecc_admin;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        
        Console.WriteLine("Testing connection to TestApps database...");
        Console.WriteLine($"Connection string: {connectionString.Replace("YOUR_PASSWORD", "***")}");
        
        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                Console.WriteLine("✅ Connection successful!");
                
                // Test a simple query
                using (var command = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        Console.WriteLine("\nTables in TestApps database:");
                        while (await reader.ReadAsync())
                        {
                            Console.WriteLine($"- {reader.GetString(0)}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Connection failed: {ex.Message}");
            Console.WriteLine($"Exception type: {ex.GetType().Name}");
        }
        
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }
}
