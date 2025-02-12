using System.Text.Json;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;

namespace server;
using Npgsql;

public class Queries
{
    private NpgsqlDataSource _db;

    public Queries(NpgsqlDataSource db)
    {
        _db = db;
    }

    public async Task<string> GetChatHistory(int chat) {
        
        // get chat history for a specific chat using chatid as a JSON file
        
        var messages = new List<object>();
        
        await using (var cmd = _db.CreateCommand("SELECT message, email, timestamp FROM messages JOIN users ON messages.sender = users.id WHERE chatid = @chatid"))
        {
            cmd.Parameters.AddWithValue("@chatid", chat);
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    messages.Add(new
                    {
                        message = reader.GetString(0),
                        sender = reader.GetString(1),
                        timestamp = reader.GetDateTime(2).ToString("o")
                    });
                }
            }
        }
        return JsonSerializer.Serialize(messages, new JsonSerializerOptions {WriteIndented = true});
    }

    public async Task<string> GetChatsForCsRep(string company)
    {
        var chats = new List<object>();

        const string sql = @"
            SELECT DISTINCT ON (chatid) chatid, message, email, timestamp, ""customer-service-user""
            FROM messages
            JOIN users ON messages.sender = users.id
            JOIN companies ON messages.company = companies.id
            WHERE companies.name = @company
            ORDER BY chatid, timestamp DESC";
        
        
        await using (var cmd = _db.CreateCommand(sql))
        {
            cmd.Parameters.AddWithValue("@company", company);
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    chats.Add(new
                    {
                        chat = reader.GetInt32(0),
                        message = reader.GetString(1),
                        sender = reader.GetString(2),
                        timestamp = reader.GetDateTime(3).ToString("o"),
                        csrep = reader.GetBoolean(4)
                    });
                }
            }
        }
        return JsonSerializer.Serialize(chats, new JsonSerializerOptions {WriteIndented = true});
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
}

public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public int Company { get; set; }
    public bool IsCustomerServiceUser { get; set; }
    public bool IsAdmin { get; set; }
}