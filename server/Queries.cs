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
            SELECT DISTINCT ON (chatid) chatid, message, email, timestamp, csrep
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

    public async Task<string> WriteChatToDB(string message, string email, int chatId, int caseType, string company, bool csrep)
    {
        // first get id for user from email and company from name, create timestamp and write to db.
        // if sender is csrep also return mailadress for sending to that ticket has been updated 
        
        int senderId;
        int companyId;
        const string getId = @"SELECT users.id, c.id
                                    FROM users
                                    JOIN public.companies c on users.company = c.id
                                    WHERE email = @email 
                                    AND c.name = @company";
        await using (var cmd = _db.CreateCommand(getId))
        {
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@company", company);
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    senderId = reader.GetInt32(0);
                    companyId = reader.GetInt32(1);
                }
            }
        }
        
        DateTime currentTime = DateTime.Now;
        const string writeToDB = @"INSERT INTO messages (message, company, casetype, sender, chatid, timestamp)
                                        values (@message, @companyId, @casetype, @senderId, @chatId, @currentTime )";
        await using (var cmd = _db.CreateCommand(writeToDB))
        {
            cmd.Parameters.AddWithValue("@message", message);
            cmd.Parameters.AddWithValue("@companyId", companyId);
            cmd.Parameters.AddWithValue("@casetype", caseType);
            cmd.Parameters.AddWithValue("@senderId", senderId);
            cmd.Parameters.AddWithValue("@chatId", chatId);
            cmd.Parameters.AddWithValue("@currentTime", currentTime);
            await using var reader = await cmd.ExecuteReaderAsync();
        }
        
        // if sender is csrep get email for customer and send them a confirmation

        if (csrep)
        {
            string customerEmail;
            const string getCustomerMail = @"SELECT u.email
                                                from messages
                                                JOIN public.users u on u.id = messages.sender
                                                WHERE u.csrep = false
                                                AND chatid = @chatId
                                                LIMIT 1";
            
            await using (var cmd = _db.CreateCommand(getCustomerMail))
            {
                cmd.Parameters.AddWithValue("@chatId", chatId);
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    customerEmail = reader.GetString(0);
                }
            }
            return customerEmail;
        }
        // return empty string if we don't need to send confirmation
        return "";
    }
    
    
    public async Task<User?> ValidateUser(string email, string password)
    {
        const string sql = @"
            SELECT id, email, password, company, csrep, admin
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