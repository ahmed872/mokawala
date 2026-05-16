using Microsoft.Data.Sqlite;
using MySqlConnector;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.IO;

namespace FleetManagementSystem.WPF.Infrastructure;

public sealed class ConnectionSettingsStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _filePath;

    public ConnectionSettingsStore()
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FleetManagementSystem");

        _filePath = Path.Combine(directory, "connection.settings");
    }

    public string FilePath => _filePath;

    public ClientConnectionProfile? Load()
    {
        if (!File.Exists(_filePath))
        {
            return null;
        }

        try
        {
            var protectedBytes = File.ReadAllBytes(_filePath);
            var clearBytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
            var json = Encoding.UTF8.GetString(clearBytes);
            return JsonSerializer.Deserialize<ClientConnectionProfile>(json, SerializerOptions);
        }
        catch
        {
            return null;
        }
    }

    public void Save(ClientConnectionProfile profile)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(profile, SerializerOptions);
        var clearBytes = Encoding.UTF8.GetBytes(json);
        var protectedBytes = ProtectedData.Protect(clearBytes, null, DataProtectionScope.CurrentUser);
        File.WriteAllBytes(_filePath, protectedBytes);
    }

    public async Task<ConnectionTestResult> TestAsync(ClientConnectionProfile profile)
    {
        try
        {
            if (profile.IsSqlite)
            {
                await using var sqliteConnection = new SqliteConnection(profile.BuildConnectionString());
                await sqliteConnection.OpenAsync();
                await using var sqliteCommand = sqliteConnection.CreateCommand();
                sqliteCommand.CommandText = "SELECT 1;";
                await sqliteCommand.ExecuteScalarAsync();
                return ConnectionTestResult.Success($"تم الاتصال بقاعدة SQLite بنجاح: {profile.SqlitePath}");
            }

            await using var connection = new MySqlConnection(profile.BuildConnectionString());
            await connection.OpenAsync();
            await using var command = new MySqlCommand("SELECT DATABASE();", connection);
            var databaseName = Convert.ToString(await command.ExecuteScalarAsync()) ?? profile.Database;
            return ConnectionTestResult.Success($"تم الاتصال بسيرفر MySQL بنجاح. قاعدة البيانات الحالية: {databaseName}");
        }
        catch (Exception ex)
        {
            return ConnectionTestResult.Failure($"فشل الاتصال: {ex.Message}");
        }
    }
}

public sealed class ConnectionTestResult
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;

    public static ConnectionTestResult Success(string message) => new()
    {
        IsSuccess = true,
        Message = message
    };

    public static ConnectionTestResult Failure(string message) => new()
    {
        IsSuccess = false,
        Message = message
    };
}
