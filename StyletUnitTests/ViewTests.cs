using Moq;
using NUnit.Framework;
using Stylet;
using Stylet.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace StyletUnitTests
{
    [TestFixture, RequiresSTA]
    public class ViewTests
    {
        private Mock<IViewManager> viewManager;

        [SetUp]
        public void SetUp()
        {
            this.viewManager = new Mock<IViewManager>();
            View.ViewManager = this.viewManager.Object;
        }

        [Test]
        public void ActionTargetStores()
        {
            var obj = new DependencyObject();
            View.SetActionTarget(obj, 5);
            Assert.AreEqual(5, View.GetActionTarget(obj));
        }

        [Test]
        public void ModelStores()
        {
            var obj = new DependencyObject();
            View.SetModel(obj, 5);
            Assert.AreEqual(5, View.GetModel(obj));
        }

        [Test]
        public void ChangingModelCallsOnModelChanged()
        {
            var obj = new DependencyObject();
            var model = new object();
            View.SetModel(obj, null);

            object oldValue = null;
            object newValue = null;
            this.viewManager.Setup(x => x.OnModelChanged(obj, It.IsAny<object>(), It.IsAny<object>()))
                .Callback<DependencyObject, object, object>((d, eOldValue, eNewValue) =>
                {
                    oldValue = eOldValue;
                    newValue = eNewValue;
                }).Verifiable();
            View.SetModel(obj, model);

            this.viewManager.Verify();
            Assert.Null(oldValue);
            Assert.AreEqual(model, newValue);
        }

        [Test]
        public void SetsContentControlContentProperty()
        {
            var obj = new ContentControl();
            var view = new UIElement();

            View.SetContentProperty(obj, view);
            Assert.AreEqual(obj.Content, view);
        }
    }
}
