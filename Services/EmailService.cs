using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MimeKit;

namespace TelMedAPI.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // =========================================================
        // VERIFICACIÓN DE CORREO
        // =========================================================
        public async Task EnviarCorreoVerificacion(string destino, string token)
        {
            var frontendUrl = _configuration["Gmail:FrontendUrl"];

            var verificationLink =
                $"{frontendUrl}/verify-email?token={token}";

            var html = $@"
                <div style='font-family: Arial, sans-serif;'>
                    <h2>Verifica tu cuenta en TelMed</h2>
                    <p>Gracias por registrarte ♡</p>
                    <p>Haz clic en el siguiente botón para confirmar tu correo:</p>

                    <a href='{verificationLink}'
                       style='
                            display:inline-block;
                            padding:12px 20px;
                            background:#2563eb;
                            color:white;
                            text-decoration:none;
                            border-radius:8px;
                            font-weight:bold;
                       '>
                        Confirma tu correo
                    </a>

                    <p style='margin-top:20px;'>
                        Si no solicitaste esta cuenta, puedes ignorar este correo.
                        <br />
                        Este correo ha sido enviado automáticamente, por favor no respondas a este mensaje.
                    </p>
                </div>";

            await EnviarCorreo(
                destino,
                "Verifica tu cuenta - TelMed Lite™ System",
                html
            );
        }

        // =========================================================
        // RECUPERACIÓN DE CONTRASEÑA
        // =========================================================
        public async Task EnviarCorreoRecuperacion(string destino, string token)
        {
            var frontendUrl = _configuration["Gmail:FrontendUrl"];

            var resetLink =
                $"{frontendUrl}/reset-password?token={token}";

            var html = $@"
                <div style='font-family: Arial, sans-serif;'>
                    <h2>Recuperación de contraseña - TelMed Lite™ System</h2>

                    <p>Recibimos una solicitud para restablecer tu contraseña.</p>
                    <p>Haz click en el siguiente botón para restablecer tu contraseña:</p>

                    <a href='{resetLink}'
                       style='
                            display:inline-block;
                            padding:12px 20px;
                            background:#dc2626;
                            color:white;
                            text-decoration:none;
                            border-radius:8px;
                            font-weight:bold;
                       '>
                        Restablecer contraseña
                    </a>

                    <p style='margin-top:20px;'>
                        Si no solicitaste este cambio, puedes ignorar este correo.
                        <br />
                        Este correo ha sido enviado automáticamente, por favor no respondas a este mensaje.
                    </p>
                </div>";

            await EnviarCorreo(
                destino,
                "Recuperar contraseña - TelMed Lite™ System",
                html
            );
        }

        // =========================================================
        // MÉTODO GENERAL GMAIL API
        // =========================================================
        private async Task EnviarCorreo(
            string destino,
            string asunto,
            string html)
        {
            var senderEmail = _configuration["Gmail:SenderEmail"];
            var credentialsPath = _configuration["Gmail:CredentialsPath"];
            var tokenPath = _configuration["Gmail:TokenPath"];
            var applicationName = _configuration["Gmail:ApplicationName"];

            string[] scopes = {
                GmailService.Scope.GmailSend
            };

            using var stream = new FileStream(
                credentialsPath,
                FileMode.Open,
                FileAccess.Read
            );

            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(tokenPath, true)
            );

            var service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = applicationName
            });

            var email = new MimeMessage();

            email.From.Add(MailboxAddress.Parse(senderEmail));
            email.To.Add(MailboxAddress.Parse(destino));
            email.Subject = asunto;

            var builder = new BodyBuilder
            {
                HtmlBody = html
            };

            email.Body = builder.ToMessageBody();

            using var memoryStream = new MemoryStream();
            await email.WriteToAsync(memoryStream);

            var rawMessage = Convert.ToBase64String(memoryStream.ToArray())
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");

            var gmailMessage = new Message
            {
                Raw = rawMessage
            };

            await service.Users.Messages.Send(
                gmailMessage,
                "me"
            ).ExecuteAsync();
        }
    }
}