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
            var viewName = model.GetType().FullName;
            var modelName = Regex.Replace(viewName, @"ViewModel", "View");
            var modelType = Assembly.GetEntryAssembly().GetType(modelName);
            if (modelType == null)
                throw new Exception(String.Format("Unable to find a View with type {0}", modelName));

            var instance = Activator.CreateInstance(modelType);
            if (!(instance is UIElement))
                throw new Exception(String.Format("Managed to create a {0}, but it wasn't a UIElement", modelName));

            return (UIElement)instance;
        }
    }
}
