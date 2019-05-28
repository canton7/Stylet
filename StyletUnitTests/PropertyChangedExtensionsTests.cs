using NUnit.Framework;
using Stylet;
using System;

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

            public IEventBinding BindStrong(NotifyingClass notifying)
            {
                // Must make sure the compiler doesn't generate an inner class for this, otherwise we're not testing the right thing
                return notifying.Bind(x => x.Foo, (o, e) => this.LastFoo = e.NewValue);
            }
        }

        [Test]
        public void StrongBindingBinds()
        {
            string newVal = null;
            var c1 = new NotifyingClass();
            c1.Bind(x => x.Foo, (o, e) => newVal = e.NewValue);
            c1.Foo = "bar";

            Assert.AreEqual("bar", newVal);
        }

        [Test]
        public void StrongBindingIgnoresOtherProperties()
        {
            string newVal = null;
            var c1 = new NotifyingClass();
            c1.Bind(x => x.Bar, (o, e) => newVal = e.NewValue);
            c1.Foo = "bar";

            Assert.AreEqual(null, newVal);
        }

        [Test]
        public void StrongBindingListensToEmptyString()
        {
            string newVal = null;
            var c1 = new NotifyingClass
            {
                Bar = "bar"
            };
            c1.Bind(x => x.Bar, (o, e) => newVal = e.NewValue);
            c1.NotifyAll();

            Assert.AreEqual("bar", newVal);
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
        public void StrongBindingPassesTarget()
        {
            var c1 = new NotifyingClass();
            object sender = null;
            c1.Bind(x => x.Foo, (o, e) => sender = o);
            c1.Foo = "foo";
            Assert.AreEqual(c1, sender);
        }

        [Test]
        public void StrongBindingUnbinds()
        {
            string newVal = null;
            var c1 = new NotifyingClass();
            var binding = c1.Bind(x => x.Bar, (o, e) => newVal = e.NewValue);
            binding.Unbind();
            c1.Bar = "bar";

            Assert.AreEqual(null, newVal);
        }

        [Test]
        public void BindAndInvokeInvokes()
        {
            var c1 = new NotifyingClass()
            {
                Foo = "FooVal",
            };
            PropertyChangedExtendedEventArgs<string> ea = null;
            c1.BindAndInvoke(s => s.Foo, (o, e) => ea = e);

            Assert.NotNull(ea);
            Assert.AreEqual("Foo", ea.PropertyName);
            Assert.AreEqual("FooVal", ea.NewValue);
        }
    }
}
