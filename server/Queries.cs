using System.Reflection;
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


    public async Task<object> GetChatHistory(User user)
    {
        
        // get chat history for a specific chat using chatid
        
        var messages = new List<object>();

        if (!user.CsRep)
        {
            // Check if customer has any messages in chat, if not kick them out  
            int sentMessages = 0;
            const string OriginalSender = @"SELECT count(chatid) 
                FROM messages
                JOIN users ON messages.sender = users.id
                JOIN public.companies c on c.id = messages.company
                WHERE chatid = @chatid AND c.name = @company AND email = @email";
            await using (var cmd = _db.CreateCommand(OriginalSender))
            {
                cmd.Parameters.AddWithValue("@chatid", user.ChatId);
                cmd.Parameters.AddWithValue("@company", user.CompanyName);
                cmd.Parameters.AddWithValue("@email", user.Email);
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        sentMessages = reader.GetInt32(0);
                    }
                }
            }
            if (sentMessages == 0)
            {
                return "no access";
            }
        }

        const string ChatHistory =
            @"SELECT message, email, timestamp, csrep
                FROM messages 
                JOIN users ON messages.sender = users.id 
                JOIN public.companies c on c.id = messages.company
                WHERE chatid = @chatid AND c.name = @company
                ORDER BY timestamp";

        await using (var cmd = _db.CreateCommand(ChatHistory))
        {
            cmd.Parameters.AddWithValue("@chatid", user.ChatId);
            cmd.Parameters.AddWithValue("@company", user.CompanyName);
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    messages.Add(new
                    {
                        message = reader.GetString(0),
                        sender = reader.GetString(1),
                        timestamp = reader.GetDateTime(2).ToString("dd/MM/yy HH:mm"),
                        csrep = reader.GetBoolean(3)
                    });
                }
            }
        }
        return JsonSerializer.Serialize(messages, new JsonSerializerOptions { WriteIndented = true });;
    }

    public async Task<string> GetChatsForCsRep(string company, bool allChats, bool getChatForAssignment)
    {
        // Get all chats for a certain company and if they are open/closed
        // Also gets chatId and returns this to /api/assignNextTicket if csrep presses "Send and take next open ticket" 
        
        var chats = new List<object>();

        const string sql = @"
            SELECT DISTINCT ON (chatid) chatid, message, u1.email AS lastmessagefrom, timestamp, u1.csrep, u2.email AS assignedcsrep, text
                FROM messages
                         JOIN users u1 ON messages.sender = u1.id
                         JOIN companies ON messages.company = companies.id
                         LEFT JOIN users u2 ON messages.assignedcsrep = u2.id 
                         JOIN casetypes ON messages.casetype = casetypes.id
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
                        timestamp = reader.GetDateTime(3).ToString("dd/MM/yy HH:mm"),
                        csrep = reader.GetBoolean(4),
                        assignedCsRep =  reader.IsDBNull(5) ? null : reader.GetString(5),
                        casetype = reader.GetString(6)
                    });
                }
            }
        }

        // return everything if box is checked in frontend, else sort open tickets
        
        if (allChats)
        {
            return JsonSerializer.Serialize(chats, new JsonSerializerOptions {WriteIndented = true});
        }
        var sorterdChats = new List<object>();
        
        // gets all unassigned chats
        
        foreach (dynamic chat in chats)
        {
            if (!chat.csrep && chat.assignedCsRep == null)
            {
                sorterdChats.Add(chat);
            }
        }
        
        // just get the chatId of the chat to return for assignment when "send and get next open ticket" is clicked
        // return as string and convert on other side, if empty string no more chats to assign
        // I don't like this and I'm starting to regret not just making a new function for this...
        
        if (getChatForAssignment)
        {
            if(sorterdChats.Count > 0){
                dynamic firstUnnassignedChat = sorterdChats[0]; 
                return Convert.ToString(firstUnnassignedChat.chat);
            }
            return "";
        }
        return JsonSerializer.Serialize(sorterdChats, new JsonSerializerOptions {WriteIndented = true});
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
        // and clear assignedcsrep

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
                    while (await reader.ReadAsync())
                    {
                        customerEmail = reader.GetString(0);
                    }
                }
            }
            const string clearCsRep = @"UPDATE messages
                                            SET assignedcsrep = null
                                            WHERE chatid = @chatid ";
            await using (var cmd = _db.CreateCommand(clearCsRep))
            {
                cmd.Parameters.AddWithValue("@chatId", chatData.chatId);
                cmd.ExecuteNonQueryAsync();
            }
            return customerEmail;
        }

        // return empty string if we don't need to send confirmation
        return "";
    }

    public async Task<User?> ValidateTempUser(string email, int chatId)
    {
        const string sql = @"
            SELECT users.id, email, password, users.company, c.name, chatid
            FROM users 
            JOIN public.companies c on c.id = users.company
            JOIN public.messages m on users.id = m.sender
            WHERE email = @email AND chatid = @chatid
            LIMIT 1";

        await using var cmd = _db.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@email", email);
        cmd.Parameters.AddWithValue("@chatid", chatId);

        await using var reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {

            var user = new User
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Email = reader.GetString(reader.GetOrdinal("email")),
                Company = reader.GetInt32(reader.GetOrdinal("company")),
                CompanyName = reader.GetString(reader.GetOrdinal("name")),
                CsRep = false,
                IsAdmin = false,
                ChatId = reader.GetInt32(reader.GetOrdinal("chatid"))
            };
            Console.WriteLine($"User from DB: {JsonSerializer.Serialize(user)}");
            Console.WriteLine(user.ChatId);
            return user;
        }

        return null;
    }

    public async Task<List<User>> GetEmployees(string company)
    {
        var users = new List<User>();

        const string sql =
            @"SELECT * 
                FROM users 
                INNER JOIN public.companies c ON c.id = users.company
                WHERE name = @company AND activeaccount = true";
        
        await using (var cmd = _db.CreateCommand(sql))
        {
            cmd.Parameters.AddWithValue("@company", company);

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var user = new User
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Email = reader.GetString(reader.GetOrdinal("email")),
                    CompanyName = reader.GetString(reader.GetOrdinal("name")),
                    CsRep = reader.GetBoolean(reader.GetOrdinal("csrep")),
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
            SELECT users.id, email, password, company, c.name, csrep, admin
            FROM users 
            JOIN public.companies c on c.id = users.company
            WHERE email = @email AND activeaccount = true";

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
                    CompanyName = reader.GetString(reader.GetOrdinal("name")),
                    CsRep = reader.GetBoolean(reader.GetOrdinal("csrep")),
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
        //Make sure that you can't have duplicate emails on the same company id in the database.
        await using (var checkCmd =
                     _db.CreateCommand(
                         "SELECT id, email, company FROM users where email = @email AND company = @company"))
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

        await using (var cmd = _db.CreateCommand(
                         "INSERT INTO users (email, csrep, admin, company) VALUES ($1, $2, $3, $4) RETURNING id;"))
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

        await using (var cmd = _db.CreateCommand(
                         "SELECT casetypes.id, casetypes.text FROM casetypes INNER JOIN public.companies c ON c.id = casetypes.company WHERE c.name = @name AND casetypes.isactive = true"))
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

    // la till fler funktioner i patricks PostNewCsRep
    public async Task <bool> PostNewCsRep(int company, string email)
    {
        try
        {
            // Lägger in användare
            await using (var cmd = _db.CreateCommand(
                             "INSERT INTO users (email, csrep, admin, company) VALUES ($1, $2, $3, $4)"))
            {
                cmd.Parameters.AddWithValue(email);
                cmd.Parameters.AddWithValue(true); //användaren är en csrep
                cmd.Parameters.AddWithValue(false); // användaren är inte en admin
                cmd.Parameters.AddWithValue(company);
                await cmd.ExecuteNonQueryAsync();
            }
        
            // Får tag i företag namnet
            string companyName = "";
            await using (var cmd = _db.CreateCommand("SELECT name FROM companies WHERE id = @companyId"))
            {
                cmd.Parameters.AddWithValue("@companyId", company);
                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    companyName = reader.GetString(0);
                }
            }
        
            // Genererar Token
            string token = await CreatePasswordResetToken(email);
        
            // Skicka mail
            Mail mail = new Mail();
            bool emailSent = await mail.SendNewCSRepWelcomeEmail(email, token, companyName);
        
            return emailSent;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating new CS rep: {ex.Message}");
            return false;
        }
    }



    public async Task<EmailTemplate?> GetEmailTemplate(int company)
    {
        // gets template for email, and if none exsists inserts new default template into db

        const string checkTemplate = @"SELECT title, greeting, content, signature FROM email_templates WHERE company = @company";
        const string createTemplate = @"INSERT INTO email_templates (company, title, greeting, content, signature) VALUES (@company, @title, @greeting, @content, @signature)";

        EmailTemplate template = new EmailTemplate(); 
            
            await using (var cmd = _db.CreateCommand(checkTemplate))
            {
                cmd.Parameters.AddWithValue("@companyId", company);
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    template.templateTitle = reader.GetString(reader.GetOrdinal("title"));
                    template.templateGreeting = reader.GetString(reader.GetOrdinal("greeting"));
                    template.templateContent = reader.GetString(reader.GetOrdinal("content"));
                    template.templateSignature = reader.GetString(reader.GetOrdinal("signature"));
                    return template;
                }
            }

            template.templateTitle = "Titel visas här";
            template.templateGreeting = "Hälsning visas här, kunds ärendenr följer härefter";
            template.templateContent = "Huvudtext visas här";
            template.templateSignature = "Signatur visas här";
            
            await using (var cmd = _db.CreateCommand(createTemplate))
            {
                cmd.Parameters.AddWithValue("@company", company);
                cmd.Parameters.AddWithValue("@title", template.templateTitle);
                cmd.Parameters.AddWithValue("@greeting", template.templateGreeting);
                cmd.Parameters.AddWithValue("@content", template.templateContent);
                cmd.Parameters.AddWithValue("@signature", template.templateSignature);
                await cmd.ExecuteNonQueryAsync();
            }
        return template;
    }

    //uppdaterar epost mallen i databasen
    public async Task<bool> UpdateEmailTemplate(EmailTemplate template, int company)
    {
        const string sql = @"UPDATE email_templates 
                            SET title = @title, greeting = @greeting, content = @content, signature = @signature 
                            WHERE company = @company";
        await using var cmd = _db.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@title", template.templateTitle);
        cmd.Parameters.AddWithValue("@greeting", template.templateGreeting);
        cmd.Parameters.AddWithValue("@content", template.templateContent);
        cmd.Parameters.AddWithValue("@signature", template.templateSignature);
        cmd.Parameters.AddWithValue("@company", company);
        int rowsAffected = await cmd.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }
    
    
    
    public async Task RemoveCsRep(int company, string email)
    {
        
        // only deactivate accounts which are csreps, in case anyone who's deactivated have a customer account
        
        const string softdelete = @"UPDATE users
                                        SET activeaccount = false
                                        WHERE email = @email AND company = @company AND csrep = true";

        await using (var cmd = _db.CreateCommand(softdelete))
        {
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@company", company);
            await cmd.ExecuteNonQueryAsync();
        }
        
    }
    public async Task<bool> ValidateTokenAndResetPassword(string email, string token, string newPassword)
    {
        // Validera token
        const string validateSql = @"
    SELECT id FROM password_reset_tokens 
    WHERE email = @email AND token = @token AND expiry_date > @now";

        await using var validateCmd = _db.CreateCommand(validateSql);
        validateCmd.Parameters.AddWithValue("email", email);
        validateCmd.Parameters.AddWithValue("token", token);
        validateCmd.Parameters.AddWithValue("now", DateTime.Now);

        var tokenId = await validateCmd.ExecuteScalarAsync();

        if (tokenId == null)
        {
            return false; // den är invalid eller så har den har gått ut
        }

        // Uppdaterar lösenord
        const string updateSql = @"
    UPDATE users SET password = @password WHERE email = @email";

        await using var updateCmd = _db.CreateCommand(updateSql);
        updateCmd.Parameters.AddWithValue("email", email);
        updateCmd.Parameters.AddWithValue("password", newPassword);
        int rowsAffected = await updateCmd.ExecuteNonQueryAsync();

        if (rowsAffected == 0)
        {
            return false; // Användaren kunde inte hittas 
        }

        // Tar bort tokenen med mailet efter man har skapat ett nytt lösenord 
        const string deleteSql = @"
    DELETE FROM password_reset_tokens WHERE email = @email";

        await using var deleteCmd = _db.CreateCommand(deleteSql);
        deleteCmd.Parameters.AddWithValue("email", email);
        await deleteCmd.ExecuteNonQueryAsync();

        return true;
    }

    public async Task assignChatToCsRep(User assignChat)
    {
        
        Console.WriteLine("chatid: "+ assignChat.ChatId + " csrep: " + assignChat.Id);
        const string assignTicket = @"UPDATE messages
                                        SET assignedcsrep = @csrep
                                        WHERE chatid = @chatid";

        await using var cmd = _db.CreateCommand(assignTicket);
        cmd.Parameters.AddWithValue("@csrep", assignChat.Id);
        cmd.Parameters.AddWithValue("@chatid", assignChat.ChatId);
        await cmd.ExecuteNonQueryAsync();
        
    }
    
    public async Task<string> CreatePasswordResetToken(string email)
    {
        // Genererar en unik token 
        string token = Guid.NewGuid().ToString("N");
        DateTime expiryDate = DateTime.Now.AddHours(24);
    
        // Kollar först om det redan finns en token med det mailet 
        const string checkSql = @"
        SELECT id FROM password_reset_tokens WHERE email = @email";
    
        await using var checkCmd = _db.CreateCommand(checkSql);
        checkCmd.Parameters.AddWithValue("email", email);
    
        var existingId = await checkCmd.ExecuteScalarAsync();
    
        if (existingId != null)
        {
            // om den finns så kommer den uppdatera Tokenen till en ny 
            const string updateSql = @"
            UPDATE password_reset_tokens 
            SET token = @token, expiry_date = @expiryDate 
            WHERE email = @email";
        
            await using var updateCmd = _db.CreateCommand(updateSql);
            updateCmd.Parameters.AddWithValue("token", token);
            updateCmd.Parameters.AddWithValue("expiryDate", expiryDate);
            updateCmd.Parameters.AddWithValue("email", email);
            await updateCmd.ExecuteNonQueryAsync();
        }
        else
        {
            // Här lägger jag in en ny token
            const string insertSql = @"
            INSERT INTO password_reset_tokens (email, token, expiry_date)
            VALUES (@email, @token, @expiryDate)";
        
            await using var insertCmd = _db.CreateCommand(insertSql);
            insertCmd.Parameters.AddWithValue("email", email);
            insertCmd.Parameters.AddWithValue("token", token);
            insertCmd.Parameters.AddWithValue("expiryDate", expiryDate);
            await insertCmd.ExecuteNonQueryAsync();
        }
        return token;
    }
        public async Task<string> GetCaseTypes(string company)
        {
            var caseTypesList = new List<object>();
            await using (var cmd = _db.CreateCommand(
                             "SELECT casetypes.id, casetypes.text FROM casetypes INNER JOIN public.companies c ON c.id = casetypes.company WHERE c.name = @company AND casetypes.isactive = true"))
            {
                cmd.Parameters.AddWithValue("@company", company);
    
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
    
        public async Task postNewCasetype(CaseTypeUpdate casetype)
        {
            Console.WriteLine(casetype.Company);
            Console.WriteLine(casetype.caseType);
            Console.WriteLine("query");
            await using (var cmd = _db.CreateCommand(
                             "INSERT INTO casetypes (text, company) VALUES (@caseType, @company)"))
            {
                cmd.Parameters.AddWithValue("@caseType", casetype.caseType);
                cmd.Parameters.AddWithValue("@company", casetype.Company);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task removeCasetype(int id)
        {
            await using (var cmd = _db.CreateCommand("UPDATE casetypes SET isactive = false WHERE id = @id"))
            {
                cmd.Parameters.AddWithValue("@id", id);
                await cmd.ExecuteNonQueryAsync(); // Exekvera SQL-kommandot
            }
        }
}    

    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public int Company { get; set; }
        public string CompanyName { get; set; }
        public bool CsRep { get; set; }
        public bool IsAdmin { get; set; }
        public int ChatId { get; set; }
    }
