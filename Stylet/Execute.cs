using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Stylet
{
    /// <summary>
    /// Generalised dispatcher, which can post and end
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

    /// <summary>
    /// Static class providing methods to easily run an action on the UI thread in various ways, and some other things
    /// </summary>
    public static class Execute
    {
        /// <summary>
        /// Should be set to the UI thread's Dispatcher. This is normally done by the Bootstrapper.
        /// </summary>
        public static IDispatcher Dispatcher;

        /// <summary>
        /// FOR TESTING ONLY. Causes everything to execute synchronously
        /// </summary>
        public static bool TestExecuteSynchronously = false;

        private static bool? inDesignMode;

        /// <summary>
        /// Default dispatcher used by PropertyChangedBase instances. Defaults to OnUIThread
        /// </summary>
        public static Action<Action> DefaultPropertyChangedDispatcher = Execute.OnUIThread;

        private static void EnsureDispatcher()
        {
            if (Dispatcher == null && !TestExecuteSynchronously)
                throw new InvalidOperationException("Execute.Dispatcher must be set before this method can be called. This should normally have been done by the Bootstrapper");
        }

        /// <summary>
        /// Dispatches the given action to be run on the UI thread asynchronously, even if the current thread is the UI thread
        /// </summary>
        public static void PostToUIThread(Action action)
        {
            EnsureDispatcher();
            if (!TestExecuteSynchronously)
                Dispatcher.Post(action);
            else
                action();
        }

        /// <summary>
        /// Dispatches the given action to be run on the UI thread asynchronously, or runs it synchronously if the current thread is the UI thread
        /// </summary>
        public static void OnUIThread(Action action)
        {
            EnsureDispatcher();
            if (!TestExecuteSynchronously && !Dispatcher.IsCurrent)
                Dispatcher.Post(action);
            else
                action();
        }

        /// <summary>
        /// Dispatches the given action to be run on the UI thread and blocks until it completes, or runs it synchronously if the current thread is the UI thread
        /// </summary>
        public static void OnUIThreadSync(Action action)
        {
            EnsureDispatcher();
            Exception exception = null;
            if (!TestExecuteSynchronously && !Dispatcher.IsCurrent)
            {
                Dispatcher.Send(() =>
                {
                    try
                    {
                        action();
                    }
                    catch (Exception e)
                    {
                        exception = e;
                    }
                });

                if (exception != null)
                    throw new System.Reflection.TargetInvocationException("An error occurred while dispatching a call to the UI Thread", exception);
            }
            else
            {
                action();
            }
        }

        /// <summary>
        /// Dispatches the given action to be run on the UI thread and returns a task that completes when the action completes, or runs it synchronously if the current thread is the UI thread
        /// </summary>
        public static Task OnUIThreadAsync(Action action)
        {
            EnsureDispatcher();
            if (!TestExecuteSynchronously && !Dispatcher.IsCurrent)
            {
                var tcs = new TaskCompletionSource<object>();
                Dispatcher.Post(() =>
                {
                    try
                    {
                        action();
                        tcs.SetResult(null);
                    }
                    catch (Exception e)
                    {
                        tcs.SetException(e);
                    }
                });
                return tcs.Task;
            }
            else
            {
                action();
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Determing if we're currently running in design mode
        /// </summary>
        public static bool InDesignMode
        {
            get
            {
                if (inDesignMode == null)
                {
                    var descriptor = DependencyPropertyDescriptor.FromProperty(DesignerProperties.IsInDesignModeProperty, typeof(FrameworkElement));
                    inDesignMode = (bool)descriptor.Metadata.DefaultValue;
                }

                return inDesignMode.Value;
            }
        }
    }
}
