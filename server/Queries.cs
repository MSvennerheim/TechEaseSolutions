using System.ComponentModel.DataAnnotations;

namespace server;
using Npgsql;

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
            
            // Obs: I produktion bör du använda proper password hashing!
            if (password == storedPassword)
            {
                return new User
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Email = reader.GetString(reader.GetOrdinal("email")),
                    Company = reader.GetInt32(reader.GetOrdinal("company")),
                    IsCustomerServiceUser = reader.GetBoolean(reader.GetOrdinal("customer-service-user")),
                    IsAdmin = reader.GetBoolean(reader.GetOrdinal("admin"))
                };
            }
        }
        
        return null;
    }
    public async Task customerTempUser(Ticket ticket)
    {
        await using (var cmd = _db.CreateCommand("INSERT INTO users (email) VALUES ($1) RETURNING id;"))
        {
            cmd.Parameters.AddWithValue(ticket.email);
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    ticket.id = reader.GetInt32(0);
                    Console.WriteLine(ticket.id);
                }
            }
        }
    }

    public async Task postNewTicket(Ticket ticket)
    {
        await using (var cmd = _db.CreateCommand("SELECT COALESCE(MAX(chatid), 0) FROM messages"))
        {
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    ticket.chatid = reader.IsDBNull(0) ? 1 : reader.GetInt32(0) + 1;
                }
                else
                {
                    ticket.chatid = 1; // Default if no rows exist
                }
            }
        }
        
        await using (var cmd = _db.CreateCommand(
                         "INSERT INTO messages (message, casetype, sender, chatid) VALUES ($1, $2, $3, $4)"))
        {
            cmd.Parameters.AddWithValue(ticket.description);
            cmd.Parameters.AddWithValue(ticket.option);
            cmd.Parameters.AddWithValue(ticket.id);
            cmd.Parameters.AddWithValue(ticket.chatid);
            await cmd.ExecuteNonQueryAsync();
        }
    }

    public async Task fetchCompanyCaseTypes()
    {
        
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