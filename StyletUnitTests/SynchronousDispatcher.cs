using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyletUnitTests
{
    internal class SynchronousDispatcher : IDispatcher
    {
        public void Post(Action action)
        {
            throw new InvalidOperationException();
        }

        public void Send(Action action)
        {
            throw new InvalidOperationException();
        }

        public bool IsCurrent
        {
            get { return true; }
        }
    }
}
