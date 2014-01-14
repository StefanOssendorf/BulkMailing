using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StefanOssendorf.BulkMailing {
    /// <summary>
    /// Allows applications to send bulk mailings by using the Simple Mail Transfer Protocol (SMTP).
    /// </summary>
    public interface IMailSender : IDisposable {
        /// <summary>
        /// Starts sending the specified <paramref name="mailMessages">messages</paramref> to an smtp server for delivery as an asynchronous operation.
        /// </summary>
        /// <param name="mailMessages">Collection of mails to send.</param>
        /// <returns>Return <see cref="Task{TResult}"/><br/>
        /// The task object representing the asynchronous operation.
        /// </returns>
        MailStreamResult StartSending(IEnumerable<MailSenderMessage> mailMessages);
        /// <summary>
        /// Starts sending the <paramref name="mailMessages">messages</paramref> to an smtp server for delivery as an asynchronous operation.
        /// </summary>
        /// <param name="mailMessages">Collection of mails to send.</param>
        /// <returns></returns>
        MailStreamResult StartSending(BlockingCollection<MailSenderMessage> mailMessages);
        /// <summary>
        /// Cancels an asynchronous operation to send an e-mail message.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        void StopSending();
    }
}