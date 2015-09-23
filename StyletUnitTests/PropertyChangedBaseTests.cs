using NUnit.Framework;
using Stylet;
using System;

namespace StyletUnitTests
{
    [TestFixture]
    public class PropertyChangedBaseTests
    {
        class PropertyChanged : PropertyChangedBase
        {
            public int IntProperty { get; set; }
            public string StringProperty
            {
                set { this.NotifyOfPropertyChange(); }
            }
            private double _doubleProperty;
            public double DoubleProperty
            {
                get { return this._doubleProperty; }
                set { SetAndNotify(ref this._doubleProperty, value); }
            }
            public void RaiseIntPropertyChangedWithExpression()
            {
                this.NotifyOfPropertyChange(() => this.IntProperty);
            }
            public void RaiseIntPropertyChangedWithString()
            {
                this.NotifyOfPropertyChange("IntProperty");
            }
        }

        [Test]
        public void RefreshRaisesPropertyChangedWithEmptyString()
        {
            var pc = new PropertyChanged();
            string changedProperty = null;
            pc.PropertyChanged += (o, e) => changedProperty = e.PropertyName;
            pc.Refresh();
            Assert.AreEqual(String.Empty, changedProperty);
        }

        [Test]
        public void NotifyOfPropertyChangedWithExpressionRaises()
        {
            var pc = new PropertyChanged();
            string changedProperty = null;
            pc.PropertyChanged += (o, e) => changedProperty = e.PropertyName;
            pc.RaiseIntPropertyChangedWithExpression();
            Assert.AreEqual("IntProperty", changedProperty);
        }

        [Test]
        public void NotifyOfPropertyChangedWithStringRaises()
        {
            var pc = new PropertyChanged();
            string changedProperty = null;
            pc.PropertyChanged += (o, e) => changedProperty = e.PropertyName;
            pc.RaiseIntPropertyChangedWithString();
            Assert.AreEqual("IntProperty", changedProperty);
        }

        [Test]
        public void NotifyOfPropertyChangedWithCallerMemberName()
        {
            var pc = new PropertyChanged();
            string changedProperty = null;
            pc.PropertyChanged += (o, e) => changedProperty = e.PropertyName;
            pc.StringProperty = "hello";
            Assert.AreEqual("StringProperty", changedProperty);
        }

        [Test]
        public void UsesDispatcher()
        {
            var pc = new PropertyChanged();
            string changedProperty = null;
            pc.PropertyChanged += (o, e) => changedProperty = e.PropertyName;

            Action action = null;
            pc.PropertyChangedDispatcher = a => action = a;

            pc.RaiseIntPropertyChangedWithExpression();
            Assert.IsNull(changedProperty);
            Assert.IsNotNull(action);

            action();
            Assert.AreEqual("IntProperty", changedProperty);
        }

        [Test]
        public void UsesStaticDispatcherByDefault()
        {
            Action action = null;
            var oldDispatcher = Execute.DefaultPropertyChangedDispatcher;
            Execute.DefaultPropertyChangedDispatcher = a => action = a;

            var pc = new PropertyChanged();
            string changedProperty = null;
            pc.PropertyChanged += (o, e) => changedProperty = e.PropertyName;

            pc.RaiseIntPropertyChangedWithExpression();
            Assert.IsNull(changedProperty);
            Assert.IsNotNull(action);

            action();
            Assert.AreEqual("IntProperty", changedProperty);

            Execute.DefaultPropertyChangedDispatcher = oldDispatcher;
        }

        [Test]
        public void SetAndNotifyWorks()
        {
            var pc = new PropertyChanged();
            string changedProperty = null;
            pc.PropertyChanged += (o, e) => changedProperty = e.PropertyName;

            pc.DoubleProperty = 5;

            Assert.AreEqual("DoubleProperty", changedProperty);
            Assert.AreEqual(5, pc.DoubleProperty);
        }
    }
}
