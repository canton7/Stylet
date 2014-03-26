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

    public interface IHasActivationState
    {
        bool IsActive { get; }
    }

    public interface IActivate : IHasActivationState
    {
        void Activate();
        event EventHandler<ActivationEventArgs> Activated;
    }

    public interface IDeactivate : IHasActivationState
    {
        void Deactivate();
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

    public interface IDialogClose
    {
        void TryClose();
    }

    public interface IClose
    {
        void Close();
        event EventHandler<CloseEventArgs> Closed;
    }

    public interface IGuardClose
    {
        Task<bool> CanCloseAsync();
    }

    public interface IScreen : IViewAware, IHaveDisplayName, IActivate, IDeactivate, IChild, IDialogClose, IClose, IGuardClose
    {
    }


    public class ActivationEventArgs : EventArgs
    {
    }

    public class DeactivationEventArgs : EventArgs
    {
    }

    public class CloseEventArgs : EventArgs
    {
    }
}
