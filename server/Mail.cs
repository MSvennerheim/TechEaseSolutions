namespace server;

using MailKit.Net.Smtp;
using MailKit;
using MimeKit;
using System.IO;

public class Mail
{
    public void generateNewIssue(Ticket ticketinformation)
    {
        MimeMessage mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress("TechEeasSolution", "kundtjanstssontest@gmail.com"));
        mimeMessage.To.Add(MailboxAddress.Parse(ticketinformation.email));
        mimeMessage.Subject = "Tack för att du skickat in ditt ärende till TechEaseSolution";


        var bodyBuilder = new BodyBuilder();
        
        bodyBuilder.HtmlBody = @$"
        <html>
        <body>
            <h2>Tack för ditt ärende!</h2>
            <p>Vi har nu tagit emot ditt ärende #{ticketinformation.chatid}.</p>
            <p>{ticketinformation.description}</p>
            <p> För att få tillgång till din chat går du in: <a href='http://localhost:5173/Chat/{ticketinformation.chatid}'>HÄR</a>
            <p>Kundtjänst kommer svara så fort de kan.</p>
            <p><img src='cid:image1'></p>
            <p>Svara inte på detta mejlet, det är autogenererat</p>
        </body>
        </html>";

        
        string imagePath = "TecheaseSolutionslogo.png";
        if (File.Exists(imagePath))
        {
            var image = bodyBuilder.LinkedResources.Add(imagePath);
            image.ContentId = "image1";
        }
        else
        {
            Console.WriteLine("Image file not found: " + imagePath);
        }

        // Set the email body
        mimeMessage.Body = bodyBuilder.ToMessageBody();

        using (SmtpClient client = new SmtpClient())
        {
            try
            {
                client.Connect("smtp.gmail.com", 465, true);
                client.Authenticate("kundtjanstssontest@gmail.com", "iitp gitd mlha yvvp"); // ⚠️ Consider using environment variables for security
                client.Send(mimeMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
                throw;
            }
            finally
            {
                client.Disconnect(true);
                client.Dispose();
            }
        }
    }

    public async Task<bool> emailConfirmationOnAnswer(string email, int chatid)
    {

        MimeMessage mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress("test", "kundtjanstssontest@gmail.com"));
        
        //mimeMessage.To.Add(MailboxAddress.Parse("max.svennerheim@gmail.com"));
        mimeMessage.To.Add(MailboxAddress.Parse(email));
        
        mimeMessage.Subject = "Ditt ärende har uppdaterats";
            
        var bodyBuilder = new BodyBuilder();

        // add hyperlink to chat
        bodyBuilder.HtmlBody = $@"
        <html>
        <body>
            <h2>Ditt ärende har uppdaterats!</h2>
            <p>Ditt ärende har fått ett svar, klicka på länken HÄR #{chatid}.</p>
            <p>Svara inte på detta mejlet, det är autogenererat</p>
        </body>
        </html>";
        
        mimeMessage.Body = bodyBuilder.ToMessageBody();

        SmtpClient client = new SmtpClient();
        try
        {
            client.Connect("smtp.gmail.com", 465, true);
            client.Authenticate("kundtjanstssontest@gmail.com", "iitp gitd mlha yvvp");
            client.Send(mimeMessage);
                
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
        finally
        {
            client.Disconnect(true);
            client.Dispose();
        }
        
        return true;
    }

    public async Task<bool> SendNewCSRepWelcomeEmail(string email, string tempToken, string companyName)
    {
        MimeMessage mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress ("TechEaseSolution", "kundtjanstssontest@gmail.com"));
        mimeMessage.To.Add(MailboxAddress.Parse(email));
        mimeMessage.Subject = $"Välkommen till {companyName}, kundtjanstssontest@gmail.com";
        
        var bodyBuilder = new BodyBuilder();
        
        //Skapa en url med token
        string resetUrl = $"http://localhost:5173/reset-password?token={tempToken}&email={Uri.EscapeDataString(email)}";
        
        bodyBuilder.HtmlBody = $@"
        <html>
        <body>
        <h2> Välkommen till {companyName} Kundtjänstsystem</h2>
        <p>Ett konto har skapats för dig i vårt system.</p>
        <p>Var vänlig och skapa ditt lösenord genom att klicka på länken nedan. Länken är giltig i 24 timmar.</p>
        <p><a href=""{resetUrl}"">Skapa ditt lösenord</p>
        <p><img src ='cid:image1'</p>
        <p>Svara inte på detta mejl, det är autogenererat</p>
        </body>
        </html>";
          
        string imagePath = "TecheaseSolutionslogo.png";
        if (File.Exists(imagePath))
        {
            var image = bodyBuilder.LinkedResources.Add(imagePath);
            image.ContentId = "image1";
        }
        else
        {
            Console.WriteLine("Image file not found: " + imagePath);
        }

        // Set the email body
        mimeMessage.Body = bodyBuilder.ToMessageBody();

        using (SmtpClient client = new SmtpClient())
        {
            try
            {
                client.Connect("smtp.gmail.com", 465, true);
                client.Authenticate("kundtjanstssontest@gmail.com", "iitp gitd mlha yvvp"); // Consider using environment variables
                client.Send(mimeMessage);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
                return false;
            }
            finally
            {
                client.Disconnect(true);
                client.Dispose();
            }
        
        } 
    }
    
}    

