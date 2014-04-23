using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Stylet
{
    public static class Execute
    {
        /// <summary>
        /// Should be set to the UI thread's SynchronizationContext. This is normally done by the Bootstrapper.
        /// </summary>
        public static SynchronizationContext SynchronizationContext;
        private static bool? inDesignMode;

        /// <summary>
        /// Default dispatcher used by PropertyChangedBase instances. Defaults to BeginOnUIThreadOrSynchronous
        /// </summary>
        public static Action<Action> DefaultPropertyChangedDispatcher = Execute.BeginOnUIThreadOrSynchronous;

        private static void EnsureSynchronizationContext()
        {
            if (SynchronizationContext == null)
                throw new Exception("Execute.SynchronizationContext must be set before this method can be called. This should normally have been done by the Bootstrapper");
        }

        /// <summary>
        /// Dispatches the given action to be run on the UI thread, even if the current thread is the UI thread
        /// </summary>
        public static void BeginOnUIThread(Action action)
        {
            EnsureSynchronizationContext();
            SynchronizationContext.Post(_ => action(), null);
        }

        /// <summary>
        /// Dispatches the given action to be run on the UI thread, or runs it synchronously if the current thread is the UI thread
        /// </summary>
        public static void BeginOnUIThreadOrSynchronous(Action action)
        {
            EnsureSynchronizationContext();
            if (SynchronizationContext != SynchronizationContext.Current)
                SynchronizationContext.Post(_ => action(), null);
            else
                action();
        }

        /// <summary>
        /// Dispatches the given action to be run on the UI thread and blocks until it completes, or runs it synchronously if the current thread is the UI thread
        /// </summary>
        public static void OnUIThread(Action action)
        {
            EnsureSynchronizationContext();
            Exception exception = null;
            if (SynchronizationContext != SynchronizationContext.Current)
            {
                SynchronizationContext.Send(_ =>
                {
                    try
                    {
                        action();
                    }
                    catch (Exception e)
                    {
                        exception = e;
                    }
                }, null);

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
            EnsureSynchronizationContext();
            if (SynchronizationContext != SynchronizationContext.Current)
            {
                var tcs = new TaskCompletionSource<object>();
                SynchronizationContext.Post(_ =>
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
                }, null);
                return tcs.Task;
            }
            else
            {
                action();
                return Task.FromResult(false);
            }
        }

        public static bool InDesignMode
        {
            get
            {
                if (inDesignMode == null)
                {
                    var prop = DesignerProperties.IsInDesignModeProperty;
                    inDesignMode = (bool)DependencyPropertyDescriptor.FromProperty(prop, typeof(FrameworkElement)).Metadata.DefaultValue;

                    if (inDesignMode.GetValueOrDefault(false) && Process.GetCurrentProcess().ProcessName.StartsWith("devenv", StringComparison.Ordinal))
                        inDesignMode = true;
                }

                return inDesignMode.GetValueOrDefault(false);
            }
        }
    }
}
