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
        public void TryActivateActivatesIScreenState()
        {
            var screen = new Mock<IScreenState>();
            ScreenExtensions.TryActivate(screen.Object);
            screen.Verify(x => x.Activate());
        }

        [Test]
        public void TryActivateDoesNothingToNonIScreenState()
        {
            var screen = new Mock<IGuardClose>(MockBehavior.Strict);
            ScreenExtensions.TryActivate(screen.Object);
        }

        [Test]
        public void TryDeactivateDeactivatesIScreenState()
        {
            var screen = new Mock<IScreenState>();
            ScreenExtensions.TryDeactivate(screen.Object);
            screen.Verify(x => x.Deactivate());
        }

        [Test]
        public void TryDeactivateDoesNothingToNonIScreenState()
        {
            var screen = new Mock<IGuardClose>(MockBehavior.Strict);
            ScreenExtensions.TryDeactivate(screen.Object);
        }

        [Test]
        public void TryCloseClosesIScreenState()
        {
            var screen = new Mock<IScreenState>();
            ScreenExtensions.TryClose(screen.Object);
            screen.Verify(x => x.Close());
        }

        [Test]
        public void TryDisposeDisposesIDisposable()
        {
            var screen = new Mock<IDisposable>();
            ScreenExtensions.TryDispose(screen.Object);
            screen.Verify(x => x.Dispose());
        }

        [Test]
        public void TryCloseDoesNothingToNonIScreenState()
        {
            var screen = new Mock<IGuardClose>(MockBehavior.Strict);
            ScreenExtensions.TryClose(screen.Object);
        }

        [Test]
        public void ConductWithActivates()
        {
            this.child.Object.ConductWith(this.parent);
            ((IScreenState)this.parent).Activate();
            this.child.Verify(x => x.Activate());
        }

        [Test]
        public void ConductWithDeactivates()
        {
            // Needs to be active....
            ((IScreenState)this.parent).Activate();
            this.child.Object.ConductWith(this.parent);
            ((IScreenState)this.parent).Deactivate();
            this.child.Verify(x => x.Deactivate());
        }

        [Test]
        public void ConductWithCloses()
        {
            this.child.Object.ConductWith(this.parent);
            ((IScreenState)this.parent).Close();
            this.child.Verify(x => x.Close());
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
