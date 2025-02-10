namespace server.Properties;
using Npgsql;
using DotNetEnv;

public class Database
{

    
    private readonly string _host = "localhost";
    private readonly string _port = "5432";
    private readonly string _username = "postgres";
    private readonly string _password; // create a .env file in /server and add this row PGPASSWORD=mypassword for testing with local db
    private readonly string _database = "postgres";

    private NpgsqlDataSource _connection;

    // metod som hämtar anslutningen
    public NpgsqlDataSource Connection()
    {
        return _connection;
    }

    // koppla upp till databasen (i constructorn)
    public Database()
    {
        Env.Load();
        _password = Environment.GetEnvironmentVariable("PGPASSWORD") ?? "didn't load correctly";
        string connectionString = $"Host={_host};Port={_port};Username={_username};Password={_password};Database={_database}";
        _connection = NpgsqlDataSource.Create(connectionString);
        
        try
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                Console.WriteLine("connected to db");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

    }

    
    
}
