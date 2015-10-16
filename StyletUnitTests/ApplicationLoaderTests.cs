using Moq;
using NUnit.Framework;
using Stylet;
using Stylet.Xaml;
using System.Windows;

namespace StyletUnitTests
{
    [TestFixture]
    public class ApplicationLoaderTests
    {
        private ApplicationLoader applicationLoader;

        [SetUp]
        public void SetUp()
        {
            // This will set up the URI registrations for the pack:// and application
            var a = Application.Current;

            this.applicationLoader = new ApplicationLoader();
        }

        [Test]
        public void ConstructorSetsResourceDictionary()
        {
            Assert.AreEqual(1, this.applicationLoader.MergedDictionaries.Count);
            var dict = this.applicationLoader.MergedDictionaries[0];
            Assert.AreEqual("pack://application:,,,/Stylet;component/Xaml/StyletResourceDictionary.xaml", dict.Source.AbsoluteUri);
        }

        [Test]
        public void UnloadsResourceDictionaryIfRequested()
        {
            this.applicationLoader.LoadStyletResources = false;
            Assert.AreEqual(0, this.applicationLoader.MergedDictionaries.Count);
        }

        [Test]
        public void CallsSetupOnBootstrapper()
        {
            var bootstrapper = new Mock<IBootstrapper>();
            this.applicationLoader.Bootstrapper = bootstrapper.Object;
            bootstrapper.Verify(x => x.Setup(Application.Current));
        }

        [Test]
        public void LoadStyletResourcesReturnsCorrectValue()
        {
            Assert.True(this.applicationLoader.LoadStyletResources);
            this.applicationLoader.LoadStyletResources = false;
            Assert.False(this.applicationLoader.LoadStyletResources);
        }

        [Test]
        public void BootstrapperReturnsCorrectValue()
        {
            Assert.Null(this.applicationLoader.Bootstrapper);
            var bootstrapper = new Mock<IBootstrapper>();
            this.applicationLoader.Bootstrapper = bootstrapper.Object;
            Assert.AreEqual(bootstrapper.Object, this.applicationLoader.Bootstrapper);
        }
    }
}
