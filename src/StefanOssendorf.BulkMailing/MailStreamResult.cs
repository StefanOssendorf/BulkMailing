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

        internal MailStreamResult(BlockingCollection<MailSendResult> result, Task sendingTask) {
            Contract.Requires<ArgumentNullException>(sendingTask != null);
            Contract.Requires<ArgumentNullException>(result != null);
            Result = result;
            mSendingTask = sendingTask;
        }
        /// <summary>
        /// Gets the Result of this sending.
        /// </summary>
        public BlockingCollection<MailSendResult> Result { get; private set; }
        /// <summary>
        /// Gets whether this sending instance has completed execution due to being canceled.;
        /// </summary>
        public bool IsStopped { get { return mSendingTask.IsCanceled; } }
        /// <summary>
        /// Gets whether this sending has completed.
        /// </summary>
        public bool IsFinished { get { return mSendingTask.IsCompleted; } }
        /// <summary>
        /// Gets whether the sending completed due to an unhandled exception.
        /// </summary>
        public bool HasError { get { return mSendingTask.IsFaulted; } }
        /// <summary>
        /// Gets the <see cref="AggregateException"/> that caused the sending to end prematurely. If the sending completed successfully or has not yet thrown any exceptions, this will return null. 
        /// </summary>
        public Exception Exception { get { return mSendingTask.Exception; } }
    }
}