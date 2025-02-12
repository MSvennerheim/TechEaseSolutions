
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
            mimeMessage.To.Add(MailboxAddress.Parse(""));
            
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
}



