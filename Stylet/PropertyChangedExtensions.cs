using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Stylet
{
    /// <summary>
    /// A binding to a PropertyChanged event, which can be used to unbind the binding
    /// </summary>
    public interface IEventBinding
    {
        void Unbind();
    }

    public static class PropertyChangedExtensions
    {
        internal class StrongPropertyChangedBinding : IEventBinding
        {
            private WeakReference<INotifyPropertyChanged> inpc;
            private PropertyChangedEventHandler handler;

            public StrongPropertyChangedBinding(INotifyPropertyChanged inpc, PropertyChangedEventHandler handler)
            {
                this.inpc = new WeakReference<INotifyPropertyChanged>(inpc);
                this.handler = handler;
            }

            public void Unbind()
            {
                INotifyPropertyChanged inpc;
                if (this.inpc.TryGetTarget(out inpc))
                {
                    inpc.PropertyChanged -= handler;
                }
            }
        }

        /// <summary>
        /// Strongly bind to PropertyChanged events for a particular property on a particular object
        /// </summary>
        /// <example>someObject.Bind(x => x.PropertyNameToBindTo, newValue => /* do something with the new value */)</example>
        /// <param name="target">Object raising the PropertyChanged event you're interested in</param>
        /// <param name="targetSelector">MemberExpression selecting the property to observe for changes (e.g x => x.PropertyName)</param>
        /// <param name="handler">Handler called whenever that property changed</param>
        /// <returns>Something which can be used to undo the binding. You can discard it if you want</returns>
        public static IEventBinding Bind<TBindTo, TMember>(this TBindTo target, Expression<Func<TBindTo, TMember>> targetSelector, Action<TMember> handler) where TBindTo : class, INotifyPropertyChanged
        {
            var propertyName = targetSelector.NameForProperty();
            var propertyAccess = targetSelector.Compile();
            // Make sure we don't capture target strongly, otherwise we'll retain it when we shouldn't
            // If it does get released, we're released from the delegate list
            var weakTarget = new WeakReference<TBindTo>(target);

            PropertyChangedEventHandler ourHandler = (o, e) =>
            {
                if (e.PropertyName == propertyName || e.PropertyName == String.Empty)
                {
                    TBindTo strongTarget;
                    if (weakTarget.TryGetTarget(out strongTarget))
                        handler(propertyAccess(strongTarget));
                }
            };

            target.PropertyChanged += ourHandler;

            var listener = new StrongPropertyChangedBinding(target, ourHandler);

            return listener;
        }
    }
}
