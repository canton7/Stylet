using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

using Expressions = System.Linq.Expressions;

namespace Stylet
{
    public class ActionCommand : ICommand
    {
        private FrameworkElement subject;
        private string methodName;
        private Func<bool> guardPropertyGetter;
        private Func<object> methodInvoker;

        private object target;

        public ActionCommand(FrameworkElement subject, string methodName)
        {
            this.subject = subject;
            this.methodName = methodName;

            this.UpdateGuardHandler();


            DependencyPropertyDescriptor.FromProperty(View.TargetProperty, typeof(View)).AddValueChanged(this.subject, (o, e) => this.UpdateGuardHandler());
        }

        private string GuardName
        {
            get { return "Can" + this.methodName; }
        }

        private void UpdateGuardHandler()
        {
            var newTarget = View.GetTarget(this.subject);

            this.guardPropertyGetter = null;
            if (newTarget != null)
            {
                var guardPropertyInfo = newTarget.GetType().GetProperty(this.GuardName);
                if (guardPropertyInfo != null && guardPropertyInfo.PropertyType == typeof(bool))
                {
                    var param = Expressions.Expression.Parameter(typeof(bool), "returnValue");
                    var propertyAccess = Expressions.Expression.Property(param, guardPropertyInfo);
                    this.guardPropertyGetter = Expressions.Expression.Lambda<Func<bool>>(propertyAccess, param).Compile();
                }
                    
            }

            var oldTarget = this.target as INotifyPropertyChanged;
            if (oldTarget != null)
                oldTarget.PropertyChanged -= this.PropertyChangedHandler;

            this.target = newTarget;

            var inpc = newTarget as INotifyPropertyChanged;
            if (this.guardPropertyGetter != null && inpc != null)
                inpc.PropertyChanged += this.PropertyChangedHandler;

            this.UpdateCanExecute();
        }

        private void UpdateMethodInvoker()
        {
            if (this.target == null)
                throw new ArgumentException("Target not set");

            var methodInfo = this.target.GetType().GetMethod(this.methodName);
            if (methodInfo == null)
                throw new Exception(String.Format("Unable to find method {0} on {1}", this.methodName, this.target.GetType().Name));
            
            var target = Expressions.Expression.Constant(this.target);
            var param = Expressions.Expression.Parameter(typeof(object), "parameter");
            Expressions.Expression call;
            
            var methodParameters = methodInfo.GetParameters();
            if (methodParameters.Length == 0)
            {
                call = Expressions.Expression.Call(target, methodInfo);
            }
            else if (methodParameters.Length == 1)
            {
                var convertedParam = Expressions.Expression.Convert(param, methodParameters[0].ParameterType);
                call = Expressions.Expression.Call(target, methodInfo, convertedParam);
            }
            else
            {
                throw new Exception(String.Format("Method {0} must accept either 0 or 1 arguments", this.methodName));
            }

            this.methodInvoker = Expressions.Expression.Lambda<Func<object>>(call, param).Compile();
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (String.IsNullOrEmpty(e.PropertyName) || e.PropertyName == this.GuardName)
            {
                this.UpdateCanExecute();
            }
        }

        private void UpdateCanExecute()
        {
            var handler = this.CanExecuteChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public bool CanExecute(object parameter)
        {
            if (this.guardPropertyGetter == null)
                return true;

            return this.guardPropertyGetter();
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (this.target == null)
                throw new ArgumentException("Target not set");

            var methodInfo = this.target.GetType().GetMethod(this.methodName);
            if (methodInfo == null)
                throw new Exception(String.Format("Unable to find method {0} on {1}", this.methodName, this.target.GetType().Name));

            if (methodInfo != null)
            {
                var parameters = methodInfo.GetParameters().Length == 1 ? new[] { parameter } : null;
                methodInfo.Invoke(this.target, parameters);
            }
        }
    }
}
