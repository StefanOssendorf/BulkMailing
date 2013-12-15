using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.CompilerServices;
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
        private bool mIsInCall;
        private SmtpClient mSingleSendSmtpClient;
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
            return await SendAsync(new MailSenderMessage { Message = mailMessage, UserIdentifier = userIdentifier }).ConfigureAwait(false);
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
            ThrowIfDisposed();
            ThrowIfAlreadyInCall();

            mIsInCall = true;
            var result = new MailSendResult { MailMessage = message.Message, UserIdentifier = message.UserIdentifier };

            mSingleSendSmtpClient = RetrieveSmtpClient();
            try {
                await mSingleSendSmtpClient.SendMailAsync(message.Message).ConfigureAwait(false);
                result.Successful = true;
            } catch (OperationCanceledException) {
                result.Cancelled = true;
            } catch (Exception exception) {
                result.Exception = exception;
                result.HasError = true;
            } finally {
                QueueSmtp(mSingleSendSmtpClient);
                mSingleSendSmtpClient = null;
                mIsInCall = false;
            }

            return result;
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
            ThrowIfAlreadyInCall();

            mIsInCall = true;
            var result = new Collection<MailSendResult>();
            try {
                result = await Task.Factory.StartNew((Func<object, Collection<MailSendResult>>)SendBulkMailMessages, mailMessages, mCancellationTokenSource.Token, TaskCreationOptions.None, TaskScheduler.Default).ConfigureAwait(false);
            } finally {
                mIsInCall = false;
            }

            return result;
        }
        /// <summary>
        /// Cancels an asynchronous operation to send an e-mail message.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        public void SendAsyncCancel() {
            ThrowIfDisposed();
            if (!mIsInCall) {
                return;
            }
            if (mSingleSendSmtpClient != null) {
                mSingleSendSmtpClient.SendAsyncCancel();
            }
            if (mCancellationTokenSource != null) {
                mCancellationTokenSource.Cancel();
            }
        }

        private Collection<MailSendResult> SendBulkMailMessages(object mailMessages) {
            var messages = new List<MailSenderMessage>((IEnumerable<MailSenderMessage>)mailMessages);
            var options = new ParallelOptions { CancellationToken = mCancellationTokenSource.Token };
            var mails = new List<MailSendResult>();

            messages.ForEach(msg => mails.Add(new MailSendResult { MailMessage = msg.Message, UserIdentifier = msg.UserIdentifier }));

            try {
                Parallel.ForEach(Partitioner.Create(0, mails.Count), options, range => {
                    SmtpClient smtp = RetrieveSmtpClient();
                    for (int i = range.Item1; i < range.Item2; i++) {   
                        SendMail(smtp, mails[i]);
                        if (options.CancellationToken.IsCancellationRequested) {
                            break;
                        }
                    }
                    QueueSmtp(smtp);
                });
            } catch (OperationCanceledException) {
                mails.Where(item => !item.HasError && !item.Successful).ToList().ForEach(item => item.Cancelled = true);
            }

            return new Collection<MailSendResult>(mails);
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
        private void ThrowIfAlreadyInCall([CallerMemberName]string callerMemberName = "") {
            if (!mIsInCall) {
                return;
            }
            throw new InvalidOperationException(string.Format("This {0} has a {1} call in progress.", GetType().Name, callerMemberName));
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
            if (mSingleSendSmtpClient != null) {
                mSingleSendSmtpClient.Dispose();
            }
            if (mCancellationTokenSource != null) {
                mCancellationTokenSource.Dispose();
            }
            if (!mSmtpClients.IsEmpty) {
                while (!mSmtpClients.IsEmpty) {
                    SmtpClient smtp;
                    mSmtpClients.TryDequeue(out smtp);
                    smtp.Dispose();
                }
            }
            mIsDisposed = true;
        }
        #endregion
    }
}