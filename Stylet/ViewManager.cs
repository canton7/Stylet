using Stylet.Logging;
using Stylet.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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
        /// Given a ViewModel instance, locate its View type (using LocateViewForModel), and instantiates it
        /// </summary>
        /// <param name="model">ViewModel to locate and instantiate the View for</param>
        /// <returns>Instantiated and setup view</returns>
        UIElement CreateViewForModel(object model);

        /// <summary>
        /// Given an instance of a ViewModel and an instance of its View, bind the two together
        /// </summary>
        /// <param name="view">View to bind to the ViewModel</param>
        /// <param name="viewModel">ViewModel to bind the View to</param>
        void BindViewToModel(UIElement view, object viewModel);

        /// <summary>
        /// Create a View for the given ViewModel, and bind the two together, if the model doesn't already have a view
        /// </summary>
        /// <param name="model">ViewModel to create a Veiw for</param>
        /// <returns>Newly created View, bound to the given ViewModel</returns>
        UIElement CreateAndBindViewForModelIfNecessary(object model);
    }

    /// <summary>
    /// Configuration passed to ViewManager (normally implemented by BootstrapperBase)
    /// </summary>
    public interface IViewManagerConfig
    {
        /// <summary>
        /// Gets the assemblies which are used for IoC container auto-binding and searching for Views.
        /// Set this in Configure() if you want to override it
        /// </summary>
        IReadOnlyList<Assembly> Assemblies { get; }

        /// <summary>
        /// Given a type, use the IoC container to fetch an instance of it
        /// </summary>
        /// <param name="type">Type of instance to fetch</param>
        /// <returns>Fetched instance</returns>
        object GetInstance(Type type);
    }

    /// <summary>
    /// Default implementation of ViewManager. Responsible for locating, creating, and settings up Views. Also owns the View.Model and View.ActionTarget attached properties
    /// </summary>
    public class ViewManager : IViewManager
    {
        private static readonly ILogger logger = LogManager.GetLogger(typeof(ViewManager));

        /// <summary>
        /// Gets or sets the assemblies searched for View types
        /// </summary>
        protected IReadOnlyList<Assembly> Assemblies { get; set; }

        /// <summary>
        /// Gets or sets the factory used to create view instances from their type
        /// </summary>
        protected Func<Type, object> ViewFactory { get; set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="ViewManager"/> class, with the given viewFactory
        /// </summary>
        /// <param name="config">Configuration to use</param>
        public ViewManager(IViewManagerConfig config)
        {
            this.Assemblies = config.Assemblies;
            this.ViewFactory = config.GetInstance;
        }

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
                logger.Info("View.Model changed for {0} from {1} to {2}", targetLocation, oldValue, newValue);
                var view = this.CreateAndBindViewForModelIfNecessary(newValue);
                View.SetContentProperty(targetLocation, view);
            }
            else
            {
                logger.Info("View.Model cleared for {0}, from {1}", targetLocation, oldValue);
                View.SetContentProperty(targetLocation, null);
            }
        }

        /// <summary>
        /// Create a View for the given ViewModel, and bind the two together, if the model doesn't already have a view
        /// </summary>
        /// <param name="model">ViewModel to create a Veiw for</param>
        /// <returns>Newly created View, bound to the given ViewModel</returns>
        public virtual UIElement CreateAndBindViewForModelIfNecessary(object model)
        {
            var modelAsViewAware = model as IViewAware;
            if (modelAsViewAware != null && modelAsViewAware.View != null)
            {
                logger.Info("ViewModel {0} already has a View attached to it. Not attaching another", model);
                return modelAsViewAware.View;
            }

            return this.CreateAndBindViewForModel(model);
        }

        /// <summary>
        /// Create a View for the given ViewModel, and bind the two together
        /// </summary>
        /// <param name="model">ViewModel to create a Veiw for</param>
        /// <returns>Newly created View, bound to the given ViewModel</returns>
        protected virtual UIElement CreateAndBindViewForModel(object model)
        {
            // Need to bind before we initialize the view
            // Otherwise e.g. the Command bindings get evaluated (by InitializeComponent) but the ActionTarget hasn't been set yet
            logger.Info("Instantiating and binding a new View to ViewModel {0}", model);
            var view = this.CreateViewForModel(model);
            this.BindViewToModel(view, model);
            return view;
        }

        /// <summary>
        /// Given the expected name for a view, locate its type (or throw an exception if a suitable type couldn't be found)
        /// </summary>
        /// <param name="viewName">View name to locate the type for</param>
        /// <returns>Type for that view name</returns>
        protected virtual Type ViewTypeForViewName(string viewName)
        {
            // TODO: This might need some more thinking
            var viewType = this.Assemblies.SelectMany(x => x.GetExportedTypes()).FirstOrDefault(x => x.FullName == viewName);
            if (viewType == null)
            {
                var e = new StyletViewLocationException(String.Format("Unable to find a View with type {0}", viewName), viewName);
                logger.Error(e);
                throw e;
            }

            logger.Info("Searching for a View with name {0}, and found {1}", viewName, viewType);

            return viewType;
        }

        /// <summary>
        /// Given the full name of a ViewModel type, determine the corresponding View type nasme
        /// </summary>
        /// <remarks>
        /// This is used internally by LocateViewForModel. If you override LocateViewForModel, you
        /// can simply ignore this method.
        /// </remarks>
        /// <param name="modelTypeName">ViewModel type name to get the View type name for</param>
        /// <returns>View type name</returns>
        protected virtual string ViewTypeNameForModelTypeName(string modelTypeName)
        {
            return Regex.Replace(modelTypeName, @"(?<=.)ViewModel(?=s?\.)|ViewModel$", "View");
        }

        /// <summary>
        /// Given the type of a model, locate the type of its View (or throw an exception)
        /// </summary>
        /// <param name="modelType">Model to find the view for</param>
        /// <returns>Type of the ViewModel's View</returns>
        protected virtual Type LocateViewForModel(Type modelType)
        {
            var modelName = modelType.FullName;
            var viewName = this.ViewTypeNameForModelTypeName(modelName);
            if (modelName == viewName)
                throw new StyletViewLocationException(String.Format("Unable to transform ViewModel name {0} into a suitable View name", modelName), viewName);

            var viewType = this.ViewTypeForViewName(viewName);
            return viewType;
        }

        /// <summary>
        /// Given a ViewModel instance, locate its View type (using LocateViewForModel), and instantiates it
        /// </summary>
        /// <param name="model">ViewModel to locate and instantiate the View for</param>
        /// <returns>Instantiated and setup view</returns>
        public virtual UIElement CreateViewForModel(object model)
        {
            var viewType = this.LocateViewForModel(model.GetType());

            if (viewType.IsAbstract || !typeof(UIElement).IsAssignableFrom(viewType))
            {
                var e = new StyletViewLocationException(String.Format("Found type for view: {0}, but it wasn't a class derived from UIElement", viewType.Name), viewType.Name);
                logger.Error(e);
                throw e;
            }

            var view = (UIElement)this.ViewFactory(viewType);

            // If it doesn't have a code-behind, this won't be called
            // We have to use this reflection here, since the InitializeComponent is a method on the View, not on any of its base classes
            var initializer = viewType.GetMethod("InitializeComponent", BindingFlags.Public | BindingFlags.Instance);
            if (initializer != null)
                initializer.Invoke(view, null);

            return view;
        }

        /// <summary>
        /// Given an instance of a ViewModel and an instance of its View, bind the two together
        /// </summary>
        /// <param name="view">View to bind to the ViewModel</param>
        /// <param name="viewModel">ViewModel to bind the View to</param>
        public virtual void BindViewToModel(UIElement view, object viewModel)
        {
            logger.Info("Setting {0}'s ActionTarget to {1}", view, viewModel);
            View.SetActionTarget(view, viewModel);

            var viewAsFrameworkElement = view as FrameworkElement;
            if (viewAsFrameworkElement != null)
            {
                logger.Info("Setting {0}'s DataContext to {1}", view, viewModel);
                viewAsFrameworkElement.DataContext = viewModel;
            }

            var viewModelAsViewAware = viewModel as IViewAware;
            if (viewModelAsViewAware != null)
            {
                logger.Info("Setting {0}'s View to {1}", viewModel, view);
                viewModelAsViewAware.AttachView(view);
            }
        }
    }

    /// <summary>
    /// Exception raised while attempting to locate a View for a ViewModel
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
    public class StyletViewLocationException : Exception
    {
        /// <summary>
        /// Name of the View in question
        /// </summary>
        public readonly string ViewTypeName;

        /// <summary>
        /// Initialises a new instance of the <see cref="StyletViewLocationException"/> class
        /// </summary>
        /// <param name="message">Message associated with the Exception</param>
        /// <param name="viewTypeName">Name of the View this question was thrown for</param>
        public StyletViewLocationException(string message, string viewTypeName)
            : base(message)
        {
            this.ViewTypeName = viewTypeName;
        }
    }
}
