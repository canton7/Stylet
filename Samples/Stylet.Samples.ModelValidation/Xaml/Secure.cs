using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Stylet.Samples.ModelValidation.Xaml
{
    public static class Secure
    {
        private static bool passwordInitialized;
        private static bool settingPassword;

        public static string GetPassword(DependencyObject obj)
        {
            return (string)obj.GetValue(PasswordProperty);
        }

        public static void SetPassword(DependencyObject obj, string value)
        {
            obj.SetValue(PasswordProperty, value);
        }

        // We play a trick here. If we set the initial value to something, it'll be set to something else when the binding kicks in,
        // and HandleBoundPasswordChanged will be called, which allows us to set up our event subscription.
        // If the binding sets us to a value which we already are, then this doesn't happen. Therefore start with a value that's
        // definitely unique.
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.RegisterAttached("Password", typeof(string), typeof(Secure), new FrameworkPropertyMetadata(Guid.NewGuid().ToString(), HandleBoundPasswordChanged)
            { 
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.LostFocus // Match the default on Binding
            });


        private static void HandleBoundPasswordChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (settingPassword)
                return;

            var passwordBox = dp as PasswordBox;
            if (passwordBox == null)
                return;

            // If this is the initial set
            if (!passwordInitialized)
            {
                passwordInitialized = true;
                passwordBox.PasswordChanged += HandlePasswordChanged;
            }

            passwordBox.Password = e.NewValue as string;
        }

        private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = (PasswordBox)sender;
            settingPassword = true;
            SetPassword(passwordBox, passwordBox.Password);
            settingPassword = false;
        }


        private static bool securePasswordInitialized;
        private static bool settingSecurePassword;

        public static SecureString GetSecurePassword(DependencyObject obj)
        {
            return (SecureString)obj.GetValue(SecurePasswordProperty);
        }

        public static void SetSecurePassword(DependencyObject obj, SecureString value)
        {
            obj.SetValue(SecurePasswordProperty, value);
        }

        // Similarly, we'll be set to something which isn't this exact instance of SecureString when the binding kicks in
        public static readonly DependencyProperty SecurePasswordProperty =
            DependencyProperty.RegisterAttached("SecurePassword", typeof(SecureString), typeof(Secure), new FrameworkPropertyMetadata(new SecureString(), HandleBoundSecurePasswordChanged)
            {
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            });


        private static void HandleBoundSecurePasswordChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (settingSecurePassword)
                return;

            var passwordBox = dp as PasswordBox;
            if (passwordBox == null)
                return;

            if (!securePasswordInitialized)
            {
                passwordBox.PasswordChanged += HandleSecurePasswordChanged;
                securePasswordInitialized = true;
            }
        }

        private static void HandleSecurePasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = (PasswordBox)sender;
            settingSecurePassword = true;
            SetSecurePassword(passwordBox, passwordBox.SecurePassword);
            settingSecurePassword = false;
        }
    }
}
