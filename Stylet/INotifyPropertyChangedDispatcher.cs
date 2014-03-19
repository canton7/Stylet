using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stylet
{
    public interface INotifyPropertyChangedDispatcher
    {
        Action<Action> PropertyChangedDispatcher { get; set; }
    }
}
