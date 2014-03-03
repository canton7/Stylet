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

        public static IPropertyChangedBinding BindWeak<TMember>(this object binder, Expression<Func<TMember>> targetSelector, Action<TMember> handler)
        {
            return BindInternal(binder, targetSelector, handler, true);
        }

        public static IPropertyChangedBinding Bind<TMember>(this object binder, Expression<Func<TMember>> targetSelector, Action<TMember> handler)
        {
            return BindInternal(binder, targetSelector, handler, false);
        }

        private static IPropertyChangedBinding BindInternal<TMember>(object binder, Expression<Func<TMember>> targetSelector, Action<TMember> handler, bool weak)
        {
            var memberSelector = targetSelector.Body as MemberExpression;
            if (memberSelector == null)
                throw new ArgumentException("Must be in the form () => someInstance.SomeProperty", "targetSelector");

            var propertyName = memberSelector.Member.Name;
            var targetExpression = memberSelector.Expression as MemberExpression;
            if (targetExpression == null)
                throw new ArgumentException("Must be in the form () => someInstance.SomeProperty", "targetSelector");

            var target = Expression.Lambda<Func<object>>(targetExpression).Compile()();
            var inpc = target as INotifyPropertyChanged;
            if (inpc == null)
                throw new ArgumentException("The someInstance in () => someInstance.SomeProperty must be an INotifyPropertyChanged", "targetSelector");

            var propertyAccess = Expression.Lambda<Func<TMember>>(memberSelector).Compile();

            IPropertyChangedBinding listener;

            if (weak)
            {
                EventHandler<PropertyChangedEventArgs> ourHandler = (o, e) =>
                {
                    handler(propertyAccess());
                };

                WeakPropertyChangedBinding weakListener = new WeakPropertyChangedBinding(binder, inpc, propertyName, ourHandler);
                listener = weakListener;

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

                PropertyChangedEventManager.AddHandler(inpc, ourHandler, propertyName);
            }
            else
            {
                PropertyChangedEventHandler ourHandler = (o, e) =>
                {
                    if (e.PropertyName == propertyName || e.PropertyName == String.Empty)
                    {
                        handler(propertyAccess());
                    }
                };

                inpc.PropertyChanged += ourHandler;

                listener = new StrongPropertyChangedBinding(inpc, ourHandler);
            }

            return listener;
        }
    }
}
