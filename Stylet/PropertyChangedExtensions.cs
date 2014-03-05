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
    public interface IPropertyChangedBinding
    {
        void Unbind();
    }

    public static class PropertyChangedExtensions
    {
        private static ConditionalWeakTable<object, List<WeakPropertyChangedBinding>> eventMapping = new ConditionalWeakTable<object, List<WeakPropertyChangedBinding>>();
        private static object eventMappingLock = new object();

        public class WeakPropertyChangedBinding : IPropertyChangedBinding
        {
            private readonly string propertyName;
            private readonly EventHandler<PropertyChangedEventArgs> handler;
            private readonly WeakReference binder;
            private readonly WeakReference<INotifyPropertyChanged> inpc;

            public WeakPropertyChangedBinding(object binder, INotifyPropertyChanged inpc, string propertyName, EventHandler<PropertyChangedEventArgs> handler)
            {
                this.binder = new WeakReference(binder);
                this.inpc = new WeakReference<INotifyPropertyChanged>(inpc);
                this.propertyName = propertyName;
                // We need to keep this strongly, in case its target is a compiler-generated class instance
                this.handler = handler;
            }

            public void Unbind()
            {
                INotifyPropertyChanged inpc;

                // If the target's still around, unregister ourselves
                if (this.inpc.TryGetTarget(out inpc))
                    PropertyChangedEventManager.RemoveHandler(inpc, this.handler, this.propertyName);

                // If the handler's still around (there's a possibility it isn't), remove us from the ConditionalWeakTable
                var handler = this.handler.Target;
                if (handler != null)
                {
                    lock (eventMappingLock)
                    {
                        List<WeakPropertyChangedBinding> listeners;
                        if (eventMapping.TryGetValue(handler, out listeners))
                            listeners.Remove(this);
                    }
                }
            }
        }

        public class StrongPropertyChangedBinding : IPropertyChangedBinding
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

        public static IPropertyChangedBinding BindWeak<TBindTo, TMember>(this TBindTo target, object binder, Expression<Func<TBindTo, TMember>> targetSelector, Action<TMember> handler) where TBindTo : class, INotifyPropertyChanged
        {
            var propertyName = targetSelector.NameForProperty();
            var propertyAccess = targetSelector.Compile();
            var weakTarget = new WeakReference<TBindTo>(target);

            EventHandler<PropertyChangedEventArgs> ourHandler = (o, e) =>
            {
                TBindTo strongTarget;
                if (weakTarget.TryGetTarget(out strongTarget))
                    handler(propertyAccess(strongTarget));
            };

            WeakPropertyChangedBinding weakListener = new WeakPropertyChangedBinding(binder, target, propertyName, ourHandler);

            // Right, we have target, propertyName, binder.
            // Now we have to keep the handler we're about to build (which has a reference to the handler we were passed) alive as long as
            // binder's alive (the handler that was passed to us might refer to a method on a compiler-generated class, which we've got
            // the only reference to, so we've got to keep that alive).
            lock (eventMappingLock)
            {
                List<WeakPropertyChangedBinding> listeners;
                if (!eventMapping.TryGetValue(binder, out listeners))
                {
                    listeners = new List<WeakPropertyChangedBinding>();
                    eventMapping.Add(binder, listeners);
                }

                listeners.Add(weakListener);
            }

            PropertyChangedEventManager.AddHandler(target, ourHandler, propertyName);

            return weakListener;
        }

        public static IPropertyChangedBinding Bind<TBindTo, TMember>(this TBindTo target, Expression<Func<TBindTo, TMember>> targetSelector, Action<TMember> handler) where TBindTo : class, INotifyPropertyChanged
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
