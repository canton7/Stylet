using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyletIntegrationTests.OnUnhandledException
{
    public class WindowViewModel : Screen
    {
        public void ThrowException()
        {
            throw new Exception("Hello");
        }
    }
}
