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
    public class ConductorAllActiveTests
    {
        private Conductor<IScreen>.Collections.AllActive conductor;

        [SetUp]
        public void SetUp()
        {
            this.conductor = new Conductor<IScreen>.Collections.AllActive();
        }

        [Test]
        public void NoActiveItemsBeforeAnyItemsActivated()
        {
            Assert.IsEmpty(this.conductor.Items);
        }

        [Test]
        public void ActivateItemAddsItemToItems()
        {
            var screen = new Mock<IScreen>();
            this.conductor.ActivateItem(screen.Object);
            Assert.That(this.conductor.Items, Is.EquivalentTo(new[] { screen.Object }));
        }

        [Test]
        public void ActivateItemDoesNotActivateIfConductorIsNotActive()
        {
            var screen = new Mock<IScreen>();
            this.conductor.ActivateItem(screen.Object);
            screen.Verify(x => x.Activate(), Times.Never);
        }

        [Test]
        public void ActivateItemDoesActiveIfConductorIsActive()
        {
            var screen = new Mock<IScreen>();
            ((IActivate)this.conductor).Activate();
            this.conductor.ActivateItem(screen.Object);
            screen.Verify(x => x.Activate());
        }

        [Test]
        public void DeactiveDeactivatesItems()
        {
            var screen = new Mock<IScreen>();
            ((IActivate)this.conductor).Activate();
            this.conductor.ActivateItem(screen.Object);
            ((IDeactivate)this.conductor).Deactivate(false);
            screen.Verify(x => x.Deactivate(false));
        }

        [Test]
        public void ClosingClosesAllItems()
        {
            var screen = new Mock<IScreen>();
            ((IActivate)this.conductor).Activate();
            this.conductor.ActivateItem(screen.Object);
            ((IDeactivate)this.conductor).Deactivate(true);
            screen.Verify(x => x.Deactivate(true));
            Assert.AreEqual(0, this.conductor.Items.Count);
        }

        [Test]
        public void ConductorCanCloseIfAllItemsCanClose()
        {
            var screen1 = new Mock<IScreen>();
            var screen2 = new Mock<IScreen>();

            screen1.Setup(x => x.CanCloseAsync()).Returns(Task.FromResult(true));
            screen2.Setup(x => x.CanCloseAsync()).Returns(Task.FromResult(true));

            this.conductor.Items.AddRange(new[] { screen1.Object, screen2.Object });
            Assert.IsTrue(this.conductor.CanCloseAsync().Result);
        }

        [Test]
        public void ConductorCanNotCloseIfAnyItemCanNotClose()
        {
            var screen1 = new Mock<IScreen>();
            var screen2 = new Mock<IScreen>();

            screen1.Setup(x => x.CanCloseAsync()).Returns(Task.FromResult(true));
            screen2.Setup(x => x.CanCloseAsync()).Returns(Task.FromResult(false));

            this.conductor.Items.AddRange(new[] { screen1.Object, screen2.Object });
            Assert.IsFalse(this.conductor.CanCloseAsync().Result);
        }

        [Test]
        public void AddingItemSetsParent()
        {
            var screen = new Mock<IScreen>();
            this.conductor.Items.Add(screen.Object);
            screen.VerifySet(x => x.Parent = this.conductor);
        }

        [Test]
        public void RemovingItemRemovesParent()
        {
            var screen = new Mock<IScreen>();
            this.conductor.Items.Add(screen.Object);
            this.conductor.Items.Remove(screen.Object);
            screen.VerifySet(x => x.Parent = null);
        }

        [Test]
        public void AddingItemTwiceDoesNotResultInDuplicates()
        {
            var screen = new Mock<IScreen>();
            this.conductor.ActivateItem(screen.Object);
            this.conductor.ActivateItem(screen.Object);
            Assert.AreEqual(1, this.conductor.Items.Count);
        }

        [Test]
        public void DeactivateItemDeactivatesItemButDoesNotRemoveFromItems()
        {
            var screen = new Mock<IScreen>();
            this.conductor.ActivateItem(screen.Object);
            this.conductor.DeactivateItem(screen.Object, false);
            screen.Verify(x => x.Deactivate(false));
            Assert.That(this.conductor.Items, Is.EquivalentTo(new[] { screen.Object }))
        }

        [Test]
        public void CloseItemDoesNothingIfItemCanNotClose()
        {
            var screen = new Mock<IScreen>();
            this.conductor.ActivateItem(screen.Object);
            this.conductor.DeactivateItem(screen.Object, true);
            screen.Verify(x => x.Deactivate(true), Times.Never);
            Assert.That(this.conductor.Items, Is.EquivalentTo(new[] { screen.Object }));
        }

        [Test]
        public void CloseItemDeactivatesItemAndRemovesFromItemsIfItemCanClose()
        {
            var screen = new Mock<IScreen>();
            screen.Setup(x => x.CanCloseAsync()).Returns(Task.FromResult(true));
            this.conductor.ActivateItem(screen.Object);
            this.conductor.DeactivateItem(screen.Object, true);
            screen.Verify(x => x.Deactivate(true));
            Assert.AreEqual(0, this.conductor.Items.Count);
        }
    }
}
