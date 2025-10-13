using Azure.Communication.Email;
using Azure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace QD.ERP.Shared
{
    public class EmailHelper
    {
        private static IConfiguration _configuration;

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static async Task<bool> SendEmailAsync(string recipientEmail, string subject, string body, string otp = null, List<IFormFile> attachments = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(recipientEmail))
                    throw new ArgumentException("Recipient email cannot be null or empty.");

                if (string.IsNullOrWhiteSpace(subject))
                    throw new ArgumentException("Email subject cannot be null or empty.");

                var emailSettings = _configuration.GetSection("EmailSettings").Get<EmailSettings>();

                if (emailSettings == null ||
                    string.IsNullOrWhiteSpace(emailSettings.ConnectionString) ||
                    string.IsNullOrWhiteSpace(emailSettings.Sender))
                {
                    throw new InvalidOperationException("Email settings are not configured properly.");
                }

                // Use OTP template only for specific subjects
                string htmlContent = !string.IsNullOrEmpty(otp) &&
                                     (subject.Contains("Forgot Password", StringComparison.OrdinalIgnoreCase) ||
                                      subject.Contains("OTP Verification", StringComparison.OrdinalIgnoreCase))
                    ? $@"
                        <html>
                            <body>
                                <p>Hello,</p>
                                <p>Verification OTP: <strong>{otp}</strong>.</p>
                                <p>Please use this code to complete your verification process.</p>
                                <p>If you did not request this code, please ignore this email.</p>
                            </body>
                        </html>"
                    : body;

                var client = new EmailClient(emailSettings.ConnectionString);

                var emailMessage = new EmailMessage(
                    senderAddress: emailSettings.Sender,
                    content: new EmailContent(subject) { Html = htmlContent },
                    recipients: new EmailRecipients(new List<EmailAddress> { new EmailAddress(recipientEmail) })
                );

                // ✅ Simplified attachment logic
                if (attachments?.Count > 0)
                {
                    foreach (var file in attachments)
                    {
                        if (file?.Length > 0)
                        {
                            using var ms = new MemoryStream();
                            await file.CopyToAsync(ms);

                            emailMessage.Attachments.Add(new EmailAttachment(
                                name: file.FileName,
                                contentType: string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
                                content: BinaryData.FromBytes(ms.ToArray())
                            ));
                        }
                    }
                }

                // Send Email
                await client.SendAsync(WaitUntil.Completed, emailMessage);
                return true;
            }
            catch (Exception ex)
            {
                // Optional: log properly using ILogger if available
                Console.WriteLine($"[EmailHelper] Error sending email: {ex.Message}");
                return false;
            }
        }
    }

    public class EmailSettings
    {
        public string ConnectionString { get; set; }
        public int MailPort { get; set; } // Unused - could be removed if not needed elsewhere
        public string SenderName { get; set; } // Optional - use if you want to show "From: Name <email>"
        public string Sender { get; set; }     // Required
    }
}
