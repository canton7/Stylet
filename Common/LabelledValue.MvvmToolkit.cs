using System.Runtime.CompilerServices;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Stylet;

public partial class LabelledValue<T> : ObservableObject
{
    private bool SetAndNotify<TProperty>(ref TProperty field, TProperty value, [CallerMemberName] string propertyName = "") =>
        this.SetProperty(ref field, value, propertyName);
}
