using Moq;
using NUnit.Framework;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyletUnitTests
{
    [TestFixture]
    public class ScreenExtensionTests
    {
        public interface IMyScreen : IScreen, IDisposable
        { }

        private Screen parent;
        private Mock<IMyScreen> child;

        [SetUp]
        public void SetUp()
        {
            this.parent = new Screen();
            this.child = new Mock<IMyScreen>();
        }

        [Test]
        public void TryActivateActivatesIActivate()
        {
            var screen = new Mock<IActivate>();
            ScreenExtensions.TryActivate(screen.Object);
            screen.Verify(x => x.Activate());
        }

        [Test]
        public void TryActivateDoesNothingToNonIActivate()
        {
            var screen = new Mock<IDeactivate>(MockBehavior.Strict);
            ScreenExtensions.TryActivate(screen.Object);
        }

        [Test]
        public void TryDeactivateDeactivatesIDeactivate()
        {
            var screen = new Mock<IDeactivate>();
            ScreenExtensions.TryDeactivate(screen.Object);
            screen.Verify(x => x.Deactivate());
        }

        [Test]
        public void TryDeactivateDoesNothingToNonIDeactivate()
        {
            var screen = new Mock<IActivate>(MockBehavior.Strict);
            ScreenExtensions.TryDeactivate(screen.Object);
        }

        [Test]
        public void TryCloseAndDisposeClosesIClose()
        {
            var screen = new Mock<IClose>();
            ScreenExtensions.TryCloseAndDispose(screen.Object);
            screen.Verify(x => x.Close());
        }

        [Test]
        public void TryCloseAndDisposeDisposesIDisposable()
        {
            var screen = new Mock<IDisposable>();
            ScreenExtensions.TryCloseAndDispose(screen.Object);
            screen.Verify(x => x.Dispose());
        }

        [Test]
        public void TryCloseAndDisposeDoesNothingToNonIClose()
        {
            var screen = new Mock<IActivate>(MockBehavior.Strict);
            ScreenExtensions.TryCloseAndDispose(screen.Object);
        }

        [Test]
        public void ConductWithActivates()
        {
            this.child.Object.ConductWith(this.parent);
            ((IActivate)this.parent).Activate();
            this.child.Verify(x => x.Activate());
        }

        [Test]
        public void ConductWithDeactivates()
        {
            // Needs to be active....
            ((IActivate)this.parent).Activate();
            this.child.Object.ConductWith(this.parent);
            ((IDeactivate)this.parent).Deactivate();
            this.child.Verify(x => x.Deactivate());
        }

        [Test]
        public void ConductWithCloses()
        {
            this.child.Object.ConductWith(this.parent);
            ((IClose)this.parent).Close();
            this.child.Verify(x => x.Close());
            this.child.Verify(x => x.Dispose());
        }

        [Test]
        public void ConductWithDoesNotRetain()
        {
            var child = new Screen();
            child.ConductWith(this.parent);

            var weakChild = new WeakReference(child);
            child = null;
            GC.Collect();

            Assert.Null(weakChild.Target);
        }
    }
}
