using System.Net;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using PaquitoBot.Application.Services.Authenticate;

namespace PaquitoBot.Api.Services;

internal sealed class PasswordResetDelivery(
    IConfiguration configuration,
    IHostEnvironment environment,
    IDataProtectionProvider provider,
    ILogger<PasswordResetDelivery> logger) : IPasswordResetDelivery
{
    private readonly ITimeLimitedDataProtector _protector =
        provider.CreateProtector("PaquitoBot.PasswordReset.v1").ToTimeLimitedDataProtector();

    private IConfigurationSection Settings => configuration.GetSection("PasswordReset");

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Settings["SmtpHost"]) &&
        MailAddress.TryCreate(Settings["From"], out _) &&
        Settings.GetValue("SmtpPort", 587) is > 0 and <= 65535 &&
        TryGetResetPage(out _) &&
        (environment.IsDevelopment() || Settings.GetValue("SmtpTls", true));

    public string Protect(Guid userId, string email, string passwordHash) =>
        _protector.Protect(JsonSerializer.Serialize(new PasswordResetTicket(userId, email, passwordHash)),
            TimeSpan.FromMinutes(20));

    public PasswordResetTicket? Unprotect(string token)
    {
        try
        {
            return JsonSerializer.Deserialize<PasswordResetTicket>(_protector.Unprotect(token));
        }
        catch (Exception exception) when (exception is CryptographicException or JsonException or FormatException)
        {
            return null;
        }
    }

    public async Task SendAsync(string email, string token, CancellationToken cancellationToken)
    {
        if (!TryGetResetPage(out var page)) return;
        // Keep the token in the fragment so it isn't sent in the initial page request.
        var link = new UriBuilder(page) { Fragment = "token=" + Uri.EscapeDataString(token) }.Uri;
        try
        {
            using var message = new MailMessage(Settings["From"]!, email)
            {
                Subject = "Paquito: restablece tu contraseña",
                Body = $"Abre este enlace en los próximos 20 minutos: {link.AbsoluteUri}\n\n"
                    + "Si no solicitaste este cambio, ignora este mensaje."
            };
            var username = Settings["SmtpUsername"];
            using var smtp = new SmtpClient(Settings["SmtpHost"]!, Settings.GetValue("SmtpPort", 587))
            {
                EnableSsl = Settings.GetValue("SmtpTls", true),
                Credentials = string.IsNullOrWhiteSpace(username)
                    ? null
                    : new NetworkCredential(username, Settings["SmtpPassword"])
            };
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(10));
            await smtp.SendMailAsync(message, timeout.Token);
        }
        catch (Exception exception) when (
            exception is SmtpException or FormatException or ArgumentException ||
            exception is OperationCanceledException && !cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Password reset delivery failed ({FailureType}).", exception.GetType().Name);
        }
    }

    private bool TryGetResetPage([NotNullWhen(true)] out Uri? page) =>
        Uri.TryCreate(Settings["ResetPageUrl"], UriKind.Absolute, out page) &&
        string.IsNullOrEmpty(page.UserInfo) && string.IsNullOrEmpty(page.Query) &&
        string.IsNullOrEmpty(page.Fragment) &&
        (page.Scheme == Uri.UriSchemeHttps ||
         environment.IsDevelopment() && page.Scheme == Uri.UriSchemeHttp && page.IsLoopback);
}
