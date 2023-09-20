using System.Net;
using MailKit.Net.Smtp;
using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MimeKit;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using CloudService.Shared;
using CloudService.Services;

namespace SendEmailFunction
{
    public class EmailSenderFunction
    {
        private readonly ISmtpClient _smtpClient;
        private readonly ILogger<EmailSenderFunction> _logger;

        public EmailSenderFunction(ISmtpClient smtpClient, ILogger<EmailSenderFunction> logger)
        {
            _smtpClient = smtpClient;
            _logger = logger;
        }

        [Function("EmailSenderFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
        {
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var emailModel = JsonConvert.DeserializeObject<EmailModel>(requestBody);

                if (emailModel == null)
                {
                    return new BadRequestResult();
                }

                var azureBlobService = new AzureBlobService();
                var filesWithSasTokens = await azureBlobService.ListAsync();

                var emailBody = new StringBuilder();
                emailBody.AppendLine("File(s) uploaded successfully. Click the links below to access the files:");

                foreach (var file in filesWithSasTokens)
                {
                    emailBody.AppendLine($"<a href=\"{file.Uri}\">{file.Name}</a>");
                }

                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse("deshiro.chi@gmail.com"));
                email.To.Add(MailboxAddress.Parse(emailModel.To));
                email.Subject = emailModel.Subject;
                email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = emailBody.ToString() };

                string smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST")!;
                int smtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT")!)!;
                string smtpUsername = Environment.GetEnvironmentVariable("SMTP_USERNAME")!;
                string smtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD")!;

                await _smtpClient.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await _smtpClient.AuthenticateAsync(smtpUsername, smtpPassword);
                await _smtpClient.SendAsync(email);
                await _smtpClient.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully.");
                return new OkResult();
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                return new BadRequestObjectResult($"Error sending email: {ex.Message}");
            }
        }
    }
}
