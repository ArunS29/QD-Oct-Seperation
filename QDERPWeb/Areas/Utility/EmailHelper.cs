    using Azure.Communication.Email;
    using Azure;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;

    namespace QD.ERP.Web.Areas.Utility
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
                    if (string.IsNullOrEmpty(recipientEmail))
                        throw new ArgumentException("Recipient email cannot be null or empty.");

                    if (string.IsNullOrEmpty(subject))
                        throw new ArgumentException("Email subject cannot be null or empty.");

                    var emailSettings = _configuration.GetSection("EmailSettings").Get<EmailSettings>();

                    if (emailSettings == null || string.IsNullOrEmpty(emailSettings.ConnectionString) || string.IsNullOrEmpty(emailSettings.Sender))
                        throw new InvalidOperationException("Email settings are not configured properly.");

                    string htmlContent;

                    // **Use OTP template only for forgot password or verification emails**
                    if (!string.IsNullOrEmpty(otp) && (subject.Contains("Forgot Password", StringComparison.OrdinalIgnoreCase) ||
                                                       subject.Contains("OTP Verification", StringComparison.OrdinalIgnoreCase)))
                    {
                        htmlContent = $@"
                        <html>
                            <body>
                                <p>Hello,</p>
                                <p>Verification OTP: <strong>{otp}</strong>.</p>
                                <p>Please use this code to complete your verification process.</p>
                                <p>If you did not request this code, please ignore this email.</p>
                            </body>
                        </html>";
                    }
                    else
                    {
                        htmlContent = body; // **Use provided body content for all other emails**
                    }

                    var client = new EmailClient(emailSettings.ConnectionString);

                    var emailMessage = new EmailMessage(
                        senderAddress: emailSettings.Sender,
                        content: new EmailContent(subject) { Html = htmlContent },
                        recipients: new EmailRecipients(new List<EmailAddress> { new EmailAddress(recipientEmail) })
                    );

                    // **Handle Attachments**
                    if (attachments?.Count > 0)
                    {
                        foreach (var file in attachments)
                        {
                            if (file.Length > 0)
                            {
                                using var ms = new MemoryStream();
                                await file.CopyToAsync(ms);

                                emailMessage.Attachments.Add(new EmailAttachment(
                                    name: file.FileName,
                                    contentType: file.ContentType,
                                    content: BinaryData.FromBytes(ms.ToArray())
                                ));
                            }
                        }
                    }

                    // **Send Email**
                    await client.SendAsync(WaitUntil.Completed, emailMessage);
                    return true;
                }
                catch (Exception ex)
                {
                    // **Log error (optional, replace with actual logging mechanism)**
                    Console.WriteLine($"Error sending email: {ex.Message}");
                    return false;
                }
            }
        }

        public class EmailSettings
        {
            public string ConnectionString { get; set; }
            public int MailPort { get; set; }
            public string SenderName { get; set; }
            public string Sender { get; set; }
        }
    }
