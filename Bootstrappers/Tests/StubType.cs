using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bootstrappers.Tests
{
    public class StubType : IDisposable
    {
        public static int DisposeCount { get; private set; }

        public static void Reset()
        {
            DisposeCount = 0;
        }

        public void Dispose()
        {
            DisposeCount++;
        }
    }
}
