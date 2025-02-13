
namespace server;

using MailKit.Net.Smtp;
using MailKit;
using MimeKit;

public class Mail
{
    public void generateNewIssue()
    {
            MimeMessage mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress("test", "kundtjanstssontest@gmail.com"));
            
            
            // Dynamic email the user enters on the webpage
            mimeMessage.To.Add(MailboxAddress.Parse("Patrik@dbi.nu"));
            
            mimeMessage.Subject = "Yack för att du skickat in ditt ärende till Tech E Solution";
            
            mimeMessage.Body = new TextPart("plain")
            {
                Text = @"Vi har nu tagit emot dit ärende! Kundtjänst kommer svara så fort dem kan"
                
            };
            
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
                throw;
            }
            finally
            {
                client.Disconnect(true);
                client.Dispose();
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



