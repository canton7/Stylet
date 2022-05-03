using Stylet.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace Stylet;

public partial interface IWindowManager
{
    /// <summary>
    /// Display a MessageBox
    /// </summary>
    /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
    /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
    /// <param name="buttons">A <see cref="System.Windows.MessageBoxButton"/> value that specifies which button or buttons to display.</param>
    /// <param name="icon">A <see cref="System.Windows.MessageBoxImage"/> value that specifies the icon to display.</param>
    /// <param name="defaultResult">A <see cref="System.Windows.MessageBoxResult"/> value that specifies the default result of the message box.</param>
    /// <param name="cancelResult">A <see cref="System.Windows.MessageBoxResult"/> value that specifies the cancel result of the message box</param>
    /// <param name="buttonLabels">A dictionary specifying the button labels, if desirable</param>
    /// <param name="flowDirection">The <see cref="System.Windows.FlowDirection"/> to use, overrides the <see cref="MessageBoxViewModel.DefaultFlowDirection"/></param>
    /// <param name="textAlignment">The <see cref="System.Windows.TextAlignment"/> to use, overrides the <see cref="MessageBoxViewModel.DefaultTextAlignment"/></param>
    /// <returns>The result chosen by the user</returns>
    MessageBoxResult ShowMessageBox(string messageBoxText, string caption = "",
        MessageBoxButton buttons = MessageBoxButton.OK,
        MessageBoxImage icon = MessageBoxImage.None,
        MessageBoxResult defaultResult = MessageBoxResult.None,
        MessageBoxResult cancelResult = MessageBoxResult.None,
        IDictionary<MessageBoxResult, string> buttonLabels = null,
        FlowDirection? flowDirection = null,
        TextAlignment? textAlignment = null);
}

public partial class WindowManager
{
    private readonly Func<IMessageBoxViewModel> messageBoxViewModelFactory;

    /// <summary>
    /// Initialises a new instance of the <see cref="WindowManager"/> class, using the given <see cref="IViewManager"/>
    /// </summary>
    /// <param name="viewManager">IViewManager to use when creating views</param>
    /// <param name="messageBoxViewModelFactory">Delegate which returns a new IMessageBoxViewModel instance when invoked</param>
    /// <param name="config">Configuration object</param>
    public WindowManager(IViewManager viewManager, Func<IMessageBoxViewModel> messageBoxViewModelFactory, IWindowManagerConfig config)
        : this(viewManager, config)
    {
        this.messageBoxViewModelFactory = messageBoxViewModelFactory;
    }

    /// <summary>
    /// Display a MessageBox
    /// </summary>
    /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
    /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
    /// <param name="buttons">A <see cref="System.Windows.MessageBoxButton"/> value that specifies which button or buttons to display.</param>
    /// <param name="icon">A <see cref="System.Windows.MessageBoxImage"/> value that specifies the icon to display.</param>
    /// <param name="defaultResult">A <see cref="System.Windows.MessageBoxResult"/> value that specifies the default result of the message box.</param>
    /// <param name="cancelResult">A <see cref="System.Windows.MessageBoxResult"/> value that specifies the cancel result of the message box</param>
    /// <param name="buttonLabels">A dictionary specifying the button labels, if desirable</param>
    /// <param name="flowDirection">The <see cref="System.Windows.FlowDirection"/> to use, overrides the <see cref="MessageBoxViewModel.DefaultFlowDirection"/></param>
    /// <param name="textAlignment">The <see cref="System.Windows.TextAlignment"/> to use, overrides the <see cref="MessageBoxViewModel.DefaultTextAlignment"/></param>
    /// <returns>The result chosen by the user</returns>
    public MessageBoxResult ShowMessageBox(string messageBoxText, string caption = "",
        MessageBoxButton buttons = MessageBoxButton.OK,
        MessageBoxImage icon = MessageBoxImage.None,
        MessageBoxResult defaultResult = MessageBoxResult.None,
        MessageBoxResult cancelResult = MessageBoxResult.None,
        IDictionary<MessageBoxResult, string> buttonLabels = null,
        FlowDirection? flowDirection = null,
        TextAlignment? textAlignment = null)
    {
        IMessageBoxViewModel vm = this.messageBoxViewModelFactory();
        vm.Setup(messageBoxText, caption, buttons, icon, defaultResult, cancelResult, buttonLabels, flowDirection, textAlignment);
        this.ShowDialog(vm);
        return vm.ClickedButton;
    }
}
