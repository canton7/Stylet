using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stylet
{
    public static class ScreenExtensions
    {
        public static void TryActivate(object screen)
        {
            var screenAsActivate = screen as IActivate;
            if (screenAsActivate != null)
                screenAsActivate.Activate();
        }

        public static void TryDeactivate(object screen, bool close)
        {
            var screenAsDeactivate = screen as IDeactivate;
            if (screenAsDeactivate != null)
                screenAsDeactivate.Deactivate(close);
        }
    }
}
