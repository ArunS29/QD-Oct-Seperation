using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace QD.ERP.Web.Areas.Utility.Controllers
{
    [Route("api/Email")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly ILogger<EmailController> _logger;
        private readonly IConfiguration _configuration;

        public EmailController(ILogger<EmailController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
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

            var attachments = new List<IFormFile>();

            // Use server-generated attachment from session (path set during report generation)
            var serverFilePath = HttpContext.Session.GetString("EmailAttachmentPath");

            if (!string.IsNullOrEmpty(serverFilePath) && System.IO.File.Exists(serverFilePath))
            {
                _logger.LogInformation($"Using session-stored attachment path: {serverFilePath}");

                IFormFile fileAttachment;

                await using (var fileStream = new FileStream(serverFilePath, FileMode.Open, FileAccess.Read))
                {
                    var memoryStream = new MemoryStream();
                    await fileStream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    fileAttachment = new FormFile(memoryStream, 0, memoryStream.Length, "ServerGeneratedAttachment", Path.GetFileName(serverFilePath))
                    {
                        Headers = new HeaderDictionary(),
                        ContentType = "application/pdf"
                    };
                }

                attachments.Add(fileAttachment);

                try
                {
                    System.IO.File.Delete(serverFilePath);
                    _logger.LogInformation($"Deleted temp file: {serverFilePath}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to delete temp file: {ex.Message}");
                }

            }
            else
            {
                _logger.LogWarning("Attachment file path is missing from session or file not found.");
            }

            bool success = await EmailHelper.SendEmailAsync(
                emailRequest.To,
                emailRequest.Subject,
                emailRequest.Body,
                null,
                attachments
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
        public string ServerGeneratedAttachment { get; set; } // Only this is used
    }
}
