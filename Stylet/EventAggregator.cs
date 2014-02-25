using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

    public interface IEventAggregator
    {
        void Subscribe(IHandle handler);
        void Unsubscribe(IHandle handler);

        void PublishWithDispatcher(object message, Action<Action> dispatcher);
        void PublishOnUIThread(object message);
        void Publish(object message);
    }

    public class EventAggregator : IEventAggregator
    {
        private readonly List<Handler> handlers = new List<Handler>();
        private readonly object handlersLock = new object();

        public void Subscribe(IHandle handler)
        {
            lock (this.handlersLock)
            {
                // Is it already subscribed?
                if (this.handlers.Any(x => x.IsHandlerForInstance(handler)))
                    return;

                this.handlers.Add(new Handler(handler));
            }
        }

        public void Unsubscribe(IHandle handler)
        {
            lock (this.handlersLock)
            {
                var existingHandler = this.handlers.FirstOrDefault(x => x.IsHandlerForInstance(handler));
                if (existingHandler != null)
                    this.handlers.Remove(existingHandler);
            }
        }

        public void PublishWithDispatcher(object message, Action<Action> dispatcher)
        {
            lock (this.handlersLock)
            {
                var messageType = message.GetType();
                var deadHandlers = this.handlers.Where(x => !x.Handle(messageType, message, dispatcher)).ToArray();
                foreach (var deadHandler in deadHandlers)
                {
                    this.handlers.Remove(deadHandler);
                }
            }
        }

        public void PublishOnUIThread(object message)
        {
            this.PublishWithDispatcher(message, Execute.OnUIThread);
        }

        public void Publish(object message)
        {
            this.PublishWithDispatcher(message, x => x());
        }

        private class Handler
        {
            private readonly WeakReference target;
            private readonly List<HandlerInvoker> invokers = new List<HandlerInvoker>();

            public Handler(object handler)
            {
                var handlerType = handler.GetType();
                this.target = new WeakReference(handler);

                foreach (var implementation in handler.GetType().GetInterfaces().Where(x => x.IsGenericType && typeof(IHandle).IsAssignableFrom(x)))
                {
                    var messageType = implementation.GetGenericArguments()[0];
                    this.invokers.Add(new HandlerInvoker(handlerType, messageType, implementation.GetMethod("Handle")));
                }
            }

            public bool IsHandlerForInstance(object subscriber)
            {
                return this.target.Target == subscriber;
            }

            public bool Handle(Type messageType, object message, Action<Action> dispatcher)
            {
                var target = this.target.Target;
                if (target == null)
                    return false;

                foreach (var invoker in this.invokers)
                {
                    invoker.Invoke(target, messageType, message, dispatcher);
                }

                return true;
            }
        }

        private class HandlerInvoker
        {
            private readonly Type messageType;
            private readonly Action<object, object> invoker;

            public HandlerInvoker(Type targetType, Type messageType, MethodInfo invocationMethod)
            {
                this.messageType = messageType;
                var targetParam = Expression.Parameter(typeof(object), "target");
                var messageParam = Expression.Parameter(typeof(object), "message");
                var castTarget = Expression.Convert(targetParam, targetType);
                var castMessage = Expression.Convert(messageParam, messageType);
                var callExpression = Expression.Call(castTarget, invocationMethod, castMessage);
                this.invoker = Expression.Lambda<Action<object, object>>(callExpression, targetParam, messageParam).Compile();
            }

            public void Invoke(object target, Type messageType, object message, Action<Action> dispatcher)
            {
                if (this.messageType.IsAssignableFrom(messageType))
                    dispatcher(() => this.invoker(target, message));
            }
        }
    }
}
