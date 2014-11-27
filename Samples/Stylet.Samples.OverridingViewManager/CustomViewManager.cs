using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Reflection;

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
        private Dictionary<Type, Type> viewModelToViewMapping;

        public CustomViewManager()
        {
            var mappings = from type in AssemblySource.Assemblies.SelectMany(x => x.GetTypes())
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
