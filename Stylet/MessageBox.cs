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
            vm.Text = text;
            vm.Title = title;
            vm.DefaultButton = defaultButton;
            vm.CancelButton = cancelButton;
            vm.Buttons = buttons;
            vm.Icon = icon;
            windowManager.ShowDialog(vm);
            return vm.ClickedButton;
        }
    }

    public interface IMessageBoxViewModel
    {
        MessageBoxButton Buttons { get; set; }
        string Title { get; set; }
        string Text { get; set; }
        MessageBoxImage Icon { get; set; }
        MessageBoxButton DefaultButton { get; set; }
        MessageBoxButton CancelButton { get; set; }
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

        private MessageBoxButton _buttons;
        public virtual MessageBoxButton Buttons
        {
            get { return this._buttons; }
            set
            {
                if (value == MessageBoxButton.Default)
                    throw new ArgumentException("MessageBoxButton.Default is not a valid value for Buttons");
                this._buttons = value;
                this.RefreshButtonList();
            }
        }

        private MessageBoxButton _defaultButton;
        public virtual MessageBoxButton DefaultButton
        {
            get { return this._defaultButton; }
            set
            {
                this._defaultButton = value;
                this.RefreshButtonList();
            }
        }

        private MessageBoxButton _cancelButton;
        public virtual MessageBoxButton CancelButton
        {
            get { return this._cancelButton; }
            set
            {
                this._cancelButton = value;
                this.RefreshButtonList();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public IObservableCollection<LabelledValue<MessageBoxButton>> ButtonList { get; protected set; }

        private LabelledValue<MessageBoxButton> _controlDefaultButton;
        [EditorBrowsable(EditorBrowsableState.Never)]
        public LabelledValue<MessageBoxButton> ControlDefaultButton
        {
            get { return this._controlDefaultButton; }
            set { SetAndNotify(ref this._controlDefaultButton, value); }
        }

        private LabelledValue<MessageBoxButton> _controlCancelButton;
        [EditorBrowsable(EditorBrowsableState.Never)]
        public LabelledValue<MessageBoxButton> ControlCancelButton
        {
            get { return this._controlCancelButton; }
            set { SetAndNotify(ref this._controlCancelButton, value); }
        }

        
        public virtual string Title
        {
            get { return this.DisplayName; }
            set { this.DisplayName = value; }
        }

        public virtual string Text { get; set; }

        private MessageBoxImage _icon;
        public virtual MessageBoxImage Icon
        {
            get { return this._icon; }
            set
            {
                this._icon = value;
                this.ImageIcon = IconMapping[this._icon];
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual Icon ImageIcon { get; set; }

        public virtual MessageBoxButton ClickedButton { get; private set; }

        public MessageBoxViewModel()
        {
            this.ButtonList = new BindableCollection<LabelledValue<MessageBoxButton>>();
        }

        protected virtual void RefreshButtonList()
        {
            this.ButtonList.Clear();
            this.ControlDefaultButton = null;
            this.ControlCancelButton = null;
            var buttonValues = Enum.GetValues(typeof(MessageBoxButton));
            foreach (MessageBoxButton val in buttonValues)
            {
                // Ignore those are are composites - i.e. aren't powers of 2
                if ((val & (val - 1)) != 0 || !this._buttons.HasFlag(val) || val == MessageBoxButton.Default)
                    continue;

                var lbv = new LabelledValue<MessageBoxButton>(ButtonLabels[val], val);
                this.ButtonList.Add(lbv);
                if (val == this.DefaultButton)
                    this.ControlCancelButton = lbv;
                else if (val == this.CancelButton)
                    this.ControlCancelButton = lbv;
            }
            if (this.ButtonList.Any())
            {
                if (this.ControlDefaultButton == null)
                {
                    if (this.DefaultButton == MessageBoxButton.Default)
                        this.ControlDefaultButton = this.ButtonList[0];
                    else
                        throw new ArgumentException("DefaultButton set to a button which doesn't appear in Buttons");
                }
                if (this.ControlCancelButton == null)
                {
                    if (this.CancelButton == MessageBoxButton.Default)
                        this.ControlCancelButton = this.ButtonList.Last();
                    else
                        throw new ArgumentException("CancelButton set to a button whcih doesn't appear in Buttons");
                }
            }
        }

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
