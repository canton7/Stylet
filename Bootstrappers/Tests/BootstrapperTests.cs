using NUnit.Framework;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bootstrappers.Tests;

public class TestRootViewModel { }

public interface ITestBootstrapper
{
    int DisposeCount { get; }
    object GetInstance(Type type);
    void ConfigureBootstrapper();
    List<string> ConfigureLog { get; }
}

public abstract class BootstrapperTests<TBootstrapper> where TBootstrapper : ITestBootstrapper, IDisposable
{
    protected TBootstrapper Bootstrapper;

    public abstract TBootstrapper CreateBootstrapper();
    protected virtual bool Autobinds { get; set; }

    [SetUp]
    public void SetUp()
    {
        this.Bootstrapper = this.CreateBootstrapper();
        this.Bootstrapper.ConfigureBootstrapper();
    }

    [Test]
    public void CallsConfiguredInCorrectOrder()
    {
        Assert.That(this.Bootstrapper.ConfigureLog, Is.EquivalentTo(new[] { "DefaultConfigureIoC", "ConfigureIoC" }));
    }

    [Test]
    public void ReturnsCorrectViewManager()
    {
        object vm = this.Bootstrapper.GetInstance(typeof(IViewManager));
        Assert.IsInstanceOf<ViewManager>(vm);
    }

    [Test]
    public void ReturnsSingletonViewManager()
    {
        object vm1 = this.Bootstrapper.GetInstance(typeof(IViewManager));
        object vm2 = this.Bootstrapper.GetInstance(typeof(IViewManager));
        Assert.AreEqual(vm1, vm2);
    }

    [Test]
    public void ReturnsCorrectWindowManager()
    {
        object wm = this.Bootstrapper.GetInstance(typeof(IWindowManager));
        Assert.IsInstanceOf<WindowManager>(wm);
    }

    [Test]
    public void ReturnsSingletonWindowManager()
    {
        object wm1 = this.Bootstrapper.GetInstance(typeof(IWindowManager));
        object wm2 = this.Bootstrapper.GetInstance(typeof(IWindowManager));
        Assert.AreEqual(wm1, wm2);
    }

    [Test]
    public void ReturnsCorrectEventAggregator()
    {
        object ea = this.Bootstrapper.GetInstance(typeof(IEventAggregator));
        Assert.IsInstanceOf<EventAggregator>(ea);
    }

    [Test]
    public void ReturnsSingletonEventAggregator()
    {
        object ea1 = this.Bootstrapper.GetInstance(typeof(IEventAggregator));
        object ea2 = this.Bootstrapper.GetInstance(typeof(IEventAggregator));
        Assert.AreEqual(ea1, ea2);
    }

    [Test]
    public void ReturnsCorrectMessageBoxViewModel()
    {
        object mb = this.Bootstrapper.GetInstance(typeof(IMessageBoxViewModel));
        Assert.IsInstanceOf<MessageBoxViewModel>(mb);
    }

    [Test]
    public void ReturnsTransientMessageBoxViewModel()
    {
        object mb1 = this.Bootstrapper.GetInstance(typeof(IMessageBoxViewModel));
        object mb2 = this.Bootstrapper.GetInstance(typeof(IMessageBoxViewModel));
        Assert.AreNotEqual(mb1, mb2);
    }

    [Test, Apartment(ApartmentState.STA)]
    public void ReturnsMessageBoxView()
    {
        object view = this.Bootstrapper.GetInstance(typeof(MessageBoxView));
        Assert.NotNull(view);
    }

    [Test, Apartment(ApartmentState.STA)]
    public void ReturnsTransientMessageBoxView()
    {
        object view1 = this.Bootstrapper.GetInstance(typeof(MessageBoxView));
        object view2 = this.Bootstrapper.GetInstance(typeof(MessageBoxView));
        Assert.AreNotEqual(view1, view2);
    }

    [Test]
    public void ResolvesAutoSelfBoundTypesFromCallingAssemblyAsTransient()
    {
        if (!this.Autobinds)
            Assert.Ignore("Autobinding not supported");

        Assert.DoesNotThrow(() => this.Bootstrapper.GetInstance(typeof(TestRootViewModel)));
        object vm1 = this.Bootstrapper.GetInstance(typeof(TestRootViewModel));
        object vm2 = this.Bootstrapper.GetInstance(typeof(TestRootViewModel));

        Assert.NotNull(vm1);
        Assert.AreNotEqual(vm1, vm2); 
    }

    [Test]
    public void ResolvesAutoSelfBoundTypesFromOwnAssemblyAsTransient()
    {
        if (!this.Autobinds)
            Assert.Ignore("Autobinding not supported");

        // Pick a random class with no dependencies...
        Assert.DoesNotThrow(() => this.Bootstrapper.GetInstance(typeof(StubType)));
        object vm1 = this.Bootstrapper.GetInstance(typeof(StubType));
        object vm2 = this.Bootstrapper.GetInstance(typeof(StubType));

        Assert.NotNull(vm1);
        Assert.AreNotEqual(vm1, vm2);
    }

    [Test]
    public void DoesNotMultiplyDisposeWindowManagerConfig()
    {
        // The bootstrapper implements the IWindowManagerConfig. Fetch the IWindowManager to force the
        // IWindowManagerConfig to be constructed, then dispose the bootstrapper, and make sure that
        // the container doesn't dispose the IWindowManagerConfig again

        object windowManager = this.Bootstrapper.GetInstance(typeof(IWindowManager));
        this.Bootstrapper.Dispose();

        Assert.AreEqual(1, this.Bootstrapper.DisposeCount);
    }

    [Test]
    public void DoesNotDisposeTransientInstances()
    {
        if (!this.Autobinds)
            Assert.Ignore("Autobinding not supported");
            
        StubType.Reset();

        object vm = this.Bootstrapper.GetInstance(typeof(StubType));
        this.Bootstrapper.Dispose();
        Assert.AreEqual(0, StubType.DisposeCount);

    }
}
