using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Mail;
using System.Threading.Tasks;

namespace StefanOssendorf.BulkMailing {
    /// <summary>
    /// Allows applications to send bulk mailings by using the Simple Mail Transfer Protocol (SMTP).
    /// </summary>
    public interface IMailSender : IDisposable {
        /// <summary>
        /// Sends the specified message to an SMTP server for delivery as an asynchronous operation.
        /// </summary>
        /// <param name="mailMessage">A <see cref="MailMessage"/> that contains the message to send.</param>
        /// <returns>Returns <see cref="Task{TResult}"/><br/>
        /// The task object representing the asynchronous operation.
        /// </returns>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        Task<MailSendResult> SendAsync(MailMessage mailMessage);
        /// <summary>
        /// Sends the specified message to an SMTP server for delivery as an asynchronous operation.
        /// </summary>
        /// <param name="mailMessage">A <see cref="MailMessage"/> that contains the message to send.</param>
        /// <param name="userIdentifier">A user-defined object to identify the <paramref name="mailMessage"/></param>
        /// <returns>Returns <see cref="Task{TResult}"/><br/>
        /// The task object representing the asynchronous operation.
        /// </returns>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        Task<MailSendResult> SendAsync(MailMessage mailMessage, object userIdentifier);
        /// <summary>
        /// Sends the specified message to an SMTP server for delivery as an asynchronous operation.
        /// </summary>
        /// <param name="message">A <see cref="MailSenderMessage"/> that contains the message to send and an identifier.</param>
        /// <returns>Returns <see cref="Task{TResult}"/><br/>
        /// The task object representing the asynchronous operation.
        /// </returns>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        Task<MailSendResult> SendAsync(MailSenderMessage message);
        /// <summary>
        /// Sends the specified <paramref name="mailMessages">messages</paramref> to an smtp server for delivery as an asynchronous operation.
        /// </summary>
        /// <param name="mailMessages">Collection of mails to send.</param>
        /// <returns>Return <see cref="Task{TResult}"/><br/>
        /// The task object representing the asynchronous operation.
        /// </returns>
        Task<Collection<MailSendResult>> SendAsync(IEnumerable<MailSenderMessage> mailMessages);
        /// <summary>
        /// Sends the <paramref name="mailMessages">messages</paramref> to an smtp server for delivery as an asynchronous operation.
        /// </summary>
        /// <param name="mailMessages">Collection of mails to send.</param>
        /// <returns></returns>
        MailStreamResult SendAsync(BlockingCollection<MailSenderMessage> mailMessages);
        /// <summary>
        /// Cancels an asynchronous operation to send an e-mail message.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        void SendAsyncCancel();
    }
}