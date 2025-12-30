using System.Security.Claims;
using Npgsql;

namespace LynxPay.Services
{
    public class ClientConnectionService
    {
        public NpgsqlConnection GetConnection(ClaimsPrincipal user)
        {
            var dbName =
                user.Claims.First(c => c.Type == "db").Value;

            var connStr =
                $"Host=xxx.neon.tech;Database={dbName};Username=xxx;Password=xxx";

            return new NpgsqlConnection(connStr);
        }
    }
}
