using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Scheduly.Application.Common.Interfaces;

namespace Scheduly.Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<EmailSettings> settings, ILogger<SmtpEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendChargeEmailAsync(
        string customerEmail,
        string customerName,
        int amountInCents,
        string referenceNumber,
        DateTime appointmentDate,
        string serviceName,
        CancellationToken cancellationToken = default)
    {
        var amount = amountInCents / 100m;
        var subject = $"Cobrança - {referenceNumber}";
        var body = $"""
            <html>
            <body style="font-family: 'Inter', Arial, sans-serif; background: #f9fafb; padding: 40px;">
                <div style="max-width: 600px; margin: 0 auto; background: white; border-radius: 12px; overflow: hidden; box-shadow: 0 1px 3px rgba(0,0,0,0.1);">
                    <div style="background: linear-gradient(135deg, #4F46E5, #3730A3); padding: 32px; text-align: center;">
                        <h1 style="color: white; margin: 0; font-size: 24px;">Scheduly</h1>
                        <p style="color: rgba(255,255,255,0.8); margin: 8px 0 0;">Confirmação de Agendamento</p>
                    </div>
                    <div style="padding: 32px;">
                        <p style="color: #374151; font-size: 16px;">Olá <strong>{customerName}</strong>,</p>
                        <p style="color: #6B7280;">Seu agendamento foi registrado com sucesso. Segue os detalhes:</p>

                        <div style="background: #F9FAFB; border-radius: 8px; padding: 20px; margin: 24px 0;">
                            <table style="width: 100%; border-collapse: collapse;">
                                <tr>
                                    <td style="padding: 8px 0; color: #6B7280;">Serviço</td>
                                    <td style="padding: 8px 0; color: #111827; font-weight: 600; text-align: right;">{serviceName}</td>
                                </tr>
                                <tr>
                                    <td style="padding: 8px 0; color: #6B7280;">Data</td>
                                    <td style="padding: 8px 0; color: #111827; font-weight: 600; text-align: right;">{appointmentDate:dd/MM/yyyy HH:mm}</td>
                                </tr>
                                <tr>
                                    <td style="padding: 8px 0; color: #6B7280;">Referência</td>
                                    <td style="padding: 8px 0; color: #111827; font-weight: 600; text-align: right;">{referenceNumber}</td>
                                </tr>
                                <tr style="border-top: 2px solid #E5E7EB;">
                                    <td style="padding: 12px 0 8px; color: #111827; font-weight: 700; font-size: 18px;">Valor</td>
                                    <td style="padding: 12px 0 8px; color: #4F46E5; font-weight: 700; font-size: 18px; text-align: right;">R$ {amount:N2}</td>
                                </tr>
                            </table>
                        </div>

                        <p style="color: #6B7280; font-size: 14px;">O pagamento está pendente. Por favor, efetue o pagamento para confirmar seu agendamento.</p>
                    </div>
                    <div style="background: #F9FAFB; padding: 16px 32px; text-align: center;">
                        <p style="color: #9CA3AF; font-size: 12px; margin: 0;">Este é um email automático do Scheduly.</p>
                    </div>
                </div>
            </body>
            </html>
            """;

        await SendEmailAsync(customerEmail, subject, body, cancellationToken);
    }

    public async Task SendReminderAsync(
        string customerEmail,
        string customerName,
        DateTime appointmentTime,
        CancellationToken cancellationToken = default)
    {
        var subject = "Lembrete de Agendamento - Scheduly";
        var body = $"""
            <html>
            <body style="font-family: Arial, sans-serif; padding: 20px;">
                <h2>Olá {customerName},</h2>
                <p>Este é um lembrete do seu agendamento em <strong>{appointmentTime:dd/MM/yyyy às HH:mm}</strong>.</p>
                <p>Não se esqueça! Estamos te esperando.</p>
                <br/>
                <p style="color: #999;">Scheduly</p>
            </body>
            </html>
            """;

        await SendEmailAsync(customerEmail, subject, body, cancellationToken);
    }

    private async Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        try
        {
            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
            };

            if (!string.IsNullOrEmpty(_settings.Username))
            {
                client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
            }

            var message = new MailMessage
            {
                From = new MailAddress(_settings.From, _settings.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true,
            };
            message.To.Add(to);

            await client.SendMailAsync(message, cancellationToken);
            _logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}: {Subject}", to, subject);
        }
    }
}
