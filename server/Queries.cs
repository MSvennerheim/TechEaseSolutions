using System.Text.Json;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;

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

    public async Task<string> GetChatHistory(int chat) {
        
        // get chat history for a specific chat using chatid as a JSON file
        
        var messages = new List<object>();

        const string ChatHistory =
            @"SELECT message, email, timestamp
                FROM messages 
                JOIN users ON messages.sender = users.id 
                WHERE chatid = @chatid
                ORDER BY timestamp ";
        
        await using (var cmd = _db.CreateCommand(ChatHistory))
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

    public async Task<string> WriteChatToDB(ChatData chatData)
    {
        // first get id for user from email and company from name, create timestamp and write to db.
        // if sender is csrep also return mailadress for sending to that ticket has been updated 
        
        int senderId = 0; // just to get it to run, make a check so these are not 0 before writing to db
        int companyId = 0;
        int caseType = 0;
        string company = "";
        
        // TODO: rewrite these following queries in a nicer way...

        const string getChatdataInfo = @"SELECT casetype, name
                                            FROM messages
                                            JOIN companies ON messages.company = companies.id
                                            WHERE chatid = @chatId
                                            LIMIT 1";

        await using (var cmd = _db.CreateCommand(getChatdataInfo))
        {
            cmd.Parameters.AddWithValue("@chatId", chatData.chatId);
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    caseType = reader.GetInt32(0);
                    company = reader.GetString(1);
                }
            }
        }
            
            
            
        const string getId = @"SELECT users.id, c.id
                                    FROM users
                                    JOIN public.companies c on users.company = c.id
                                    WHERE email = @email 
                                    AND c.name = @company";
        await using (var cmd = _db.CreateCommand(getId))
        {
            cmd.Parameters.AddWithValue("@email", chatData.email);
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
            cmd.Parameters.AddWithValue("@message", chatData.message);
            cmd.Parameters.AddWithValue("@companyId", companyId);
            cmd.Parameters.AddWithValue("@casetype", caseType);
            cmd.Parameters.AddWithValue("@senderId", senderId);
            cmd.Parameters.AddWithValue("@chatId", chatData.chatId);
            cmd.Parameters.AddWithValue("@currentTime", currentTime);
            await using var reader = await cmd.ExecuteReaderAsync();
        }
        
        // if sender is csrep get email for customer and send them a confirmation

        if (chatData.csrep)
        {
            string customerEmail = "";
            const string getCustomerMail = @"SELECT u.email
                                                from messages
                                                JOIN public.users u on u.id = messages.sender
                                                WHERE u.csrep = false
                                                AND chatid = @chatId
                                                LIMIT 1";
            
            await using (var cmd = _db.CreateCommand(getCustomerMail))
            {
                cmd.Parameters.AddWithValue("@chatId", chatData.chatId);
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        customerEmail = reader.GetString(0);
                    }
                    else
                    {
                        Console.WriteLine("Couldnt find customer mail, chatid: " + chatData.chatId);
                    }
                }
            }
            return customerEmail;
        }
        // return empty string if we don't need to send confirmation
        return "";
    }

    public async Task<List<User>> GetEmployees()
    {
        var users = new List<User>();

        await using (var cmd = _db.CreateCommand(
                         "SELECT * FROM users INNER JOIN public.companies c ON c.id = users.company where (admin = 'true' or csrep = 'true')"))
        {
            await using var reader = await cmd.ExecuteReaderAsync();
        
            while (await reader.ReadAsync())
            {
                var user = new User
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Email = reader.GetString(reader.GetOrdinal("email")),
                    CompanyName = reader.GetString(reader.GetOrdinal("name")),
                    IsCustomerServiceUser = reader.GetBoolean(reader.GetOrdinal("csrep")),
                    IsAdmin = reader.GetBoolean(reader.GetOrdinal("admin"))
                };

                users.Add(user);
            }
        }

        return users;
    }

    
    
    public async Task<User?> ValidateUser(string email, string password)
    {
        const string sql = @"
            SELECT users.id, email, password, company, csrep, admin, name
            FROM users
            inner join public.companies c on c.id = users.company
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
                    IsAdmin = reader.GetBoolean(reader.GetOrdinal("admin")),
                    CompanyName = reader.GetString(reader.GetOrdinal("name"))
                };
                Console.WriteLine($"User from DB: {JsonSerializer.Serialize(user)}");
                return user;
            }
        }
        
        return null;
    }
    public async Task customerTempUser(Ticket ticket)
    {
        await using (var checkCmd =
                     _db.CreateCommand("SELECT id, email, company FROM users where email = @email AND company = @company"))
        {
            checkCmd.Parameters.AddWithValue("@email", ticket.email);
            checkCmd.Parameters.AddWithValue("@company", ticket.companyId);
            await using (var reader = await checkCmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    if (reader.HasRows) 
                    { 
                        ticket.id = reader.GetInt32(reader.GetOrdinal("id")); 
                        return;
                    }
                }

            }
        }
        
        await using (var cmd = _db.CreateCommand("INSERT INTO users (email, csrep, admin, company) VALUES ($1, $2, $3, $4) RETURNING id;"))
        {
            cmd.Parameters.AddWithValue(ticket.email);
            cmd.Parameters.AddWithValue(false);
            cmd.Parameters.AddWithValue(false);
            cmd.Parameters.AddWithValue(ticket.companyId);
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
    
    
    // TEST

    public async Task<string> fetchCaseTypes()
    {
        var caseTypesList = new List<string>();
        await using (var cmd = _db.CreateCommand("SELECT casetypes.text From CaseTypes"))
        {
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    caseTypesList.Add(reader.GetString(0));
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
    public string? CompanyName { get; set; }
}