using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace StefanOssendorf.BulkMailing {
    /// <summary>
    /// Allows applications to send bulk mailings by using the Simple Mail Transfer Protocol (SMTP).
    /// </summary>
    public class MailSender : IMailSender {
        #region [ Fields ]
        private readonly ISmtpClientFactory mSmtpClientFactory;
        private readonly CancellationTokenSource mCancellationTokenSource;
        private readonly ConcurrentQueue<ISmtpClient> mSmtpClients;
        private volatile bool mIsDisposed;
        private volatile int mSendingCounter;
        #endregion
        #region [ Constructors ]
        /// <summary>
        /// Initializes a new instance of the <see cref="MailSender"/> class by using <see cref="SmtpClientFactory"/>.
        /// </summary>
        public MailSender()
            : this(new SmtpClientFactory()) {

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="MailSender"/> class by using the provided configuration for <see cref="SmtpClientFactory"/>.
        /// </summary>
        /// <param name="smtpClientConfiguration">Configuration</param>
        public MailSender(SmtpClientConfiguration smtpClientConfiguration)
            : this(new SmtpClientFactory(smtpClientConfiguration.Clone())) {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="MailSender"/> class by using the provided <see cref="ISmtpClientFactory">factory</see>.
        /// </summary>
        /// <param name="smtpClientFactory"></param>
        public MailSender(ISmtpClientFactory smtpClientFactory) :
            this(smtpClientFactory, new CancellationTokenSource()) {
        }
        internal MailSender(ISmtpClientFactory smtpClientFactory, CancellationTokenSource cancellationTokenSource) {
            Contract.Requires<ArgumentNullException>(smtpClientFactory != null);
            mSmtpClientFactory = smtpClientFactory;
            mCancellationTokenSource = cancellationTokenSource;
            mSmtpClients = new ConcurrentQueue<ISmtpClient>();
            mSendingCounter = 0;
        }
        #endregion
        #region Implementation of IMailSender
        /// <summary>
        /// Starts sending the specified <paramref name="mailMessages">messages</paramref> to an smtp server for delivery as an background operation.
        /// </summary>
        /// <param name="mailMessages">Collection of mails to send.</param>
        /// <returns></returns>
        public MailStreamResult StartSending(IEnumerable<MailSenderMessage> mailMessages) {
            ThrowIfDisposed();
            return StartSending(mailMessages.ToBlockingCollection());
        }
        /// <summary>
        /// Starts sending the <paramref name="mailMessages">messages</paramref> to an smtp server for delivery as an background operation.
        /// </summary>
        /// <param name="mailMessages">Collection of mails to send.</param>
        /// <returns></returns>
        public MailStreamResult StartSending(BlockingCollection<MailSenderMessage> mailMessages) {
            ThrowIfDisposed();
            return Send(mailMessages);
        }
        /// <summary>
        /// Stops the currently sending tasks.
        /// </summary>
        public void StopSending() {
            ThrowIfDisposed();
            mCancellationTokenSource.Cancel();
        }
        #endregion
        private MailStreamResult Send(BlockingCollection<MailSenderMessage> inputStream) {
#pragma warning disable 420
            Interlocked.Increment(ref mSendingCounter);
#pragma warning restore 420
            var outputStream = new BlockingCollection<MailSendResult>();
            var tcs = new TaskCompletionSource<int>();
            Task.Run(() => {
                try {
                    Parallel.ForEach(inputStream.GetConsumingPartitioner(), new ParallelOptions { CancellationToken = mCancellationTokenSource.Token }, message => {
                        var result = new MailSendResult(message);
                        ISmtpClient smtp = null;
                        try {
                            smtp = RetrieveSmtpClient();
                            SendMail(smtp, result);
                        } finally {
                            if (smtp != null) {
                                QueueSmtp(smtp);
                            }
                            outputStream.Add(result);
                        }
                    });
                    tcs.SetResult(1);
                } catch (OperationCanceledException) {
                    Parallel.ForEach(inputStream.GetConsumingPartitioner(), message => outputStream.Add(new MailSendResult(message) { Canceled = true }));
                    tcs.SetCanceled();
                } catch (Exception exc) {
                    tcs.SetException(exc);
                } finally {
                    outputStream.CompleteAdding();
#pragma warning disable 420
                    Interlocked.Decrement(ref mSendingCounter);
#pragma warning restore 420
                }
            }).ConfigureAwait(false);

            return new MailStreamResult(outputStream, tcs.Task);
        }
        private static void SendMail(ISmtpClient smtp, MailSendResult sendResult) {
            try {
                smtp.Send(sendResult.MailMessage);
                sendResult.Successful = true;
            } catch (Exception exception) {
                sendResult.Exception = exception;
            }
        }
        #region [ Helper methods ]
        private void ThrowIfDisposed() {
            if (!mIsDisposed) {
                return;
            }
            throw new ObjectDisposedException(GetType().FullName);
        }
        private ISmtpClient RetrieveSmtpClient() {
            ThrowIfDisposed();
            ISmtpClient smtp;
            if (!mSmtpClients.TryDequeue(out smtp)) {
                smtp = mSmtpClientFactory.Create();
            }
            return smtp;
        }
        private void QueueSmtp(ISmtpClient smtp) {
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
            if (mSendingCounter > 0) {
                mCancellationTokenSource.Cancel();
            }

            mSmtpClients.GetConsumingEnumerable().ForEach(client => client.TryDispose());
            mCancellationTokenSource.TryDispose();
            mIsDisposed = true;
        }
        #endregion
    }
}