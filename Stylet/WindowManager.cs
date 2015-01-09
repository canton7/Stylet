using Stylet.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Stylet
{
    /// <summary>
    /// Manager capable of taking a ViewModel instance, instantiating its View and showing it as a dialog or window
    /// </summary>
    public interface IWindowManager
    {
        /// <summary>
        /// Given a ViewModel, show its corresponding View as a window
        /// </summary>
        /// <param name="viewModel">ViewModel to show the View for</param>
        void ShowWindow(object viewModel);

        /// <summary>
        /// Given a ViewModel, show its corresponding View as a Dialog
        /// </summary>
        /// <param name="viewModel">ViewModel to show the View for</param>
        /// <returns>DialogResult of the View</returns>
        bool? ShowDialog(object viewModel);

        /// <summary>
        /// Display a MessageBox
        /// </summary>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="buttons">A System.Windows.MessageBoxButton value that specifies which button or buttons to display.</param>
        /// <param name="icon">A System.Windows.MessageBoxImage value that specifies the icon to display.</param>
        /// <param name="defaultResult">A System.Windows.MessageBoxResult value that specifies the default result of the message box.</param>
        /// <param name="cancelResult">A System.Windows.MessageBoxResult value that specifies the cancel result of the message box</param>
        /// <param name="options">A System.Windows.MessageBoxOptions value object that specifies the options.</param>
        /// <param name="buttonLabels">A dictionary specifying the button labels, if desirable</param>
        /// <returns>The result chosen by the user</returns>
        MessageBoxResult ShowMessageBox(string messageBoxText, string caption = null,
            MessageBoxButton buttons = MessageBoxButton.OK,
            MessageBoxImage icon = MessageBoxImage.None,
            MessageBoxResult defaultResult = MessageBoxResult.None,
            MessageBoxResult cancelResult = MessageBoxResult.None,
            MessageBoxOptions options = MessageBoxOptions.None,
            IDictionary<MessageBoxResult, string> buttonLabels = null);
    }

    /// <summary>
    /// Default implementation of IWindowManager, is capable of showing a ViewModel's View as a dialog or a window
    /// </summary>
    public class WindowManager : IWindowManager
    {
        private static readonly ILogger logger = LogManager.GetLogger(typeof(WindowManager));
        private readonly IViewManager viewManager;
        private readonly Func<IMessageBoxViewModel> messageBoxViewModelFactory;

        /// <summary>
        /// Initialises a new instance of the <see cref="WindowManager"/> class, using the given <see cref="IViewManager"/>
        /// </summary>
        /// <param name="viewManager">IViewManager to use when creating views</param>
        /// <param name="messageBoxViewModelFactory">Delegate which returns a new IMessageBoxViewModel instance when invoked</param>
        public WindowManager(IViewManager viewManager, Func<IMessageBoxViewModel> messageBoxViewModelFactory)
        {
            this.viewManager = viewManager;
            this.messageBoxViewModelFactory = messageBoxViewModelFactory;
        }

        /// <summary>
        /// Given a ViewModel, show its corresponding View as a window
        /// </summary>
        /// <param name="viewModel">ViewModel to show the View for</param>
        public void ShowWindow(object viewModel)
        {
            this.CreateWindow(viewModel, false).Show();
        }

        /// <summary>
        /// Given a ViewModel, show its corresponding View as a Dialog
        /// </summary>
        /// <param name="viewModel">ViewModel to show the View for</param>
        /// <returns>DialogResult of the View</returns>
        public bool? ShowDialog(object viewModel)
        {
            return this.CreateWindow(viewModel, true).ShowDialog();
        }

        /// <summary>
        /// Display a MessageBox
        /// </summary>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="buttons">A System.Windows.MessageBoxButton value that specifies which button or buttons to display.</param>
        /// <param name="icon">A System.Windows.MessageBoxImage value that specifies the icon to display.</param>
        /// <param name="defaultResult">A System.Windows.MessageBoxResult value that specifies the default result of the message box.</param>
        /// <param name="cancelResult">A System.Windows.MessageBoxResult value that specifies the cancel result of the message box</param>
        /// <param name="options">A System.Windows.MessageBoxOptions value object that specifies the options.</param>
        /// <param name="buttonLabels">A dictionary specifying the button labels, if desirable</param>
        /// <returns>The result chosen by the user</returns>
        public MessageBoxResult ShowMessageBox(string messageBoxText, string caption = "",
            MessageBoxButton buttons = MessageBoxButton.OK,
            MessageBoxImage icon = MessageBoxImage.None,
            MessageBoxResult defaultResult = MessageBoxResult.None,
            MessageBoxResult cancelResult = MessageBoxResult.None,
            MessageBoxOptions options = MessageBoxOptions.None,
            IDictionary<MessageBoxResult, string> buttonLabels = null)
        {
            var vm = this.messageBoxViewModelFactory();
            vm.Setup(messageBoxText, caption, buttons, icon, defaultResult, cancelResult, options, buttonLabels);
            this.ShowDialog(vm);
            return vm.ClickedButton;
        }

        /// <summary>
        /// Given a ViewModel, create its View, ensure that it's a Window, and set it up
        /// </summary>
        /// <param name="viewModel">ViewModel to create the window for</param>
        /// <param name="isDialog">True if the window will be used as a dialog</param>
        /// <returns>Window which was created and set up</returns>
        protected virtual Window CreateWindow(object viewModel, bool isDialog)
        {
            var view = this.viewManager.CreateAndBindViewForModel(viewModel);
            var window = view as Window;
            if (window == null)
            {
                var e = new ArgumentException(String.Format("Tried to show {0} as a window, but it isn't a Window", view == null ? "(null)" : view.GetType().Name));
                logger.Error(e);
                throw e;
            }

            var haveDisplayName = viewModel as IHaveDisplayName;
            if (haveDisplayName != null && BindingOperations.GetBindingBase(window, Window.TitleProperty) == null)
            {
                var binding = new Binding("DisplayName") { Mode = BindingMode.TwoWay };
                window.SetBinding(Window.TitleProperty, binding);
            }

            if (isDialog)
            {
                var owner = this.InferOwnerOf(window);
                if (owner != null)
                    window.Owner = owner;
                logger.Info("Displaying ViewModel {0} with View {1} as a Dialog", viewModel, window);
            }
            else
            {
                logger.Info("Displaying ViewModel {0} with View {1} as a Window", viewModel, window);
            }

            // This gets itself retained by the window, by registering events
            new WindowConductor(window, viewModel);

            return window;
        }

        private Window InferOwnerOf(Window window)
        {
            if (Application.Current == null)
                return null;

            // We can end up in a really weird situation if they try and display more than one dialog as the application's closing
            // Basically the MainWindow's no long active, so the second dialog chooses the first dialog as its owner... But the first dialog
            // hasn't yet been shown, so we get an exception ("cannot set owner property to a Window which has not been previously shown").
            // Not sure whether checking IsLoaded is the correct solution, but...
            var active = Application.Current.Windows.OfType<Window>().Where(x => x.IsActive).FirstOrDefault() ?? Application.Current.MainWindow;
            return active == window ? null : active;
        }

        private class WindowConductor : IChildDelegate
        {
            private readonly Window window;
            private readonly object viewModel;

            public WindowConductor(Window window, object viewModel)
            {
                this.window = window;
                this.viewModel = viewModel;

                // They won't be able to request a close unless they implement IChild anyway...
                var viewModelAsChild = this.viewModel as IChild;
                if (viewModelAsChild != null)
                    viewModelAsChild.Parent = this;

                ScreenExtensions.TryActivate(this.viewModel);

                var viewModelAsClose = this.viewModel as IClose;
                if (viewModelAsClose != null)
                    window.Closed += this.WindowClosed;

                if (this.viewModel is IGuardClose)
                    window.Closing += this.WindowClosing;

                if (this.viewModel is IActivate || this.viewModel is IDeactivate)
                    window.StateChanged += this.WindowStateChanged;
            }

            private void WindowStateChanged(object sender, EventArgs e)
            {
                switch (this.window.WindowState)
                {
                    case WindowState.Maximized:
                    case WindowState.Normal:
                        logger.Info("Window {0} maximized/restored: activating", this.window);
                        ScreenExtensions.TryActivate(this.viewModel);
                        break;

                    case WindowState.Minimized:
                        logger.Info("Window {0} minimized: deactivating", this.window);
                        ScreenExtensions.TryDeactivate(this.viewModel);
                        break;
                }
            }

            private void WindowClosed(object sender, EventArgs e)
            {
                // Logging was done in the Closing handler

                this.window.StateChanged -= this.WindowStateChanged;
                this.window.Closed -= this.WindowClosed;
                this.window.Closing -= this.WindowClosing; // Not sure this is required

                ScreenExtensions.TryCloseAndDispose(this.viewModel);
            }

            private async void WindowClosing(object sender, CancelEventArgs e)
            {
                if (e.Cancel)
                    return;

                logger.Info("ViewModel {0} close requested because its View was closed", this.viewModel);

                // See if the task completed synchronously
                var task = ((IGuardClose)this.viewModel).CanCloseAsync();
                if (task.IsCompleted)
                {
                    // The closed event handler will take things from here if we don't cancel
                    if (!task.Result)
                        logger.Info("Close of ViewModel {0} cancelled because CanCloseAsync returned false", this.viewModel);
                    e.Cancel = !task.Result;
                }
                else
                {
                    e.Cancel = true;
                    logger.Info("Delaying closing of ViewModel {0} because CanCloseAsync is completing asynchronously", this.viewModel);
                    if (await task)
                    {
                        this.window.Closing -= this.WindowClosing;
                        this.window.Close();
                        // The Closed event handler handles unregistering the events, and closing the ViewModel
                    }
                    else
                    {
                        logger.Info("Close of ViewModel {0} cancelled because CanCloseAsync returned false", this.viewModel);
                    }
                }
            }

            /// <summary>
            /// Close was requested by the child
            /// </summary>
            /// <param name="item">Item to close</param>
            /// <param name="dialogResult">DialogResult to close with, if it's a dialog</param>
            async void IChildDelegate.CloseItem(object item, bool? dialogResult)
            {
                if (item != this.viewModel)
                {
                    logger.Warn("IChildDelegate.CloseItem called with item {0} which is _not_ our ViewModel {1}", item, this.viewModel);
                    return;
                }

                var guardClose = this.viewModel as IGuardClose;
                if (guardClose != null && !await guardClose.CanCloseAsync())
                {
                    logger.Info("Close of ViewModel {0} cancelled because CanCloseAsync returned false", this.viewModel);
                    return;
                }

                logger.Info("ViewModel {0} close requested with DialogResult {1} because it called TryClose", this.viewModel, dialogResult);

                this.window.StateChanged -= this.WindowStateChanged;
                this.window.Closed -= this.WindowClosed;
                this.window.Closing -= this.WindowClosing;

                // Need to call this after unregistering the event handlers, as it causes the window
                // to be closed
                if (dialogResult != null)
                    this.window.DialogResult = dialogResult;

                ScreenExtensions.TryCloseAndDispose(this.viewModel);

                this.window.Close();
            }
        }
    }
}
