using Moq;
using NUnit.Framework;
using Stylet;
using System;
using System.Threading.Tasks;

namespace StyletUnitTests
{
    [TestFixture]
    public class ConductorAllActiveTests
    {
        public interface IMyScreen : IScreen, IDisposable
        { }

        private class MyConductor : Conductor<IScreen>.Collection.AllActive
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
        public void ActivateItemDeactivatesIfConductorIsNotActive()
        {
            var screen = new Mock<IScreen>();
            this.conductor.ActivateItem(screen.Object);
            screen.Verify(x => x.Deactivate());
        }

        [Test]
        public void ActivateItemActivatesfConductorIsActive()
        {
            var screen = new Mock<IScreen>();
            ((IScreenState)this.conductor).Activate();
            this.conductor.ActivateItem(screen.Object);
            screen.Verify(x => x.Activate());
        }

        [Test]
        public void ActivateItemDoesNotDeactivateIfConductorIsActive()
        {
            var screen = new Mock<IScreen>();
            ((IScreenState)this.conductor).Activate();
            this.conductor.ActivateItem(screen.Object);
            screen.Verify(x => x.Deactivate(), Times.Never);
        }

        [Test]
        public void DeactiveDeactivatesItems()
        {
            var screen = new Mock<IScreen>();
            ((IScreenState)this.conductor).Activate();
            this.conductor.ActivateItem(screen.Object);
            ((IScreenState)this.conductor).Deactivate();
            screen.Verify(x => x.Deactivate());
        }

        [Test]
        public void ClosingClosesAllItems()
        {
            var screen = new Mock<IMyScreen>();
            ((IScreenState)this.conductor).Activate();
            this.conductor.ActivateItem(screen.Object);
            ((IScreenState)this.conductor).Close();
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
        public void ConductorCanCloseAsyncCallsCanCloseOnSelfBeforeChildren()
        {
            var screen = new Mock<IScreen>();

            this.conductor.ActivateItem(screen.Object);
            this.conductor.CanCloseValue = false;

            Assert.IsFalse(this.conductor.CanCloseAsync().Result);

            screen.Verify(x => x.CanCloseAsync(), Times.Never());
        }

        [Test]
        public void AddingItemActivatesAndSetsParent()
        {
            ((IScreenState)this.conductor).Activate();
            var screen = new Mock<IScreen>();
            this.conductor.Items.Add(screen.Object);
            screen.VerifySet(x => x.Parent = this.conductor);
            screen.Verify(x => x.Activate());
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
        }

        [Test]
        public void RemovingItemDisposesIfDisposeChildrenIsTrue()
        {
            var screen = new Mock<IMyScreen>();
            this.conductor.Items.Add(screen.Object);
            this.conductor.Items.Remove(screen.Object);
            screen.Verify(x => x.Dispose());
        }

        [Test]
        public void RemovingItemDoesNotDisposeIfDisposeChildrenIsFalse()
        {
            var screen = new Mock<IMyScreen>();
            this.conductor.DisposeChildren = false;
            this.conductor.Items.Add(screen.Object);
            this.conductor.Items.Remove(screen.Object);
            screen.Verify(x => x.Dispose(), Times.Never);
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
        public void ClosingConductorClosesAllItems()
        {
            var screen1 = new Mock<IMyScreen>();
            screen1.SetupGet(x => x.Parent).Returns(this.conductor);
            var screen2 = new Mock<IMyScreen>();
            screen2.SetupGet(x => x.Parent).Returns(this.conductor);
            this.conductor.ActivateItem(screen1.Object);
            this.conductor.ActivateItem(screen2.Object);

            ((IScreenState)this.conductor).Close();
            screen1.Verify(x => x.Close());
            screen1.Verify(x => x.Dispose());
            screen1.VerifySet(x => x.Parent = null);
            screen2.Verify(x => x.Close());
            screen2.Verify(x => x.Dispose());
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
            screen.Verify(x => x.Dispose());
            CollectionAssert.DoesNotContain(this.conductor.Items, screen.Object);
        }

        [Test]
        public void AddRangeActivatesAddedItems()
        {
            var screen = new Mock<IMyScreen>();
            ((IScreenState)this.conductor).Activate();

            this.conductor.Items.AddRange(new[] { screen.Object });

            screen.Verify(x => x.Activate());
        }

        [Test]
        public void RemoveRangeClosesRemovedItems()
        {
            var screen = new Mock<IMyScreen>();
            this.conductor.ActivateItem(screen.Object);

            this.conductor.Items.RemoveRange(new[] { screen.Object });

            screen.Verify(x => x.Close());
        }

        [Test]
        public void ClearingItemsClosesAndDisposes()
        {
            var screen1 = new Mock<IMyScreen>();
            var screen2 = new Mock<IMyScreen>();

            this.conductor.ActivateItem(screen1.Object);
            this.conductor.ActivateItem(screen2.Object);

            this.conductor.Items.Clear();

            screen1.Verify(x => x.Deactivate());
            screen1.Verify(x => x.Close());
            screen1.Verify(x => x.Dispose());

            screen2.Verify(x => x.Deactivate());
            screen2.Verify(x => x.Close());
            screen2.Verify(x => x.Dispose());
        }
    }
}
