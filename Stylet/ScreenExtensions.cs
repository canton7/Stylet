using System;
using System.Windows;

namespace Stylet
{
    /// <summary>
    /// Handy extensions for working with screens
    /// </summary>
    public static class ScreenExtensions
    {
        /// <summary>
        /// Attempt to activate the screen, if it implements IActivate
        /// </summary>
        /// <param name="screen">Screen to activate</param>
        public static void TryActivate(object screen)
        {
            var screenAsActivate = screen as IActivate;
            if (screenAsActivate != null)
                screenAsActivate.Activate();
        }

        /// <summary>
        /// Attempt to deactivate the screen, if it implements IDeactivate
        /// </summary>
        /// <param name="screen">Screen to deactivate</param>
        public static void TryDeactivate(object screen)
        {
            var screenAsDeactivate = screen as IDeactivate;
            if (screenAsDeactivate != null)
                screenAsDeactivate.Deactivate();
        }

        /// <summary>
        /// Try to close the screen, if it implements IClose
        /// </summary>
        /// <param name="screen">Screen to close</param>
        public static void TryClose(object screen)
        {
            var screenAsClose = screen as IClose;
            if (screenAsClose != null)
                screenAsClose.Close();
        }

        /// <summary>
        /// Try to dispose a screen, if it implements IDisposable
        /// </summary>
        /// <param name="screen">Screen to dispose</param>
        public static void TryDispose(object screen)
        {
            var screenAsDispose = screen as IDisposable;
            if (screenAsDispose != null)
                screenAsDispose.Dispose();
        }

        /// <summary>
        /// Activate the child whenever the parent is activated
        /// </summary>
        /// <example>child.ActivateWith(this)</example>
        /// <param name="child">Child to activate whenever the parent is activated</param>
        /// <param name="parent">Parent to observe</param>
        public static void ActivateWith(this IActivate child, IActivate parent)
        {
            WeakEventManager<IActivate, ActivationEventArgs>.AddHandler(parent, "Activated", (o, e) => child.Activate());
        }

        /// <summary>
        /// Deactivate the child whenever the parent is deactivated
        /// </summary>
        /// <example>child.DeactivateWith(this)</example>
        /// <param name="child">Child to deactivate whenever the parent is deacgtivated</param>
        /// <param name="parent">Parent to observe</param>
        public static void DeactivateWith(this IDeactivate child, IDeactivate parent)
        {
            WeakEventManager<IDeactivate, DeactivationEventArgs>.AddHandler(parent, "Deactivated", (o, e) => child.Deactivate());
        }

        /// <summary>
        /// Close the child whenever the parent is closed
        /// </summary>
        /// <example>child.CloseWith(this)</example>
        /// <param name="child">Child to close when the parent is closed</param>
        /// <param name="parent">Parent to observe</param>
        public static void CloseWith(this IClose child, IClose parent)
        {
            // Using TryCloseAndDispose ensures that Dispose is called if necessary
            WeakEventManager<IClose, CloseEventArgs>.AddHandler(parent, "Closed", (o, e) => TryClose(child));
        }

        /// <summary>
        /// Activate, Deactivate, or Close the child whenever the parent is Activated, Deactivated, or Closed
        /// </summary>
        /// <example>child.ConductWith(this)</example>
        /// <typeparam name="TChild">Type of the child</typeparam>
        /// <typeparam name="TParent">Type of the parent</typeparam>
        /// <param name="child">Child to conduct with the parent</param>
        /// <param name="parent">Parent to observe</param>
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
