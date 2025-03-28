using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QD.ERP.Web.Areas.Utility.Controllers
{
    [Route("api/Email")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly ILogger<EmailController> _logger;

        public EmailController(ILogger<EmailController> logger)
        {
            _logger = logger;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendEmail([FromForm] EmailRequest emailRequest)
        {
            if (string.IsNullOrEmpty(emailRequest.To) ||
                string.IsNullOrEmpty(emailRequest.Subject) ||
                string.IsNullOrEmpty(emailRequest.Body))
            {
                return BadRequest(new { success = false, message = "All fields are required." });
            }

            _logger.LogInformation($"Sending email to {emailRequest.To} with subject {emailRequest.Subject}");

            // Extract file attachments
            var attachments = new List<IFormFile>();
            if (emailRequest.Attachments != null)
            {
                attachments.AddRange(emailRequest.Attachments);
            }

            // ✅ Fixed: Explicitly passing 'null' for OTP
            bool success = await EmailHelper.SendEmailAsync(
                emailRequest.To,
                emailRequest.Subject,
                emailRequest.Body,
                null, // ✅ OTP explicitly set to null
                attachments // ✅ Attachments passed in correct order
            );

            if (success)
            {
                return Ok(new { success = true, message = "Email sent successfully." });
            }
            else
            {
                _logger.LogError($"Failed to send email to {emailRequest.To}");
                return StatusCode(500, new { success = false, message = "Failed to send email." });
            }
        }
    }

    public class EmailRequest
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public List<IFormFile> Attachments { get; set; }
    }
}
