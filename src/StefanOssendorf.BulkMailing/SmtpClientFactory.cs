using System;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Mail;

namespace StefanOssendorf.BulkMailing {
    /// <summary>
    /// Default implementation of <see cref="ISmtpClientFactory"/> to utilize <see cref="SmtpClient"/>.
    /// </summary>
    public class SmtpClientFactory : ISmtpClientFactory {
        #region [ Fields ]
        private readonly SmtpClientConfiguration mConfiguration;
        #endregion
        #region [ Constructors ]
        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpClientFactory"/> class by using configuration file settings. 
        /// </summary>
        public SmtpClientFactory()
            : this(new EmptySmtpClientConfiguration()) {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpClientFactory"/> class by using the provided configuration.
        /// </summary>
        /// <param name="configuration"></param>
        public SmtpClientFactory(SmtpClientConfiguration configuration) {
            Contract.Requires<ArgumentNullException>(configuration.IsNotNull());
            mConfiguration = configuration.Clone();
        }
        #endregion
        #region Implementation of ISmtpClientFactory
        /// <summary>
        /// Creates a new <see cref="ISmtpClient"/> for usage.
        /// </summary>
        /// <returns>Return a new instance of an <see cref="ISmtpClient"/> implementation.</returns>
        public ISmtpClient Create() {
            var smtp = new SmtpClientWrapper();
            if (mConfiguration.IsEmpty) {
                return smtp;
            }
            // Set UseDefaultCredentials before Credentials otherwise the credentials are overwritten
            smtp.UseDefaultCredentials = mConfiguration.UseDefaultCredentials;
            smtp.Port = mConfiguration.Port;
            smtp.Host = mConfiguration.Host;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            if (!smtp.UseDefaultCredentials) {
                smtp.Credentials = new NetworkCredential(mConfiguration.UserName, mConfiguration.Password);
            }
            smtp.EnableSsl = mConfiguration.EnableSsl;
            return smtp;
        }
        #endregion
    }
}