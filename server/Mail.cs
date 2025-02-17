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
        
        mimeMessage.To.Add(MailboxAddress.Parse(email));
        
        mimeMessage.Subject = "Ditt ärende har uppdaterats";
            
        var builder = new BodyBuilder();

        builder.HtmlBody = $@"<p>Ditt ärende har uppdaterats</p> </br>
                            <a href=´http://localhost:5173/Chat/{chatid}´ >klicka här för att se ditt ärende</a>";
        
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
    
}
