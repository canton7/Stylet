using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stylet
{
    /// <summary>
    /// Knows how to dispatch its PropertyChanged events using a given dispatcher
    /// </summary>
    public interface INotifyPropertyChangedDispatcher
    {
        /// <summary>
        /// The dispatcher to use. Called with an action, which should itself be called in the appropriate context
        /// </summary>
        Action<Action> PropertyChangedDispatcher { get; set; }
    }
}
