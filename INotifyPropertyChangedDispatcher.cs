using System;

namespace Stylet
{
    /// <summary>
    /// Knows how to dispatch its PropertyChanged events using a given dispatcher
    /// </summary>
    public interface INotifyPropertyChangedDispatcher
    {
        /// <summary>
        /// Gets or sets the dispatcher to use.
        /// Called with an action, which should itself be called in the appropriate context
        /// </summary>
        Action<Action> PropertyChangedDispatcher { get; set; }
    }
}
