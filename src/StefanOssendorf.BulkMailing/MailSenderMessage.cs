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
    }
}