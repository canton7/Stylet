using Moq;
using NUnit.Framework;
using Stylet;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace StyletUnitTests
{
    [TestFixture, RequiresSTA]
    public class WindowManagerTests
    {
        public interface IMyScreen : IScreen, IDisposable
        { }

        private class TestException : Exception { }

        private class MyWindowManager : WindowManager
        {
            public MyWindowManager(IViewManager viewManager, Func<IMessageBoxViewModel> messageBoxViewModelFactory, IWindowManagerConfig config)
                : base(viewManager, messageBoxViewModelFactory, config) { }

            public Window CreateWindow(object viewModel, bool isDialog)
            {
                return base.CreateWindow(viewModel, isDialog, null);
            }
        }

        private class WindowManagerWithoutCreateWindow : WindowManager
        {
            public WindowManagerWithoutCreateWindow(IViewManager viewManager, Func<IMessageBoxViewModel> messageBoxViewModelFactory, IWindowManagerConfig config)
                : base(viewManager, messageBoxViewModelFactory, config) { }

            protected override Window CreateWindow(object viewModel, bool isDialog, IViewAware ownerViewModel)
            {
                throw new TestException(); // ABORT! ABORT!
            }
        }

        private class MyWindow : Window
        {
            public new void OnClosing(CancelEventArgs e)
            {
                base.OnClosing(e);
            }

            public bool OnClosedCalled;
            protected override void OnClosed(EventArgs e)
            {
                base.OnClosed(e);
                this.OnClosedCalled = true;
            }

            public new void OnStateChanged(EventArgs e)
            {
                base.OnStateChanged(e);
            }
        }

        private Mock<IViewManager> viewManager;
        private Mock<IMessageBoxViewModel> messageBoxViewModel;
        private Mock<IWindowManagerConfig> config;
        private MyWindowManager windowManager;

        [SetUp]
        public void SetUp()
        {
            this.viewManager = new Mock<IViewManager>();
            this.messageBoxViewModel = new Mock<IMessageBoxViewModel>();
            this.config = new Mock<IWindowManagerConfig>();
            this.windowManager = new MyWindowManager(this.viewManager.Object, () => this.messageBoxViewModel.Object, this.config.Object);
        }

        [Test]
        public void CreateWindowAsksViewManagerForView()
        {
            var model = new object();
            this.viewManager.Setup(x => x.CreateAndBindViewForModelIfNecessary(model)).Verifiable();
            // Don't care if this throws - that's OK
            try { this.windowManager.CreateWindow(model, false);  }
            catch (Exception) { }
            this.viewManager.VerifyAll();
        }

        [Test]
        public void CreateWindowThrowsIfViewIsntAWindow()
        {
            var model = new object();
            this.viewManager.Setup(x => x.CreateAndBindViewForModelIfNecessary(model)).Returns(new UIElement());
            Assert.Throws<StyletInvalidViewTypeException>(() => this.windowManager.CreateWindow(model, false));
        }

        [Test]
        public void CreateWindowSetsUpTitleBindingIfViewModelIsIHaveDisplayName()
        {
            var model = new Screen();
            var window = new Window();
            this.viewManager.Setup(x => x.CreateAndBindViewForModelIfNecessary(model)).Returns(window);
            
            this.windowManager.CreateWindow(model, false);

            var e = window.GetBindingExpression(Window.TitleProperty);
            Assert.NotNull(e);
            Assert.AreEqual(BindingMode.TwoWay, e.ParentBinding.Mode);
            Assert.AreEqual("DisplayName", e.ParentBinding.Path.Path);
        }

        [Test]
        public void CreateWindowDoesNotSetUpTitleBindingIfTitleHasAValueAlready()
        {
            var model = new Screen();
            var window = new Window();
            window.Title = "Foo";
            this.viewManager.Setup(x => x.CreateAndBindViewForModelIfNecessary(model)).Returns(window);

            this.windowManager.CreateWindow(model, false);

            var e = window.GetBindingExpression(Window.TitleProperty);
            Assert.IsNull(e);
            Assert.AreEqual("Foo", window.Title);
        }

        [Test]
        public void CreateWindowDoesNotSetUpTitleBindingIfTitleHasABindingAlready()
        {
            var model = new Screen();
            var window = new Window();
            var binding = new Binding("Test") { Mode = BindingMode.TwoWay };
            window.SetBinding(Window.TitleProperty, binding);
            this.viewManager.Setup(x => x.CreateAndBindViewForModelIfNecessary(model)).Returns(window);

            this.windowManager.CreateWindow(model, false);

            var e = window.GetBindingExpression(Window.TitleProperty);
            Assert.AreEqual("Test", e.ParentBinding.Path.Path);
        }

        [Test]
        public void CreateWindowDoesSetUpTitleBindingIfTitleIsNameOfTheClass()
        {
            var model = new Screen();
            var window = new Window();
            window.Title = "Window";
            this.viewManager.Setup(x => x.CreateAndBindViewForModelIfNecessary(model)).Returns(window);

            this.windowManager.CreateWindow(model, false);

            var e = window.GetBindingExpression(Window.TitleProperty);
            Assert.AreEqual("DisplayName", e.ParentBinding.Path.Path);
        }

        [Test]
        public void CreateWindowActivatesViewModel()
        {
            var model = new Mock<IScreen>();
            this.viewManager.Setup(x => x.CreateAndBindViewForModelIfNecessary(model.Object)).Returns(new Window());
            this.windowManager.CreateWindow(model.Object, false);
            model.Verify(x => x.Activate());
        }

        [Test]
        public void WindowStateChangedActivatesIfMaximized()
        {
            var model = new Mock<IScreen>();
            var window = new MyWindow();
            this.viewManager.Setup(x => x.CreateAndBindViewForModelIfNecessary(model.Object)).Returns(window);
            this.windowManager.CreateWindow(model.Object, false);
            window.WindowState = WindowState.Maximized;
            window.OnStateChanged(EventArgs.Empty);
            model.Verify(x => x.Activate());
        }

        [Test]
        public void WindowStateChangedActivatesIfNormal()
        {
            var model = new Mock<IScreen>();
            var window = new MyWindow();
            this.viewManager.Setup(x => x.CreateAndBindViewForModelIfNecessary(model.Object)).Returns(window);
            this.windowManager.CreateWindow(model.Object, false);
            window.WindowState = WindowState.Normal;
            window.OnStateChanged(EventArgs.Empty);
            model.Verify(x => x.Activate());
        }

        [Test]
        public void WindowStateChangedDeactivatesIfMinimized()
        {
            var model = new Mock<IScreen>();
            var window = new MyWindow();
            this.viewManager.Setup(x => x.CreateAndBindViewForModelIfNecessary(model.Object)).Returns(window);
            this.windowManager.CreateWindow(model.Object, false);
            window.WindowState = WindowState.Minimized;
            window.OnStateChanged(EventArgs.Empty);
            model.Verify(x => x.Deactivate());
        }

        [Test]
        public void WindowClosingDoesNothingIfAlreadyCancelled()
        {
            var model = new Screen();
            var window = new MyWindow();
            this.viewManager.Setup(x => x.CreateAndBindViewForModelIfNecessary(model)).Returns(window);
            this.windowManager.CreateWindow(model, false);
            window.OnClosing(new CancelEventArgs(true));
        }

        [Test]
        public void WindowClosingCancelsIfCanCloseAsyncReturnsSynchronousFalse()
        {
            var model = new Mock<IScreen>();
            var window = new MyWindow();
            this.viewManager.Setup(x => x.CreateAndBindViewForModelIfNecessary(model.Object)).Returns(window);
            this.windowManager.CreateWindow(model.Object, false);
            model.Setup(x => x.CanCloseAsync()).Returns(Task.FromResult(false));
            var ea = new CancelEventArgs();
            window.OnClosing(ea);
            Assert.True(ea.Cancel);
        }

        [Test]
        public void WindowClosingDoesNotCancelIfCanCloseAsyncReturnsSynchronousTrue()
        {
            var model = new Mock<IScreen>();
            var window = new MyWindow();
            this.viewManager.Setup(x => x.CreateAndBindViewForModelIfNecessary(model.Object)).Returns(window);
            this.windowManager.CreateWindow(model.Object, false);
            model.Setup(x => x.CanCloseAsync()).Returns(Task.FromResult(true));
            var ea = new CancelEventArgs();
            window.OnClosing(ea);
            Assert.False(ea.Cancel);
        }

        [Test]
        public void WindowClosingCancelsIfCanCloseAsyncReturnsAsynchronousFalse()
        {
            var model = new Mock<IScreen>();
            var window = new MyWindow();
            this.viewManager.Setup(x => x.CreateAndBindViewForModelIfNecessary(model.Object)).Returns(window);
            this.windowManager.CreateWindow(model.Object, false);
            model.Setup(x => x.CanCloseAsync()).Returns(Task.Delay(10).ContinueWith(t => false));
            var ea = new CancelEventArgs();
            window.OnClosing(ea);
            Assert.True(ea.Cancel);
        }

        [Test]
        public void WindowClosingCancelsIfCanCloseAsyncReturnsAsynchronous()
        {
            var model = new Mock<IScreen>();
            var window = new MyWindow();
            this.viewManager.Setup(x => x.CreateAndBindViewForModelIfNecessary(model.Object)).Returns(window);
            this.windowManager.CreateWindow(model.Object, false);
            var tcs = new TaskCompletionSource<bool>();
            model.Setup(x => x.CanCloseAsync()).Returns(tcs.Task);
            var ea = new CancelEventArgs();
            window.OnClosing(ea);
            Assert.True(ea.Cancel);
        }

        [Test]
        public void WindowClosingClosesWindowIfCanCloseAsyncCompletesTrue()
        {
            var model = new Mock<IMyScreen>();
            var window = new MyWindow();
            this.viewManager.Setup(x => x.CreateAndBindViewForModelIfNecessary(model.Object)).Returns(window);
            this.windowManager.CreateWindow(model.Object, false);
            var tcs = new TaskCompletionSource<bool>();
            model.Setup(x => x.CanCloseAsync()).Returns(tcs.Task);
            window.OnClosing(new CancelEventArgs());
            model.Verify(x => x.Close(), Times.Never);
            tcs.SetResult(true);
            model.Verify(x => x.Close(), Times.Once);

            Assert.True(window.OnClosedCalled);

            // Check it didn't call WindowClosing again - just the first time
            model.Verify(x => x.CanCloseAsync(), Times.Once);
        }

        [Test]
        public void CloseItemDoesNothingIfItemIsWrong()
        {
            var model = new Screen();
            var window = new MyWindow();
            this.viewManager.Setup(x => x.CreateAndBindViewForModelIfNecessary(model)).Returns(window);
            this.windowManager.CreateWindow(model, false);
            ((IChildDelegate)model.Parent).CloseItem(new object());
        }

        [Test]
        public void CloseItemDoesNothingIfCanCloseReturnsFalse()
        {
            var model = new Mock<IScreen>();
            var window = new MyWindow();
            this.viewManager.Setup(x => x.CreateAndBindViewForModelIfNecessary(model.Object)).Returns(window);
            object parent = null;
            model.SetupSet(x => x.Parent = It.IsAny<object>()).Callback((object x) => parent = x);
            this.windowManager.CreateWindow(model.Object, false);

            model.Setup(x => x.CanCloseAsync()).Returns(Task.Delay(10).ContinueWith(t => false));
            ((IChildDelegate)parent).CloseItem(model.Object);
        }

        [Test]
        public void CloseItemClosesAndWindowViewModelIfCanCloseReturnsTrue()
        {
            var model = new Mock<IMyScreen>();
            var window = new MyWindow();
            this.viewManager.Setup(x => x.CreateAndBindViewForModelIfNecessary(model.Object)).Returns(window);
            object parent = null;
            model.SetupSet(x => x.Parent = It.IsAny<object>()).Callback((object x) => parent = x);
            this.windowManager.CreateWindow(model.Object, true);

            model.Setup(x => x.CanCloseAsync()).ReturnsAsync(true);
            ((IChildDelegate)parent).CloseItem(model.Object);

            model.Verify(x => x.Close());
            Assert.True(window.OnClosedCalled);
        }

        [Test]
        public void ShowMessageBoxShowsMessageBox()
        {
            var wm = new WindowManagerWithoutCreateWindow(this.viewManager.Object, () => this.messageBoxViewModel.Object, this.config.Object);

            try { wm.ShowMessageBox("text", "title", MessageBoxButton.OKCancel, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxResult.Cancel, null, FlowDirection.RightToLeft, TextAlignment.Right); }
            catch (TestException) { }

            this.messageBoxViewModel.Verify(x => x.Setup("text", "title", MessageBoxButton.OKCancel, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxResult.Cancel, null, FlowDirection.RightToLeft, TextAlignment.Right));
        }

        [Test]
        public void CreateWindowSetsWindowStartupLocationToCenterScreenIfThereIsNoOwnerAndItHasNotBeenSetAlready()
        {
            var model = new object();
            var window = new Window();
            this.viewManager.Setup(x => x.CreateAndBindViewForModelIfNecessary(model)).Returns(window);

            this.windowManager.CreateWindow(model, false);

            Assert.AreEqual(WindowStartupLocation.CenterScreen, window.WindowStartupLocation);
        }

        [Test]
        public void CreateWindowDoesNotSetStartupLocationIfItIsNotManual()
        {
            var model = new object();
            var window = new Window();
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.viewManager.Setup(x => x.CreateAndBindViewForModelIfNecessary(model)).Returns(window);

            this.windowManager.CreateWindow(model, false);

            Assert.AreEqual(WindowStartupLocation.CenterOwner, window.WindowStartupLocation);
        }

        [Test]
        public void CreateWindowDoesNotSetStartupLocationIfLeftSet()
        {
            var model = new object();
            var window = new Window();
            window.Left = 1;
            this.viewManager.Setup(x => x.CreateAndBindViewForModelIfNecessary(model)).Returns(window);

            this.windowManager.CreateWindow(model, false);

            Assert.AreEqual(WindowStartupLocation.Manual, window.WindowStartupLocation);
        }

        [Test]
        public void CreateWindowDoesNotSetStartupLocationIfTopSet()
        {
            var model = new object();
            var window = new Window();
            window.Top = 1;
            this.viewManager.Setup(x => x.CreateAndBindViewForModelIfNecessary(model)).Returns(window);

            this.windowManager.CreateWindow(model, false);

            Assert.AreEqual(WindowStartupLocation.Manual, window.WindowStartupLocation);
        }

        [Test]
        public void CreateWindowSetsOwnerIfAvailable()
        {
            // We can't actually set the window successfully, since it'll throw an InvalidOperationException
            // ("can't set owner to a window which has not been shown previously")

            var model = new object();
            var window = new Window();
            var activeWindow = new Window();

            this.viewManager.Setup(x => x.CreateAndBindViewForModelIfNecessary(model)).Returns(window);
            this.config.Setup(x => x.GetActiveWindow()).Returns(activeWindow).Verifiable();

            this.windowManager.CreateWindow(model, true);

            this.config.VerifyAll();
        }
    }
}
