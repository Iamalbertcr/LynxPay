//cuando el cliente se registra se crea una base de datos para el cliente y se guarda aqui
using LynxPay.Services;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace LynxPay.Controllers
{
    [ApiController]
    [Route("api/clients")]
    public class ClientsController : ControllerBase
    {
        private readonly DatabaseProvisioningService _dbService;
        private readonly IConfiguration _config;

        public ClientsController(
            DatabaseProvisioningService dbService,
            IConfiguration config)
        {
            _dbService = dbService;
            _config = config;
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterClientRequest request)
        {
            // 1️⃣ Crear base del cliente
            var dbName =
                await _dbService.CreateClientDatabaseAsync(request.BusinessName);

            // 2️⃣ Guardar en central
            var centralConn =
                _config.GetConnectionString("CentralDb");

            await using var conn = new NpgsqlConnection(centralConn);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(
                "INSERT INTO clients (name, database_name) VALUES (@n,@d)",
                conn);

            cmd.Parameters.AddWithValue("n", request.BusinessName);
            cmd.Parameters.AddWithValue("d", dbName);

            await cmd.ExecuteNonQueryAsync();

            return Ok(new
            {
                message = "Comercio registrado",
                database = dbName
            });
        }
    }

    public record RegisterClientRequest(string BusinessName);
}
