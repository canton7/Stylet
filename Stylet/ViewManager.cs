using Stylet.Xaml;
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
    /// <summary>
    /// Responsible for managing views. Locates the correct view, instantiates it, attaches it to its ViewModel correctly, and handles the View.Model attached property
    /// </summary>
    public interface IViewManager
    {
        /// <summary>
        /// Called by the View.Model attached property when the ViewModel its bound to changes
        void OnModelChanged(DependencyObject targetLocation, DependencyPropertyChangedEventArgs e);

        /// <summary>
        /// Given an instance of a ViewModel, locate the correct view for it, and instantiate it
        /// </summary>
        /// <param name="model">ViewModel to locate the view for</param>
        /// <returns>An instance of the correct view</returns>
        UIElement CreateViewForModel(object model);

        /// <summary>
        /// Given an instance of a ViewModel and an instance of its View, bind the two together
        /// </summary>
        /// <param name="view">View to bind to the ViewModel</param>
        /// <param name="viewModel">ViewModel to bind the View to</param>
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
                    view = this.CreateViewForModel(e.NewValue);
                    this.BindViewToModel(view, e.NewValue);
                }

                View.SetContentProperty(targetLocation, view);
            }
            else
            {
                View.SetContentProperty(targetLocation, null);
            }
        }

        public virtual Type LocateViewForModel(Type modelType)
        {
            var viewName = Regex.Replace(modelType.FullName, @"ViewModel", "View");
            // TODO: This might need some more thinking
            var viewType = AssemblySource.Assemblies.SelectMany(x => x.GetExportedTypes()).FirstOrDefault(x => x.FullName == viewName);

            if (viewType == null)
                throw new Exception(String.Format("Unable to find a View with type {0}", viewName));

            return viewType;
        }

        public virtual UIElement CreateViewForModel(object model)
        {
            var viewType = this.LocateViewForModel(model.GetType());

            if (viewType.IsInterface || viewType.IsAbstract || !typeof(UIElement).IsAssignableFrom(viewType))
                throw new Exception(String.Format("Found type for view: {0}, but it wasn't a class derived from UIElement", viewType.Name));

            var view = (UIElement)IoC.GetInstance(viewType, null);

            // If it doesn't have a code-behind, this won't be called
            var initializer = viewType.GetMethod("InitializeComponent", BindingFlags.Public | BindingFlags.Instance);
            if (initializer != null)
                initializer.Invoke(view, null);

            return view;
        }

        public virtual void BindViewToModel(UIElement view, object viewModel)
        {
            View.SetActionTarget(view, viewModel);

            var viewAsFrameworkElement = view as FrameworkElement;
            if (viewAsFrameworkElement != null)
                viewAsFrameworkElement.DataContext = viewModel;

            var viewModelAsViewAware = viewModel as IViewAware;
            if (viewModelAsViewAware != null)
                viewModelAsViewAware.AttachView(view);
        }
    }
}
