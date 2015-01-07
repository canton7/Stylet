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

            public IEventBinding BindStrong(NotifyingClass notifying)
            {
                // Must make sure the compiler doesn't generate an inner class for this, otherwise we're not testing the right thing
                return notifying.Bind(x => x.Foo, (o, e) => this.LastFoo = e.NewValue);
            }

            public IEventBinding BindWeak(NotifyingClass notifying)
            {
                return notifying.BindWeak(x => x.Foo, (o, e) => this.LastFoo = e.NewValue);
            }
        }

        private string newVal;
        private object sender;

        [SetUp]
        public void SetUp()
        {
            this.newVal = null;
            this.sender = null;
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
            var c1 = new NotifyingClass();
            c1.Bar = "bar";
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
        public void WeakBindingBinds()
        {
            var c1 = new NotifyingClass();
            c1.BindWeak(x => x.Foo, (o, e) => this.newVal = e.NewValue);
            c1.Foo = "bar";

            Assert.AreEqual("bar", this.newVal);
        }

        [Test]
        public void WeakBindingIgnoresOtherProperties()
        {
            var c1 = new NotifyingClass();
            c1.BindWeak(x => x.Bar, (o, e) => this.newVal = e.NewValue);
            c1.Foo = "bar";

            Assert.IsNull(this.newVal);
        }

        [Test]
        public void WeakBindingListensToEmptyString()
        {
            var c1 = new NotifyingClass();
            c1.Bar = "bar";
            c1.BindWeak(x => x.Bar, (o, e) => this.newVal = e.NewValue);
            c1.NotifyAll();

            Assert.AreEqual("bar", this.newVal);
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
        public void WeakBindingDoesNotRetainNotifier()
        {
            var binding = new BindingClass();
            var notifying = new NotifyingClass();
            // Means of determining whether the class has been disposed
            var weakNotifying = new WeakReference<NotifyingClass>(notifying);
            // Retain binder, as that shouldn't affect anything
            var binder = binding.BindWeak(notifying);

            notifying = null;
            GC.Collect();
            Assert.IsFalse(weakNotifying.TryGetTarget(out notifying));
        }

        [Test]
        public void WeakBindingUnbinds()
        {
            var c1 = new NotifyingClass();
            var binding = c1.BindWeak(x => x.Bar, (o, e) => this.newVal = e.NewValue);
            binding.Unbind();
            c1.Bar = "bar";

            Assert.IsNull(this.newVal);
        }
        
        [Test]
        public void BindWeakPassesSender()
        {
            var c1 = new NotifyingClass();
            c1.BindWeak(x => x.Foo, (o, e) => this.sender = o);
            c1.Foo = "foo";
            Assert.AreEqual(c1, this.sender);
        }

        [Test]
        public void BindWeakThrowsIfTargetIsCompilerGenerated()
        {
            var c1 = new NotifyingClass();
            string newVal = null;
            Assert.Throws<InvalidOperationException>(() => c1.BindWeak(x => x.Foo, (o, e) => newVal = e.NewValue));
        }
    }
}
