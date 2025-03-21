using Azure.Communication.Email;
using Azure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.DAL.Entities;
using Microsoft.Extensions.Logging;
using QD.ERP.Web.Service;
using Microsoft.Extensions.Caching.Memory;
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




        public static async Task<bool> SendEmailAsync(string recipientEmail, string subject, string otp)
        {





            // Construct the HTML content for the email
            var htmlContent = $@"
        <html>
            <body>
                <p>Hello,</p>
                <p>verification OTP <strong>{otp}</strong>.</p>
                <p>Please use this code to complete your verification process.</p>
                <p>If you did not request this code, please ignore this email.</p>
            </body>
        </html>";

            var emailSettings = _configuration.GetSection("EmailSettings").Get<EmailSettings>();

            var client = new EmailClient(emailSettings.ConnectionString);

            var emailMessage = new EmailMessage(
                senderAddress: emailSettings.Sender,
                content: new EmailContent(subject)
                {
                    Html = htmlContent,
                },
                recipients: new EmailRecipients(new List<EmailAddress> { new EmailAddress(recipientEmail) })
            );

            try
            {
                // Send the email
                await client.SendAsync(WaitUntil.Completed, emailMessage);
                return true;
            }
            catch (Exception)
            {
                // Handle exception (log it, rethrow it, etc.)
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
