using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace StyletIntegrationTests.Actions
{
    public class ActionsViewModel : Screen
    {
        private bool _checkboxIsChecked;
        public bool CheckboxIsChecked
        {
            get { return this._checkboxIsChecked; }
            set
            {
                this.SetAndNotify(ref this._checkboxIsChecked, value);
                this.NotifyOfPropertyChange(() => this.CanCommandButton);
            }
        }

        public ActionsViewModel()
        {
            this.DisplayName = "Actions";
        }

        public bool CanCommandButton
        {
            get { return this.CheckboxIsChecked; }
        }
        public void CommandButton(string parameter)
        {
            MessageBox.Show(String.Format("Parameter is '{0}'", parameter));
        }

        public void EventButtonNoArgs()
        {
            MessageBox.Show("Success!");
        }

        public void EventButtonWithArgs(RoutedEventArgs e)
        {
            string buttonText = (string)(((Button)e.Source).Content);
            MessageBox.Show(buttonText == "Button 2" ? "Success!" : "Fail: Sender was not sent successfully");
        }
    }
}
