
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
    
    
    //Behövde lägga ett sätt att hämta databas anslutning efter som att jag ville använda sql i program.cs
    public NpgsqlDataSource GetDbConnection()
    {
        return _db;
    }
    
    
    //Skapade en klass där en unik token skapas som då är en engångstoken som kan både användas som autentisering eller åtkomstkontroll. Faktiskt väldigt användbart
    public async Task<string> GenerateAndStoreToken(int chatId, string email)
    {
        var token = Guid.NewGuid().ToString();

        const string insertTokenSql =
            @"INSERT INTO case_tokens (token, case_id, email, expires_at) 
          VALUES (@token, @case_id, @email, @expires_at);";

        await using (var cmd = _db.CreateCommand(insertTokenSql))
        {
            cmd.Parameters.AddWithValue("@token", token);
            cmd.Parameters.AddWithValue("@case_id", chatId); // Match SQL parameter name
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@expires_at", DateTime.UtcNow.AddHours(24)); // Match SQL parameter name
            await cmd.ExecuteNonQueryAsync();
        }
        return token;
    }

    public async Task<bool> ValidateToken(string token)
    {
        const string sql = @"SELECT COUNT(*) FROM case_tokens WHERE token = @token AND expires_at > NOW();";

        await using (var cmd = _db.CreateCommand(sql))
        {
            cmd.Parameters.AddWithValue("@token", token);
            var result = await cmd.ExecuteScalarAsync(); // ExecuteScalarAsync returnerar ett object
            return Convert.ToInt32(result) > 0; // Konvertera resultatet till int
        }
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

        const string sql = @"SELECT DISTINCT ON (messages.chatid) messages.chatid,messages.message,users.email,messages.timestamp,COALESCE(users.csrep, false) as csrep
FROM messages JOIN users ON messages.sender = users.id  JOIN companies ON messages.company = companies.id WHERE companies.name = @company ORDER BY messages.chatid, messages.timestamp DESC";
    
        try 
        {
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
                            csrep = reader.IsDBNull(4) ? false : reader.GetBoolean(4)
                        });
                    }
                }
            }
            return JsonSerializer.Serialize(chats, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetChatsForCsRep: {ex.Message}");
            throw; 
        }
    }

    public async Task<string> WriteChatToDB(string message, string email, int chatId, int caseType, string company, bool csrep)
    {
        // first get id for user from email and company from name, create timestamp and write to db.
        // if sender is csrep also return mailadress for sending to that ticket has been updated 
        
        int senderId = 0; // just to get it to run, make a check so these are not 0 before writing to db
        int companyId = 0; 
        
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
            
            
            if (password == storedPassword)
            {
                var user = new User
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Email = reader.GetString(reader.GetOrdinal("email")),
                    Company = reader.GetInt32(reader.GetOrdinal("company")),
                    IsCustomerServiceUser = reader.GetBoolean(reader.GetOrdinal("csrep")),
                    IsAdmin = reader.GetBoolean(reader.GetOrdinal("admin"))
                };
                Console.WriteLine($"User from DB: {JsonSerializer.Serialize(user)}");
                return user;
            }
        }
        
        return null;
    }
    public async Task customerTempUser(Ticket ticket)
    {
        //Make sure that you can't have duplicate emails in the database.
        
        await using (var cmd = _db.CreateCommand("INSERT INTO users (email) VALUES ($1) RETURNING id;"))
        {
            cmd.Parameters.AddWithValue(ticket.email);
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    ticket.id = reader.GetInt32(0);
                }
            }
        }
    }

    public async Task postNewTicket(Ticket ticket)
    {
        // Query to get the highest existing chatid and increment it by 1, add it to the Ticket class.
        await using (var cmd = _db.CreateCommand("SELECT COALESCE(MAX(chatid), 0) FROM messages"))
        {
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                
                // find the highest number, if no row exist. Set chatid to 1.
                if (await reader.ReadAsync())
                {
                    ticket.chatid = reader.IsDBNull(0) ? 1 : reader.GetInt32(0) + 1;
                }
                else
                {
                    ticket.chatid = 1;
                }
            }
        }
        
        DateTime now = DateTime.Now;
        
        //query to insert collected data in to the message table, (create a ticket)
        await using (var cmd = _db.CreateCommand(
                         "INSERT INTO messages (message, casetype, sender, chatid, timestamp, company) VALUES ($1, $2, $3, $4, $5, $6)"))
        {
            cmd.Parameters.AddWithValue(ticket.description);
            cmd.Parameters.AddWithValue(int.Parse(ticket.option));
            cmd.Parameters.AddWithValue(ticket.id);
            cmd.Parameters.AddWithValue(ticket.chatid);
            cmd.Parameters.AddWithValue(now);
            
            cmd.Parameters.AddWithValue(ticket.companyId);
            await cmd.ExecuteNonQueryAsync();
        }
    }

    public async Task CompanyName(Ticket ticket)
    {
        await using (var cmd = _db.CreateCommand(
                         "SELECT casetypes.id, c.id FROM casetypes INNER JOIN public.companies c on c.id = casetypes.company WHERE casetypes.id = $1"))
        {
            cmd.Parameters.AddWithValue(int.Parse(ticket.option));
            await using var reader = await cmd.ExecuteReaderAsync();
            {
                while (await reader.ReadAsync())
                {
                    ticket.companyId = reader.GetInt32(1);
                }
            }
        }
    }

    public async Task<string> GetCompanyName(string name)
    {
        var caseTypesList = new List<object>();

        await using (var cmd = _db.CreateCommand("SELECT casetypes.id, casetypes.text FROM casetypes INNER JOIN public.companies c ON c.id = casetypes.company WHERE c.name = @name"))
        {
            cmd.Parameters.AddWithValue("@name", name);
        
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    caseTypesList.Add(new
                    {
                        caseId = reader.GetInt32(0),
                        caseType = reader.GetString(1)
                    });
                }
            }
        }
        return JsonSerializer.Serialize(caseTypesList, new JsonSerializerOptions { WriteIndented = true });
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