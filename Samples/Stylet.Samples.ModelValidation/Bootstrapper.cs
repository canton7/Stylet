using System;
using FluentValidation;
using Stylet.Samples.ModelValidation.Pages;

namespace Stylet.Samples.ModelValidation
{
    public class Bootstrapper : Bootstrapper<ShellViewModel>
    {
        protected override void ConfigureIoC(StyletIoC.IStyletIoCBuilder builder)
        {
            builder.Bind(typeof(IModelValidator<>)).To(typeof(FluentModelValidator<>));
            builder.Bind(typeof(IValidator<>)).ToAllImplementations();
        }
    }
}
