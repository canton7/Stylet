using Moq;
using NUnit.Framework;
using Stylet.Xaml;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xaml;

namespace StyletUnitTests
{
    [TestFixture, RequiresSTA]
    public class ActionExtensionTests
    {
        private ActionExtension actionExtension;
        private Mock<IProvideValueTarget> provideValueTarget;
        private Mock<IRootObjectProvider> rootObjectProvider;
        private Mock<IServiceProvider> serviceProvider;

        private class TestExtensions
        {
            public static readonly RoutedEvent TestEvent = EventManager.RegisterRoutedEvent("Test", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TestExtensions));
            public static void AddTestHandler(DependencyObject d, RoutedEventHandler handler)
            {
                if (d is UIElement uie)
                    uie.AddHandler(TestExtensions.TestEvent, handler);
            }
            public static void RemoveTestHandler(DependencyObject d, RoutedEventHandler handler)
            {
                if (d is UIElement uie)
                    uie.RemoveHandler(TestExtensions.TestEvent, handler);
            }

            public static void InvalidHandler(DependencyObject d)
            {
            }
        }

        [SetUp]
        public void SetUp()
        {
            this.actionExtension = new ActionExtension("MethodName");

            this.provideValueTarget = new Mock<IProvideValueTarget>();
            this.provideValueTarget.Setup(x => x.TargetObject).Returns(new FrameworkElement());

            this.rootObjectProvider = new Mock<IRootObjectProvider>();

            this.serviceProvider = new Mock<IServiceProvider>();
            this.serviceProvider.Setup(x => x.GetService(typeof(IProvideValueTarget))).Returns(this.provideValueTarget.Object);
            this.serviceProvider.Setup(x => x.GetService(typeof(IRootObjectProvider))).Returns(this.rootObjectProvider.Object);
        }

        [Test]
        public void ReturnsThisIfTargetObjectIsNotFrameworkElement()
        {
            this.provideValueTarget.Setup(x => x.TargetObject).Returns(null);

            Assert.AreEqual(this.actionExtension, this.actionExtension.ProvideValue(this.serviceProvider.Object));
        }

        [Test]
        public void ReturnsCommandActionIfTargetObjectPropertyTypeIsICommand()
        {
            this.provideValueTarget.Setup(x => x.TargetProperty).Returns(Button.CommandProperty);

            object value = this.actionExtension.ProvideValue(this.serviceProvider.Object);
            Assert.IsInstanceOf<CommandAction>(value);

            var action = (CommandAction)value;

            Assert.AreEqual(action.Subject, this.provideValueTarget.Object.TargetObject);
            Assert.AreEqual("MethodName", action.MethodName);
        }

        [Test]
        public void ReturnsEventActionIfTargetObjectPropertyIsEventInfo()
        {
            this.provideValueTarget.Setup(x => x.TargetProperty).Returns(typeof(Button).GetEvent("Click"));

            Assert.IsInstanceOf<RoutedEventHandler>(this.actionExtension.ProvideValue(this.serviceProvider.Object));
        }

        [Test]
        public void ReturnsEventActionIfTargetIsAttachedEvent()
        {
            this.provideValueTarget.Setup(x => x.TargetProperty).Returns(typeof(TestExtensions).GetMethod("AddTestHandler"));

            Assert.IsInstanceOf<RoutedEventHandler>(this.actionExtension.ProvideValue(this.serviceProvider.Object));
        }

        [Test]
        public void ThrowsArgumentExceptionIfIsMethodInfoWithWrongParameters()
        {
            this.provideValueTarget.Setup(x => x.TargetProperty).Returns(typeof(TestExtensions).GetMethod("InvalidHandler"));
            Assert.Throws<ArgumentException>(() => this.actionExtension.ProvideValue(this.serviceProvider.Object));
        }

        [Test]
        public void ThrowsArgumentExceptionIfTargetObjectNotDependencyPropertyOrEventInfo()
        {
            this.provideValueTarget.Setup(x => x.TargetProperty).Returns(5);

            Assert.Throws<ArgumentException>(() => this.actionExtension.ProvideValue(this.serviceProvider.Object));
        }

        [Test]
        public void ReturnsEventActionIfTargetIsCommandBinding()
        {
            this.provideValueTarget.Setup(x => x.TargetObject).Returns(new CommandBinding());
            this.provideValueTarget.Setup(x => x.TargetProperty).Returns(typeof(CommandBinding).GetEvent("Executed"));
            this.rootObjectProvider.Setup(x => x.RootObject).Returns(new DependencyObject());

            Assert.IsInstanceOf<ExecutedRoutedEventHandler>(this.actionExtension.ProvideValue(this.serviceProvider.Object));
        }

        [Test]
        public void ThrowsIfTargetIsCommandBindingAndRootObjectNotSet()
        {
            this.provideValueTarget.Setup(x => x.TargetObject).Returns(new CommandBinding());
            this.provideValueTarget.Setup(x => x.TargetProperty).Returns(typeof(CommandBinding).GetEvent("Executed"));

            Assert.Throws<InvalidOperationException>(() => this.actionExtension.ProvideValue(this.serviceProvider.Object));
        }
    }
}
