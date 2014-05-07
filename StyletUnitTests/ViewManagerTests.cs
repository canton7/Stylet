using Moq;
using NUnit.Framework;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace StyletUnitTests
{
    public class ViewManagerTestsViewModel
    {
    }

    public class ViewManagerTestsView
    {
    }

    [TestFixture, RequiresSTA]
    public class ViewManagerTests
    {
        private interface I1 { }
        private abstract class AC1 { }
        private class C1 { }

        private class CreatingAndBindingViewManager : ViewManager
        {
            public UIElement View;
            public object RequestedModel;
            public override UIElement CreateViewForModel(object model)
            {
                this.RequestedModel = model;
                return this.View;
            }

            public UIElement BindViewToModelView;
            public object BindViewtoModelViewModel;
            public override void BindViewToModel(UIElement view, object viewModel)
            {
                this.BindViewToModelView = view;
                this.BindViewtoModelViewModel = viewModel;
            }
        }

        private class LocatingViewManager : ViewManager
        {
            public Type LocatedViewType;
            public override Type LocateViewForModel(Type modelType)
            {
 	             return this.LocatedViewType;
            }
        }


        private class TestView : UIElement
        {
            public bool InitializeComponentCalled;
            public void InitializeComponent()
            {
                this.InitializeComponentCalled = true;
            }
        }

        private ViewManager viewManager;

        [SetUp]
        public void SetUp()
        {
            this.viewManager = new ViewManager();
        }

        [Test]
        public void OnModelChangedDoesNothingIfNoChange()
        {
            var val = new object();
            this.viewManager.OnModelChanged(null, new DependencyPropertyChangedEventArgs(View.ModelProperty, val, val));
        }

        [Test]
        public void OnModelChangedSetsNullIfNewValueNull()
        {
            var target = new ContentControl();
            this.viewManager.OnModelChanged(target, new DependencyPropertyChangedEventArgs(View.ModelProperty, 5, null));
            Assert.Null(target.Content);
        }

        [Test]
        public void OnModelChangedUsesViewIfAlreadySet()
        {
            var target = new ContentControl();
            var model = new Mock<IScreen>();
            var view = new UIElement();

            model.Setup(x => x.View).Returns(view);
            this.viewManager.OnModelChanged(target, new DependencyPropertyChangedEventArgs(View.ModelProperty, null, model.Object));

            Assert.AreEqual(view, target.Content);
        }

        [Test]
        public void OnModelChangedCreatesAndBindsView()
        {
            var target = new ContentControl();
            var model = new object();
            var view = new UIElement();
            var viewManager = new CreatingAndBindingViewManager();

            viewManager.View = view;

            viewManager.OnModelChanged(target, new DependencyPropertyChangedEventArgs(View.ModelProperty, null, model));

            Assert.AreEqual(viewManager.RequestedModel, model);
            Assert.AreEqual(viewManager.BindViewToModelView, view);
            Assert.AreEqual(viewManager.BindViewtoModelViewModel, model);
            Assert.AreEqual(view, target.Content);
        }

        [Test]
        public void LocateViewforModelThrowsIfViewNotFound()
        {
            Assert.Throws<Exception>(() => this.viewManager.LocateViewForModel(typeof(C1)));
        }

        [Test]
        public void LocateViewforModelFindsViewForModel()
        {
            Execute.TestExecuteSynchronously = true;
            AssemblySource.Assemblies.Add(Assembly.GetExecutingAssembly());
            var viewType = this.viewManager.LocateViewForModel(typeof(ViewManagerTestsViewModel));
            Assert.AreEqual(typeof(ViewManagerTestsView), viewType);
        }

        [Test]
        public void CreateViewForModelThrowsIfViewIsNotConcreteUIElement()
        {
            var viewManager = new LocatingViewManager();

            viewManager.LocatedViewType = typeof(I1);
            Assert.Throws<Exception>(() => viewManager.CreateViewForModel(new object()));

            viewManager.LocatedViewType = typeof(AC1);
            Assert.Throws<Exception>(() => viewManager.CreateViewForModel(new object()));

            viewManager.LocatedViewType = typeof(C1);
            Assert.Throws<Exception>(() => viewManager.CreateViewForModel(new object()));
        }

        [Test]
        public void CreateViewForModelCallsFetchesViewAndCallsInitializeComponent()
        {
            var view = new TestView();
            IoC.GetInstance = (t, k) =>
            {
                Assert.AreEqual(typeof(TestView), t);
                Assert.Null(k);
                return view;
            };
            var viewManager = new LocatingViewManager();
            viewManager.LocatedViewType = typeof(TestView);

            var returnedView = viewManager.CreateViewForModel(new object());

            Assert.True(view.InitializeComponentCalled);
            Assert.AreEqual(view, returnedView);
        }

        [Test]
        public void CreateViewForModelDoesNotComplainIfNoInitializeComponentMethod()
        {
            var view = new UIElement();
            IoC.GetInstance = (t, k) =>
            {
                Assert.AreEqual(typeof(UIElement), t);
                Assert.Null(k);
                return view;
            };
            var viewManager = new LocatingViewManager();
            viewManager.LocatedViewType = typeof(UIElement);

            var returnedView = viewManager.CreateViewForModel(new object());

            Assert.AreEqual(view, returnedView);
        }

        [Test]
        public void BindViewToModelSetsActionTarget()
        {
            var view = new UIElement();
            var model = new object();
            this.viewManager.BindViewToModel(view, model);

            Assert.AreEqual(model, View.GetActionTarget(view));
        }

        [Test]
        public void BindViewToModelSetsDataContext()
        {
            var view = new FrameworkElement();
            var model = new object();
            this.viewManager.BindViewToModel(view, model);

            Assert.AreEqual(model, view.DataContext);
        }

        [Test]
        public void BindViewToModelAttachesView()
        {
            var view = new UIElement();
            var model = new Mock<IViewAware>();
            this.viewManager.BindViewToModel(view, model.Object);

            model.Verify(x => x.AttachView(view));
        }
    }
}
