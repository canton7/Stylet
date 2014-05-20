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
    /// <summary>
    /// Specifies button(s) which are displayed on a MessageBox, and which button was clicked
    /// </summary>
    [Flags]
    public enum MessageBoxButtons
    {
        /// <summary>
        /// When used as the defaultButton or cancelButton, specified that it should be chosen automatically.
        /// Not valid for specifying which buttons to display.
        /// </summary>
        Default = 0,

        /// <summary>
        /// "Yes" button
        /// </summary>
        Yes = 1,

        /// <summary>
        /// "No" button
        /// </summary>
        No = 2,

        /// <summary>
        /// "OK" button
        /// </summary>
        OK = 4,

        /// <summary>
        /// "Cancel" button
        /// </summary>
        Cancel = 8,

        /// <summary>
        /// Display both the "Yes" and "No" buttons. Only valid for specifying which buttons to display
        /// </summary>
        YesNo = Yes | No,

        /// <summary>
        /// Display the "Yes", "No", and "Cancel" button. Only valid for specifying which buttons to display
        /// </summary>
        YesNoCancel = Yes | No | Cancel,

        /// <summary>
        /// Display both the "OK" and "Cancel" buttons. Only valid for specifying which buttons to display
        /// </summary>
        OKCancel = OK | Cancel,
    }

    public static class MessageBoxWindowManagerExtensions
    {
        /// <summary>
        /// Show a MessageBox, which looks very similar to the WPF MessageBox, but allows your ViewModel to be unit tested
        /// </summary>
        /// <param name="windowManager">WindowManager to use to display the MessageBox</param>
        /// <param name="text">Text to display in the body of the MessageBox</param>
        /// <param name="title">Title to display in the titlebar of the MessageBox</param>
        /// <param name="buttons">Button, or Buttons, to display on the MessageBox</param>
        /// <param name="icon">Icon to display to the left of the text. This also determines the sound played when the MessageBox is shown</param>
        /// <param name="defaultButton">Button pressed when the user presses Enter. Defaults to the leftmost button</param>
        /// <param name="cancelButton">Button pressed when the user preses Esc or clicks the red X on the titlebar. Defaults to the rightmost button</param>
        /// <returns>Which button the user clicked</returns>
        public static MessageBoxButtons ShowMessageBox(this IWindowManager windowManager, string text, string title, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxImage icon = MessageBoxImage.None, MessageBoxButtons defaultButton = MessageBoxButtons.Default, MessageBoxButtons cancelButton = MessageBoxButtons.Default)
        {
            var vm = IoC.Get<IMessageBoxViewModel>();
            vm.Setup(text, title, buttons, icon, defaultButton, cancelButton);
            windowManager.ShowDialog(vm);
            return vm.ClickedButton;
        }
    }

    /// <summary>
    /// Interface for a MessageBoxViewModel. MessageBoxWindowManagerExtensions.ShowMessageBox will use the configured implementation of this
    /// </summary>
    public interface IMessageBoxViewModel
    {
        /// <summary>
        /// Setup the MessageBoxViewModel with the information it needs
        /// </summary>
        /// <param name="text">Text to display in the body of the MessageBox</param>
        /// <param name="title">Title to display in the titlebar of the MessageBox</param>
        /// <param name="buttons">Button, or Buttons, to display on the MessageBox</param>
        /// <param name="icon">Icon to display to the left of the text. This also determines the sound played when the MessageBox is shown</param>
        /// <param name="defaultButton">Button pressed when the user presses Enter. Defaults to the leftmost button</param>
        /// <param name="cancelButton">Button pressed when the user preses Esc or clicks the red X on the titlebar. Defaults to the rightmost button</param>
        void Setup(string text, string title, MessageBoxButtons buttons, MessageBoxImage icon, MessageBoxButtons defaultButton, MessageBoxButtons cancelButton);

        /// <summary>
        /// After the user has clicked a button, holds which button was clicked
        /// </summary>
        MessageBoxButtons ClickedButton { get; }
    }

    /// <summary>
    /// Default implementation of IMessageBoxViewModel, and is therefore the ViewModel shown by default by ShowMessageBox
    /// </summary>
    public class MessageBoxViewModel : Screen, IMessageBoxViewModel
    {
        /// <summary>
        /// Mapping of button to text to display on that button. You can modify this to localize your application.
        /// </summary>
        public static IDictionary<MessageBoxButtons, string> ButtonLabels { get; set; }

        /// <summary>
        /// Mapping of MessageBoxImage to the SystemIcon to display. You can customize this if you really want.
        /// </summary>
        public static IDictionary<MessageBoxImage, Icon> IconMapping { get; set; }

        /// <summary>
        /// Mapping of MessageBoxImage to the sound to play when the MessageBox is shown. You can customize this if you really want.
        /// </summary>
        public static IDictionary<MessageBoxImage, SystemSound> SoundMapping { get; set; }

        static MessageBoxViewModel()
        {
            ButtonLabels = new Dictionary<MessageBoxButtons, string>()
            {
                { MessageBoxButtons.OK, "OK" },
                { MessageBoxButtons.Cancel, "Cancel" },
                { MessageBoxButtons.Yes, "Yes" },
                { MessageBoxButtons.No, "No" },
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

        /// <summary>
        /// Setup the MessageBoxViewModel with the information it needs
        /// </summary>
        /// <param name="text">Text to display in the body of the MessageBox</param>
        /// <param name="title">Title to display in the titlebar of the MessageBox</param>
        /// <param name="buttons">Button, or Buttons, to display on the MessageBox</param>
        /// <param name="icon">Icon to display to the left of the text. This also determines the sound played when the MessageBox is shown</param>
        /// <param name="defaultButton">Button pressed when the user presses Enter. Defaults to the leftmost button</param>
        /// <param name="cancelButton">Button pressed when the user preses Esc or clicks the red X on the titlebar. Defaults to the rightmost button</param>
        public void Setup(string text, string title, MessageBoxButtons buttons, MessageBoxImage icon, MessageBoxButtons defaultButton, MessageBoxButtons cancelButton)
        {
            if (buttons == MessageBoxButtons.Default)
                throw new ArgumentException("MessageBoxButton.Default is not a valid value for Buttons", "buttons");

            this.Text = text;
            this.DisplayName = title;
            this.Icon = icon;

            var buttonList = new List<LabelledValue<MessageBoxButtons>>();
            this.ButtonList = buttonList;
            var buttonValues = Enum.GetValues(typeof(MessageBoxButtons));
            foreach (MessageBoxButtons val in buttonValues)
            {
                // Ignore those are are composites - i.e. aren't powers of 2
                if ((val & (val - 1)) != 0 || !buttons.HasFlag(val) || val == MessageBoxButtons.Default)
                    continue;

                var lbv = new LabelledValue<MessageBoxButtons>(ButtonLabels[val], val);
                buttonList.Add(lbv);
                if (val == defaultButton)
                    this.DefaultButton = lbv;
                else if (val == cancelButton)
                    this.CancelButton = lbv;
            }
            // If they didn't specify a button which we showed, then pick a default, if we can
            if (this.DefaultButton == null)
            {
                if (defaultButton == MessageBoxButtons.Default&& this.ButtonList.Any())
                    this.DefaultButton = buttonList[0];
                else
                    throw new ArgumentException("DefaultButton set to a button which doesn't appear in Buttons");
            }
            if (this.CancelButton == null)
            {
                if (cancelButton == MessageBoxButtons.Default && this.ButtonList.Any())
                    this.CancelButton = buttonList.Last();
                else
                    throw new ArgumentException("CancelButton set to a button which doesn't appear in Buttons");
            }
        }

        /// <summary>
        /// List of buttons which are shown in the View.
        /// </summary>
        public IEnumerable<LabelledValue<MessageBoxButtons>> ButtonList { get; protected set; }

        /// <summary>
        /// Item in ButtonList which is the Default button
        /// </summary>
        public LabelledValue<MessageBoxButtons> DefaultButton { get; set; }

        /// <summary>
        /// Item in ButtonList which is the Cancel button
        /// </summary>
        public LabelledValue<MessageBoxButtons> CancelButton { get; set; }      

        /// <summary>
        /// Text which is shown in the body of the MessageBox
        /// </summary>
        public virtual string Text { get; set; }

        /// <summary>
        /// Icon which the user specified
        /// </summary>
        public virtual MessageBoxImage Icon { get; set; }

        /// <summary>
        /// Icon which is shown next to the text in the View
        /// </summary>
        public virtual Icon ImageIcon
        {
            get { return IconMapping[this.Icon]; }
        }

        /// <summary>
        /// Which button the user clicked, once they've clicked a button
        /// </summary>
        public virtual MessageBoxButtons ClickedButton { get; private set; }

        protected override void OnViewLoaded()
        {
            // There might not be a sound, or it might be null
            SystemSound sound;
            SoundMapping.TryGetValue(this.Icon, out sound);
            if (sound != null)
                sound.Play();
        }

        public void ButtonClicked(MessageBoxButtons button)
        {
            this.ClickedButton = button;
            this.TryClose(true);
        }
    }
}
