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
    public class PropertyChangedExtensionsTests
    {
        class NotifyingClass : PropertyChangedBase
        {
            private string _foo;
            public string Foo
            {
                get { return this._foo; }
                set { SetAndNotify(ref this._foo, value); }
            }

            private string _bar;
            public string Bar
            {
                get { return this._bar; }
                set { SetAndNotify(ref this._bar, value);  }
            }

            public void NotifyAll()
            {
                this.NotifyOfPropertyChange(String.Empty);
            }
        }

        class BindingClass
        {
            public string LastFoo;

            public IPropertyChangedBinding BindStrong(NotifyingClass notifying)
            {
                // Must make sure the compiler doesn't generate an inner class for this, otherwise we're not testing the right thing
                return notifying.Bind(x => x.Foo, x => this.LastFoo = x);
            }

            public IPropertyChangedBinding BindWeak(NotifyingClass notifying)
            {
                return notifying.BindWeak(this, x => x.Foo, x => this.LastFoo = x);
            }
        }

        [Test]
        public void StrongBindingBinds()
        {
            string newVal = null;
            var c1 = new NotifyingClass();
            c1.Bind(x => x.Foo, x => newVal = x);
            c1.Foo = "bar";

            Assert.AreEqual("bar", newVal);
        }

        [Test]
        public void StrongBindingIgnoresOtherProperties()
        {
            string newVal = null;
            var c1 = new NotifyingClass();
            c1.Bind(x => x.Bar, x => newVal = x);
            c1.Foo = "bar";

            Assert.AreEqual(null, newVal);
        }

        [Test]
        public void StrongBindingListensToEmptyString()
        {
            string newVal = null;
            var c1 = new NotifyingClass();
            c1.Bar = "bar";
            c1.Bind(x => x.Bar, x => newVal = x);
            c1.NotifyAll();

            Assert.AreEqual("bar", newVal);
        }

        [Test]
        public void StrongBindingRetainsBindingClass()
        {
            var binding = new BindingClass();

            // Means of determining whether the class has been disposed
            var weakBinding = new WeakReference<BindingClass>(binding);

            var notifying = new NotifyingClass();
            binding.BindStrong(notifying);

            binding = null;
            GC.Collect();
            Assert.IsTrue(weakBinding.TryGetTarget(out binding));
        }

        [Test]
        public void StrongBindingDoesNotRetainNotifier()
        {
            var binding = new BindingClass();
            var notifying = new NotifyingClass();
            // Means of determining whether the class has been disposed
            var weakNotifying = new WeakReference<NotifyingClass>(notifying);
            // Retain the IPropertyChangedBinding, in case that causes NotifyingClass to be retained
            var binder = binding.BindStrong(notifying);

            notifying = null;
            GC.Collect();
            Assert.IsFalse(weakNotifying.TryGetTarget(out notifying));
        }

        [Test]
        public void StrongBindingUnbinds()
        {
            string newVal = null;
            var c1 = new NotifyingClass();
            var binding = c1.Bind(x => x.Bar, x => newVal = x);
            binding.Unbind();
            c1.Bar = "bar";

            Assert.AreEqual(null, newVal);
        }

        [Test]
        public void WeakBindingBinds()
        {
            string newVal = null;
            var c1 = new NotifyingClass();
            c1.BindWeak(this, x => x.Foo, x => newVal = x);
            c1.Foo = "bar";

            Assert.AreEqual("bar", newVal);
        }

        [Test]
        public void WeakBindingIgnoresOtherProperties()
        {
            string newVal = null;
            var c1 = new NotifyingClass();
            c1.BindWeak(this, x => x.Bar, x => newVal = x);
            c1.Foo = "bar";

            Assert.AreEqual(null, newVal);
        }

        [Test]
        public void WeakBindingListensToEmptyString()
        {
            string newVal = null;
            var c1 = new NotifyingClass();
            c1.Bar = "bar";
            c1.BindWeak(this, x => x.Bar, x => newVal = x);
            c1.NotifyAll();

            Assert.AreEqual("bar", newVal);
        }

        [Test]
        public void WeakBindingDoesNotRetainBindingClass()
        {
            var binding = new BindingClass();

            // Means of determining whether the class has been disposed
            var weakBinding = new WeakReference<BindingClass>(binding);

            var notifying = new NotifyingClass();
            binding.BindWeak(notifying);

            binding = null;
            GC.Collect();
            Assert.IsFalse(weakBinding.TryGetTarget(out binding));
        }

        [Test]
        public void WeakBindingRetainsClassIfIPropertyChangedBindingRetained()
        {
            var binding = new BindingClass();

            // Means of determining whether the class has been disposed
            var weakBinding = new WeakReference<BindingClass>(binding);

            var notifying = new NotifyingClass();
            // Retain this
            var binder = binding.BindWeak(notifying);

            binding = null;
            GC.Collect();
            Assert.IsTrue(weakBinding.TryGetTarget(out binding));
        }

        [Test]
        public void WeakBindingDoesNotRetainNotifier()
        {
            var binding = new BindingClass();
            var notifying = new NotifyingClass();
            // Means of determining whether the class has been disposed
            var weakNotifying = new WeakReference<NotifyingClass>(notifying);
            // Retain binder, in case that affects anything
            var binder = binding.BindWeak(notifying);

            notifying = null;
            GC.Collect();
            Assert.IsFalse(weakNotifying.TryGetTarget(out notifying));
        }

        [Test]
        public void WeakBindingUnbinds()
        {
            string newVal = null;
            var c1 = new NotifyingClass();
            var binding = c1.BindWeak(this, x => x.Bar, x => newVal = x);
            binding.Unbind();
            c1.Bar = "bar";

            Assert.AreEqual(null, newVal);
        }
    }
}
