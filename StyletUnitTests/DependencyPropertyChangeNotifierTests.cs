using NUnit.Framework;
using Stylet;
using Stylet.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StyletUnitTests
{
    [TestFixture]
    public class DependencyPropertyChangeNotifierTests
    {
        [Test]
        public void ThrowsIfTargetIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => DependencyPropertyChangeNotifier.AddValueChanged(null, View.ActionTargetProperty, (d, e) => { }));
        }

        [Test]
        public void ThrowsIfPropertyIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => DependencyPropertyChangeNotifier.AddValueChanged(new DependencyObject(), (PropertyPath)null, (d, e) => { }));
            Assert.Throws<ArgumentNullException>(() => DependencyPropertyChangeNotifier.AddValueChanged(new DependencyObject(), (DependencyProperty)null, (d, e) => { }));
        }

        [Test]
        public void ThrowsIfHandlerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => DependencyPropertyChangeNotifier.AddValueChanged(new DependencyObject(), View.ActionTargetProperty, null));
        }

        [Test]
        public void DoesNotRetainTarget()
        {
            var target = new DependencyObject();
            var weakTarget = new WeakReference(target);

            DependencyPropertyChangeNotifier.AddValueChanged(target, View.ActionTargetProperty, (d, e) => { });

            target = null;
            GC.Collect();

            Assert.IsFalse(weakTarget.IsAlive);
        }

        [Test]
        public void NotifiesOfChange()
        {
            var view = new DependencyObject();

            var value1 = new object();
            var value2 = new object();

            View.SetActionTarget(view, value1);

            DependencyObject subject = null;
            DependencyPropertyChangedEventArgs ea = default(DependencyPropertyChangedEventArgs);
            DependencyPropertyChangeNotifier.AddValueChanged(view, View.ActionTargetProperty, (d, e) =>
            {
                subject = d;
                ea = e;
            });

            View.SetActionTarget(view, value2);

            Assert.AreEqual(view, subject);
            Assert.AreEqual(value1, ea.OldValue);
            Assert.AreEqual(value2, ea.NewValue);
        }

        [Test]
        public void HandlerNotCalledBeforeDependencyPropertyChanged()
        {
            var view = new DependencyObject();

            var called = false;
            DependencyPropertyChangeNotifier.AddValueChanged(view, View.ActionTargetProperty, (d, e) => called = true);

            Assert.IsFalse(called);
        }

        [Test]
        public void DisposeUnsubscribes()
        {
            var view = new DependencyObject();

            var called = false;
            var disposable = DependencyPropertyChangeNotifier.AddValueChanged(view, View.ActionTargetProperty, (d, e) => called = true);

            disposable.Dispose();

            View.SetActionTarget(view, new object());

            Assert.IsFalse(called);
        }
    }
}
