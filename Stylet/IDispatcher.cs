using System;
using System.Windows.Threading;

namespace Stylet
{
    /// <summary>
    /// Generalised dispatcher, which can post and send
    /// </summary>
    public interface IDispatcher
    {
        /// <summary>
        /// Execute asynchronously
        /// </summary>
        /// <param name="action">Action to execute</param>
        void Post(Action action);

        /// <summary>
        /// Execute synchronously
        /// </summary>
        /// <param name="action">Action to execute</param>
        void Send(Action action);

        /// <summary>
        /// Gets a value indicating whether the current thread is the thread being dispatched to
        /// </summary>
        bool IsCurrent { get; }
    }

    internal class DispatcherWrapper : IDispatcher
    {
        private readonly Dispatcher dispatcher;

        public DispatcherWrapper(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        public void Post(Action action)
        {
            this.dispatcher.BeginInvoke(action);
        }

        public void Send(Action action)
        {
            this.dispatcher.Invoke(action);
        }

        public bool IsCurrent
        {
            get { return this.dispatcher.CheckAccess(); }
        }
    }

    internal class SynchronousDispatcher : IDispatcher
    {
        public void Post(Action action)
        {
            action();
        }

        public void Send(Action action)
        {
            action();
        }

        public bool IsCurrent
        {
            get { return true; }
        }
    }
}
