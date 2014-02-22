using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

        public static void ActivateWith(this IActivate child, IActivate parent)
        {
            WeakEventManager<IActivate, ActivationEventArgs>.AddHandler(parent, "Activated", (o, e) => child.Activate());
        }

        public static void DeactivateWith(this IDeactivate child, IDeactivate parent)
        {
            WeakEventManager<IDeactivate, DeactivationEventArgs>.AddHandler(parent, "Deactivated", (o, e) => child.Deactivate(e.WasClosed));
        }

        public static void ConductWith<TChild, TParent>(this TChild child, TParent parent)
            where TChild : IActivate, IDeactivate
            where TParent : IActivate, IDeactivate
        {
            child.ActivateWith(parent);
            child.DeactivateWith(parent);
        }
    }
}
