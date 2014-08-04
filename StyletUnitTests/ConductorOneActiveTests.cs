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
    public class ConductorOneActiveTests
    {
        public interface IMyScreen : IScreen, IDisposable
        { }

        private Conductor<IScreen>.Collection.OneActive conductor;

        [TestFixtureSetUp]
        public void SetUpFixture()
        {
            Execute.TestExecuteSynchronously = true;
        }

        [SetUp]
        public void SetUp()
        {
            this.conductor = new Conductor<IScreen>.Collection.OneActive();
        }

        [Test]
        public void NoActiveItemsBeforeAnyItemsActivated()
        {
            Assert.IsEmpty(this.conductor.Items);
        }

        [Test]
        public void ActivatingItemActivatesAndAddsToItems()
        {
            var screen = new Mock<IScreen>();
            ((IActivate)this.conductor).Activate();
            this.conductor.ActivateItem(screen.Object);

            screen.Verify(x => x.Activate());
            Assert.AreEqual(screen.Object, this.conductor.ActiveItem);
            Assert.That(this.conductor.Items, Is.EquivalentTo(new[] { screen.Object }));
        }

        [Test]
        public void ActivatingItemDeactivatesPreviousItem()
        {
            var screen1 = new Mock<IScreen>();
            var screen2 = new Mock<IScreen>();
            ((IActivate)this.conductor).Activate();

            this.conductor.ActivateItem(screen1.Object);
            this.conductor.ActivateItem(screen2.Object);

            screen1.Verify(x => x.Deactivate());
            screen2.Verify(x => x.Activate());

            Assert.AreEqual(screen2.Object, this.conductor.ActiveItem);
            Assert.AreEqual(new[] { screen1.Object, screen2.Object }, this.conductor.Items);
        }

        [Test]
        public void SettingActiveItemActivatesItem()
        {
            var screen = new Mock<IScreen>();
            ((IActivate)this.conductor).Activate();
            this.conductor.ActiveItem = screen.Object;
            screen.Verify(x => x.Activate());
            Assert.AreEqual(this.conductor.ActiveItem, screen.Object);
        }

        [Test]
        public void ClosingActiveItemChoosesPreviousItemIfAvailable()
        {
            var screen1 = new Mock<IScreen>();
            var screen2 = new Mock<IScreen>();
            var screen3 = new Mock<IScreen>();
            ((IActivate)this.conductor).Activate();

            this.conductor.Items.AddRange(new[] { screen1.Object, screen2.Object, screen3.Object });
            this.conductor.ActivateItem(screen2.Object);

            screen2.Setup(x => x.CanCloseAsync()).Returns(Task.FromResult(true));

            this.conductor.CloseItem(screen2.Object);
            Assert.AreEqual(screen1.Object, this.conductor.ActiveItem);
            Assert.AreEqual(new[] { screen1.Object, screen3.Object }, this.conductor.Items);
        }

        [Test]
        public void ClosingActiveItemChoosesNextItemIfNoPreviousItem()
        {
            var screen1 = new Mock<IScreen>();
            var screen2 = new Mock<IScreen>();
            var screen3 = new Mock<IScreen>();
            ((IActivate)this.conductor).Activate();

            this.conductor.Items.AddRange(new[] { screen1.Object, screen2.Object, screen3.Object });
            this.conductor.ActivateItem(screen3.Object);

            screen3.Setup(x => x.CanCloseAsync()).Returns(Task.FromResult(true));

            this.conductor.CloseItem(screen3.Object);
            Assert.AreEqual(screen2.Object, this.conductor.ActiveItem);
            Assert.AreEqual(new[] { screen1.Object, screen2.Object }, this.conductor.Items);
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
            ((IDeactivate)this.conductor).Deactivate();
            screen.Verify(x => x.Deactivate());
        }

        [Test]
        public void ClosingClosesAllItems()
        {
            var screen = new Mock<IMyScreen>();
            ((IActivate)this.conductor).Activate();
            this.conductor.ActivateItem(screen.Object);
            ((IClose)this.conductor).Close();
            screen.Verify(x => x.Close());
            screen.Verify(x => x.Dispose());
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
        public void AddingItemDoesNotChangeActiveItem()
        {
            var screen1 = new Mock<IScreen>();
            var screen2 = new Mock<IScreen>();
            this.conductor.ActivateItem(screen1.Object);
            this.conductor.Items.Add(screen2.Object);

            Assert.AreEqual(this.conductor.ActiveItem, screen1.Object);
            screen2.Verify(x => x.Activate(), Times.Never);
            screen1.Verify(x => x.Deactivate(), Times.Never);
        }

        [Test]
        public void RemovingItemClosesAndRemovesParent()
        {
            var screen = new Mock<IMyScreen>();
            screen.SetupGet(x => x.Parent).Returns(this.conductor);
            this.conductor.Items.Add(screen.Object);
            this.conductor.Items.Remove(screen.Object);
            screen.VerifySet(x => x.Parent = null);
            screen.Verify(x => x.Close());
            screen.Verify(x => x.Dispose());
        }

        [Test]
        public void RemovingActiveItemActivatesAnotherItem()
        {
            ((IActivate)this.conductor).Activate();
            var screen1 = new Mock<IMyScreen>();
            var screen2 = new Mock<IMyScreen>();
            this.conductor.ActivateItem(screen1.Object);
            this.conductor.Items.Add(screen2.Object);

            this.conductor.Items.Remove(screen1.Object);

            Assert.AreEqual(this.conductor.ActiveItem, screen2.Object);
            screen2.Verify(x => x.Activate());
            screen1.Verify(x => x.Close());
            screen1.Verify(x => x.Dispose());
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
            this.conductor.DeactivateItem(screen.Object);
            screen.Verify(x => x.Deactivate());
            Assert.That(this.conductor.Items, Is.EquivalentTo(new[] { screen.Object }));
        }

        [Test]
        public void CloseItemDoesNothingIfItemCanNotClose()
        {
            var screen = new Mock<IMyScreen>();
            this.conductor.ActivateItem(screen.Object);
            this.conductor.CloseItem(screen.Object);
            screen.Verify(x => x.Close(), Times.Never);
            screen.Verify(x => x.Dispose(), Times.Never);
            Assert.That(this.conductor.Items, Is.EquivalentTo(new[] { screen.Object }));
        }

        [Test]
        public void CloseItemDeactivatesItemAndRemovesFromItemsIfItemCanClose()
        {
            var screen = new Mock<IMyScreen>();
            screen.Setup(x => x.CanCloseAsync()).Returns(Task.FromResult(true));
            this.conductor.ActivateItem(screen.Object);
            this.conductor.CloseItem(screen.Object);
            screen.Verify(x => x.Close());
            screen.Verify(x => x.Dispose());
            Assert.AreEqual(0, this.conductor.Items.Count);
        }

        [Test]
        public void ClosingConductorClosesActiveItem()
        {
            var screen1 = new Mock<IMyScreen>();
            screen1.SetupGet(x => x.Parent).Returns(this.conductor);
            this.conductor.ActivateItem(screen1.Object);
            ((IClose)this.conductor).Close();
            screen1.Verify(x => x.Close());
            screen1.Verify(x => x.Dispose());
            screen1.VerifySet(x => x.Parent = null);
        }

        [Test]
        public void ClosesItemIfItemRequestsClose()
        {
            var screen = new Mock<IMyScreen>();
            this.conductor.ActivateItem(screen.Object);
            screen.Setup(x => x.CanCloseAsync()).Returns(Task.FromResult(true));
            ((IChildDelegate)this.conductor).CloseItem(screen.Object);

            screen.Verify(x => x.Close());
            screen.Verify(x => x.Dispose());
            Assert.Null(this.conductor.ActiveItem);
        }
    }
}
