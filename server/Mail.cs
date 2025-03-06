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
        string encodedEmail = Uri.EscapeDataString(ticketinformation.email);


        var bodyBuilder = new BodyBuilder();
        
        bodyBuilder.HtmlBody = @$"
        <html>
        <body>
            <h2>Tack för ditt ärende!</h2>
            <p>Vi har nu tagit emot ditt ärende #{ticketinformation.chatid}.</p>
            <p>{ticketinformation.description}</p>
            <p> För att få tillgång till din chat går du in: <a href='http://localhost:5173/guestlogin/{ticketinformation.chatid}?email={encodedEmail}'>HÄR</a>
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
        string encodedEmail = Uri.EscapeDataString(email);
        
        
        bodyBuilder.HtmlBody = $@"
        <html>
        <body>
            <h2>Ditt ärende har uppdaterats!</h2>
            <p>Ditt ärende har fått ett svar, klicka på länken <a href='http://localhost:5173/guestlogin/{chatid}?email={encodedEmail}'>HÄR</a>.</p>
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
    
    /*
    
    public async Task NewCSRep()
    {
                MimeMessage mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress("TechEeasSolution", "kundtjanstssontest@gmail.com"));
        mimeMessage.To.Add(MailboxAddress.Parse());
        mimeMessage.Subject = "Välkommen till .....";


        var bodyBuilder = new BodyBuilder();
        
        bodyBuilder.HtmlBody = @$"
        <html>
        <body>
            <h2>Välkommen till ......</h2>
            <p>Var vänlig och byta dit lösenord, tryck på länken nedan så kommer du att skickas vidare.</p>
            <a href=""> <p>Hejsan<p> </a>
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
                client.Authenticate("kundtjanstssontest@gmail.com", "iitp gitd mlha yvvp");
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
     
    */
    
}
