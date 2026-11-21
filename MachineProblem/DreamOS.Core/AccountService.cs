using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace DreamOS.Core;

/// <summary>Local accounts shared by Dream OS sessions on this Windows profile.</summary>
public sealed class AccountService
{
    private const int Iterations = 210_000;
    private readonly string connectionString;
    public const string DefaultConnectionString = @"Server=(localdb)\MSSQLLocalDB;Database=DreamOS;Integrated Security=true;Encrypt=true;TrustServerCertificate=true;Connect Timeout=10";
    public AccountService(string? connectionString = null)
    {
        var builder = new SqlConnectionStringBuilder(connectionString ?? DefaultConnectionString);
        if (string.IsNullOrWhiteSpace(builder.InitialCatalog))
            throw new ArgumentException("The account connection must specify a database name.");
        this.connectionString = builder.ConnectionString;
        var database = builder.InitialCatalog;
        builder.InitialCatalog = "master";
        using (var server = new SqlConnection(builder.ConnectionString))
        {
            server.Open();
            using var create = server.CreateCommand();
            create.CommandText = "IF DB_ID(@database) IS NULL BEGIN DECLARE @sql nvarchar(max) = N'CREATE DATABASE ' + QUOTENAME(@database); EXEC sys.sp_executesql @sql; END";
            create.Parameters.Add("@database", SqlDbType.NVarChar, 128).Value = database;
            // CREATE DATABASE is not allowed inside a transaction.
            try { create.ExecuteNonQuery(); }
            catch (SqlException ex) when (ex.Number == 1801) { /* Another instance created it. */ }
        }
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            IF OBJECT_ID(N'dbo.Accounts', N'U') IS NULL
            CREATE TABLE dbo.Accounts (
                Username nvarchar(30) COLLATE Latin1_General_100_CI_AS NOT NULL PRIMARY KEY,
                PasswordHash varbinary(32) NOT NULL,
                Salt varbinary(16) NOT NULL,
                Iterations int NOT NULL,
                CreatedUtc datetime2 NOT NULL
            );
            """;
        command.ExecuteNonQuery();
    }
    private SqlConnection Open()
    {
        var connection = new SqlConnection(connectionString);
        try { connection.Open(); return connection; }
        catch { connection.Dispose(); throw; }
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
        command.CommandText = "INSERT INTO dbo.Accounts (Username,PasswordHash,Salt,Iterations,CreatedUtc) VALUES (@name,@hash,@salt,@iterations,@created)";
        command.Parameters.Add("@name", SqlDbType.NVarChar, 30).Value = username;
        command.Parameters.Add("@hash", SqlDbType.VarBinary, 32).Value = hash;
        command.Parameters.Add("@salt", SqlDbType.VarBinary, 16).Value = salt;
        command.Parameters.Add("@iterations", SqlDbType.Int).Value = Iterations;
        command.Parameters.Add("@created", SqlDbType.DateTime2).Value = DateTime.UtcNow;
        try { command.ExecuteNonQuery(); }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        { throw new ArgumentException("That username is already taken. Please choose another."); }
    }
    public bool Authenticate(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || username.Trim().Length > 30 || password.Length > 100) return false;
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT PasswordHash,Salt,Iterations FROM dbo.Accounts WHERE Username=@name";
        command.Parameters.Add("@name", SqlDbType.NVarChar, 30).Value = username.Trim();
        using var reader = command.ExecuteReader();
        if (!reader.Read()) return false;
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, (byte[])reader[1], reader.GetInt32(2), HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals((byte[])reader[0], actual);
    }
}
