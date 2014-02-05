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
            SynchronizationContext.Post(_ => action(), null);
        }

        public static Task OnUIThreadAsync(Action action)
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
    }
}
