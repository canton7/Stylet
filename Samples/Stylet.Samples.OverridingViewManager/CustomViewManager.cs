using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Stylet.Samples.OverridingViewManager;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ViewModelAttribute : Attribute
{
    public ViewModelAttribute(Type viewModel)
    {
        this.ViewModel = viewModel;
    }

    public Type ViewModel { get; }
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
            throw new Exception(string.Format("Could not find View for ViewModel {0}", modelType.Name));
        return viewType;
    }
}
