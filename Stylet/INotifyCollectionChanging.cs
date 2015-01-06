using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stylet
{
    /// <summary>
    /// Notifies listeners of the intention to perform dynamic changes, such as when items get added and removed or the whole list is refreshed.
    /// </summary>
    public interface INotifyCollectionChanging
    {
        /// <summary>
        /// Occurs when the collection will change
        /// </summary>
        event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}
