using System;
using System.Net;
using System.Net.Mail;

namespace StefanOssendorf.BulkMailing {
    internal class SmtpClientWrapper : ISmtpClient {
        #region [ Fields ]
        private bool mIsDisposed;
        private readonly SmtpClient mClient;
        #endregion
        #region [ Constructor ]
        public SmtpClientWrapper()
            : this(new SmtpClient()) {
        }
        /// <summary>
        /// For testing purpose.
        /// </summary>
        /// <param name="client"></param>
        internal SmtpClientWrapper(SmtpClient client) {
            mClient = client;
        }
        #endregion
        #region Implementation of ISmtpClient
        public void Send(MailMessage mailMessage) {
            mClient.Send(mailMessage);
        }
        #endregion
        public bool UseDefaultCredentials { get { return mClient.UseDefaultCredentials; } set { mClient.UseDefaultCredentials = value; } }
        public int Port { get { return mClient.Port; } set { mClient.Port = value; } }
        public string Host { get { return mClient.Host; } set { mClient.Host = value; } }
        public SmtpDeliveryMethod DeliveryMethod { get { return mClient.DeliveryMethod; } set { mClient.DeliveryMethod = value; } }
        public ICredentialsByHost Credentials { get { return mClient.Credentials; } set { mClient.Credentials = value; } }
        public bool EnableSsl { get { return mClient.EnableSsl; } set { mClient.EnableSsl = value; } }

        #region Implementation of IDisposable
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing) {
            if (!disposing || mIsDisposed) {
                return;
            }
            mClient.Dispose();
            mIsDisposed = true;
        }
        #endregion
    }
}