using Moq;
using NUnit.Framework;
using Stylet;
using System;
using System.Threading.Tasks;

namespace StyletUnitTests
{
    [TestFixture]
    public class ConductorOneActiveTests
    {
        public interface IMyScreen : IScreen, IDisposable
        { }

        private class MyConductor : Conductor<IScreen>.Collection.OneActive
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
        public void ActivatingItemActivatesAndAddsToItems()
        {
            var screen = new Mock<IScreen>();
            ((IScreenState)this.conductor).Activate();
            this.conductor.ActivateItem(screen.Object);

            screen.Verify(x => x.Activate());
            screen.Verify(x => x.Deactivate());
            Assert.AreEqual(screen.Object, this.conductor.ActiveItem);
            Assert.That(this.conductor.Items, Is.EquivalentTo(new[] { screen.Object }));
        }

        [Test]
        public void ActivatingItemDeactivatesPreviousItem()
        {
            var screen1 = new Mock<IScreen>();
            var screen2 = new Mock<IScreen>();
            ((IScreenState)this.conductor).Activate();

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
            ((IScreenState)this.conductor).Activate();
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
            ((IScreenState)this.conductor).Activate();

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
            ((IScreenState)this.conductor).Activate();

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
            ((IScreenState)this.conductor).Activate();
            this.conductor.ActivateItem(screen.Object);
            screen.Verify(x => x.Activate());
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
            screen.Setup(x => x.Close()).Callback(() => System.Diagnostics.Debug.WriteLine("FOO"));

            ((IScreenState)this.conductor).Activate();
            this.conductor.ActivateItem(screen.Object);
            ((IScreenState)this.conductor).Close();
            screen.Verify(x => x.Close(), Times.Once());
            screen.Verify(x => x.Dispose(), Times.Once());
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
            ((IScreenState)this.conductor).Activate();
            this.conductor.ActivateItem(screen1.Object);

            // This is an implementation detail
            screen1.Verify(x => x.Deactivate(), Times.Once);
            screen1.Verify(x => x.Activate(), Times.Once);

            this.conductor.Items.Add(screen2.Object);

            Assert.AreEqual(this.conductor.ActiveItem, screen1.Object);
            screen2.Verify(x => x.Activate(), Times.Never);
            screen1.Verify(x => x.Deactivate(), Times.Once); // The one deactivate from earlier
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
        public void RemovingActiveItemActivatesAnotherItem()
        {
            ((IScreenState)this.conductor).Activate();
            var screen1 = new Mock<IMyScreen>();
            var screen2 = new Mock<IMyScreen>();
            this.conductor.ActivateItem(screen1.Object);
            this.conductor.Items.Add(screen2.Object);

            this.conductor.Items.Remove(screen1.Object);

            Assert.AreEqual(this.conductor.ActiveItem, screen2.Object);
            screen2.Verify(x => x.Activate());
            screen1.Verify(x => x.Close());
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
            Assert.AreEqual(0, this.conductor.Items.Count);
        }

        [Test]
        public void CloseItemDisposesIfDisposeChildrenIsTrue()
        {
            var screen = new Mock<IMyScreen>();
            screen.Setup(x => x.CanCloseAsync()).Returns(Task.FromResult(true));
            this.conductor.ActivateItem(screen.Object);
            this.conductor.CloseItem(screen.Object);
            screen.Verify(x => x.Dispose());
        }

        [Test]
        public void CloseItemDoesNotDisposeIfDisposeChildrenIsFalse()
        {
            var screen = new Mock<IMyScreen>();
            this.conductor.DisposeChildren = false;
            screen.Setup(x => x.CanCloseAsync()).Returns(Task.FromResult(true));
            this.conductor.ActivateItem(screen.Object);
            this.conductor.CloseItem(screen.Object);
            screen.Verify(x => x.Dispose(), Times.Never);
        }

        [Test]
        public void ClosingConductorClosesActiveItem()
        {
            var screen1 = new Mock<IMyScreen>();
            screen1.SetupGet(x => x.Parent).Returns(this.conductor);
            this.conductor.ActivateItem(screen1.Object);
            ((IScreenState)this.conductor).Close();
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

            screen2.Verify(x => x.Close());
            screen2.Verify(x => x.Dispose());
        }

        [Test]
        public void ClosingDoesNotCloseThenDeactivateActiveItem()
        {
            // If it calls Close then Deactivate, this causes OnInitialActivate to be called again (in order to
            // activate the screen before deactivating it again).

            var screen = new Mock<IMyScreen>();

            this.conductor.ActivateItem(screen.Object);
            screen.ResetCalls();

            int sequence = 0;
            screen.Setup(x => x.Deactivate()).Callback(() => Assert.AreEqual(0, sequence++));
            screen.Setup(x => x.Close()).Callback(() => Assert.AreEqual(1, sequence++));

            ((IScreenState)this.conductor).Close();

            Assert.AreEqual(2, sequence);
        }

        [Test]
        public void ReplacingActiveItemDoesNotCloseThenDeactivate()
        {
            // If it calls Close then Deactivate, this causes OnInitialActivate to be called again (in order to
            // activate the screen before deactivating it again).

            var screen1 = new Mock<IMyScreen>();
            var screen2 = new Mock<IMyScreen>();

            this.conductor.ActivateItem(screen1.Object);
            screen1.ResetCalls();

            int sequence = 0;
            screen1.Setup(x => x.Deactivate()).Callback(() => Assert.AreEqual(0, sequence++));
            screen1.Setup(x => x.Close()).Callback(() => Assert.AreEqual(1, sequence++));

            this.conductor.Items[0] = screen2.Object;

            Assert.AreEqual(2, sequence);
        }

        [Test]
        public void RemovingActiveItemDoesNotCloseThenDeactivate()
        {
            // If it calls Close then Deactivate, this causes OnInitialActivate to be called again (in order to
            // activate the screen before deactivating it again).

            var screen1 = new Mock<IMyScreen>();
            var screen2 = new Mock<IMyScreen>();

            this.conductor.ActivateItem(screen1.Object);
            screen1.ResetCalls();

            int sequence = 0;
            screen1.Setup(x => x.Deactivate()).Callback(() => Assert.AreEqual(0, sequence++));
            screen1.Setup(x => x.Close()).Callback(() => Assert.AreEqual(1, sequence++));

            this.conductor.Items.RemoveAt(0);

            Assert.AreEqual(2, sequence);
        }

        [Test]
        public void ClosingTheActiveItemDoesNotDisposeItTwice()
        {
            var screen = new Mock<IMyScreen>();
            screen.Setup(x => x.CanCloseAsync()).ReturnsAsync(true);

            this.conductor.ActivateItem(screen.Object);
            this.conductor.CloseItem(screen.Object);

            screen.Verify(x => x.Dispose(), Times.Once());
        }

        [Test]
        public void ClosingTheNonActiveItemDoesNotDisposeItTwice()
        {
            var screen1 = new Mock<IMyScreen>();
            var screen2 = new Mock<IMyScreen>();
            screen1.Setup(x => x.CanCloseAsync()).ReturnsAsync(true);

            this.conductor.ActivateItem(screen1.Object);
            this.conductor.ActivateItem(screen2.Object);
            this.conductor.CloseItem(screen1.Object);

            screen1.Verify(x => x.Dispose(), Times.Once());
        }
    }
}

