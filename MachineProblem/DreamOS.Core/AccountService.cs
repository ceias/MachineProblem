using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace DreamOS.Core;

/// <summary>Local accounts shared by Dream OS sessions on this Windows profile.</summary>
public sealed class AccountService
{
    private const int Iterations = 210_000;
    private readonly string connectionString;
    public AccountService(string databasePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(databasePath))!);
        connectionString = new SqliteConnectionStringBuilder { DataSource = databasePath, Mode = SqliteOpenMode.ReadWriteCreate }.ToString();
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Accounts (
                Username TEXT PRIMARY KEY COLLATE NOCASE,
                PasswordHash BLOB NOT NULL,
                Salt BLOB NOT NULL,
                Iterations INTEGER NOT NULL,
                CreatedUtc TEXT NOT NULL
            );
            """;
        command.ExecuteNonQuery();
    }
    private SqliteConnection Open()
    {
        var connection = new SqliteConnection(connectionString);
        connection.Open();
        return connection;
    }
    public void Register(string username, string password)
    {
        username = username.Trim();
        if (!Regex.IsMatch(username, @"\A[A-Za-z0-9_]{3,30}\z"))
            throw new ArgumentException("Use 3–30 letters, numbers, or underscores for your username.");
        if (password.Length < 8 || password.Length > 100)
            throw new ArgumentException("Choose a password with 8–100 characters.");
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, 32);
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Accounts (Username,PasswordHash,Salt,Iterations,CreatedUtc) VALUES ($name,$hash,$salt,$iterations,$created)";
        command.Parameters.AddWithValue("$name", username);
        command.Parameters.AddWithValue("$hash", hash);
        command.Parameters.AddWithValue("$salt", salt);
        command.Parameters.AddWithValue("$iterations", Iterations);
        command.Parameters.AddWithValue("$created", DateTime.UtcNow.ToString("O"));
        try { command.ExecuteNonQuery(); }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        { throw new ArgumentException("That username is already taken. Please choose another."); }
    }
    public bool Authenticate(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || password.Length > 100) return false;
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT PasswordHash,Salt,Iterations FROM Accounts WHERE Username=$name";
        command.Parameters.AddWithValue("$name", username.Trim());
        using var reader = command.ExecuteReader();
        if (!reader.Read()) return false;
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, (byte[])reader[1], reader.GetInt32(2), HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals((byte[])reader[0], actual);
    }
}
