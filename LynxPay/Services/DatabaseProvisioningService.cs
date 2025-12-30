//Este servicio conecta a POSTGRES, ejecuta CREATE DATABASE y luego devuelve el nombre
using Npgsql;

namespace LynxPay.Services
{
    public class DatabaseProvisioningService
    {
        private readonly IConfiguration _config;

        public DatabaseProvisioningService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<string> CreateClientDatabaseAsync(string businessName)
        {
            // 1️⃣ Normalizamos el nombre
            var dbName = "lynxpay_" + businessName
                .ToLower()
                .Replace(" ", "_")
                .Replace("-", "_");

            // 2️⃣ Conexión al servidor (postgres)
            var masterConn =
                _config.GetConnectionString("MasterDb");

            await using var conn = new NpgsqlConnection(masterConn);
            await conn.OpenAsync();

            // 3️⃣ Verificar si ya existe
            var existsCmd = new NpgsqlCommand(
                "SELECT 1 FROM pg_database WHERE datname = @name", conn);
            existsCmd.Parameters.AddWithValue("name", dbName);

            var exists = await existsCmd.ExecuteScalarAsync();
            if (exists != null)
                return dbName;

            // 4️⃣ Crear la base
            var createCmd = new NpgsqlCommand(
                $"CREATE DATABASE \"{dbName}\"", conn);

            await createCmd.ExecuteNonQueryAsync();

            return dbName;
        }
    }
}
