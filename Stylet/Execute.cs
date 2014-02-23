using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Stylet
{
    public static class Execute
    {
        public static SynchronizationContext SynchronizationContext;

        public static void OnUIThread(Action action)
        {
            // If we're already on the given SynchronizationContext, or it hasn't been set, run synchronously
            if (SynchronizationContext != null && SynchronizationContext != SynchronizationContext.Current)
                SynchronizationContext.Post(_ => action(), null);
            else
                action();
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
    }
}
