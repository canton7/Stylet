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

        public static void TryDeactivate(object screen)
        {
            var screenAsDeactivate = screen as IDeactivate;
            if (screenAsDeactivate != null)
                screenAsDeactivate.Deactivate();
        }

        public static void TryClose(object screen)
        {
            var screenAsClose = screen as IClose;
            if (screenAsClose != null)
                screenAsClose.Close();
        }

        public static void ActivateWith(this IActivate child, IActivate parent)
        {
            WeakEventManager<IActivate, ActivationEventArgs>.AddHandler(parent, "Activated", (o, e) => child.Activate());
        }

        public static void DeactivateWith(this IDeactivate child, IDeactivate parent)
        {
            WeakEventManager<IDeactivate, DeactivationEventArgs>.AddHandler(parent, "Deactivated", (o, e) => child.Deactivate());
        }

        public static void CloseWith(this IClose child, IClose parent)
        {
            WeakEventManager<IClose, CloseEventArgs>.AddHandler(parent, "Closed", (o, e) => child.Close());
        }

        public static void ConductWith<TChild, TParent>(this TChild child, TParent parent)
            where TChild : IActivate, IDeactivate, IClose
            where TParent : IActivate, IDeactivate, IClose
        {
            child.ActivateWith(parent);
            child.DeactivateWith(parent);
            child.CloseWith(parent);
        }
    }
}
