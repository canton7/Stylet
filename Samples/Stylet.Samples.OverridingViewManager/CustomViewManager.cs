using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Stylet.Samples.OverridingViewManager
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class ViewModelAttribute : Attribute
    {
        readonly Type viewModel;

        public ViewModelAttribute(Type viewModel)
        {
            this.viewModel = viewModel;
        }

        public Type ViewModel
        {
            get { return viewModel; }
        }
    }

    public class CustomViewManager : ViewManager
    {
        // Dictionary of ViewModel type -> View type
        private readonly Dictionary<Type, Type> viewModelToViewMapping;

        public CustomViewManager(ViewManagerConfig config)
            : base(config)
        {
            var mappings = from type in this.ViewAssemblies.SelectMany(x => x.GetExportedTypes())
                           let attribute = type.GetCustomAttribute<ViewModelAttribute>()
                           where attribute != null && typeof(UIElement).IsAssignableFrom(type)
                           select new { View = type, ViewModel = attribute.ViewModel };

            this.viewModelToViewMapping = mappings.ToDictionary(x => x.ViewModel, x => x.View);
        }

        protected override Type LocateViewForModel(Type modelType)
        {
            Type viewType;
            if (!this.viewModelToViewMapping.TryGetValue(modelType, out viewType))
                throw new Exception(String.Format("Could not find View for ViewModel {0}", modelType.Name));
            return viewType;
        }
    }
}
