using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

//namespace Stylet;

public partial class Test : ObservableValidator
{
}

public partial class Test
{
}

//public partial class Screen : ObservableValidator
//{
//    public Screen(IDictionary<object, object> items) : base(items)
//    {
//        this.Initialize();
//    }

//    public Screen(IServiceProvider serviceProvider, IDictionary<object, object> items) : base(serviceProvider, items)
//    {
//        this.Initialize();
//    }

//    public Screen(ValidationContext validationContext) : base(validationContext)
//    {
//        this.Initialize();
//    }

//    private bool SetAndNotify<T>(ref T field, T value, [CallerMemberName] string propertyName = "") =>
//        this.SetProperty(ref field, value, propertyName);

//    private void NotifyOfPropertyChange(string propertyName) =>
//        this.OnPropertyChanged(propertyName);
//}
