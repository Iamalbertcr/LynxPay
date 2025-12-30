using LynxPay.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace LynxPay.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/ventas")]
    public class VentasController : ControllerBase
    {
        private readonly ClientConnectionService _conn;

        public VentasController(ClientConnectionService conn)
        {
            _conn = conn;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            if (User?.Identity == null || !User.Identity.IsAuthenticated)
                return Unauthorized();

            await using var conn = _conn.GetConnection(User);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(
                "SELECT * FROM ventas", conn);

            await using var reader = await cmd.ExecuteReaderAsync();
            var list = new List<object>();

            while (await reader.ReadAsync())
                list.Add(reader.GetInt32(0));

            return Ok(list);
        }
    }
}
