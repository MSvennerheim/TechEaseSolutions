using System.ComponentModel.DataAnnotations;
using System.Text.Json;

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
                    IsCustomerServiceUser = reader.GetBoolean(reader.GetOrdinal("csrep")),
                    IsAdmin = reader.GetBoolean(reader.GetOrdinal("admin"))
                };
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