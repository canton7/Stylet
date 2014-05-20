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
        public static MessageBoxResult ShowMessageBox(this IWindowManager windowManager, string text, string title, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None, MessageBoxResult defaultButton = MessageBoxResult.None, MessageBoxResult cancelButton = MessageBoxResult.None)
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
        void Setup(string text, string title, MessageBoxButton buttons, MessageBoxImage icon, MessageBoxResult defaultButton, MessageBoxResult cancelButton);

        /// <summary>
        /// After the user has clicked a button, holds which button was clicked
        /// </summary>
        MessageBoxResult ClickedButton { get; }
    }

    /// <summary>
    /// Default implementation of IMessageBoxViewModel, and is therefore the ViewModel shown by default by ShowMessageBox
    /// </summary>
    public class MessageBoxViewModel : Screen, IMessageBoxViewModel
    {
        /// <summary>
        /// Mapping of button to text to display on that button. You can modify this to localize your application.
        /// </summary>
        public static IDictionary<MessageBoxResult, string> ButtonLabels { get; set; }

        public static IDictionary<MessageBoxButton, MessageBoxResult[]> ButtonToResults { get; set; }

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
            ButtonLabels = new Dictionary<MessageBoxResult, string>()
            {
                { MessageBoxResult.OK, "OK" },
                { MessageBoxResult.Cancel, "Cancel" },
                { MessageBoxResult.Yes, "Yes" },
                { MessageBoxResult.No, "No" },
            };

            ButtonToResults = new Dictionary<MessageBoxButton, MessageBoxResult[]>()
            {
                { MessageBoxButton.OK, new[] { MessageBoxResult.OK } },
                { MessageBoxButton.OKCancel, new[] { MessageBoxResult.OK, MessageBoxResult.Cancel } },
                { MessageBoxButton.YesNo, new[] { MessageBoxResult.Yes, MessageBoxResult.No } },
                { MessageBoxButton.YesNoCancel, new[] { MessageBoxResult.Yes, MessageBoxResult.No, MessageBoxResult.Cancel} },
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
        public void Setup(string text, string title, MessageBoxButton buttons, MessageBoxImage icon, MessageBoxResult defaultButton, MessageBoxResult cancelButton)
        {
            this.Text = text;
            this.DisplayName = title;
            this.Icon = icon;

            var buttonList = new List<LabelledValue<MessageBoxResult>>();
            this.ButtonList = buttonList;
            foreach (var val in ButtonToResults[buttons])
            {
                var lbv = new LabelledValue<MessageBoxResult>(ButtonLabels[val], val);
                buttonList.Add(lbv);
                if (val == defaultButton)
                    this.DefaultButton = lbv;
                else if (val == cancelButton)
                    this.CancelButton = lbv;
            }
            // If they didn't specify a button which we showed, then pick a default, if we can
            if (this.DefaultButton == null)
            {
                if (defaultButton == MessageBoxResult.None && this.ButtonList.Any())
                    this.DefaultButton = buttonList[0];
                else
                    throw new ArgumentException("DefaultButton set to a button which doesn't appear in Buttons");
            }
            if (this.CancelButton == null)
            {
                if (cancelButton == MessageBoxResult.None && this.ButtonList.Any())
                    this.CancelButton = buttonList.Last();
                else
                    throw new ArgumentException("CancelButton set to a button which doesn't appear in Buttons");
            }
        }

        /// <summary>
        /// List of buttons which are shown in the View.
        /// </summary>
        public IEnumerable<LabelledValue<MessageBoxResult>> ButtonList { get; protected set; }

        /// <summary>
        /// Item in ButtonList which is the Default button
        /// </summary>
        public LabelledValue<MessageBoxResult> DefaultButton { get; set; }

        /// <summary>
        /// Item in ButtonList which is the Cancel button
        /// </summary>
        public LabelledValue<MessageBoxResult> CancelButton { get; set; }      

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
        /// Whether to display an icon
        /// </summary>
        public virtual bool ImageIconVisible
        {
            get { return this.Icon != MessageBoxImage.None; }
        }

        /// <summary>
        /// Which button the user clicked, once they've clicked a button
        /// </summary>
        public virtual MessageBoxResult ClickedButton { get; private set; }

        protected override void OnViewLoaded()
        {
            // There might not be a sound, or it might be null
            SystemSound sound;
            SoundMapping.TryGetValue(this.Icon, out sound);
            if (sound != null)
                sound.Play();
        }

        public void ButtonClicked(MessageBoxResult button)
        {
            this.ClickedButton = button;
            this.TryClose(true);
        }
    }
}
