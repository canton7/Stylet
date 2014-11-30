using Moq;
using NUnit.Framework;
using Stylet;
using Stylet.Xaml;
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

        private class AccessibleViewManager : ViewManager
        {
            public AccessibleViewManager() : base(new List<Assembly>(), null) { }

            public new UIElement CreateViewForModel(object model)
            {
                return base.CreateViewForModel(model);
            }

            public new void BindViewToModel(UIElement view, object viewModel)
            {
                base.BindViewToModel(view, viewModel);
            }
        }

        private class CreatingAndBindingViewManager : ViewManager
        {
            public UIElement View;
            public object RequestedModel;

            public CreatingAndBindingViewManager() : base(new List<Assembly>(), null) { }

            protected override UIElement CreateViewForModel(object model)
            {
                this.RequestedModel = model;
                return this.View;
            }

            public UIElement BindViewToModelView;
            public object BindViewtoModelViewModel;
            protected override void BindViewToModel(UIElement view, object viewModel)
            {
                this.BindViewToModelView = view;
                this.BindViewtoModelViewModel = viewModel;
            }
        }

        private class LocatingViewManager : ViewManager
        {
            public LocatingViewManager(Func<Type, UIElement> viewFactory) : base(new List<Assembly>(), viewFactory) { }

            public Type LocatedViewType;
            protected override Type LocateViewForModel(Type modelType)
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

        private class MyViewManager : ViewManager
        {
            public MyViewManager(List<Assembly> assemblies) : base(assemblies, null) { }

            public new Type LocateViewForModel(Type modelType)
            {
                return base.LocateViewForModel(modelType);
            }
        }

        private MyViewManager viewManager;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Execute.TestExecuteSynchronously = true;
        }

        [SetUp]
        public void SetUp()
        {
            this.viewManager = new MyViewManager(new List<Assembly>());
        }

        [Test]
        public void OnModelChangedDoesNothingIfNoChange()
        {
            var val = new object();
            this.viewManager.OnModelChanged(null, val, val);
        }

        [Test]
        public void OnModelChangedSetsNullIfNewValueNull()
        {
            var target = new ContentControl();
            this.viewManager.OnModelChanged(target, 5, null);
            Assert.Null(target.Content);
        }

        [Test]
        public void OnModelChangedUsesViewIfAlreadySet()
        {
            var target = new ContentControl();
            var model = new Mock<IScreen>();
            var view = new UIElement();

            model.Setup(x => x.View).Returns(view);
            this.viewManager.OnModelChanged(target, null, model.Object);

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

            viewManager.OnModelChanged(target, null, model);

            Assert.AreEqual(viewManager.RequestedModel, model);
            Assert.AreEqual(viewManager.BindViewToModelView, view);
            Assert.AreEqual(viewManager.BindViewtoModelViewModel, model);
            Assert.AreEqual(view, target.Content);
        }

        [Test]
        public void LocateViewForModelThrowsIfViewNotFound()
        {
            Assert.Throws<StyletViewLocationException>(() => this.viewManager.LocateViewForModel(typeof(C1)));
        }

        [Test]
        public void LocateViewForModelFindsViewForModel()
        {
            var viewManager = new MyViewManager(new List<Assembly>() { Assembly.GetExecutingAssembly() });
            Execute.TestExecuteSynchronously = true;
            var viewType = viewManager.LocateViewForModel(typeof(ViewManagerTestsViewModel));
            Assert.AreEqual(typeof(ViewManagerTestsView), viewType);
        }

        [Test]
        public void CreateViewForModelThrowsIfViewIsNotConcreteUIElement()
        {
            var viewManager = new LocatingViewManager(null);

            viewManager.LocatedViewType = typeof(I1);
            Assert.Throws<StyletViewLocationException>(() => viewManager.CreateAndBindViewForModel(new object()));

            viewManager.LocatedViewType = typeof(AC1);
            Assert.Throws<StyletViewLocationException>(() => viewManager.CreateAndBindViewForModel(new object()));

            viewManager.LocatedViewType = typeof(C1);
            Assert.Throws<StyletViewLocationException>(() => viewManager.CreateAndBindViewForModel(new object()));
        }

        [Test]
        public void CreateViewForModelCallsFetchesViewAndCallsInitializeComponent()
        {
            var view = new TestView();
            var viewManager = new LocatingViewManager(type =>
            {
                Assert.AreEqual(typeof(TestView), type);
                return view;
            });
            viewManager.LocatedViewType = typeof(TestView);

            var returnedView = viewManager.CreateAndBindViewForModel(new object());

            Assert.True(view.InitializeComponentCalled);
            Assert.AreEqual(view, returnedView);
        }

        [Test]
        public void CreateViewForModelDoesNotComplainIfNoInitializeComponentMethod()
        {
            var view = new UIElement();
            var viewManager = new LocatingViewManager(type =>
            {
                Assert.AreEqual(typeof(UIElement), type);
                return view;
            });
            viewManager.LocatedViewType = typeof(UIElement);

            var returnedView = viewManager.CreateAndBindViewForModel(new object());

            Assert.AreEqual(view, returnedView);
        }

        [Test]
        public void BindViewToModelSetsActionTarget()
        {
            var view = new UIElement();
            var model = new object();
            var viewManager = new AccessibleViewManager();
            viewManager.BindViewToModel(view, model);

            Assert.AreEqual(model, View.GetActionTarget(view));
        }

        [Test]
        public void BindViewToModelSetsDataContext()
        {
            var view = new FrameworkElement();
            var model = new object();
            var viewManager = new AccessibleViewManager();
            viewManager.BindViewToModel(view, model);

            Assert.AreEqual(model, view.DataContext);
        }

        [Test]
        public void BindViewToModelAttachesView()
        {
            var view = new UIElement();
            var model = new Mock<IViewAware>();
            var viewManager = new AccessibleViewManager();
            viewManager.BindViewToModel(view, model.Object);

            model.Verify(x => x.AttachView(view));
        }
    }
}
