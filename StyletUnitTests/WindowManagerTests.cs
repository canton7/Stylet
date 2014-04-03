using Moq;
using NUnit.Framework;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StyletUnitTests
{
    [TestFixture, RequiresSTA]
    public class WindowManagerTests
    {
        private Mock<IViewManager> viewManager;
        private WindowManager windowManager;

        [SetUp]
        public void SetUp()
        {
            this.viewManager = new Mock<IViewManager>();
            this.windowManager = new WindowManager();

            IoC.GetInstance = (service, key) => this.viewManager.Object;
        }

        [Test]
        public void ShowWindowAsksViewManagerForView()
        {
            var model = new object();
            this.viewManager.Setup(x => x.LocateViewForModel(model)).Verifiable();
            // Don't care if this throws - that's OK
            try { this.windowManager.ShowWindow(model);  }
            catch (Exception) { }
            this.viewManager.VerifyAll();
        }

        [Test]
        public void ShowWindowThrowsIfViewIsntAWindow()
        {
            var model = new object();
            this.viewManager.Setup(x => x.LocateViewForModel(model)).Returns(new UIElement());
            Assert.Throws<Exception>(() => this.windowManager.ShowWindow(model));
        }
    }
}
