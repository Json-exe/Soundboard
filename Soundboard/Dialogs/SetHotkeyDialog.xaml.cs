using System;
using System.Windows;
using System.Windows.Input;
using Soundboard.Classes;
using Soundboard.Codes;

namespace Soundboard.Dialogs;

public partial class SetHotkeyDialog : Window
{
    private readonly SoundClass _sound;
    private string _selectedHotkey = "";

    public SetHotkeyDialog(SoundClass sound)
    {
        _sound = sound;
        InitializeComponent();
        CurrentHotkey.Content = _sound.HotKey;
    }

    private void SaveHotkey_OnClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_selectedHotkey))
        {
            MessageBox.Show("Please select a hotkey!", "Warning!", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        var serviceProvider = (IServiceProvider)Application.Current.Resources["ServiceProvider"];
        var systemHandler = (SystemHandler)serviceProvider.GetService(typeof(SystemHandler))!;
        systemHandler.DialogBlockingGrid.Visibility = Visibility.Collapsed;
        // Save the hotkey to the sound inside the system handler
        var sound = systemHandler.Sounds.Find(x => x.Name == _sound.Name);
        if (sound != null) sound.HotKey = _selectedHotkey;
        Close();
    }

    private void ShortcutTextBox_OnKeyDown(object sender, KeyEventArgs e)
    {
        var hotkey = "";
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            hotkey += "Ctrl + ";
        }

        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
        {
            hotkey += "Shift + ";
        }

        hotkey += e.Key.ToString();

        ShortcutTextBox.Text = hotkey;

        _selectedHotkey = hotkey;

        e.Handled = true;
    }

    private void Exit_OnClick(object sender, RoutedEventArgs e)
    {
        var serviceProvider = (IServiceProvider)Application.Current.Resources["ServiceProvider"];
        var systemHandler = (SystemHandler)serviceProvider.GetService(typeof(SystemHandler))!;
        systemHandler.DialogBlockingGrid.Visibility = Visibility.Collapsed;
        Close();
    }
}