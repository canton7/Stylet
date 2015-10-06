using Moq;
using NUnit.Framework;
using Stylet;
using Stylet.Xaml;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace StyletUnitTests
{
    [TestFixture, RequiresSTA]
    public class ViewTests
    {
        private class TestViewModel
        {
            public BindableCollection<object> SubViewModels { get; set; }

            public object SubViewModel { get; set; }

            public TestViewModel()
            {
                this.SubViewModels = new BindableCollection<object>() { new object() };
                this.SubViewModel = new object();
            }
        }

        private Mock<IViewManager> viewManager;

        [SetUp]
        public void SetUp()
        {
            this.viewManager = new Mock<IViewManager>();
        }

        [TearDown]
        public void TearDown()
        {
            Execute.InDesignMode = false;
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
            var obj = new FrameworkElement();
            obj.Resources.Add(View.ViewManagerResourceKey, this.viewManager.Object);
            View.SetModel(obj, 5);
            Assert.AreEqual(5, View.GetModel(obj));
        }

        [Test]
        public void ChangingModelCallsOnModelChanged()
        {
            var obj = new FrameworkElement();
            obj.Resources.Add(View.ViewManagerResourceKey, this.viewManager.Object);
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

        [Test]
        public void SetContentControlThrowsIfNoContentProperty()
        {
            var obj = new DependencyObject();
            var view = new UIElement();

            Assert.Throws<InvalidOperationException>(() => View.SetContentProperty(obj, view));
        }

        [Test]
        public void SettingModelThrowsExceptionIfViewManagerNotSet()
        {
            var view = new FrameworkElement();
            Assert.Throws<InvalidOperationException>(() => View.SetModel(view, new object()));
        }

        [Test]
        public void InDesignModeSettingViewModelWithBrokenBindingGivesAppropriateMessage()
        {
            Execute.InDesignMode = true;

            var element = new ContentControl();
            // Don't set View.Model to a binding - just a random object
            View.SetModel(element, null);

            Assert.IsInstanceOf<TextBlock>(element.Content);

            var content = (TextBlock)element.Content;
            Assert.AreEqual("View for [Broken Binding]", content.Text);
        }

        [Test]
        public void InDesignModeSettingViewModelWithCollectionBindingGivesAppropriateMessage()
        {
            Execute.InDesignMode = true;

            var element = new ContentControl();
            var vm = new TestViewModel();

            var binding = new Binding();
            binding.Source = vm;
            element.SetBinding(View.ModelProperty, binding);

            Assert.IsInstanceOf<TextBlock>(element.Content);

            var content = (TextBlock)element.Content;
            Assert.AreEqual("View for child ViewModel on TestViewModel", content.Text);
        }

        [Test]
        public void InDesignModeSettingViewModelWithGoodBindingGivesAppropriateMessage()
        {
            Execute.InDesignMode = true;

            var element = new ContentControl();
            var vm = new TestViewModel();

            var binding = new Binding("SubViewModel");
            binding.Source = vm;
            element.SetBinding(View.ModelProperty, binding);

            Assert.IsInstanceOf<TextBlock>(element.Content);

            var content = (TextBlock)element.Content;
            Assert.AreEqual("View for TestViewModel.SubViewModel", content.Text);
        }
    }
}
