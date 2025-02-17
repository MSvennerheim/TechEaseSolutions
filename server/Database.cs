namespace server.Properties;
using Npgsql;
using DotNetEnv;

public class Database
{

    // create a .env file in /server and add these rows with replacement for relevant local data
    //
    // PGHOST=localhost
    // PGPORT=myport
    // PGDATABASE=mydb
    // PGUSER=myuser
    // PGPASSWORD=mypassword
    //
    // for testing with local db
    
    private readonly string _host;
    private readonly string _port;
    private readonly string _username;
    private readonly string _password;
    private readonly string _database;

    private NpgsqlDataSource _connection;

    public NpgsqlDataSource Connection()
    {
        return _connection;
    }

    public Database()
    {
        Env.Load();
        _host = Environment.GetEnvironmentVariable("PGHOST") ?? "no host found";
        _port = Environment.GetEnvironmentVariable("PGPORT") ?? "no port found";
        _username = Environment.GetEnvironmentVariable("PGUSER") ?? "no user found";
        _password = Environment.GetEnvironmentVariable("PGPASSWORD") ?? "no pw found";
        _database = Environment.GetEnvironmentVariable("PGDATABASE") ?? "no db found";
        
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
