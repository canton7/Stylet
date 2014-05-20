using Moq;
using NUnit.Framework;
using Stylet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StyletUnitTests
{
    [TestFixture]
    public class MessageBoxTests
    {
        private class MyMessageBoxViewModel : MessageBoxViewModel
        {
            public void OnViewLoaded()
            {
                base.OnViewLoaded();
            }
        }

        private MessageBoxViewModel vm;

        [SetUp]
        public void SetUp()
        {
            this.vm = new MessageBoxViewModel();
        }

        [Test]
        public void ShowMessageBoxShowsMessageBox()
        {
            var vm = new Mock<IMessageBoxViewModel>();
            IoC.GetInstance = (o, k) =>
            {
                Assert.AreEqual(typeof(IMessageBoxViewModel), o);
                Assert.AreEqual(null, k);
                return vm.Object;
            };

            var windowManager = new Mock<IWindowManager>();

            vm.SetupGet(x => x.ClickedButton).Returns(MessageBoxResult.OK);
            var result = MessageBoxWindowManagerExtensions.ShowMessageBox(windowManager.Object, "this is the text", "this is the title", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation, MessageBoxResult.OK, MessageBoxResult.Cancel);

            vm.Verify(x => x.Setup("this is the text", "this is the title", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation, MessageBoxResult.OK, MessageBoxResult.Cancel));
            windowManager.Verify(x => x.ShowDialog(vm.Object));

            Assert.AreEqual(MessageBoxResult.OK, result);
        }

        [Test]
        public void SetsTextCorrectly()
        {
            this.vm.Setup("this is the text", null, MessageBoxButton.OK, System.Windows.MessageBoxImage.None, MessageBoxResult.None, MessageBoxResult.None);
            Assert.AreEqual("this is the text", this.vm.Text);
        }

        [Test]
        public void SetsTitleCorrectly()
        {
            this.vm.Setup(null, "this is the title", MessageBoxButton.OK, System.Windows.MessageBoxImage.None, MessageBoxResult.None, MessageBoxResult.None);
            Assert.AreEqual("this is the title", this.vm.DisplayName);
        }

        [Test]
        public void DisplaysRequestedButtons()
        {
            this.vm.Setup(null, null, MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.None, MessageBoxResult.None, MessageBoxResult.None);
            var buttons = vm.ButtonList.ToList();
            Assert.AreEqual(2, buttons.Count);
            Assert.AreEqual("OK", buttons[0].Label);
            Assert.AreEqual(MessageBoxResult.OK, buttons[0].Value);
            Assert.AreEqual("Cancel", buttons[1].Label);
            Assert.AreEqual(MessageBoxResult.Cancel, buttons[1].Value);
        }

        [Test]
        public void SetsDefaultButtonToTheRequestedButton()
        {
            this.vm.Setup(null, null, MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.None, MessageBoxResult.Cancel, MessageBoxResult.None);
            Assert.AreEqual(this.vm.ButtonList.ElementAt(1), this.vm.DefaultButton);
        }

        [Test]
        public void SetsDefaultToLeftmostButtonIfDefaultRequested()
        {
            this.vm.Setup(null, null, MessageBoxButton.YesNoCancel, System.Windows.MessageBoxImage.None, MessageBoxResult.None, MessageBoxResult.None);
            Assert.AreEqual(this.vm.ButtonList.ElementAt(0), this.vm.DefaultButton);
        }

        [Test]
        public void ThrowsIfTheRequestedDefaultButtonIsNotDisplayed()
        {
            Assert.Throws<ArgumentException>(() => this.vm.Setup(null, null, MessageBoxButton.OKCancel, MessageBoxImage.None, MessageBoxResult.Yes, MessageBoxResult.None));
        }

        [Test]
        public void SetsCancelButtonToTheRequestedButton()
        {
            this.vm.Setup(null, null, MessageBoxButton.YesNoCancel, MessageBoxImage.None, MessageBoxResult.None, MessageBoxResult.No);
            Assert.AreEqual(this.vm.ButtonList.ElementAt(1), this.vm.CancelButton);
        }

        [Test]
        public void SetsCancelToRighmostButtonIfDefaultRequested()
        {
            this.vm.Setup(null, null, MessageBoxButton.OKCancel, MessageBoxImage.None, MessageBoxResult.None, MessageBoxResult.None);
            Assert.AreEqual(this.vm.ButtonList.ElementAt(1), this.vm.CancelButton);
        }

        [Test]
        public void ThrowsIfRequestedCancelButtonIsNotDisplayed()
        {
            Assert.Throws<ArgumentException>(() => this.vm.Setup(null, null, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None, MessageBoxResult.No));
        }

        [Test]
        public void SetsIconCorrectly()
        {
            this.vm.Setup(null, null, MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.None, MessageBoxResult.None);
            Assert.AreEqual(SystemIcons.Exclamation, this.vm.ImageIcon);
        }

        [Test]
        public void SetsResultAndClosesWhenButtonClicked()
        {
            var parent = new Mock<IChildDelegate>();
            this.vm.Parent = parent.Object;
            this.vm.Setup(null, null, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None, MessageBoxResult.None);
            
            this.vm.ButtonClicked(MessageBoxResult.No);

            parent.Verify(x => x.CloseItem(this.vm, true));
            Assert.AreEqual(MessageBoxResult.No, this.vm.ClickedButton);
        }

        [Test]
        public void PlaysNoSoundIfNoSoundToPlay()
        {
            var vm = new MyMessageBoxViewModel();
            // Can't test it actually playing the sound
            vm.Setup(null, null, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None, MessageBoxResult.None);
            Assert.DoesNotThrow(() => vm.OnViewLoaded());
        }
    }
}
