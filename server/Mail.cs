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
        mimeMessage.From.Add(new MailboxAddress("Tech E Solution", "kundtjanstssontest@gmail.com"));
        mimeMessage.To.Add(MailboxAddress.Parse(ticketinformation.email));
        mimeMessage.Subject = "Tack för att du skickat in ditt ärende till Tech E Solution";

        // Create a BodyBuilder to include HTML and an embedded image
        var bodyBuilder = new BodyBuilder();

        // Add HTML body with embedded image
        bodyBuilder.HtmlBody = @$"
        <html>
        <body>
            <h2>Tack för ditt ärende!</h2>
            <p>Vi har nu tagit emot ditt ärende #{ticketinformation.chatid}.</p>
            <p>Kundtjänst kommer svara så fort de kan.</p>
            <p><img src='cid:image1'></p>
            <p>Svara inte på detta mejlet, det är autogenererat</p>
        </body>
        </html>";

        // Attach the image and set Content-ID
        string imagePath = "TecheaseSolutionslogo.png"; // Change this to the actual image path
        if (File.Exists(imagePath))
        {
            var image = bodyBuilder.LinkedResources.Add(imagePath);
            image.ContentId = "image1"; // This ID is used in the <img src='cid:image1'>
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
}
