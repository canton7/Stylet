using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Stylet
{
    public interface IHandle
    {
    }

    public interface IHandle<TMessageType> : IHandle
    {
        void Handle(TMessageType message);
    }

    public class EventAggregator
    {
        private readonly List<Handler> handlers = new List<Handler>();

        public void Subscribe(IHandle handler)
        {

        }

        public void Publish(object message)
        {

        }

        private class Handler
        {
            private readonly WeakReference target;
            private readonly Dictionary<Type, Action<object>> handlers = new Dictionary<Type, Action<object>>();

            public Handler(object handler)
            {
                this.target = new WeakReference(target);

                foreach (var implementation in this.target.GetType().GetInterfaces().Where(x => x.IsGenericType && typeof(IHandle).IsAssignableFrom(x)))
                {
                    var type = implementation.GetGenericArguments()[0];
                    var method = type.GetMethod("Handle");
                    var param = Expression.Parameter(type, "message");
                    var caller = Expression.Lambda<Action<object>>(Expression.Call(method, param), param).Compile();
                    this.handlers.Add(type, caller);
                }
            }

            public bool IsOfType(object subscriberType)
            {
                return this.target.Target == subscriberType;
            }

            public bool Handle(Type messageType, object message)
            {
                var target = this.target.Target;
                if (target == null)
                    return false;

                return true;
            }
        }
    }
}
