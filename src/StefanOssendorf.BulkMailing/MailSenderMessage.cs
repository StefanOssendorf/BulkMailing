using System;
using System.Diagnostics.Contracts;
using System.Net.Mail;

namespace StefanOssendorf.BulkMailing {
    /// <summary>
    /// Allows to identify a <see cref="MailMessage"/>.
    /// </summary>
    public class MailSenderMessage {
        /// <summary>
        /// A user-defined object.
        /// </summary>
        public object UserIdentifier { get; set; }
        /// <summary>
        /// A <see cref="MailMessage"/> that contains the message to send.
        /// </summary>
        public MailMessage Message { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="MailSenderMessage"/> class .
        /// </summary>
        public MailSenderMessage() {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="MailSender"/> class with a <paramref name="message"/>.
        /// </summary>
        /// <param name="message"></param>
        public MailSenderMessage(MailMessage message) : this(message, null) {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="MailSender"/> with a <paramref name="message"/> and an <paramref name="userIdentifier"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="userIdentifier"></param>
        public MailSenderMessage(MailMessage message, object userIdentifier) {
            Contract.Requires<ArgumentNullException>(message != null);
            Message = message;
            UserIdentifier = userIdentifier;
        }
    }
}