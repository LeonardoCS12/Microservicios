using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

/// <summary >
/// Servicio encargado de enviar correos electrónicos para restablecimiento de
/// contraseña.
/// </summary >
public class PasswordResetEmail
{
// Configuración del servidor SMTP
private readonly string _smtpServer = "://gmail.com"; // O el servidor de
// tu elección
private readonly int _smtpPort = 587; // Puerto SMTP
private readonly string _smtpUser = "arroyovelascofernandooctavio@gmail.com "; // Tu correo electrónico
private readonly string _smtpPassword = "thyfinpiqcgfenxu"; // Tu
// contraseña de correo o App Password
/// <summary >
/// Envía un correo electrónico con un enlace para restablecer la
/// contraseña.
/// </summary >
/// <param name="email">Correo electrónico del destinatario.</param >
/// <param name="token">Token seguro para restablecimiento de contraseña.</param >
/// <returns >Tarea asincrónica que representa el envío del correo.</returns >
public async Task SendPasswordResetEmail(string email , string token)
{
// Construcción del enlace de restablecimiento
var resetLink = $"https://tusitio.com -password?token={token}"; //
// Enlace de restablecimiento con token
// Creación del mensaje de correo
var mailMessage = new MailMessage
{
From = new MailAddress(_smtpUser),
Subject = "Restablecimiento de Contraseña",
Body = $"<p>Haz clic en el siguiente enlace para restablecer tu contraseña:</p><a href=\"{resetLink}\">Restablecer contraseña </a >",
IsBodyHtml = true
};

mailMessage.To.Add(email);
// Envío del correo usando SmtpClient
using (var smtpClient = new SmtpClient(_smtpServer , _smtpPort))
{
smtpClient.Credentials = new NetworkCredential(_smtpUser ,
_smtpPassword);
smtpClient.EnableSsl = true;// Habilita cifrado SSL
await smtpClient.SendMailAsync(mailMessage);
}
}
}
