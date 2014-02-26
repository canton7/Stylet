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
    public static class PropertyChangedExtensions
    {
        private static ConditionalWeakTable<INotifyPropertyChanged, List<Listener> eventMapping = new ConditionalWeakTable<INotifyPropertyChanged, List<Listener>();
        private static object eventMappingLock = new object();

        private class Listener
        {
            public readonly string PropertyName;
            public readonly EventHandler<PropertyChangedEventArgs> Handler;

            public Listener(string propertyName, EventHandler<PropertyChangedEventArgs> handler)
            {
                this.PropertyName = propertyName;
                this.Handler = handler;
            }
        }

        public static void Bind<TClass, TMember>(this TClass item, Expression<Func<TClass, TMember>> selector, Action<TMember> handler) where TClass : INotifyPropertyChanged
        {
            var propertyName = selector.NameForProperty();
            var compiledSelector = selector.Compile();
            List<Listener> listeners;
            EventHandler<PropertyChangedEventArgs> ourHandler;

            lock (eventMappingLock)
            {
                if (!eventMapping.TryGetValue(item, out listeners))
                {
                    listeners = new List<Listener>();
                    eventMapping.Add(item, listeners);
                }

                ourHandler = (s, e) => handler(compiledSelector(item));

                listeners.Add(new Listener(propertyName, ourHandler));
            }

            PropertyChangedEventManager.AddHandler(item, ourHandler, propertyName);
        }
    }
}
