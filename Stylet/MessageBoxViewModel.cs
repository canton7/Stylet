using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Windows;

namespace Stylet
{
    /// <summary>
    /// Optional configuration for a MessageBox
    /// </summary>
    public class MessageBoxConfig
    {
        /// <summary>
        /// Text to display in the body of the MessageBox
        /// </summary>
        public MessageBoxButton Buttons { get; set; }

        /// <summary>
        /// Icon to display to the left of the text. This also determines the sound played when the MessageBox is shown
        /// </summary>
        public MessageBoxImage Icon { get; set; }

        /// <summary>
        /// Button pressed when the user presses Enter. Defaults to the leftmost button
        /// </summary>
        public MessageBoxResult DefaultButton { get; set; }

        /// <summary>
        /// Button pressed when the user preses Esc or clicks the red X on the titlebar. Defaults to the rightmost button
        /// </summary>
        public MessageBoxResult CancelButton { get; set; }

        /// <summary>
        /// Additional options
        /// </summary>
        public MessageBoxOptions Options { get; set; }

        /// <summary>
        /// You may override the text for individual buttons on a case-by-case basis
        /// </summary>
        public IDictionary<MessageBoxResult, string> ButtonLabels { get; set; }

        /// <summary>
        /// Create a new instance with default configuration
        /// </summary>
        public MessageBoxConfig()
        {
            this.Buttons = MessageBoxButton.OK;
            this.Icon = MessageBoxImage.None;
            this.DefaultButton = MessageBoxResult.None;
            this.CancelButton = MessageBoxResult.None;
            this.Options = MessageBoxOptions.None;
            this.ButtonLabels = null;
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
        /// <param name="config">Other configuration. Optional</param>
        void Setup(string text, string title, MessageBoxConfig config = null);

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

        /// <summary>
        /// Mapping of MessageBoxButton values to the buttons which should be displayed
        /// </summary>
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
        /// <param name="config">Other configuration. Optional</param>
        public void Setup(string text, string title, MessageBoxConfig config = null)
        {
            config = config ?? new MessageBoxConfig();

            this.Text = text;
            this.DisplayName = title;
            this.Icon = config.Icon;

            var buttonList = new List<LabelledValue<MessageBoxResult>>();
            this.ButtonList = buttonList;
            foreach (var val in ButtonToResults[config.Buttons])
            {
                string label;
                if (config.ButtonLabels == null || !config.ButtonLabels.TryGetValue(val, out label))
                    label = ButtonLabels[val];
                    
                var lbv = new LabelledValue<MessageBoxResult>(label, val);
                buttonList.Add(lbv);
                if (val == config.DefaultButton)
                    this.DefaultButton = lbv;
                else if (val == config.CancelButton)
                    this.CancelButton = lbv;
            }
            // If they didn't specify a button which we showed, then pick a default, if we can
            if (this.DefaultButton == null)
            {
                if (config.DefaultButton == MessageBoxResult.None && this.ButtonList.Any())
                    this.DefaultButton = buttonList[0];
                else
                    throw new ArgumentException("DefaultButton set to a button which doesn't appear in Buttons");
            }
            if (this.CancelButton == null)
            {
                if (config.CancelButton == MessageBoxResult.None && this.ButtonList.Any())
                    this.CancelButton = buttonList.Last();
                else
                    throw new ArgumentException("CancelButton set to a button which doesn't appear in Buttons");
            }

            this.FlowDirection = config.Options.HasFlag(MessageBoxOptions.RtlReading) ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            this.TextAlignment = (config.Options.HasFlag(MessageBoxOptions.RightAlign) == config.Options.HasFlag(MessageBoxOptions.RtlReading)) ? TextAlignment.Left : TextAlignment.Right;
        }

        /// <summary>
        /// List of buttons which are shown in the View.
        /// </summary>
        public IEnumerable<LabelledValue<MessageBoxResult>> ButtonList { get; protected set; }

        /// <summary>
        /// Item in ButtonList which is the Default button
        /// </summary>
        public LabelledValue<MessageBoxResult> DefaultButton { get; protected set; }

        /// <summary>
        /// Item in ButtonList which is the Cancel button
        /// </summary>
        public LabelledValue<MessageBoxResult> CancelButton { get; protected set; }      

        /// <summary>
        /// Text which is shown in the body of the MessageBox
        /// </summary>
        public virtual string Text { get; protected set; }

        /// <summary>
        /// Icon which the user specified
        /// </summary>
        public virtual MessageBoxImage Icon { get; protected set; }

        /// <summary>
        /// Icon which is shown next to the text in the View
        /// </summary>
        public virtual Icon ImageIcon
        {
            get { return IconMapping[this.Icon]; }
        }

        /// <summary>
        /// Which way the document should flow
        /// </summary>
        public virtual FlowDirection FlowDirection { get; protected set; }

        /// <summary>
        /// Text alignment of the message
        /// </summary>
        public virtual TextAlignment TextAlignment { get; protected set; }

        /// <summary>
        /// Which button the user clicked, once they've clicked a button
        /// </summary>
        public virtual MessageBoxResult ClickedButton { get; protected set; }

        /// <summary>
        /// When the View loads, play a sound if appropriate
        /// </summary>
        protected override void OnViewLoaded()
        {
            // There might not be a sound, or it might be null
            SystemSound sound;
            SoundMapping.TryGetValue(this.Icon, out sound);
            if (sound != null)
                sound.Play();
        }

        /// <summary>
        /// Called when MessageBoxView when the user clicks a button
        /// </summary>
        /// <param name="button">Button which was clicked</param>
        public void ButtonClicked(MessageBoxResult button)
        {
            this.ClickedButton = button;
            this.TryClose(true);
        }
    }
}
