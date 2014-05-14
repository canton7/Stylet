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

        // Using a DependencyProperty as the backing store for Password.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.RegisterAttached("Password", typeof(string), typeof(Secure), new FrameworkPropertyMetadata(String.Empty, HandleBoundPasswordChanged)
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

        public static readonly DependencyProperty SecurePasswordProperty =
            DependencyProperty.RegisterAttached("SecurePassword", typeof(SecureString), typeof(Secure), new FrameworkPropertyMetadata(new SecureString(), HandleBoundSecurePasswordChanged) { BindsTwoWayByDefault = true });


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
