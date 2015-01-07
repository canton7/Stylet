using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        void Post(Action action);

        /// <summary>
        /// Execute synchronously
        /// </summary>
        void Send(Action action);

        /// <summary>
        /// True if invocation isn't required
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
