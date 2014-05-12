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

        public void ValidateModel()
        {
            base.Validate();
        }
    }

    public class UserViewModelValidator : AbstractValidator<UserViewModel>
    {
        public UserViewModelValidator()
        {
            RuleFor(x => x.UserName).Length(1, 20);
            RuleFor(x => x.Email).EmailAddress();
            RuleFor(x => x.Password).Matches("[0-9]").WithMessage("Must contain a number");
            RuleFor(x => x.PasswordConfirmation).Equal(s => s.Password).WithMessage("Should match Password"); ;
        }
    }
}
