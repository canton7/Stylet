using Moq;
using NUnit.Framework;
using Stylet;
using Stylet.Xaml;
using System;
using System.Collections.Generic;
using System.Reflection;
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
        private ViewManagerConfig viewManagerConfig;

        private class AccessibleViewManager : ViewManager
        {
            public AccessibleViewManager(ViewManagerConfig config) : base(config) { }

            public new UIElement CreateViewForModel(object model)
            {
                return base.CreateViewForModel(model);
            }

            public new void BindViewToModel(UIElement view, object viewModel)
            {
                base.BindViewToModel(view, viewModel);
            }

            public new string ViewTypeNameForModelTypeName(string modelTypeName)
            {
                return base.ViewTypeNameForModelTypeName(modelTypeName);
            }

            public new Type LocateViewForModel(Type modelType)
            {
                return base.LocateViewForModel(modelType);
            }

            public new Type ViewTypeForViewName(string viewName)
            {
                return base.ViewTypeForViewName(viewName);
            }
        }

        private class CreatingAndBindingViewManager : ViewManager
        {
            public UIElement View;
            public object RequestedModel;

            public CreatingAndBindingViewManager(ViewManagerConfig config) : base(config) { }

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
            public LocatingViewManager(ViewManagerConfig config) : base(config) { }

            public Type LocatedViewType;
            protected override Type LocateViewForModel(Type modelType)
            {
 	             return this.LocatedViewType;
            }
        }

        private class ResolvingViewManager : ViewManager
        {
            public ResolvingViewManager(ViewManagerConfig config) : base(config) { }

            public Type ViewType;
            protected override Type ViewTypeForViewName(string viewName)
            {
                return ViewType;
            }

            public new Type LocateViewForModel(Type modelType)
            {
                return base.LocateViewForModel(modelType);
            }

            protected override string ViewTypeNameForModelTypeName(string modelTypeName)
            {
                return "testy";
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

        private AccessibleViewManager viewManager;

        [SetUp]
        public void SetUp()
        {
            this.viewManagerConfig = new ViewManagerConfig()
            {
                ViewFactory = type => null,
            };
            this.viewManager = new AccessibleViewManager(this.viewManagerConfig);
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
            var viewManager = new CreatingAndBindingViewManager(this.viewManagerConfig);

            viewManager.View = view;

            viewManager.OnModelChanged(target, null, model);

            Assert.AreEqual(viewManager.RequestedModel, model);
            Assert.AreEqual(viewManager.BindViewToModelView, view);
            Assert.AreEqual(viewManager.BindViewtoModelViewModel, model);
            Assert.AreEqual(view, target.Content);
        }

        [Test]
        public void OnModelChangedThrowsIfViewIsAWindow()
        {
            var target = new ContentControl();
            var model = new object();
            var view = new Window();
            var viewManager = new CreatingAndBindingViewManager(this.viewManagerConfig);

            viewManager.View = view;

            Assert.Throws<StyletInvalidViewTypeException>(() => viewManager.OnModelChanged(target, null, model));
        }

        [Test]
        public void CreateViewForModelReturnsNullIfViewNotFound()
        {
            var config = new ViewManagerConfig()
            {
                ViewAssemblies = new List<Assembly>() { typeof(BootstrapperBase).Assembly, Assembly.GetExecutingAssembly() },
                ViewFactory = type => null,
            };

            var viewManager = new AccessibleViewManager(config);
            Assert.IsNull(viewManager.ViewTypeForViewName("Test"));
        }

        [Test]
        public void LocateViewForModelThrowsIfNameTranslationDoesntWork()
        {
           Assert.Throws<StyletViewLocationException>(() => this.viewManager.LocateViewForModel(typeof(C1)));
        }

        [Test]
        public void LocateViewForModelThrowsIfTypeLocationDoesntWork()
        {
            var config = new ViewManagerConfig()
            {
                ViewFactory = type => null,
            };
            var viewManager = new ResolvingViewManager(config);
            viewManager.ViewType = null;
            Assert.Throws<StyletViewLocationException>(() => viewManager.LocateViewForModel(typeof(C1)));
        }

        [Test]
        public void LocateViewForModelFindsViewForModel()
        {
            var config = new ViewManagerConfig()
            {
                ViewFactory = type => null,
                ViewAssemblies = new List<Assembly>() { Assembly.GetExecutingAssembly() }
            };
            var viewManager = new AccessibleViewManager(config);
            var viewType = viewManager.LocateViewForModel(typeof(ViewManagerTestsViewModel));
            Assert.AreEqual(typeof(ViewManagerTestsView), viewType);
        }

        [Test]
        public void CreateViewForModelIfNecessaryThrowsIfViewIsNotConcreteUIElement()
        {
            var viewManager = new LocatingViewManager(this.viewManagerConfig);

            viewManager.LocatedViewType = typeof(I1);
            Assert.Throws<StyletViewLocationException>(() => viewManager.CreateAndBindViewForModelIfNecessary(new object()));

            viewManager.LocatedViewType = typeof(AC1);
            Assert.Throws<StyletViewLocationException>(() => viewManager.CreateAndBindViewForModelIfNecessary(new object()));

            viewManager.LocatedViewType = typeof(C1);
            Assert.Throws<StyletViewLocationException>(() => viewManager.CreateAndBindViewForModelIfNecessary(new object()));
        }

        [Test]
        public void CreateAndBindViewForModelIfNecessaryCallsFetchesViewAndCallsInitializeComponent()
        {
            var view = new TestView();
            var config = new ViewManagerConfig()
            {
                ViewFactory = type => view
            };
            var viewManager = new LocatingViewManager(config);
            viewManager.LocatedViewType = typeof(TestView);

            var returnedView = viewManager.CreateAndBindViewForModelIfNecessary(new object());

            Assert.True(view.InitializeComponentCalled);
            Assert.AreEqual(view, returnedView);
        }

        [Test]
        public void CreateAndBindViewForModelReturnsViewIfAlreadySet()
        {
            var view = new TestView();
            var viewModel = new Mock<IViewAware>();
            viewModel.SetupGet(x => x.View).Returns(view);

            var returnedView = this.viewManager.CreateAndBindViewForModelIfNecessary(viewModel.Object);

            Assert.AreEqual(view, returnedView);
        }

        [Test]
        public void CreateViewForModelDoesNotComplainIfNoInitializeComponentMethod()
        {
            var view = new UIElement();
            var config = new ViewManagerConfig()
            {
                ViewFactory = type => view,
            };
            var viewManager = new LocatingViewManager(config);
            viewManager.LocatedViewType = typeof(UIElement);

            var returnedView = viewManager.CreateAndBindViewForModelIfNecessary(new object());

            Assert.AreEqual(view, returnedView);
        }

        [Test]
        public void BindViewToModelDoesNotSetActionTarget()
        {
            var view = new UIElement();
            var viewManager = new AccessibleViewManager(this.viewManagerConfig);
            viewManager.BindViewToModel(view, new object());

            Assert.AreEqual(View.InitialActionTarget, View.GetActionTarget(view));
        }

        [Test]
        public void BindViewToModelSetsDataContext()
        {
            var view = new FrameworkElement();
            var model = new object();
            var viewManager = new AccessibleViewManager(this.viewManagerConfig);
            viewManager.BindViewToModel(view, model);

            Assert.AreEqual(model, view.DataContext);
        }

        [Test]
        public void BindViewToModelAttachesView()
        {
            var view = new UIElement();
            var model = new Mock<IViewAware>();
            var viewManager = new AccessibleViewManager(this.viewManagerConfig);
            viewManager.BindViewToModel(view, model.Object);

            model.Verify(x => x.AttachView(view));
        }

        

        [Test]
        public void ViewNameResolutionWorksAsExpected()
        {
            var viewManager = new AccessibleViewManager(this.viewManagerConfig);

            Assert.AreEqual("Root.Test.ThingView", viewManager.ViewTypeNameForModelTypeName("Root.Test.ThingViewModel"));
            Assert.AreEqual("Root.Views.ThingView", viewManager.ViewTypeNameForModelTypeName("Root.ViewModels.ThingViewModel"));
            Assert.AreEqual("Root.View.ThingView", viewManager.ViewTypeNameForModelTypeName("Root.ViewModel.ThingViewModel"));
            Assert.AreEqual("Root.View.ViewModelThing", viewManager.ViewTypeNameForModelTypeName("Root.ViewModel.ViewModelThing"));
            Assert.AreEqual("Root.ThingViews.ThingView", viewManager.ViewTypeNameForModelTypeName("Root.ThingViewModels.ThingViewModel"));
            Assert.AreEqual("Root.ThingView.ThingView", viewManager.ViewTypeNameForModelTypeName("Root.ThingViewModel.ThingViewModel"));

            Assert.AreEqual("Root.ViewModelsNamespace.ThingView", viewManager.ViewTypeNameForModelTypeName("Root.ViewModelsNamespace.ThingViewModel"));
            Assert.AreEqual("Root.ViewModelNamespace.ThingView", viewManager.ViewTypeNameForModelTypeName("Root.ViewModelNamespace.ThingViewModel"));
            Assert.AreEqual("Root.NamespaceOfViews.ThingView", viewManager.ViewTypeNameForModelTypeName("Root.NamespaceOfViewModels.ThingViewModel"));
            Assert.AreEqual("Root.NamespaceOfView.ThingView", viewManager.ViewTypeNameForModelTypeName("Root.NamespaceOfViewModel.ThingViewModel"));

            Assert.AreEqual("ViewModels.TestView", viewManager.ViewTypeNameForModelTypeName("ViewModels.TestViewModel"));
        }
    }
}
