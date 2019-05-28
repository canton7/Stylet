using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Stylet
{
    /// <summary>
    /// Extension of PropertyChangedEventArgs, which includes the new value of the property
    /// </summary>
    /// <typeparam name="TProperty">Type of property being notified</typeparam>
    public class PropertyChangedExtendedEventArgs<TProperty> : PropertyChangedEventArgs
    {
        /// <summary>
        /// Gets the new value of the property
        /// </summary>
        public virtual TProperty NewValue { get; private set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="PropertyChangedExtendedEventArgs{TProperty}"/> class
        /// </summary>
        /// <param name="propertyName">Name of the property which changed</param>
        /// <param name="newValue">New value of the property which changed</param>
        public PropertyChangedExtendedEventArgs(string propertyName, TProperty newValue)
            : base(propertyName)
        {
            this.NewValue = newValue;
        }
    }

    /// <summary>
    /// A binding to a PropertyChanged event, which can be used to unbind the binding
    /// </summary>
    public interface IEventBinding
    {
        /// <summary>
        /// Unbind this event binding, so that it will no longer receive events
        /// </summary>
        void Unbind();
    }

    /// <summary>
    /// Class holding extension methods on INotifyPropertyChanged, to allow strong/weak binding
    /// </summary>
    public static class PropertyChangedExtensions
    {
        internal class StrongPropertyChangedBinding : IEventBinding
        {
            private readonly WeakReference<INotifyPropertyChanged> inpc;
            private readonly PropertyChangedEventHandler handler;

            public StrongPropertyChangedBinding(INotifyPropertyChanged inpc, PropertyChangedEventHandler handler)
            {
                this.inpc = new WeakReference<INotifyPropertyChanged>(inpc);
                this.handler = handler;
            }

            public void Unbind()
            {
                if (this.inpc.TryGetTarget(out INotifyPropertyChanged inpc))
                    inpc.PropertyChanged -= this.handler;
            }
        }

        internal class WeakPropertyChangedHandler<TSource, TProperty> : IEventBinding where TSource : class, INotifyPropertyChanged
        {
            private readonly WeakReference<TSource> source;
            private readonly EventHandler<PropertyChangedExtendedEventArgs<TProperty>> handler;
            private readonly string propertyName;
            private readonly Func<TSource, TProperty> valueSelector;

            public WeakPropertyChangedHandler(TSource source, Expression<Func<TSource, TProperty>> selector, EventHandler<PropertyChangedExtendedEventArgs<TProperty>> handler)
            {
                // We keep a strong reference to the handler, and have the PropertyChangedEventManager keep a weak
                // reference to us. This means that anyone retaining us will also retain the handler.

                this.source = new WeakReference<TSource>(source);
                this.handler = handler;
                this.propertyName = selector.NameForProperty();
                this.valueSelector = selector.Compile();

                PropertyChangedEventManager.AddHandler(source, this.PropertyChangedHandler, this.propertyName);
            }

            private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
            {
                var got = this.source.TryGetTarget(out TSource source);
                // We should never hit this case. The PropertyChangedeventManager shouldn't call us if the source became null
                Debug.Assert(got);
                this.handler(source, new PropertyChangedExtendedEventArgs<TProperty>(this.propertyName, this.valueSelector(source)));
            }

            public void Unbind()
            {
                if (this.source.TryGetTarget(out TSource source))
                    PropertyChangedEventManager.RemoveHandler(source, this.PropertyChangedHandler, this.propertyName);
            }
        }

        internal class WeakPropertyChangedBinding : IEventBinding
        {
            private readonly WeakReference<IEventBinding> wrappedBinding;

            public WeakPropertyChangedBinding(IEventBinding wrappedBinding)
            {
                this.wrappedBinding = new WeakReference<IEventBinding>(wrappedBinding);
            }

            public void Unbind()
            {
                if (this.wrappedBinding.TryGetTarget(out IEventBinding wrappedBinding))
                    wrappedBinding.Unbind();
            }
        }

        /// <summary>
        /// Strongly bind to PropertyChanged events for a particular property on a particular object
        /// </summary>
        /// <example>someObject.Bind(x => x.PropertyNameToBindTo, newValue => /* do something with the new value */)</example>
        /// <typeparam name="TSource">Type of object providing the PropertyChanged event</typeparam>
        /// <typeparam name="TProperty">Type of property for which the event is raised</typeparam>
        /// <param name="target">Object raising the PropertyChanged event you're interested in</param>
        /// <param name="targetSelector">MemberExpression selecting the property to observe for changes (e.g x => x.PropertyName)</param>
        /// <param name="handler">Handler called whenever that property changed</param>
        /// <returns>Something which can be used to undo the binding. You can discard it if you want</returns>
        public static IEventBinding Bind<TSource, TProperty>(this TSource target, Expression<Func<TSource, TProperty>> targetSelector, EventHandler<PropertyChangedExtendedEventArgs<TProperty>> handler) where TSource : class, INotifyPropertyChanged
        {
            return BindImpl(target, targetSelector, handler, invoke: false);
        }

        /// <summary>
        /// Strongly bind to PropertyChanged events for a particular property on a particular object,
        /// and invoke the handler straight away
        /// </summary>
        /// <remarks>
        /// This immediately calls <paramref name="handler"/> with the current value of the property.
        /// </remarks>
        /// <example>someObject.BindAndInvoke(x => x.PropertyNameToBindTo, newValue => /* do something with the new value */)</example>
        /// <typeparam name="TSource">Type of object providing the PropertyChanged event</typeparam>
        /// <typeparam name="TProperty">Type of property for which the event is raised</typeparam>
        /// <param name="target">Object raising the PropertyChanged event you're interested in</param>
        /// <param name="targetSelector">MemberExpression selecting the property to observe for changes (e.g x => x.PropertyName)</param>
        /// <param name="handler">Handler called whenever that property changed</param>
        /// <returns>Something which can be used to undo the binding. You can discard it if you want</returns>
        public static IEventBinding BindAndInvoke<TSource, TProperty>(this TSource target, Expression<Func<TSource, TProperty>> targetSelector, EventHandler<PropertyChangedExtendedEventArgs<TProperty>> handler) where TSource : class, INotifyPropertyChanged
        {
            return BindImpl(target, targetSelector, handler, invoke: true);
        }

        private static IEventBinding BindImpl<TSource, TProperty>(TSource target, Expression<Func<TSource, TProperty>> targetSelector, EventHandler<PropertyChangedExtendedEventArgs<TProperty>> handler, bool invoke) where TSource : class, INotifyPropertyChanged
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (targetSelector == null)
                throw new ArgumentNullException(nameof(targetSelector));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var propertyName = targetSelector.NameForProperty();
            var propertyAccess = targetSelector.Compile();
            // Make sure we don't capture target strongly, otherwise we'll retain it when we shouldn't
            // If it does get released, we're released from the delegate list
            var weakTarget = new WeakReference<TSource>(target);

            void ourHandler(object o, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == propertyName || e.PropertyName == String.Empty)
                {
                    if (weakTarget.TryGetTarget(out TSource strongTarget))
                        handler(strongTarget, new PropertyChangedExtendedEventArgs<TProperty>(propertyName, propertyAccess(strongTarget)));
                }
            }

            target.PropertyChanged += ourHandler;

            var listener = new StrongPropertyChangedBinding(target, ourHandler);

            if (invoke)
            {
                handler(target, new PropertyChangedExtendedEventArgs<TProperty>(propertyName, propertyAccess(target)));
            }

            return listener;
        }

        /// <summary>
        /// Obsolete: Weakly bind to PropertyChanged events for a particular property on a particular object
        /// </summary>
        /// <example>someObject.Bind(x => x.PropertyNameToBindTo, newValue => /* do something with the new value */)</example>
        /// <typeparam name="TSource">Type of object providing the PropertyChanged event</typeparam>
        /// <typeparam name="TProperty">Type of property for which the event is raised</typeparam>
        /// <param name="target">Object raising the PropertyChanged event you're interested in</param>
        /// <param name="targetSelector">MemberExpression selecting the property to observe for changes (e.g x => x.PropertyName)</param>
        /// <param name="handler">Handler called whenever that property changed</param>
        /// <returns>Something which can be used to undo the binding. You can discard it if you want</returns>
        [Obsolete("Don't use this - use Bind instead and explicitly .Unbind it when appropriate.", error: true)]
        public static IEventBinding BindWeak<TSource, TProperty>(this TSource target, Expression<Func<TSource, TProperty>> targetSelector, EventHandler<PropertyChangedExtendedEventArgs<TProperty>> handler) where TSource : class, INotifyPropertyChanged
        {
            var attribute = handler.Target.GetType().GetCustomAttribute<CompilerGeneratedAttribute>();
            if (attribute != null)
                throw new InvalidOperationException("Handler passed to BindWeak refers to a compiler-generated class. You may not capture local variables in the handler");

            var binding = new WeakPropertyChangedHandler<TSource, TProperty>(target, targetSelector, handler);
            return new WeakPropertyChangedBinding(binding);
        }
    }
}
