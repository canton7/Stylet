using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Stylet
{
    public static class ViewLocator
    {
        public static UIElement LocateForModel(object model)
        {
            var modelName = model.GetType().FullName;
            var viewName = Regex.Replace(modelName, @"ViewModel", "View");
            var viewType = Assembly.GetEntryAssembly().GetType(viewName);

            if (viewType == null)
                throw new Exception(String.Format("Unable to find a View with type {0}", viewName));

            if (viewType.IsInterface || viewType.IsAbstract || !typeof(UIElement).IsAssignableFrom(viewType))
                throw new Exception(String.Format("Found type for view : {0}, but it wasn't a class derived from UIElement", viewType.Name));

            var view = (UIElement)IoC.GetInstance(viewType, null);

            // If it doesn't have a code-behind, this won't be called
            var initializer = viewType.GetMethod("InitializeComponent", BindingFlags.Public | BindingFlags.Instance);
            if (initializer != null)
                initializer.Invoke(view, null);

            return (UIElement)view;
        }
    }
}
