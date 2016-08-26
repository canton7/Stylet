using Moq;
using NUnit.Framework;
using Stylet;
using System;
using System.Threading.Tasks;

namespace StyletUnitTests
{
    [TestFixture]
    public class ConductorNavigatingTests
    {
        public interface IMyScreen : IScreen, IDisposable
        { }

        private class MyConductor : Conductor<IScreen>.StackNavigation
        {
            public bool CanCloseValue = true;
            public override async Task<bool> CanCloseAsync()
            {
                return this.CanCloseValue && await base.CanCloseAsync();
            }
        }

        private MyConductor conductor;

        [SetUp]
        public void SetUp()
        {
            this.conductor = new MyConductor();
        }

        [Test]
        public void ActiveItemIsNullBeforeAnyItemsActivated()
        {
            Assert.IsNull(this.conductor.ActiveItem);
            Assert.That(this.conductor.GetChildren(), Is.EquivalentTo(new IScreen[] { null }));
        }

        [Test]
        public void InitialActivateSetsItemAsActiveItem()
        {
            var screen = new Mock<IScreen>();
            this.conductor.ActivateItem(screen.Object);
            Assert.AreEqual(screen.Object, this.conductor.ActiveItem);
        }

        [Test]
        public void InitialActivateDoesNotActivateItemIfConductorIsNotActive()
        {
            var screen = new Mock<IScreen>();
            this.conductor.ActivateItem(screen.Object);
            screen.Verify(x => x.Activate(), Times.Never);
        }

        [Test]
        public void InitialActivateActivatesItemIfConductorIsActive()
        {
            ((IScreenState)this.conductor).Activate();
            var screen = new Mock<IScreen>();
            this.conductor.ActivateItem(screen.Object);
            screen.Verify(x => x.Activate());
        }

        [Test]
        public void InitialActivateDeactivatesItemIfConductorIsDeactivated()
        {
            var screen = new Mock<IScreen>();
            this.conductor.ActivateItem(screen.Object);
            screen.Verify(x => x.Deactivate());
        }

        [Test]
        public void ActivatesActiveItemWhenActivated()
        {
            var screen = new Mock<IScreen>();
            this.conductor.ActivateItem(screen.Object);
            screen.Verify(x => x.Activate(), Times.Never);

            ((IScreenState)this.conductor).Activate();
            screen.Verify(x => x.Activate());
        }

        [Test]
        public void DeactivatesActiveItemWhenDeactivated()
        {
            ((IScreenState)this.conductor).Activate();
            var screen = new Mock<IScreen>();
            this.conductor.ActivateItem(screen.Object);
            ((IScreenState)this.conductor).Deactivate();
            screen.Verify(x => x.Deactivate());
        }

        [Test]
        public void ActivateDeactivatesPreviousItemIfConductorIsActiveAndPreviousItemCanClose()
        {
            var screen1 = new Mock<IScreen>();
            var screen2 = new Mock<IScreen>();
            ((IScreenState)this.conductor).Activate();
            this.conductor.ActivateItem(screen1.Object);
            screen1.Setup(x => x.CanCloseAsync()).Returns(Task.FromResult(true));
            this.conductor.ActivateItem(screen2.Object);
            screen1.Verify(x => x.Deactivate());
        }

        [Test]
        public void ActivatingCurrentScreenReactivatesScreen()
        {
            var screen = new Mock<IMyScreen>();
            ((IScreenState)this.conductor).Activate();
            this.conductor.ActivateItem(screen.Object);
            this.conductor.ActivateItem(screen.Object);
            screen.Verify(x => x.Activate(), Times.Exactly(2));
            screen.Verify(x => x.Close(), Times.Never);
            screen.Verify(x => x.Dispose(), Times.Never);
        }

        [Test]
        public void CloseItemDoesNothingIfToldToDeactiveInactiveItem()
        {
            var screen1 = new Mock<IMyScreen>();
            var screen2 = new Mock<IMyScreen>();
            ((IScreenState)this.conductor).Activate();
            this.conductor.ActivateItem(screen1.Object);
            this.conductor.CloseItem(screen2.Object);

            screen1.Verify(x => x.Close(), Times.Never);
            screen1.Verify(x => x.Dispose(), Times.Never);
            screen2.Verify(x => x.Activate(), Times.Never);
        }

        [Test]
        public void DeactiveDoesNotChangeActiveItem()
        {
            var screen = new Mock<IScreen>();
            ((IScreenState)this.conductor).Activate();
            this.conductor.ActivateItem(screen.Object);
            this.conductor.DeactivateItem(screen.Object);

            screen.Verify(x => x.Deactivate());
            Assert.AreEqual(this.conductor.ActiveItem, screen.Object);
        }

        [Test]
        public void ActivateSetsConductorAsItemsParent()
        {
            var screen = new Mock<IScreen>();
            this.conductor.ActivateItem(screen.Object);
            screen.VerifySet(x => x.Parent = this.conductor);
        }
        
        [Test]
        public void SettingActiveItemActivatesItem()
        {
            var screen = new Mock<IScreen>();
            ((IScreenState)this.conductor).Activate();
            this.conductor.ActiveItem =screen.Object;
            screen.Verify(x => x.Activate());
            Assert.AreEqual(this.conductor.ActiveItem, screen.Object);
        }

        [Test]
        public void CloseClosesAndRemovesItemsParent()
        {
            var screen = new Mock<IScreen>();
            screen.Setup(x => x.CanCloseAsync()).Returns(Task.FromResult(true));
            screen.Setup(x => x.Parent).Returns(this.conductor);
            this.conductor.ActivateItem(screen.Object);
            this.conductor.CloseItem(screen.Object);
            screen.Verify(x => x.Close());
            screen.VerifySet(x => x.Parent = null);
        }

        [Test]
        public void CloseDisposesItemIfDisposeChildrenIsTrue()
        {
            var screen = new Mock<IMyScreen>();
            screen.Setup(x => x.CanCloseAsync()).Returns(Task.FromResult(true));
            this.conductor.ActivateItem(screen.Object);
            this.conductor.CloseItem(screen.Object);
            screen.Verify(x => x.Dispose());
        }

        [Test]
        public void CloseDoesNotDisposeItemIfDisposeChildrenIsFalse()
        {
            var screen = new Mock<IMyScreen>();
            screen.Setup(x => x.CanCloseAsync()).Returns(Task.FromResult(true));
            this.conductor.DisposeChildren = false;
            this.conductor.ActivateItem(screen.Object);
            this.conductor.CloseItem(screen.Object);
            screen.Verify(x => x.Dispose(), Times.Never);
        }
        
        [Test]
        public void CanCloseReturnsTrueIfNoActiveItem()
        {
            Assert.IsTrue(this.conductor.CanCloseAsync().Result);
        }

        [Test]
        public void CanCloseReturnsAllItemsCanClose()
        {
            var screen1 = new Mock<IScreen>();
            var screen2 = new Mock<IScreen>();
            this.conductor.ActivateItem(screen1.Object);
            this.conductor.ActivateItem(screen2.Object);
            screen1.Setup(x => x.CanCloseAsync()).Returns(Task.FromResult(true));
            screen2.Setup(x => x.CanCloseAsync()).Returns(Task.FromResult(false));
            Assert.IsFalse(this.conductor.CanCloseAsync().Result);
        }

        [Test]
        public void ConductorCanCloseAsyncCallsCanCloseOnSelfBeforeChildren()
        {
            var screen = new Mock<IScreen>();

            this.conductor.ActivateItem(screen.Object);
            this.conductor.CanCloseValue = false;

            Assert.IsFalse(this.conductor.CanCloseAsync().Result);

            screen.Verify(x => x.CanCloseAsync(), Times.Never());
        }

        [Test]
        public void DeactivatingActiveItemGoesBack()
        {
            ((IScreenState)this.conductor).Activate();
            var screen1 = new Mock<IScreen>();
            var screen2 = new Mock<IScreen>();

            this.conductor.ActivateItem(screen1.Object);
            screen1.Verify(x => x.Activate());

            this.conductor.ActivateItem(screen2.Object);
            screen2.Verify(x => x.Activate());

            this.conductor.DeactivateItem(screen2.Object);
            screen2.Verify(x => x.Deactivate(), Times.Once);
            screen1.Verify(x => x.Activate(), Times.Once);
        }

        [Test]
        public void ClearClosesAllItemsExceptCurrent()
        {
            ((IScreenState)this.conductor).Activate();
            var screen1 = new Mock<IScreen>();
            var screen2 = new Mock<IScreen>();
            this.conductor.ActivateItem(screen1.Object);
            this.conductor.ActivateItem(screen2.Object);
            this.conductor.Clear();

            Assert.AreEqual(screen2.Object, this.conductor.ActiveItem);

            screen2.Setup(x => x.CanCloseAsync()).Returns(Task.FromResult(true));
            this.conductor.GoBack();
            Assert.IsNull(this.conductor.ActiveItem);
        }

        [Test]
        public void CloseClosesAllItems()
        {
            var screen1 = new Mock<IMyScreen>();
            screen1.SetupGet(x => x.Parent).Returns(this.conductor);
            var screen2 = new Mock<IMyScreen>();
            screen2.SetupGet(x => x.Parent).Returns(this.conductor);
            this.conductor.ActivateItem(screen1.Object);
            this.conductor.ActivateItem(screen2.Object);

            ((IScreenState)this.conductor).Close();
            screen1.Verify(x => x.Close());
            screen1.VerifySet(x => x.Parent = null);
            screen2.Verify(x => x.Close());
            screen2.VerifySet(x => x.Parent = null);
        }

        [Test]
        public void ClosesItemIfItemRequestsClose()
        {
            var screen = new Mock<IMyScreen>();
            this.conductor.ActivateItem(screen.Object);
            screen.Setup(x => x.CanCloseAsync()).Returns(Task.FromResult(true));
            ((IChildDelegate)this.conductor).CloseItem(screen.Object);

            screen.Verify(x => x.Close());
            Assert.Null(this.conductor.ActiveItem);
        }
    }
}
