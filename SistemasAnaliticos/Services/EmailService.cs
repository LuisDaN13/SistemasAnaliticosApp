using Azure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using SistemasAnaliticos.Entidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemasAnaliticos.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody);
    }

    public sealed class EmailService : IEmailService, IDisposable
    {
        private readonly ConfiguracionEmail _settings;
        private readonly GraphServiceClient _graphClient;
        private bool _disposed;

        public EmailService(IOptions<ConfiguracionEmail> options)
        {
            _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));

            // Validaciones mínimas
            if (string.IsNullOrWhiteSpace(_settings.TenantId))
                throw new ArgumentException("TenantId es requerido");
            if (string.IsNullOrWhiteSpace(_settings.ClientId))
                throw new ArgumentException("ClientId es requerido");
            if (string.IsNullOrWhiteSpace(_settings.ClientSecret))
                throw new ArgumentException("ClientSecret es requerido");
            if (string.IsNullOrWhiteSpace(_settings.FromEmail))
                throw new ArgumentException("FromEmail es requerido");

            // Autenticación moderna (Graph v5)
            var credential = new ClientSecretCredential(
                _settings.TenantId,
                _settings.ClientId,
                _settings.ClientSecret
            );

            _graphClient = new GraphServiceClient(credential);
        }

        public async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("toEmail no puede estar vacío");
            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException("subject no puede estar vacío");
            if (string.IsNullOrWhiteSpace(htmlBody))
                throw new ArgumentException("htmlBody no puede estar vacío");

            var message = new Message
            {
                Subject = subject,
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = htmlBody
                },
                ToRecipients = new List<Recipient>
                {
                    new Recipient
                    {
                        EmailAddress = new EmailAddress
                        {
                            Address = toEmail,
                            Name = toName
                        }
                    }
                }
            };

            try
            {
                await _graphClient.Users[_settings.FromEmail]
                    .SendMail
                    .PostAsync(new Microsoft.Graph.Users.Item.SendMail.SendMailPostRequestBody
                    {
                        Message = message,
                        SaveToSentItems = true
                    });
            }
            catch (ServiceException ex)
            {
                throw new InvalidOperationException(
                    $"Error al enviar correo con Graph API: {ex.Message}", ex);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }
    }
}
