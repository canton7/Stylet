using Moq;
using NUnit.Framework;
using Stylet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace StyletUnitTests
{
    [TestFixture, RequiresSTA]
    public class WindowManagerTests
    {
        private class MyWindowManager : WindowManager
        {
            public MyWindowManager(IViewManager viewManager) : base(viewManager) { }

            public new Window CreateWindow(object viewModel, bool isDialog)
            {
                return base.CreateWindow(viewModel, isDialog);
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
        private MyWindowManager windowManager;

        [SetUp]
        public void SetUp()
        {
            this.viewManager = new Mock<IViewManager>();
            this.windowManager = new MyWindowManager(this.viewManager.Object);

            IoC.GetInstance = (service, key) => this.viewManager.Object;
        }

        [Test]
        public void CreateWindowAsksViewManagerForView()
        {
            var model = new object();
            this.viewManager.Setup(x => x.CreateViewForModel(model)).Verifiable();
            // Don't care if this throws - that's OK
            try { this.windowManager.CreateWindow(model, false);  }
            catch (Exception) { }
            this.viewManager.VerifyAll();
        }

        [Test]
        public void CreateWindowThrowsIfViewIsntAWindow()
        {
            var model = new object();
            this.viewManager.Setup(x => x.CreateViewForModel(model)).Returns(new UIElement());
            Assert.Throws<Exception>(() => this.windowManager.CreateWindow(model, false));
        }

        [Test]
        public void CreateWindowBindsViewToModel()
        {
            var model = new object();
            var window = new Window();
            this.viewManager.Setup(x => x.CreateViewForModel(model)).Returns(window);

            this.windowManager.CreateWindow(model, false);

            this.viewManager.Verify(x => x.BindViewToModel(window, model));
        }

        [Test]
        public void CreateWindowSetsUpTitleBindingIfViewModelIsIHaveDisplayName()
        {
            var model = new Screen();
            var window = new Window();
            this.viewManager.Setup(x => x.CreateViewForModel(model)).Returns(window);
            
            this.windowManager.CreateWindow(model, false);

            var e = window.GetBindingExpression(Window.TitleProperty);
            Assert.AreEqual(BindingMode.TwoWay, e.ParentBinding.Mode);
            Assert.AreEqual("DisplayName", e.ParentBinding.Path.Path);
        }

        [Test]
        public void CreateWindowActivatesViewModel()
        {
            var model = new Mock<IScreen>();
            this.viewManager.Setup(x => x.CreateViewForModel(model.Object)).Returns(new Window());
            this.windowManager.CreateWindow(model.Object, false);
            model.Verify(x => x.Activate());
        }

        [Test]
        public void WindowStateChangedActivatesIfMaximized()
        {
            var model = new Mock<IScreen>();
            var window = new MyWindow();
            this.viewManager.Setup(x => x.CreateViewForModel(model.Object)).Returns(window);
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
            this.viewManager.Setup(x => x.CreateViewForModel(model.Object)).Returns(window);
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
            this.viewManager.Setup(x => x.CreateViewForModel(model.Object)).Returns(window);
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
            this.viewManager.Setup(x => x.CreateViewForModel(model)).Returns(window);
            this.windowManager.CreateWindow(model, false);
            window.OnClosing(new CancelEventArgs(true));
        }

        [Test]
        public void WindowClosingCancelsIfCanCloseAsyncReturnsSynchronousFalse()
        {
            var model = new Mock<IScreen>();
            var window = new MyWindow();
            this.viewManager.Setup(x => x.CreateViewForModel(model.Object)).Returns(window);
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
            this.viewManager.Setup(x => x.CreateViewForModel(model.Object)).Returns(window);
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
            this.viewManager.Setup(x => x.CreateViewForModel(model.Object)).Returns(window);
            this.windowManager.CreateWindow(model.Object, false);
            model.Setup(x => x.CanCloseAsync()).Returns(Task.Delay(1).ContinueWith(t => false));
            var ea = new CancelEventArgs();
            window.OnClosing(ea);
            Assert.True(ea.Cancel);
        }

        [Test]
        public void WindowClosingCancelsIfCanCloseAsyncReturnsAsynchronousTrue()
        {
            var model = new Mock<IScreen>();
            var window = new MyWindow();
            this.viewManager.Setup(x => x.CreateViewForModel(model.Object)).Returns(window);
            this.windowManager.CreateWindow(model.Object, false);
            model.Setup(x => x.CanCloseAsync()).Returns(Task.Delay(1).ContinueWith(t => true));
            var ea = new CancelEventArgs();
            window.OnClosing(ea);
            Assert.True(ea.Cancel);
        }

        [Test]
        public void WindowClosingClosesWindowIfCanCloseAsyncCompletesTrue()
        {
            var model = new Mock<IScreen>();
            var window = new MyWindow();
            this.viewManager.Setup(x => x.CreateViewForModel(model.Object)).Returns(window);
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
            this.viewManager.Setup(x => x.CreateViewForModel(model)).Returns(window);
            this.windowManager.CreateWindow(model, false);
            ((IChildDelegate)model.Parent).CloseItem(new object());
        }

        [Test]
        public void CloseItemDoesNothingIfCanCloseReturnsFalse()
        {
            var model = new Mock<IScreen>();
            var window = new MyWindow();
            this.viewManager.Setup(x => x.CreateViewForModel(model.Object)).Returns(window);
            object parent = null;
            model.SetupSet(x => x.Parent = It.IsAny<object>()).Callback((object x) => parent = x);
            this.windowManager.CreateWindow(model.Object, false);

            model.Setup(x => x.CanCloseAsync()).Returns(Task.Delay(1).ContinueWith(t => false));
            ((IChildDelegate)parent).CloseItem(model.Object);
        }

        [Test]
        public void CloseItemClosesAndWindowViewModelIfCanCloseReturnsTrue()
        {
            var model = new Mock<IScreen>();
            var window = new MyWindow();
            this.viewManager.Setup(x => x.CreateViewForModel(model.Object)).Returns(window);
            object parent = null;
            model.SetupSet(x => x.Parent = It.IsAny<object>()).Callback((object x) => parent = x);
            this.windowManager.CreateWindow(model.Object, true);

            model.Setup(x => x.CanCloseAsync()).ReturnsAsync(true);
            ((IChildDelegate)parent).CloseItem(model.Object);

            model.Verify(x => x.Close());
            Assert.True(window.OnClosedCalled);
        }
    }
}
