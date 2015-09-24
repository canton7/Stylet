using System;

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
            var screenAsScreenState = screen as IScreenState;
            if (screenAsScreenState != null)
                screenAsScreenState.Activate();
        }

        /// <summary>
        /// Attempt to deactivate the screen, if it implements IDeactivate
        /// </summary>
        /// <param name="screen">Screen to deactivate</param>
        public static void TryDeactivate(object screen)
        {
            var screenAsScreenState = screen as IScreenState;
            if (screenAsScreenState != null)
                screenAsScreenState.Deactivate();
        }

        /// <summary>
        /// Try to close the screen, if it implements IClose
        /// </summary>
        /// <param name="screen">Screen to close</param>
        public static void TryClose(object screen)
        {
            var screenAsScreenState = screen as IScreenState;
            if (screenAsScreenState != null)
                screenAsScreenState.Close();
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
        public static void ActivateWith(this IScreenState child, IScreenState parent)
        {
            var weakChild = new WeakReference<IScreenState>(child);
            EventHandler<ActivationEventArgs> handler = null;
            handler = (o, e) =>
            {
                IScreenState strongChild;
                if (weakChild.TryGetTarget(out strongChild))
                    strongChild.Activate();
                else
                    parent.Activated -= handler;
            };
            parent.Activated += handler;
        }

        /// <summary>
        /// Deactivate the child whenever the parent is deactivated
        /// </summary>
        /// <example>child.DeactivateWith(this)</example>
        /// <param name="child">Child to deactivate whenever the parent is deacgtivated</param>
        /// <param name="parent">Parent to observe</param>
        public static void DeactivateWith(this IScreenState child, IScreenState parent)
        {
            var weakChild = new WeakReference<IScreenState>(child);
            EventHandler<DeactivationEventArgs> handler = null;
            handler = (o, e) =>
            {
                IScreenState strongChild;
                if (weakChild.TryGetTarget(out strongChild))
                    strongChild.Deactivate();
                else
                    parent.Deactivated -= handler;
            };
            parent.Deactivated += handler;
        }

        /// <summary>
        /// Close the child whenever the parent is closed
        /// </summary>
        /// <example>child.CloseWith(this)</example>
        /// <param name="child">Child to close when the parent is closed</param>
        /// <param name="parent">Parent to observe</param>
        public static void CloseWith(this IScreenState child, IScreenState parent)
        {
            var weakChild = new WeakReference<IScreenState>(child);
            EventHandler<CloseEventArgs> handler = null;
            handler = (o, e) =>
            {
                IScreenState strongChild;
                if (weakChild.TryGetTarget(out strongChild))
                    TryClose(strongChild);
                else
                    parent.Closed -= handler;
            };
            parent.Closed += handler;
        }

        /// <summary>
        /// Activate, Deactivate, or Close the child whenever the parent is Activated, Deactivated, or Closed
        /// </summary>
        /// <example>child.ConductWith(this)</example>
        /// <param name="child">Child to conduct with the parent</param>
        /// <param name="parent">Parent to observe</param>
        public static void ConductWith(this IScreenState child, IScreenState parent)
        {
            child.ActivateWith(parent);
            child.DeactivateWith(parent);
            child.CloseWith(parent);
        }
    }
}
