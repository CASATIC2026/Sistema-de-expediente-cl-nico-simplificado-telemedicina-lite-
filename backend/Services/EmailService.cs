using MailKit.Net.Smtp;
using MailKit.Security;
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
                    <h2>Verifica tu cuenta en TelMed™</h2>

                    <p>Gracias por registrarte a TelMed Lite™♡</p>

                    <p>
                        Haz clic en el siguiente botón para confirmar tu correo:
                    </p>

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
                        Si no solicitaste esta cuenta, puedes ignorar este correo.<br />
                        Este correo ha sido enviado automáticamente, por favor no respondas a este mensaje.
                         <br />
                        TelMed & Health Lite™ System
                    </p>
                </div>";

            await EnviarCorreo(
                destino,
                "Verifica tu cuenta - TelMed™",
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
                    <h2>Recuperación de contraseña - TelMed™</h2>

                    <p>
                        Recibimos una solicitud para restablecer tu contraseña.<br />
                        Haz clic en el siguiente botón para crear una nueva contraseña:
                    </p>

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
                        Restablece tu contraseña
                    </a>

                    <p style='margin-top:20px;'>
                        Si no solicitaste este cambio, puedes ignorar este correo.<br />
                        Este correo ha sido enviado automáticamente, por favor no respondas a este mensaje.
                        <br />
                        TelMed & Health Lite™ System
                    </p>
                </div>";

            await EnviarCorreo(
                destino,
                "Recuperar contraseña - TelMed Lite™",
                html
            );
        }

        // =========================================================
        // ENVÍO GENERAL SMTP
        // =========================================================
        private async Task EnviarCorreo(
            string destino,
            string asunto,
            string html)
        {
            var senderEmail = _configuration["Gmail:SenderEmail"];
            var appPassword = _configuration["Gmail:AppPassword"];

            var email = new MimeMessage();

            email.From.Add(
                MailboxAddress.Parse(senderEmail)
            );

            email.To.Add(
                MailboxAddress.Parse(destino)
            );

            email.Subject = asunto;

            var builder = new BodyBuilder
            {
                HtmlBody = html
            };

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                "smtp.gmail.com",
                587,
                SecureSocketOptions.StartTls
            );

            await smtp.AuthenticateAsync(
                senderEmail,
                appPassword
            );

            await smtp.SendAsync(email);

            await smtp.DisconnectAsync(true);
        }
    }
}