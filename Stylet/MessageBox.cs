using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Stylet
{
    [Flags]
    public enum MessageBoxButton
    {
        Default = 0,
        Yes = 1,
        No = 2,
        OK = 4,
        Cancel = 8,
        YesNo = Yes | No,
        YesNoCancel = Yes | No | Cancel,
        OKCancel = OK | Cancel,
    }

    public static class MessageBoxWindowManagerExtensions
    {
        public static MessageBoxButton ShowMessageBox(this IWindowManager windowManager, string text, string title, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None, MessageBoxButton defaultButton = MessageBoxButton.Default, MessageBoxButton cancelButton = MessageBoxButton.Default)
        {
            var vm = IoC.Get<IMessageBoxViewModel>();
            vm.Setup(text, title, buttons, icon, defaultButton, cancelButton);
            windowManager.ShowDialog(vm);
            return vm.ClickedButton;
        }
    }

    public interface IMessageBoxViewModel
    {
        void Setup(string text, string title, MessageBoxButton buttons, MessageBoxImage icon, MessageBoxButton defaultButton, MessageBoxButton cancelButton);
        MessageBoxButton ClickedButton { get; }
    }

    public class MessageBoxViewModel : Screen, IMessageBoxViewModel
    {
        public static IDictionary<MessageBoxButton, string> ButtonLabels { get; set; }
        public static IDictionary<MessageBoxImage, Icon> IconMapping { get; set; }
        public static IDictionary<MessageBoxImage, SystemSound> SoundMapping { get; set; }

        static MessageBoxViewModel()
        {
            ButtonLabels = new Dictionary<MessageBoxButton, string>()
            {
                { MessageBoxButton.OK, "OK" },
                { MessageBoxButton.Cancel, "Cancel" },
                { MessageBoxButton.Yes, "Yes" },
                { MessageBoxButton.No, "No" },
            };

            IconMapping = new Dictionary<MessageBoxImage, Icon>()
            {
                // Most of the MessageBoxImage values are duplicates - we can't list them here
                { MessageBoxImage.None, null },
                { MessageBoxImage.Error, SystemIcons.Error },
                { MessageBoxImage.Question, SystemIcons.Question },
                { MessageBoxImage.Exclamation, SystemIcons.Exclamation },
                { MessageBoxImage.Information, SystemIcons.Information },
            };

            SoundMapping = new Dictionary<MessageBoxImage, SystemSound>()
            {
                { MessageBoxImage.None, null },
                { MessageBoxImage.Error, SystemSounds.Hand },
                { MessageBoxImage.Question, SystemSounds.Question },
                { MessageBoxImage.Exclamation, SystemSounds.Exclamation },
                { MessageBoxImage.Information, SystemSounds.Asterisk },
            };
        }

        public void Setup(string text, string title, MessageBoxButton buttons, MessageBoxImage icon, MessageBoxButton defaultButton, MessageBoxButton cancelButton)
        {
            if (buttons == MessageBoxButton.Default)
                throw new ArgumentException("MessageBoxButton.Default is not a valid value for Buttons", "buttons");

            this.Text = text;
            this.DisplayName = title;
            this.Icon = icon;

            var buttonList = new List<LabelledValue<MessageBoxButton>>();
            this.ButtonList = buttonList;
            var buttonValues = Enum.GetValues(typeof(MessageBoxButton));
            foreach (MessageBoxButton val in buttonValues)
            {
                // Ignore those are are composites - i.e. aren't powers of 2
                if ((val & (val - 1)) != 0 || !buttons.HasFlag(val) || val == MessageBoxButton.Default)
                    continue;

                var lbv = new LabelledValue<MessageBoxButton>(ButtonLabels[val], val);
                buttonList.Add(lbv);
                if (val == defaultButton)
                    this.DefaultButton = lbv;
                else if (val == cancelButton)
                    this.CancelButton = lbv;
            }
            if (this.ButtonList.Any())
            {
                if (this.DefaultButton == null)
                {
                    if (defaultButton == MessageBoxButton.Default)
                        this.DefaultButton = buttonList[0];
                    else
                        throw new ArgumentException("DefaultButton set to a button which doesn't appear in Buttons");
                }
                if (this.CancelButton == null)
                {
                    if (cancelButton == MessageBoxButton.Default)
                        this.CancelButton = buttonList.Last();
                    else
                        throw new ArgumentException("CancelButton set to a button which doesn't appear in Buttons");
                }
            }
        }

        public IEnumerable<LabelledValue<MessageBoxButton>> ButtonList { get; protected set; }

        public LabelledValue<MessageBoxButton> DefaultButton { get; set; }

        public LabelledValue<MessageBoxButton> CancelButton { get; set; }      

        public virtual string Text { get; set; }

        public virtual MessageBoxImage Icon { get; set; }

        public virtual Icon ImageIcon
        {
            get { return IconMapping[this.Icon]; }
        }

        public virtual MessageBoxButton ClickedButton { get; private set; }

        protected override void OnViewLoaded()
        {
            SystemSound sound;
            SoundMapping.TryGetValue(this.Icon, out sound);
            if (sound != null)
                sound.Play();
        }

        public void ButtonClicked(MessageBoxButton button)
        {
            this.ClickedButton = button;
            this.TryClose(true);
        }
    }

    public class IconConverter : IValueConverter
    {
        public static IconConverter Instance = new IconConverter();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var icon = value as Icon;
            if (icon == null)
                return null;
            var bs = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            return bs;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
