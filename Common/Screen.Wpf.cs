using Stylet.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;

namespace Stylet;

public partial class Screen : ValidatingModelBase
{
    /// <summary>
    /// Initialises a new instance of the <see cref="Screen"/> class, which can validate properties using the given validator
    /// </summary>
    /// <param name="validator">Validator to use</param>
    public Screen(IModelValidator validator) : base(validator)
    {
        this.Initialize();
    }
}
