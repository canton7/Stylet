using System;
using System.Windows;
using System.Windows.Threading;

namespace Stylet
{
    /// <summary>
    /// Generalised dispatcher, which can post and send.
    /// Used by <see cref="Execute"/>.
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

    /// <summary>
    /// <see cref="IDispatcher"/> implementation which can dispatch using <see cref="Dispatcher"/>
    /// </summary>
    public class ApplicationDispatcher : IDispatcher
    {
        private readonly Dispatcher dispatcher;

        /// <summary>
        /// Initialises a new instance of the <see cref="ApplicationDispatcher"/> class with the given <see cref="Dispatcher"/>
        /// </summary>
        /// <param name="dispatcher"><see cref="Dispatcher"/> to use, normally Application.Current.Dispatcher</param>
        public ApplicationDispatcher(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="ApplicationDispatcher"/> class with the given <see cref="Application"/>
        /// </summary>
        /// <param name="application"><see cref="Application"/> to use, normally Application</param>
        public ApplicationDispatcher(Application application)
            : this(application?.Dispatcher ?? throw new ArgumentNullException(nameof(application)))
        {
        }

        /// <inheritdoc/>
        public void Post(Action action)
        {
            this.dispatcher.BeginInvoke(action);
        }

        /// <inheritdoc/>
        public void Send(Action action)
        {
            this.dispatcher.Invoke(action);
        }

        /// <inheritdoc/>
        public bool IsCurrent
        {
            get { return this.dispatcher.CheckAccess(); }
        }
    }

    /// <summary>
    /// <see cref="IDispatcher"/> implementation whcih dispatches synchronously.
    /// Usually used for unit testing.
    /// </summary>
    public class SynchronousDispatcher : IDispatcher
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="SynchronousDispatcher"/>
        /// </summary>
        public static SynchronousDispatcher Instance { get; } = new SynchronousDispatcher();
        private SynchronousDispatcher() { }

        /// <inheritdoc/>
        public void Post(Action action)
        {
            action();
        }

        /// <inheritdoc/>
        public void Send(Action action)
        {
            action();
        }

        /// <inheritdoc/>
        public bool IsCurrent
        {
            get { return true; }
        }
    }
}
