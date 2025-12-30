using System.Security.Claims;
using System.Text.RegularExpressions;
using Npgsql;

namespace LynxPay.Services
{
    public interface IClientConnectionService
    {
        /// <summary>
        /// Crea y abre una conexión a la base de datos del cliente indicada en la claim "db".
        /// El llamador es responsable de disponer la conexión.
        /// </summary>
        /// <param name="user">ClaimsPrincipal que contiene la claim "db".</param>
        /// <returns>NpgsqlConnection abierta.</returns>
        Task<NpgsqlConnection> GetOpenConnectionAsync(ClaimsPrincipal user);
    }

    public class ClientConnectionService : IClientConnectionService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<ClientConnectionService> _logger;
        private readonly string _centralConnectionString;
        private static readonly Regex ValidDbName = new("^[a-z0-9_]+$", RegexOptions.Compiled);

        public ClientConnectionService(IConfiguration config, ILogger<ClientConnectionService> logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _centralConnectionString = _config.GetConnectionString("CentralDb")
                ?? throw new InvalidOperationException("No se ha encontrado la cadena de conexión 'CentralDb' en la configuración.");
        }

        public async Task<NpgsqlConnection> GetOpenConnectionAsync(ClaimsPrincipal user)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            var dbClaim = user.FindFirst("db")?.Value;
            if (string.IsNullOrWhiteSpace(dbClaim))
                throw new ArgumentException("La identidad no contiene la claim 'db' o está vacía.", nameof(user));

            dbClaim = dbClaim.Trim().ToLowerInvariant();

            if (!ValidDbName.IsMatch(dbClaim))
                throw new ArgumentException("Nombre de base de datos inválido. Sólo se permiten caracteres a-z, 0-9 y guion bajo.", nameof(user));

            var builder = new NpgsqlConnectionStringBuilder(_centralConnectionString)
            {
                Database = dbClaim
            };

            var conn = new NpgsqlConnection(builder.ConnectionString);

            try
            {
                _logger.LogDebug("Abriendo conexión a la base de datos del cliente: {Database}", dbClaim);
                await conn.OpenAsync();
                return conn;
            }
            catch
            {
                conn.Dispose();
                _logger.LogError("No se pudo abrir la conexión para la base de datos {Database}.", dbClaim);
                throw;
            }
        }

        internal NpgsqlConnection? GetConnection(ClaimsPrincipal user)
        {
            throw new NotImplementedException();
        }
    }
}
