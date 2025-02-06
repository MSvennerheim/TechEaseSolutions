using System.ComponentModel.DataAnnotations;
using MimeKit;

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
            
            mimeMessage.Subject = "tjenixen";
            
            mimeMessage.Body = new TextPart("plain")
            {
                Text = @"asdasdasdasdasda"
            };
            
            SmtpClient client = new SmtpClient();
            try
            {
                client.Connect("smtp.gmail.com", 465, true);
                client.Authenticate("kundtjanstssontest@gmail.com", "pfbq mnjw ifry qdyh");
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



