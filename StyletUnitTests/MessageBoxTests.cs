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
    public class MessageBoxTests
    {
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

            vm.SetupGet(x => x.ClickedButton).Returns(MessageBoxButtons.OK);
            var result = MessageBoxWindowManagerExtensions.ShowMessageBox(windowManager.Object, "this is the text", "this is the title", MessageBoxButtons.OKCancel, System.Windows.MessageBoxImage.Exclamation, MessageBoxButtons.OK, MessageBoxButtons.Cancel);

            vm.Verify(x => x.Setup("this is the text", "this is the title", MessageBoxButtons.OKCancel, System.Windows.MessageBoxImage.Exclamation, MessageBoxButtons.OK, MessageBoxButtons.Cancel));
            windowManager.Verify(x => x.ShowDialog(vm.Object));

            Assert.AreEqual(MessageBoxButtons.OK, result);
        }
    }
}
