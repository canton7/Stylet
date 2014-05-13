using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stylet.Samples.ModelValidation.Pages
{
    public class UserViewModel : Screen
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordConfirmation { get; set; }

        public bool AutoValidate
        {
            get { return this.autoValidate; }
            set { this.autoValidate = value; this.NotifyOfPropertyChange(); }
        }

        public UserViewModel(IModelValidator<UserViewModel> validator) : base(validator)
        {
        }

        protected override void OnValidationStateChanged(IEnumerable<string> changedProperties)
        {
            base.OnValidationStateChanged(changedProperties);
            // Fody can't weave other assemblies, so we have to manually raise this
            this.NotifyOfPropertyChange(() => this.CanSubmit);
        }

        public bool CanSubmit
        {
            get { return !this.AutoValidate || !this.HasErrors; }
        }
        public void Submit()
        {
            if (this.Validate())
                System.Windows.MessageBox.Show("Successfully submitted");
        }
    }

    public class UserViewModelValidator : AbstractValidator<UserViewModel>
    {
        public UserViewModelValidator()
        {
            RuleFor(x => x.UserName).NotEmpty().Length(1, 20);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().Matches("[0-9]").WithMessage("{PropertyName} must contain a number");
            RuleFor(x => x.PasswordConfirmation).NotEmpty().Equal(s => s.Password).WithMessage("{PropertyName} should match Password");
        }
    }
}
