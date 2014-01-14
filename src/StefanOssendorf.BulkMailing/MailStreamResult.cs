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
        /// <summary>
        /// For testing purposes.
        /// </summary>
        internal Task BackgroundTask { get { return mSendingTask; } }

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
        /// Gets whether the backgroundwork instance has completed execution due to being canceled.
        /// </summary>
        public bool IsStopped { get { return mSendingTask.IsCanceled; } }
        /// <summary>
        /// Gets whether the backgroundwork has successfully completed.
        /// </summary>
        public bool IsFinished { get { return mSendingTask.Status == TaskStatus.RanToCompletion; } }
        /// <summary>
        /// Gets whether the backgroudnwork has completed due to an unhandled exception.
        /// </summary>
        public bool HasError { get { return mSendingTask.IsFaulted; } }
        /// <summary>
        /// Gets the <see cref="AggregateException"/> that caused the backgroundwork to end prematurely. If the backgroundwork completed successfully or has not yet thrown any exceptions, this will return null. 
        /// </summary>
        public Exception Exception { get { return mSendingTask.Exception; } }
    }
}