namespace server.Properties;
using Npgsql;
using DotNetEnv;

public class Database
{
    private readonly string _connectionString;

    public Database()
    {
        Env.Load(); // Ladda .env-filen

        string host = Environment.GetEnvironmentVariable("PGHOST") ?? "localhost";
        string port = Environment.GetEnvironmentVariable("PGPORT") ?? "5432";
        string user = Environment.GetEnvironmentVariable("PGUSER") ?? "postgres";
        string password = Environment.GetEnvironmentVariable("PGPASSWORD") ?? "lirare888";
        string database = Environment.GetEnvironmentVariable("PGDATABASE") ?? "postgres";

        _connectionString = $"Host={host};Port={port};Username={user};Password={password};Database={database}";
        
        Console.WriteLine($"✅ Ansluter till databasen med connection string: {_connectionString}");

    }

    public NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
