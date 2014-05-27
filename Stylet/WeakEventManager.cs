using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Stylet
{
    /// <summary>
    /// A manager capable of creating a weak event subscription for INotifyPropertyChanged events from a source to a subscriber. Manager MUST be owned by the subscriber.
    /// </summary>
    public interface IWeakEventManager
    {
        /// <summary>
        /// Create a weak event subscription from the source, to the given handler
        /// </summary>
        /// <typeparam name="TSource">Type of the source</typeparam>
        /// <typeparam name="TProperty">Type of the property to subscribe to on the source</typeparam>
        /// <param name="source">Source object, whic implements INotifyPropertyChanged, to subscribe to</param>
        /// <param name="selector">Describes which property to observe, e.g. (x => x.SomeProperty)</param>
        /// <param name="handler">Callback to be called whenever the property changes. Is passed the new value of the property</param>
        /// <returns>An event binding, which can be used to unregister the subscription</returns>
        IEventBinding BindWeak<TSource, TProperty>(TSource source, Expression<Func<TSource, TProperty>> selector, Action<TProperty> handler)
            where TSource : class, INotifyPropertyChanged;
    }

    internal class WeakPropertyBinding<TSource, TProperty> : IEventBinding where TSource : class, INotifyPropertyChanged
    {
        // Make sure we don't end up retaining the source
        private readonly WeakReference<TSource> source;
        private readonly string propertyName;
        private readonly Func<TSource, TProperty> valueSelector;
        private readonly Action<TProperty> handler;
        private readonly Action<IEventBinding> remover;

        public WeakPropertyBinding(TSource source, Expression<Func<TSource, TProperty>> selector, Action<TProperty> handler, Action<IEventBinding> remover)
        {
            this.source = new WeakReference<TSource>(source);
            this.propertyName = selector.NameForProperty();
            this.valueSelector = selector.Compile();
            this.handler = handler;
            this.remover = remover;

            PropertyChangedEventManager.AddHandler(source, this.PropertyChangedHandler, this.propertyName);
        }

        internal void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            TSource source;
            var got = this.source.TryGetTarget(out source);
            // We should never hit this case. The PropertyChangedeventManager shouldn't call us if the source became null
            Debug.Assert(got);
            if (got)
                this.handler(this.valueSelector(source));
        }

        public void Unbind()
        {
            TSource source;
            if (this.source.TryGetTarget(out source))
                PropertyChangedEventManager.RemoveHandler(source, this.PropertyChangedHandler, this.propertyName);
            this.remover(this);
        }
    }

    /// <summary>
    /// Default implementation of IWeakEventManager: a manager capable of creating a weak event subscription for INotifyPropertyChanged events from a source to a subscriber. Manager MUST be owned by the subscriber.
    /// </summary>
    public class WeakEventManager : IWeakEventManager
    {
        private object bindingsLock = new object();
        private List<IEventBinding> bindings = new List<IEventBinding>();

        /// <summary>
        /// Create a weak event subscription from the source, to the given handler
        /// </summary>
        /// <typeparam name="TSource">Type of the source</typeparam>
        /// <typeparam name="TProperty">Type of the property to subscribe to on the source</typeparam>
        /// <param name="source">Source object, whic implements INotifyPropertyChanged, to subscribe to</param>
        /// <param name="selector">Describes which property to observe, e.g. (x => x.SomeProperty)</param>
        /// <param name="handler">Callback to be called whenever the property changes. Is passed the new value of the property</param>
        /// <returns>An event binding, which can be used to unregister the subscription</returns>
        public IEventBinding BindWeak<TSource, TProperty>(TSource source, Expression<Func<TSource, TProperty>> selector, Action<TProperty> handler)
            where TSource : class, INotifyPropertyChanged
        {
            // So, the handler's target might point to the class that owns us, or it might point to a compiler-generated class
            // We assume we're owned by whatever determines how long the handler's target should live for
            // Therefore we'll retain the handler's target for as long as we're alive (unless it's unregistered)

            // The PropertyChangedEventManager is safe to use with delegates whose targets aren't compiler-generated, so we can
            // ensure we provide a delegate with a non-compiler-generated target.
            // To do this, we'll create a new WeakPropertyBinding instance, and retain it ourselves (so it lives as long as we do,
            // and therefore as long as the thing that owns us does). The PropertyChangedEventManager will have a weak reference to 
            // the WeakPropertyBinding instance, so once we release it, it will too.

            var propertyName = selector.NameForProperty();

            var binding = new WeakPropertyBinding<TSource, TProperty>(source, selector, handler, this.Remove);
            lock (this.bindingsLock)
            {
                this.bindings.Add(binding);
            }

            return binding;
        }

        internal void Remove(IEventBinding binding)
        {
            lock (this.bindingsLock)
            {
                this.bindings.Remove(binding);
            }
        }
    }
}
