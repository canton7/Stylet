using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace MicroMVVM
{
    public static class ViewLocator
    {
        public static UIElement LocateForModel(object model)
        {
            var modelName = model.GetType().FullName;
            var viewName = Regex.Replace(modelName, @"ViewModel", "View");
            var viewType = Assembly.GetEntryAssembly().GetType(modelName);
            if (viewType == null)
                throw new Exception(String.Format("Unable to find a View with type {0}", viewName));

            var instance = Activator.CreateInstance(viewType);
            if (!(instance is UIElement))
                throw new Exception(String.Format("Managed to create a {0}, but it wasn't a UIElement", viewName));

            var initializer = viewType.GetMethod("InitializeComponent", BindingFlags.Public | BindingFlags.Instance);
            if (initializer != null)
                initializer.Invoke(instance, null);

            return (UIElement)instance;
        }
    }
}
