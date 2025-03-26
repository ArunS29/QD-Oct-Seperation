using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

[Route("api/email")]
[ApiController]
public class EmailController : ControllerBase
{
    [HttpPost("send")]
    public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
    {
        try
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("your-email@gmail.com", "your-app-password"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("your-email@gmail.com"),
                Subject = request.EmailSubject,
                Body = request.EmailMessage,
                IsBodyHtml = false,
            };
            mailMessage.To.Add(request.EmailTo);

            await smtpClient.SendMailAsync(mailMessage);
            return Ok(new { message = "Email sent successfully!" });
        }
        catch
        {
            return BadRequest(new { message = "Error sending email!" });
        }
    }
}

public class EmailRequest
{
    public string EmailTo { get; set; }
    public string EmailSubject { get; set; }
    public string EmailMessage { get; set; }
}
