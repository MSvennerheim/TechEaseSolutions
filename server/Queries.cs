namespace server;
using Npgsql;
using System.Text.Json;
public class Queries
{
    private NpgsqlDataSource _db;

    public Queries(NpgsqlDataSource db)
    {
        _db = db;
    }
    
    public async Task<User?> ValidateUser(string email, string password)
    {
        const string sql = @"
            SELECT id, email, password, company, ""customer-service-user"", admin
            FROM users 
            WHERE email = @email";

        await using var cmd = _db.CreateCommand(sql);
        cmd.Parameters.AddWithValue("email", email);

        await using var reader = await cmd.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            var storedPassword = reader.GetString(reader.GetOrdinal("password"));
            
            
            if (password == storedPassword)
            {
                var user = new User
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Email = reader.GetString(reader.GetOrdinal("email")),
                    Company = reader.GetInt32(reader.GetOrdinal("company")),
                    IsCustomerServiceUser = reader.GetBoolean(reader.GetOrdinal("customer-service-user")),
                    IsAdmin = reader.GetBoolean(reader.GetOrdinal("admin"))
                };
                Console.WriteLine($"User from DB: {JsonSerializer.Serialize(user)}");
                return user;
            }
        }
        
        return null;
    }
}

public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public int Company { get; set; }
    public bool IsCustomerServiceUser { get; set; }
    public bool IsAdmin { get; set; }
}