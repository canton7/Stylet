using System;
using System.Runtime.ExceptionServices;
using System.Windows.Data;

namespace Stylet.Xaml
{
    /// <summary>
    /// <see cref="Binding"/> subclass which rethrows exceptions encountered on setting the source
    /// </summary>
    public class RethrowingBinding : Binding
    {
        /// <inheritdoc/>
        public RethrowingBinding()
        {
            this.UpdateSourceExceptionFilter = this.ExceptionFilter;
        }

        /// <inheritdoc/>
        public RethrowingBinding(string path)
            : base(path)
        {
            this.UpdateSourceExceptionFilter = this.ExceptionFilter;
        }

        private object ExceptionFilter(object bindExpression, Exception exception)
        {
            var edi = ExceptionDispatchInfo.Capture(exception);
            Execute.OnUIThread(() => edi.Throw());
            return exception;
        }
    }
}
