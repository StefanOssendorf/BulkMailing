using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace StefanOssendorf.BulkMailing {
    /// <summary>
    /// Represents an mailresult.
    /// </summary>
    public class MailStreamResult {
        private readonly Task mSendingTask;

        internal MailStreamResult(BlockingCollection<MailSendResult> output, Task sendingTask) {
            Contract.Requires<ArgumentNullException>(sendingTask != null);
            Contract.Requires<ArgumentNullException>(output != null);
            Output = output;
            mSendingTask = sendingTask;
        }
        /// <summary>
        /// Gets the output of this sending.
        /// </summary>
        public BlockingCollection<MailSendResult> Output { get; private set; }
        /// <summary>
        /// Gets whether this sending instance has completed execution due to being canceled.;
        /// </summary>
        public bool IsCanceled { get { return mSendingTask.IsCanceled; } }
        /// <summary>
        /// Gets whether this sending has completed.
        /// </summary>
        public bool IsCompleted { get { return mSendingTask.IsCompleted; } }
        /// <summary>
        /// Gets the <see cref="AggregateException"/> that caused the sending to end prematurely. If the sending completed successfully or has not yet thrown any exceptions, this will return null. 
        /// </summary>
        public Exception Exception { get { return mSendingTask.Exception; } }
        /// <summary>
        /// Gets whether the sending completed due to an unhandled exception.
        /// </summary>
        public bool HasError { get { return mSendingTask.IsFaulted; } }

    }
}