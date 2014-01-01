using System;
using System.Net.Mail;

namespace StefanOssendorf.BulkMailing {
    /// <summary>
    /// Defines a method to send <see cref="MailMessage"/>.
    /// </summary>
    public interface ISmtpClient : IDisposable {
        /// <summary>
        /// Sends the specified <paramref name="mailMessage">message</paramref>.
        /// </summary>
        /// <param name="mailMessage">Message to send.</param>
        void Send(MailMessage mailMessage);
    }
}