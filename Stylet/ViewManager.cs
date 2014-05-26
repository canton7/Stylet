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
        /// Called by View whenever its current View.Model changes. Will locate and instantiate the correct view, and set it as the target's Content
        /// </summary>
        /// <param name="targetLocation">Thing which View.Model was changed on. Will have its Content set</param>
        /// <param name="oldValue">Previous value of View.Model</param>
        /// <param name="newValue">New value of View.Model</param>
        void OnModelChanged(DependencyObject targetLocation, object oldValue, object newValue);

        /// <summary>
        /// Given a ViewModel instance, locate its View type (using LocateViewForModel), instantiates and initializes it
        /// </summary>
        /// <param name="model">ViewModel to locate and instantiate the View for</param>
        /// <returns>Instantiated and setup view</returns>
        UIElement CreateAndSetupViewForModel(object model);

        /// <summary>
        /// Given an instance of a ViewModel and an instance of its View, bind the two together
        /// </summary>
        /// <param name="view">View to bind to the ViewModel</param>
        /// <param name="viewModel">ViewModel to bind the View to</param>
        void BindViewToModel(UIElement view, object viewModel);
    }

    /// <summary>
    /// Default implementation of ViewManager. Responsible for locating, creating, and settings up Views. Also owns the View.Model and View.ActionTarget attached properties
    /// </summary>
    public class ViewManager : IViewManager
    {
        /// <summary>
        /// Called by View whenever its current View.Model changes. Will locate and instantiate the correct view, and set it as the target's Content
        /// </summary>
        /// <param name="targetLocation">Thing which View.Model was changed on. Will have its Content set</param>
        /// <param name="oldValue">Previous value of View.Model</param>
        /// <param name="newValue">New value of View.Model</param>
        public virtual void OnModelChanged(DependencyObject targetLocation, object oldValue, object newValue)
        {
            if (oldValue == newValue)
                return;

            if (newValue != null)
            {
                UIElement view;
                var viewModelAsViewAware = newValue as IViewAware;
                if (viewModelAsViewAware != null && viewModelAsViewAware.View != null)
                {
                    view = viewModelAsViewAware.View;
                }
                else
                {
                    view = this.CreateAndSetupViewForModel(newValue);
                    this.BindViewToModel(view, newValue);
                }

                View.SetContentProperty(targetLocation, view);
            }
            else
            {
                View.SetContentProperty(targetLocation, null);
            }
        }

        /// <summary>
        /// Given the expected name for a view, locate its type (or throw an exception if a suitable type couldn't be found)
        /// </summary>
        /// <param name="viewName">View name to locate the type for</param>
        /// <returns>Type for that view name</returns>
        public virtual Type ViewTypeForViewName(string viewName)
        {
            // TODO: This might need some more thinking
            var viewType = AssemblySource.Assemblies.SelectMany(x => x.GetExportedTypes()).FirstOrDefault(x => x.FullName == viewName);
            if (viewType == null)
                throw new Exception(String.Format("Unable to find a View with type {0}", viewName));

            return viewType;
        }

        /// <summary>
        /// Given the type of a model, locate the type of its View (or throw an exception)
        /// </summary>
        /// <param name="modelType">Model to find the view for</param>
        /// <returns>Type of the ViewModel's View</returns>
        public virtual Type LocateViewForModel(Type modelType)
        {
            var viewName = Regex.Replace(modelType.FullName, @"ViewModel", "View");
            var viewType = this.ViewTypeForViewName(viewName);

            return viewType;
        }

        /// <summary>
        /// Given an instance of a ViewModel and an instance of its View, bind the two together
        /// </summary>
        /// <param name="view">View to bind to the ViewModel</param>
        /// <param name="viewModel">ViewModel to bind the View to</param>
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

        /// <summary>
        /// Given a ViewModel instance, locate its View type (using LocateViewForModel), instantiates and initializes it, and binds it to the ViewModel (using BindViewToModel)
        /// </summary>
        /// <param name="model">ViewModel to locate and instantiate the View for</param>
        /// <returns>Instantiated and setup view</returns>
        public virtual UIElement CreateAndSetupViewForModel(object model)
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
    }
}
