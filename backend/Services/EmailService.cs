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

        private string GetRequiredConfigValue(string key)
        {
            var value = _configuration[key];
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"Configuration value '{key}' is required for email sending.");
            }

            return value.Trim();
        }

        private string NormalizeFrontendUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new InvalidOperationException("Configuration value 'Gmail:FrontendUrl' is required for email links.");
            }

            return url.TrimEnd('/');
        }

        // =========================================================
        // VERIFICACIÓN DE CORREO
        // =========================================================
        public async Task EnviarCorreoVerificacion(string destino, string token)
        {
            var frontendUrl = NormalizeFrontendUrl(_configuration["Gmail:FrontendUrl"]);

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
            var frontendUrl = NormalizeFrontendUrl(_configuration["Gmail:FrontendUrl"]);

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
        // NOTIFICACIÓN DE RECORDATORIO DE CITA
        // =========================================================
        public async Task EnviarNotificacionRecordatorioCita(
            string destino,
            string nombrePaciente,
            string nombreDoctor,
            DateTimeOffset fechaInicio,
            string motivo,
            string tipoConsulta,
            string? linkReunion)
        {
            var fechaLocal = ConvertToElSalvadorTime(fechaInicio);
            var fechaFormateada = fechaLocal.ToString("dddd, dd 'de' MMMM yyyy 'a las' HH:mm");
            var reunionHtml = string.IsNullOrWhiteSpace(linkReunion)
                ? string.Empty
                : $@"<p><strong>Link de la videollamada:</strong><br /><a href='{linkReunion}'>{linkReunion}</a></p>";

            var html = $@"
                <div style='font-family: Arial, sans-serif;'>
                    <h2>Recordatorio de tu cita con TelMed™</h2>
                    <p>Hola {nombrePaciente},</p>
                    <p>Este es un recordatorio de tu cita programada para:</p>
                    <ul>
                        <li><strong>Fecha:</strong> {fechaFormateada}</li>
                        <li><strong>Doctor:</strong> {nombreDoctor}</li>
                        <li><strong>Motivo:</strong> {motivo}</li>
                        <li><strong>Tipo de consulta:</strong> {tipoConsulta}</li>
                    </ul>
                    {reunionHtml}
                    <p style='margin-top:20px;'>
                        Si necesitas cambiar o cancelar esta cita, hazlo con al menos 24 horas de anticipación.<br />
                        Este correo ha sido enviado automáticamente, por favor no respondas a este mensaje.
                        <br />
                        TelMed & Health Lite™ System
                    </p>
                </div>";

            await EnviarCorreo(
                destino,
                "Recordatorio: Tienes una cita en 24 horas - TelMed™",
                html
            );
        }

        private static DateTimeOffset ConvertToElSalvadorTime(DateTimeOffset fecha)
        {
            try
            {
                var zonaElSalvador = TimeZoneInfo.FindSystemTimeZoneById("America/El_Salvador");
                return TimeZoneInfo.ConvertTime(fecha, zonaElSalvador);
            }
            catch
            {
                var zonaElSalvador = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
                return TimeZoneInfo.ConvertTime(fecha, zonaElSalvador);
            }
        }

        // =========================================================
        // NOTIFICACIÓN DE CANCELACIÓN DE CITA
        // =========================================================
        public async Task EnviarNotificacionCancelacionCita(
            string destino,
            string nombrePaciente,
            string nombreDoctor,
            DateTimeOffset fechaInicio,
            string motivo,
            string tipoConsulta)
        {
            var fechaFormateada = fechaInicio.ToString("dddd, dd 'de' MMMM yyyy 'a las' HH:mm");

            var html = $@"
                <div style='font-family: Arial, sans-serif;'>
                    <h2>Cita cancelada - TelMed™</h2>
                    <p>Hola {nombrePaciente},</p>
                    <p>La cita programada ha sido cancelada correctamente.</p>
                    <ul>
                        <li><strong>Fecha:</strong> {fechaFormateada}</li>
                        <li><strong>Doctor:</strong> {nombreDoctor}</li>
                        <li><strong>Motivo:</strong> {motivo}</li>
                        <li><strong>Tipo de consulta:</strong> {tipoConsulta}</li>
                    </ul>
                    <p style='margin-top:20px;'>
                        Si necesitas reprogramar, vuelve a ingresar al sistema y agenda una nueva cita.<br />
                        Este correo ha sido enviado automáticamente, por favor no respondas a este mensaje.
                        <br />
                        TelMed & Health Lite™ System
                    </p>
                </div>";

            await EnviarCorreo(
                destino,
                "Tu cita ha sido cancelada - TelMed™",
                html
            );
        }

        public async Task EnviarNotificacionCancelacionAdmin(
            string nombrePaciente,
            string nombreDoctor,
            DateTimeOffset fechaInicio,
            string motivo,
            string tipoConsulta)
        {
            var adminDestino = _configuration["Gmail:SenderEmail"];
            if (string.IsNullOrWhiteSpace(adminDestino))
                return;

            var fechaFormateada = fechaInicio.ToString("dddd, dd 'de' MMMM yyyy 'a las' HH:mm");

            var html = $@"
                <div style='font-family: Arial, sans-serif;'>
                    <h2>Notificación: cita cancelada</h2>
                    <p>Se ha cancelado una cita programada.</p>
                    <ul>
                        <li><strong>Paciente:</strong> {nombrePaciente}</li>
                        <li><strong>Doctor:</strong> {nombreDoctor}</li>
                        <li><strong>Fecha:</strong> {fechaFormateada}</li>
                        <li><strong>Motivo:</strong> {motivo}</li>
                        <li><strong>Tipo de consulta:</strong> {tipoConsulta}</li>
                    </ul>
                    <p style='margin-top:20px;'>
                        Este correo ha sido enviado automáticamente, por favor no respondas a este mensaje.
                    </p>
                </div>";

            await EnviarCorreo(
                adminDestino,
                "Notificación de cita cancelada - TelMed™",
                html
            );
        }

        public async Task EnviarCorreoIncapacidad(
            string destino,
            string nombrePaciente,
            string nombreDoctor,
            DateTime fechaInicio,
            DateTime fechaFin,
            int dias,
            string motivo,
            string observaciones,
            byte[]? pdfAdjunto = null,
            string? nombreAdjunto = null)
        {
            var fechaInicioFormateada = fechaInicio.ToString("dd/MM/yyyy");
            var fechaFinFormateada = fechaFin.ToString("dd/MM/yyyy");

            var html = $@"
                <div style='font-family: Arial, sans-serif;'>
                    <h2>Incapacidad médica - TelMed™</h2>
                    <p>Hola {nombrePaciente},</p>
                    <p>Tu incapacidad ha sido generada correctamente.</p>
                    <ul>
                        <li><strong>Doctor:</strong> {nombreDoctor}</li>
                        <li><strong>Desde:</strong> {fechaInicioFormateada}</li>
                        <li><strong>Hasta:</strong> {fechaFinFormateada}</li>
                        <li><strong>Días:</strong> {dias}</li>
                        <li><strong>Motivo:</strong> {motivo}</li>
                    </ul>
                    {(string.IsNullOrWhiteSpace(observaciones) ? string.Empty : $"<p><strong>Observaciones:</strong> {observaciones}</p>")}
                    <p style='margin-top:20px;'>
                        Si no puedes descargarla desde el sistema, revisa tu bandeja de entrada o contacta a tu médico.
                        <br />
                        Este correo ha sido enviado automáticamente, por favor no respondas a este mensaje.
                    </p>
                </div>";

            await EnviarCorreo(
                destino,
                "Tu incapacidad médica en TelMed™",
                html,
                pdfAdjunto,
                nombreAdjunto
            );
        }

        // =========================================================
        // ENVÍO GENERAL SMTP
        // =========================================================
        private async Task EnviarCorreo(
            string destino,
            string asunto,
            string html,
            byte[]? archivoAdjunto = null,
            string? nombreAdjunto = null)
        {
            if (string.IsNullOrWhiteSpace(destino))
            {
                throw new ArgumentException("Destination email address is required.", nameof(destino));
            }

            var senderEmail = GetRequiredConfigValue("Gmail:SenderEmail");
            var appPassword = GetRequiredConfigValue("Gmail:AppPassword");

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

            if (archivoAdjunto != null && !string.IsNullOrWhiteSpace(nombreAdjunto))
            {
                builder.Attachments.Add(nombreAdjunto, archivoAdjunto, new ContentType("application", "pdf"));
            }

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