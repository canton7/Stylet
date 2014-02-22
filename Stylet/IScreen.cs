using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Stylet
{
    public interface IViewAware
    {
        UIElement View { get; }
        void AttachView(UIElement view);
    }

    public interface IActivate
    {
        bool IsActive { get; }
        void Activate();
        event EventHandler<ActivationEventArgs> Activated;
    }

    public interface IDeactivate
    {
        void Deactivate(bool close);
        event EventHandler<DeactivationEventArgs> Deactivated;
    }

    public interface IHaveDisplayName
    {
        string DisplayName { get; set; }
    }

    public interface IChild
    {
        object Parent { get; set; }
    }

    public interface IClose
    {
        void TryClose();
    }

    public interface IGuardClose : IClose
    {
        Task<bool> CanCloseAsync();
    }

    public interface IScreen : IViewAware, IHaveDisplayName, IActivate, IDeactivate, IChild, IGuardClose
    {
    }


    public class ActivationEventArgs : EventArgs
    {
    }

    public class DeactivationEventArgs : EventArgs
    {
        public bool WasClosed;
    }
}
