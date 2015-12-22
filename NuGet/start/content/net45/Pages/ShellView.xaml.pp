<Window x:Class="$rootnamespace$.Pages.ShellView"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:$rootnamespace$.Pages"
        xmlns:s="https://github.com/canton7/Stylet"
        mc:Ignorable="d"
        d:DataContext="{d:DesignInstance local:ShellView}"
        Title="Stylet Start Project"
        Width="350" Height="200">
    <StackPanel VerticalAlignment="Center">
        <TextBlock FontSize="30"
                   HorizontalAlignment="Center">
            Hello Stylet!
        </TextBlock>
        <TextBlock Margin="0,20,0,0"
                   FontSize="20"
                   HorizontalAlignment="Center">
            Now delete MainWindow.xaml.
        </TextBlock>
    </StackPanel>
</Window>
