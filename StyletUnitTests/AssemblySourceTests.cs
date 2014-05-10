using NUnit.Framework;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StyletUnitTests
{
    [TestFixture]
    public class AssemblySourceTests
    {
        [TestFixtureSetUp]
        public void SetUpFixture()
        {
            Execute.TestExecuteSynchronously = true;
            AssemblySource.Assemblies.Clear();
        }

        [Test]
        public void TestAssemblies()
        {
            var assembly = Assembly.GetExecutingAssembly();
            AssemblySource.Assemblies.Add(assembly);
            CollectionAssert.AreEqual(AssemblySource.Assemblies, new[] { assembly });
        }
    }
}
