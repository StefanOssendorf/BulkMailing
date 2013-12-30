using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace StefanOssendorf.BulkMailing {
    /// <summary>
    /// Allows applications to send bulk mailings by using the Simple Mail Transfer Protocol (SMTP).
    /// </summary>
    public class MailSender : IDisposable {
        #region [ Fields ]
        private readonly SmtpConfiguration mSmtpConfiguration;
        private readonly CancellationTokenSource mCancellationTokenSource;
        private readonly ConcurrentQueue<SmtpClient> mSmtpClients;
        private bool mIsDisposed;
        #endregion
        /// <summary>
        /// Initializes a new instance of the <see cref="MailSender"/> class by using configuration file settings. 
        /// </summary>
        public MailSender()
            : this(new EmptySmtpConfiguration()) {

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="MailSender"/> class by using the provided configuration.
        /// </summary>
        /// <param name="smtpConfiguration"></param>
        public MailSender(SmtpConfiguration smtpConfiguration) {
            Contract.Requires<ArgumentNullException>(smtpConfiguration != null);
            mSmtpConfiguration = smtpConfiguration.Clone();
            mCancellationTokenSource = new CancellationTokenSource();
            mSmtpClients = new ConcurrentQueue<SmtpClient>();
        }

        /// <summary>
        /// Sends the specified message to an SMTP server for delivery as an asynchronous operation.
        /// </summary>
        /// <param name="mailMessage">A <see cref="MailMessage"/> that contains the message to send.</param>
        /// <returns>Returns <see cref="Task{TResult}"/><br/>
        /// The task object representing the asynchronous operation.
        /// </returns>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        public async Task<MailSendResult> SendAsync(MailMessage mailMessage) {
            return await SendAsync(mailMessage, null).ConfigureAwait(false);
        }
        /// <summary>
        /// Sends the specified message to an SMTP server for delivery as an asynchronous operation.
        /// </summary>
        /// <param name="mailMessage">A <see cref="MailMessage"/> that contains the message to send.</param>
        /// <param name="userIdentifier">A user-defined object to identify the <paramref name="mailMessage"/></param>
        /// <returns>Returns <see cref="Task{TResult}"/><br/>
        /// The task object representing the asynchronous operation.
        /// </returns>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        public async Task<MailSendResult> SendAsync(MailMessage mailMessage, object userIdentifier) {
            return await SendAsync(new MailSenderMessage(mailMessage, userIdentifier)).ConfigureAwait(false);
        }
        /// <summary>
        /// Sends the specified message to an SMTP server for delivery as an asynchronous operation.
        /// </summary>
        /// <param name="message">A <see cref="MailSenderMessage"/> that contains the message to send and an identifier.</param>
        /// <returns>Returns <see cref="Task{TResult}"/><br/>
        /// The task object representing the asynchronous operation.
        /// </returns>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        public async Task<MailSendResult> SendAsync(MailSenderMessage message) {
            var result = await SendAsync(new List<MailSenderMessage> { message }).ConfigureAwait(false);
            return result[0];
        }
        /// <summary>
        /// Sends the specified <paramref name="mailMessages">messages</paramref> to an smtp server for delivery as an asynchronous operation.
        /// </summary>
        /// <param name="mailMessages">Collection of mails to send.</param>
        /// <returns>Return <see cref="Task{TResult}"/><br/>
        /// The task object representing the asynchronous operation.
        /// </returns>
        public async Task<Collection<MailSendResult>> SendAsync(IEnumerable<MailSenderMessage> mailMessages) {
            ThrowIfDisposed();
            var blockCollection = new BlockingCollection<MailSenderMessage>();
            Parallel.ForEach(mailMessages, blockCollection.Add);
            blockCollection.CompleteAdding();

            var result = new Collection<MailSendResult>();
            var tmpResult = SendAsync(blockCollection);
            await Task.Run(() => tmpResult.Output.GetConsumingEnumerable().ForEach(result.Add)).ConfigureAwait(false);
            return result;
        }
        /// <summary>
        /// Sends the <paramref name="mailMessages">messages</paramref> to an smtp server for delivery as an asynchronous operation.
        /// </summary>
        /// <param name="mailMessages">Collection of mails to send.</param>
        /// <returns></returns>
        public MailStreamResult SendAsync(BlockingCollection<MailSenderMessage> mailMessages) {
            ThrowIfDisposed();
            //return await Task.Factory.StartNew((Func<object, MailStreamResult>)StartStreamSendAsync, mailMessages).ConfigureAwait(false);
            return StartStreamSendAsync(mailMessages);
        }
        /// <summary>
        /// Cancels an asynchronous operation to send an e-mail message.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        public void SendAsyncCancel() {
            ThrowIfDisposed();
            if (mCancellationTokenSource != null) {
                mCancellationTokenSource.Cancel();
            }
        }
        private MailStreamResult StartStreamSendAsync(object state) {
            var inputStream = (BlockingCollection<MailSenderMessage>)state;
            var outputStream = new BlockingCollection<MailSendResult>();
            var task = Task.Run(() => {
                try {
                    Parallel.ForEach(inputStream.GetConsumingPartitioner(), new ParallelOptions { CancellationToken = mCancellationTokenSource.Token }, message => {
                        var result = new MailSendResult(message);
                        SmtpClient smtp = null;
                        try {
                            smtp = RetrieveSmtpClient();
                            SendMail(smtp, result);
                        } catch (OperationCanceledException) {
                            result.Canceled = true;
                        } finally {
                            if (smtp != null) {
                                QueueSmtp(smtp);
                            }
                            outputStream.Add(result);
                        }
                    });
                } catch (OperationCanceledException) {
                    Parallel.ForEach(inputStream.GetConsumingPartitioner(), message => outputStream.Add(new MailSendResult(message) { Canceled = true }));
                } finally {
                    outputStream.CompleteAdding();
                }
            });
            task.ConfigureAwait(false);
            return new MailStreamResult(outputStream, task);
        }
        private static void SendMail(SmtpClient smtp, MailSendResult sendResult) {
            try {
                smtp.Send(sendResult.MailMessage);
                sendResult.Successful = true;
            } catch (Exception exception) {
                sendResult.Exception = exception;
                sendResult.HasError = true;
            }
        }
        #region [ Helper methods ]
        private void ThrowIfDisposed() {
            if (!mIsDisposed) {
                return;
            }
            throw new ObjectDisposedException(GetType().FullName);
        }
        private SmtpClient CreateAndConfigureSmtpClient() {
            var smtp = new SmtpClient();
            if (mSmtpConfiguration.IsEmpty) {
                return smtp;
            }
            // Set UseDefaultCredentials before Credentials otherwise the credentials are overwritten
            smtp.UseDefaultCredentials = mSmtpConfiguration.UseDefaultCredentials;
            smtp.Port = mSmtpConfiguration.Port;
            smtp.Host = mSmtpConfiguration.Host;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            if (!smtp.UseDefaultCredentials) {
                smtp.Credentials = new NetworkCredential(mSmtpConfiguration.UserName, mSmtpConfiguration.Password);
            }
            smtp.EnableSsl = mSmtpConfiguration.EnableSsl;
            return smtp;
        }
        private SmtpClient RetrieveSmtpClient() {
            SmtpClient smtp;
            if (!mSmtpClients.TryDequeue(out smtp)) {
                smtp = CreateAndConfigureSmtpClient();
            }
            return smtp;
        }
        private void QueueSmtp(SmtpClient smtp) {
            mSmtpClients.Enqueue(smtp);
        }
        #endregion
        #region Implementation of IDisposable
        /// <summary>
        /// Releases all resources used by the <see cref="MailSender"/>.
        /// </summary>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing) {
            if (!disposing || mIsDisposed) {
                return;
            }
            if (mCancellationTokenSource != null) {
                mCancellationTokenSource.Dispose();
            }
            if (mSmtpClients != null) {
                mSmtpClients.GetConsumingEnumerable().ForEach(smtp => smtp.Dispose());
            }
            mIsDisposed = true;
        }
        #endregion
    }
}