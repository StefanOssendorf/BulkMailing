using System;
using System.Net.Mail;

namespace StefanOssendorf.BulkMailing {
    /// <summary>
    /// Represents the result of a mailing from <see cref="MailSender"/>
    /// </summary>
    public class MailSendResult {
        /// <summary>
        /// Gets a value indicating that the sending has been successful.
        /// </summary>
        public bool Successful { get; internal set; }
        /// <summary>
        /// Gets a value indicating that the sending has been cancelled.
        /// </summary>
        public bool Canceled { get; internal set; }
        /// <summary>
        /// Gets a value indicating that an error has occurred.
        /// </summary>
        public bool HasError { get { return Exception.IsNotNull(); } }
        /// <summary>
        /// Gets a value indicating which error occurred during sending.
        /// </summary>
        public Exception Exception { get; internal set; }
        /// <summary>
        /// The message which was send.
        /// </summary>
        public MailMessage MailMessage { get; private set; }
        /// <summary>
        /// The user-defined object.
        /// </summary>
        public object UserIdentifier { get; private set; }
        internal MailSendResult(MailSenderMessage message) {
            MailMessage = message.Message;
            UserIdentifier = message.UserIdentifier;
        }
    }
}