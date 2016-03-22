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
    /// Configuration object consumed by <see cref="ViewManager"/>
    /// </summary>
    public class ViewManagerConfig
    {
        /// <summary>
        /// Gets or sets the ViewFactory to use
        /// </summary>
        public Func<Type, object> ViewFactory { get; set; }

        /// <summary>
        /// Gets or sets the Assembles to search for views in
        /// </summary>
        public List<Assembly> ViewAssemblies { get; set; }
    }

    /// <summary>
    /// Default implementation of ViewManager. Responsible for locating, creating, and settings up Views. Also owns the View.Model and View.ActionTarget attached properties
    /// </summary>
    public class ViewManager : IViewManager
    {
        private static readonly ILogger logger = LogManager.GetLogger(typeof(ViewManager));

        private Func<Type, object> _viewFactory; // This is assigned by the ctor

        /// <summary>
        /// Gets or sets the delegate used to retrieve an instance of a view
        /// </summary>
        public Func<Type, object> ViewFactory
        {
            get { return this._viewFactory; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                this._viewFactory = value;
            }
        }

        private List<Assembly> _viewAssemblies; // This is assigned by the ctor

        /// <summary>
        /// Gets or sets the assemblies which are used for IoC container auto-binding and searching for Views.
        /// </summary>
        public List<Assembly> ViewAssemblies
        {
            get { return this._viewAssemblies; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                this._viewAssemblies = value;
            }
        }

        private Dictionary<string, string> _namespaceTransformations = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets a set of transformations to be applied to the ViewModel's namespace: string to find -> string to replace it with
        /// </summary>
        public Dictionary<string, string> NamespaceTransformations
        {
            get { return this._namespaceTransformations; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                this._namespaceTransformations = value;
            }
        }

        private string _viewNameSuffix = "View";

        /// <summary>
        /// Gets or sets the suffix replacing 'ViewModel' (see <see cref="ViewModelNameSuffix"/>). Defaults to 'View'
        /// </summary>
        public string ViewNameSuffix
        {
            get { return this._viewNameSuffix; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                this._viewNameSuffix = value;
            }
        }

        private string _viewModelNameSuffix = "ViewModel";

        /// <summary>
        /// Gets or sets the suffix of ViewModel names, defaults to 'ViewModel'. This will be replaced by <see cref="ViewNameSuffix"/>
        /// </summary>
        public string ViewModelNameSuffix
        {
            get { return this._viewModelNameSuffix; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                this._viewModelNameSuffix = value;
            }
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="ViewManager"/> class, with the given viewFactory
        /// </summary>
        /// <param name="config">Configuration object</param>
        public ViewManager(ViewManagerConfig config)
        {
            // Config.ViewAssemblies cannot be null - ViewManagerConfig ensures this
            if (config.ViewFactory == null)
                throw new ArgumentNullException("viewFactory");
            if (config.ViewAssemblies == null)
                throw new ArgumentNullException("viewAssemblies");

            this.ViewFactory = config.ViewFactory;
            this.ViewAssemblies = config.ViewAssemblies;
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
                if (view is Window)
                {
                    var e = new StyletInvalidViewTypeException(String.Format("s:View.Model=\"...\" tried to show a View of type '{0}', but that View derives from the Window class. " +
                    "Make sure any Views you display using s:View.Model=\"...\" do not derive from Window (use UserControl or similar)", view.GetType().Name));
                    logger.Error(e);
                    throw e;
                }
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
        /// Given the expected name for a view, locate its type (or return null if a suitable type couldn't be found)
        /// </summary>
        /// <param name="viewName">View name to locate the type for</param>
        /// <param name="extraAssemblies">Extra assemblies to search through</param>
        /// <returns>Type for that view name</returns>
        protected virtual Type ViewTypeForViewName(string viewName, IEnumerable<Assembly> extraAssemblies)
        {
            return this.ViewAssemblies.Concat(extraAssemblies).Select(x => x.GetType(viewName)).FirstOrDefault(x => x != null);
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
            string transformed = modelTypeName;

            foreach (var transformation in this.NamespaceTransformations)
            {
                if (transformed.StartsWith(transformation.Key + "."))
                {
                    transformed = transformation.Value + transformed.Substring(transformation.Key.Length);
                    break;
                }
            }

            transformed = Regex.Replace(transformed,
                String.Format(@"(?<=.){0}(?=s?\.)|{0}$", Regex.Escape(this.ViewModelNameSuffix)),
                Regex.Escape(this.ViewNameSuffix));

            return transformed;
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

            // Also include the ViewModel's assembly, to be helpful
            var viewType = this.ViewTypeForViewName(viewName, new[] { modelType.Assembly });
            if (viewType == null)
            {
                var e = new StyletViewLocationException(String.Format("Unable to find a View with type {0}", viewName), viewName);
                logger.Error(e);
                throw e;
            }
            else
            {
                logger.Info("Searching for a View with name {0}, and found {1}", viewName, viewType);
            }

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

            this.InitializeView(view, viewType);

            return view;
        }

        /// <summary>
        /// Given a view, take steps to initialize it (for example calling InitializeComponent)
        /// </summary>
        /// <param name="view">View to initialize</param>
        /// <param name="viewType">Type of view, passed for efficiency reasons</param>
        public virtual void InitializeView(UIElement view, Type viewType)
        {
            // If it doesn't have a code-behind, this won't be called
            // We have to use this reflection here, since the InitializeComponent is a method on the View, not on any of its base classes
            var initializer = viewType.GetMethod("InitializeComponent", BindingFlags.Public | BindingFlags.Instance);
            if (initializer != null)
                initializer.Invoke(view, null);
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

    /// <summary>
    /// Exception raise when the located View is of the wrong type (Window when expected UserControl, etc)
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
    public class StyletInvalidViewTypeException : Exception
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="StyletInvalidViewTypeException"/> class
        /// </summary>
        /// <param name="message">Message associated with the Exception</param>
        public StyletInvalidViewTypeException(string message)
            : base(message)
        { }
    }
}
