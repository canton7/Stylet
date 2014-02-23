using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Stylet
{
    public interface IViewManager
    {
        void OnModelChanged(DependencyObject targetLocation, DependencyPropertyChangedEventArgs e);
        UIElement LocateViewForModel(object model);
        void BindViewToModel(UIElement view, object viewModel);
    }

    public class ViewManager : IViewManager
    {
        public virtual void OnModelChanged(DependencyObject targetLocation, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue == e.NewValue)
                return;

            if (e.NewValue != null)
            {
                UIElement view;
                var viewModelAsViewAware = e.NewValue as IViewAware;
                if (viewModelAsViewAware != null && viewModelAsViewAware.View != null)
                {
                    view = viewModelAsViewAware.View;
                }
                else
                {
                    view = this.LocateViewForModel(e.NewValue);
                    this.BindViewToModel(view, e.NewValue);
                }

                View.SetContentProperty(targetLocation, view);
            }
            else
            {
                View.SetContentProperty(targetLocation, null);
            }
        }

        public virtual UIElement LocateViewForModel(object model)
        {
            var modelName = model.GetType().FullName;
            var viewName = Regex.Replace(modelName, @"ViewModel", "View");
            // TODO: This might need some more thinking
            var viewType = AssemblySource.Assemblies.SelectMany(x => x.GetExportedTypes()).FirstOrDefault(x => x.FullName == viewName);

            if (viewType == null)
                throw new Exception(String.Format("Unable to find a View with type {0}", viewName));

            if (viewType.IsInterface || viewType.IsAbstract || !typeof(UIElement).IsAssignableFrom(viewType))
                throw new Exception(String.Format("Found type for view : {0}, but it wasn't a class derived from UIElement", viewType.Name));

            var view = (UIElement)IoC.GetInstance(viewType, null);

            // If it doesn't have a code-behind, this won't be called
            var initializer = viewType.GetMethod("InitializeComponent", BindingFlags.Public | BindingFlags.Instance);
            if (initializer != null)
                initializer.Invoke(view, null);

            return view;
        }

        public virtual void BindViewToModel(UIElement view, object viewModel)
        {
            View.SetTarget(view, viewModel);

            var viewAsFrameworkElement = view as FrameworkElement;
            if (viewAsFrameworkElement != null)
                viewAsFrameworkElement.DataContext = viewModel;

            var viewModelAsViewAware = viewModel as IViewAware;
            if (viewModelAsViewAware != null)
                viewModelAsViewAware.AttachView(view);
        }
    }
}
