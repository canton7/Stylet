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
    public class ConductorTests
    {
        private Conductor<IScreen> conductor;

        [SetUp]
        public void SetUp()
        {
            this.conductor = new Conductor<IScreen>();
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
            ((IActivate)this.conductor).Activate();
            var screen = new Mock<IScreen>();
            this.conductor.ActivateItem(screen.Object);
            screen.Verify(x => x.Activate());
        }

        [Test]
        public void ActivatesActiveItemWhenActivated()
        {
            var screen = new Mock<IScreen>();
            this.conductor.ActivateItem(screen.Object);
            screen.Verify(x => x.Activate(), Times.Never);

            ((IActivate)this.conductor).Activate();
            screen.Verify(x => x.Activate());
        }

        [Test]
        public void DeactivatesActiveItemWhenDeactivated()
        {
            ((IActivate)this.conductor).Activate();
            var screen = new Mock<IScreen>();
            this.conductor.ActivateItem(screen.Object);
            ((IDeactivate)this.conductor).Deactivate(false);
            screen.Verify(x => x.Deactivate(false));
        }

        [Test]
        public void ActivateDeactivatesPreviousItemIfConductorIsActiveAndPreviousItemCanClose()
        {
            var screen1 = new Mock<IScreen>();
            var screen2 = new Mock<IScreen>();
            ((IActivate)this.conductor).Activate();
            this.conductor.ActivateItem(screen1.Object);
            screen1.Setup(x => x.CanCloseAsync()).Returns(Task.FromResult(true));
            this.conductor.ActivateItem(screen2.Object);
            screen1.Verify(x => x.Deactivate(true));
        }
    }
}
