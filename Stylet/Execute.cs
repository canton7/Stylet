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
        public static SynchronizationContext SynchronizationContext;
        private static bool? inDesignMode;

        public static Action<Action> DefaultPropertyChangedDispatcher = Execute.OnUIThread;

        public static void BeginOnUIThread(Action action)
        {
            // If we're already on the given SynchronizationContext, or it hasn't been set, run synchronously
            if (SynchronizationContext != null && SynchronizationContext != SynchronizationContext.Current)
                SynchronizationContext.Post(_ => action(), null);
            else
                action();
        }

        public static void OnUIThread(Action action)
        {
            Exception exception = null;
            if (SynchronizationContext != null && SynchronizationContext != SynchronizationContext.Current)
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

        public static Task OnUIThreadAsync(Action action)
        {
            // If we're already on the given SynchronizationContext, or it hasn't been set, run synchronously
            if (SynchronizationContext != null && SynchronizationContext != SynchronizationContext.Current)
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
