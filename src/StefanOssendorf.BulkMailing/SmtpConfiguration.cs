using System;
using System.Net;
using System.Net.Mail;

namespace StefanOssendorf.BulkMailing {
    /// <summary>
    /// Represents the SMTP-configuration.
    /// </summary>
    public class SmtpConfiguration {
        internal virtual Boolean IsEmpty { get { return false; } }
        /// <summary>
        /// Specify whether the <see cref="SmtpClient"/> uses Secure Sockets Layer (SSL) to encrypt the connection.
        /// </summary>
        public bool EnableSsl { get; set; }
        /// <summary>
        /// Gets or sets the name or IP address of the host used for SMTP transactions.
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// Gets or sets the port used for SMTP transactions.
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// Gets or sets a <see cref="bool"/> value that controls whether the <see cref="CredentialCache.DefaultCredentials">DefaultCredentials</see> are sent with requests.
        /// </summary>
        public bool UseDefaultCredentials { get; set; }
        /// <summary>
        /// Gets or sets the username used to authenticate the sender.
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// Gets or sets the password used to authenticate the sender.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Initialize a new instance of the <see cref="SmtpConfiguration"/> class.
        /// </summary>
        public SmtpConfiguration() {
            Port = 25;
        }

        internal virtual SmtpConfiguration Clone() {
            return new SmtpConfiguration {
                EnableSsl = EnableSsl,
                Host = Host,
                Password = Password,
                Port = Port,
                UseDefaultCredentials = UseDefaultCredentials,
                UserName = UserName
            };
        }
    }
}