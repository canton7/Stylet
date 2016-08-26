using Moq;
using NUnit.Framework;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace StyletUnitTests
{
    [TestFixture]
    public class ValidatingModelBaseTests
    {
        private class MyModel : ValidatingModelBase
        {
            public MyModel() : base() { }
            public MyModel(IModelValidator validator) : base(validator) { }

            private int _intProperty;
            public int IntProperty
            {
                get { return this._intProperty; }
                set { SetAndNotify(ref this._intProperty, value); }
            }

            public new bool AutoValidate
            {
                get { return base.AutoValidate; }
                set { base.AutoValidate = value; }
            }

            public new IModelValidator Validator
            {
                get { return base.Validator; }
                set { base.Validator = value; }
            }

            public new bool Validate()
            {
                return base.Validate();
            }

            public new Task<bool> ValidateAsync()
            {
                return base.ValidateAsync();
            }

            public new bool ValidateProperty(string propertyName)
            {
                return base.ValidateProperty(propertyName);
            }

            public new bool ValidateProperty<TProperty>(Expression<Func<TProperty>> property)
            {
                return base.ValidateProperty(property);
            }

            public new Task<bool> ValidatePropertyAsync(string propertyName)
            {
                return base.ValidatePropertyAsync(propertyName);
            }

            public new Task<bool> ValidatePropertyAsync<TProperty>(Expression<Func<TProperty>> property)
            {
                return base.ValidatePropertyAsync(property);
            }
        }

        private Mock<IModelValidator> validator;
        private MyModel model;

        [SetUp]
        public void SetUp()
        {
            this.validator = new Mock<IModelValidator>();
            this.model = new MyModel();
            this.model.Validator = this.validator.Object;
        }

        [Test]
        public void PropertySetsAndInitialisesModelValidator()
        {
            this.validator.Verify(x => x.Initialize(this.model));
            Assert.AreEqual(validator.Object, this.model.Validator);
        }

        [Test]
        public void ConstructorSetsAndInitialisesModelValidator()
        {
            this.validator.Verify(x => x.Initialize(model));
            Assert.AreEqual(validator.Object, model.Validator);
        }

        [Test]
        public void ThrowsIfAskedToValidateAndNoValidatorSet()
        {
            this.model.Validator = null;
            Assert.Throws<InvalidOperationException>(() => this.model.Validate());
            Assert.Throws<InvalidOperationException>(() => this.model.ValidateProperty("test"));
        }

        [Test]
        public void ValidateCallsAdapterValidate()
        {
            this.validator.Setup(x => x.ValidateAllPropertiesAsync()).ReturnsAsync(new Dictionary<string, IEnumerable<string>>() { { "property", new[] { "error1", "error2" } } }).Verifiable();
            this.model.Validate();

            this.validator.Verify();
        }

        [Test]
        public void ValidateAsyncCallsAdapterValidate()
        {
            this.validator.Setup(x => x.ValidateAllPropertiesAsync()).ReturnsAsync(new Dictionary<string, IEnumerable<string>>()).Verifiable();
            this.model.ValidateAsync().Wait();

            this.validator.Verify();
        }

        [Test]
        public void ValidatePropertyByNameCallsAdapterValidate()
        {
            this.validator.Setup(x => x.ValidatePropertyAsync("test")).ReturnsAsync(Enumerable.Empty<string>()).Verifiable();
            this.model.ValidateProperty("test");

            this.validator.Verify();
        }

        [Test]
        public void ValidatePropertyAsyncByNameCallsAdapterValidate()
        {
            this.validator.Setup(x => x.ValidatePropertyAsync("test")).ReturnsAsync(Enumerable.Empty<string>()).Verifiable();
            this.model.ValidatePropertyAsync("test").Wait();

            this.validator.Verify();
        }

        [Test]
        public void ValidatePropertyAsyncWitNullCallsAdapterValidatePropertyWithEmptyString()
        {
            this.validator.Setup(x => x.ValidatePropertyAsync(String.Empty)).ReturnsAsync(Enumerable.Empty<string>()).Verifiable();
            this.model.ValidatePropertyAsync(String.Empty).Wait();

            this.validator.Verify();
        }


        [Test]
        public void ValidatePropertyAsyncWithEmptyStringCallsAdapterValidatePropertyWithEmptyString()
        {
            this.validator.Setup(x => x.ValidatePropertyAsync(String.Empty)).ReturnsAsync(Enumerable.Empty<string>()).Verifiable();
            this.model.ValidatePropertyAsync(String.Empty).Wait();

            this.validator.Verify();
        }
        [Test]
        public void ValidatePropertyByExpressionCallsAdapterValidate()
        {
            this.validator.Setup(x => x.ValidatePropertyAsync("IntProperty")).ReturnsAsync(Enumerable.Empty<string>()).Verifiable();
            this.model.ValidateProperty(() => this.model.IntProperty);

            this.validator.Verify();
        }

        [Test]
        public void ValidatePropertAsyncByExpressionCallsAdapterValidate()
        {
            this.validator.Setup(x => x.ValidatePropertyAsync("IntProperty")).ReturnsAsync(Enumerable.Empty<string>()).Verifiable();
            this.model.ValidatePropertyAsync(() => this.model.IntProperty).Wait();

            this.validator.Verify();
        }

        [Test]
        public void ValidatePropertyReturnsTrueIfValidationPassed()
        {
            this.validator.Setup(x => x.ValidatePropertyAsync("IntProperty")).ReturnsAsync(null);
            var result = this.model.ValidateProperty("IntProperty");
            Assert.True(result);

            this.validator.Setup(x => x.ValidatePropertyAsync("IntProperty")).ReturnsAsync(new string[0]);
            result = this.model.ValidateProperty("IntProperty");
            Assert.True(result);
        }

        [Test]
        public void ValidatePropertyReturnsFalseIfValidationFailed()
        {
            this.validator.Setup(x => x.ValidatePropertyAsync("IntProperty")).ReturnsAsync(new[] { "error" });
            var result = this.model.ValidateProperty("IntProperty");
            Assert.False(result);
        }

        [Test]
        public void ValidateReturnsTrueIfValidationPassed()
        {
            this.validator.Setup(x => x.ValidateAllPropertiesAsync()).ReturnsAsync(new Dictionary<string, IEnumerable<string>>()
                {
                    { "IntProperty", null }
                });
            var result = this.model.Validate();
            Assert.True(result);

            this.validator.Setup(x => x.ValidateAllPropertiesAsync()).ReturnsAsync(new Dictionary<string, IEnumerable<string>>()
                {
                    { "IntProperty", new string[0] }
                });
            result = this.model.Validate();
            Assert.True(result);
        }

        [Test]
        public void ValidateReturnsFalseIfValidationFailed()
        {
            this.validator.Setup(x => x.ValidateAllPropertiesAsync()).ReturnsAsync(new Dictionary<string, IEnumerable<string>>()
                {
                    { "IntProperty", new[] { "error" } }
                });
            var result = this.model.Validate();
            Assert.False(result);
        }

        [Test]
        public void EventRaisedAndHasErrorsChangedIfErrorWasNullAndNowIsNot()
        {
            this.validator.Setup(x => x.ValidatePropertyAsync("IntProperty")).ReturnsAsync(null);
            this.model.ValidateProperty("IntProperty");

            string changedProperty = null;
            this.model.ErrorsChanged += (o, e) => changedProperty = e.PropertyName;
            bool hasErrorsRaised = false;
            this.model.PropertyChanged += (o, e) => { if (e.PropertyName == "HasErrors") hasErrorsRaised = true; };

            this.validator.Setup(x => x.ValidatePropertyAsync("IntProperty")).ReturnsAsync(new[] { "error" });
            this.model.ValidateProperty("IntProperty");

            Assert.AreEqual("IntProperty", changedProperty);
            Assert.True(hasErrorsRaised);
        }

        [Test]
        public void EventRaisedAndHasErrorsChangedIfErrorWasEmptyArrayAndNowIsNot()
        {
            this.validator.Setup(x => x.ValidatePropertyAsync("IntProperty")).ReturnsAsync(new string[0]);
            this.model.ValidateProperty("IntProperty");

            string changedProperty = null;
            this.model.ErrorsChanged += (o, e) => changedProperty = e.PropertyName;
            bool hasErrorsRaised = false;
            this.model.PropertyChanged += (o, e) => { if (e.PropertyName == "HasErrors") hasErrorsRaised = true; };

            this.validator.Setup(x => x.ValidatePropertyAsync("IntProperty")).ReturnsAsync(new[] { "error" });
            this.model.ValidateProperty("IntProperty");

            Assert.AreEqual("IntProperty", changedProperty);
            Assert.True(hasErrorsRaised);
        }

        [Test]
        public void EventRaisedAndHasErrorsChangedIfErrorWasSetAndIsNowNull()
        {
            this.validator.Setup(x => x.ValidatePropertyAsync("IntProperty")).ReturnsAsync(new[] { "error" });
            this.model.ValidateProperty("IntProperty");

            string changedProperty = null;
            this.model.ErrorsChanged += (o, e) => changedProperty = e.PropertyName;
            bool hasErrorsRaised = false;
            this.model.PropertyChanged += (o, e) => { if (e.PropertyName == "HasErrors") hasErrorsRaised = true; };

            this.validator.Setup(x => x.ValidatePropertyAsync("IntProperty")).ReturnsAsync(null);
            this.model.ValidateProperty("IntProperty");

            Assert.AreEqual("IntProperty", changedProperty);
            Assert.True(hasErrorsRaised);
        }

        [Test]
        public void EventRaisedAndHasErrorsChangedIfErrorWasSetAndIsNowEmptyArray()
        {
            this.validator.Setup(x => x.ValidatePropertyAsync("IntProperty")).ReturnsAsync(new[] { "error" });
            this.model.ValidateProperty("IntProperty");

            string changedProperty = null;
            this.model.ErrorsChanged += (o, e) => changedProperty = e.PropertyName;
            bool hasErrorsRaised = false;
            this.model.PropertyChanged += (o, e) => { if (e.PropertyName == "HasErrors") hasErrorsRaised = true; };

            this.validator.Setup(x => x.ValidatePropertyAsync("IntProperty")).ReturnsAsync(new string[0]);
            this.model.ValidateProperty("IntProperty");

            Assert.AreEqual("IntProperty", changedProperty);
            Assert.True(hasErrorsRaised);
        }

        [Test]
        public void EventRaisedAndHasErrorsChangedIfValidateAllAndErrorsChange()
        {
            // Set up some initial errors
            this.validator.Setup(x => x.ValidateAllPropertiesAsync()).ReturnsAsync(new Dictionary<string, IEnumerable<string>>()
                {
                    { "IntProperty", new[] { "error" } },
                    { "OtherProperty", null },
                    { "OtherOtherProperty", new string[0] },
                    { "PropertyThatWillDisappear", new[] { "error" } },
                });
            this.model.Validate();

            this.validator.Setup(x => x.ValidateAllPropertiesAsync()).ReturnsAsync(new Dictionary<string, IEnumerable<string>>()
                {
                    { "IntProperty", new[] { "error" } },
                    { "OtherProperty", new[] { "error" } },
                    { "OtherOtherProperty", new string[0] },
                    { "NewOKProperty", null },
                    { "NewNotOKProperty", new[] { "woo" } },
                });

            var errors = new List<string>();
            this.model.ErrorsChanged += (o, e) => errors.Add(e.PropertyName);
            int hasErrorsChangedCount = 0;
            this.model.PropertyChanged += (o, e) => { if (e.PropertyName == "HasErrors") hasErrorsChangedCount++; };

            this.model.Validate();

            Assert.That(errors, Is.EquivalentTo(new[] { "OtherProperty", "NewOKProperty", "NewNotOKProperty", "PropertyThatWillDisappear" }));
            Assert.AreEqual(1, hasErrorsChangedCount);
        }

        [Test]
        public void GetErrorsReturnsNullIfNoErrorsForThatProperty()
        {
            var errors = this.model.GetErrors("FooBar");
            Assert.Null(errors);
        }

        [Test]
        public void GetErrorsReturnsErrorsForProperty()
        {
            this.validator.Setup(x => x.ValidatePropertyAsync("IntProperty")).ReturnsAsync(new[] { "error1", "error2" });
            this.model.ValidateProperty("IntProperty");
            var errors = this.model.GetErrors("IntProperty");
            Assert.That(errors, Is.EquivalentTo(new[] { "error1", "error2" }));
        }

        [Test]
        public void GetErrorsWithNullReturnsModelErrors()
        {
            this.validator.Setup(x => x.ValidateAllPropertiesAsync()).ReturnsAsync(new Dictionary<string, IEnumerable<string>>()
            {
                { "", new[] { "error1", "error2" } }
            });

            this.model.Validate();
            var errors = this.model.GetErrors(null);
            Assert.That(errors, Is.EquivalentTo(new[] { "error1", "error2" }));
        }

        [Test]
        public void SettingPropertyValidatesIfAutoValidateIsTrue()
        {
            this.model.IntProperty = 5;
            this.validator.Verify(x => x.ValidatePropertyAsync("IntProperty"));
        }

        [Test]
        public void SettingPropertyDoesNotValidateIfAutoValidateIsFalse()
        {
            this.model.AutoValidate = false;
            this.model.IntProperty = 5;
            this.validator.Verify(x => x.ValidatePropertyAsync("IntProperty"), Times.Never);
        }
    }
}
