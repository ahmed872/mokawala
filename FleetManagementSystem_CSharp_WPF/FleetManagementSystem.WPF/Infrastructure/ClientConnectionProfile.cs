using MySqlConnector;

namespace FleetManagementSystem.WPF.Infrastructure;

public sealed class ClientConnectionProfile
{
    public string Provider { get; set; } = "MySql";
    public string Server { get; set; } = "192.168.1.10";
    public int Port { get; set; } = 3306;
    public string Database { get; set; } = "FleetManagementDB";
    public string Username { get; set; } = "fleet_user";
    public string Password { get; set; } = string.Empty;
    public bool UseSsl { get; set; }
    public string SqlitePath { get; set; } = "fleet-management.preview.db";

    public bool IsMySql => Provider.Equals("MySql", StringComparison.OrdinalIgnoreCase);
    public bool IsSqlite => Provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase);

    public string BuildConnectionString()
    {
        if (IsSqlite)
        {
            var sqlitePath = string.IsNullOrWhiteSpace(SqlitePath) ? "fleet-management.preview.db" : SqlitePath.Trim();
            return $"Data Source={sqlitePath}";
        }

        var builder = new MySqlConnectionStringBuilder
        {
            Server = string.IsNullOrWhiteSpace(Server) ? "127.0.0.1" : Server.Trim(),
            Port = (uint)(Port <= 0 ? 3306 : Port),
            Database = string.IsNullOrWhiteSpace(Database) ? "FleetManagementDB" : Database.Trim(),
            UserID = string.IsNullOrWhiteSpace(Username) ? "root" : Username.Trim(),
            Password = Password ?? string.Empty,
            CharacterSet = "utf8mb4",
            SslMode = UseSsl ? MySqlSslMode.Preferred : MySqlSslMode.None,
            AllowUserVariables = true
        };

        return builder.ConnectionString;
    }

    public string ToDisplayText()
    {
        return IsSqlite
            ? $"SQLite | {SqlitePath}"
            : $"MySQL | {Server}:{Port} / {Database}";
    }

    public ClientConnectionProfile Clone()
    {
        return new ClientConnectionProfile
        {
            Provider = Provider,
            Server = Server,
            Port = Port,
            Database = Database,
            Username = Username,
            Password = Password,
            UseSsl = UseSsl,
            SqlitePath = SqlitePath
        };
    }

    public static ClientConnectionProfile FromConnectionString(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return new ClientConnectionProfile();
        }

        if (connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase) ||
            connectionString.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
        {
            var sqlitePath = connectionString.Replace("Data Source=", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();
            return new ClientConnectionProfile
            {
                Provider = "Sqlite",
                SqlitePath = string.IsNullOrWhiteSpace(sqlitePath) ? "fleet-management.preview.db" : sqlitePath
            };
        }

        try
        {
            var builder = new MySqlConnectionStringBuilder(connectionString);
            return new ClientConnectionProfile
            {
                Provider = "MySql",
                Server = builder.Server,
                Port = (int)(builder.Port == 0 ? 3306 : builder.Port),
                Database = builder.Database,
                Username = builder.UserID,
                Password = builder.Password,
                UseSsl = builder.SslMode != MySqlSslMode.None
            };
        }
        catch
        {
            return new ClientConnectionProfile();
        }
    }
}
