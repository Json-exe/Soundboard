using System;
using System.Windows;
using System.Windows.Controls;
using Soundboard.Codes;
using Soundboard.Pages;

namespace Soundboard.Components;

public partial class Navigation : Page
{
    public Navigation()
    {
        InitializeComponent();
        contentFrame.NavigationService.Navigate(new Sounds());
        var serviceProvider = (IServiceProvider)Application.Current.Resources["ServiceProvider"];
        var systemHandler = (SystemHandler)serviceProvider.GetService(typeof(SystemHandler))!;
        systemHandler.Navigation = this;
    }

    private void Button_AddSoundClick(object sender, RoutedEventArgs e)
    {
        contentFrame.NavigationService.Navigate(new AddSound());
    }

    private void Button_SoundsClick(object sender, RoutedEventArgs e)
    {
        contentFrame.NavigationService.Navigate(new Sounds());
    }

    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
        // Stop all currently playing sounds that are being played by the music player
        var serviceProvider = (IServiceProvider)Application.Current.Resources["ServiceProvider"];
        var systemHandler = (SystemHandler)serviceProvider.GetService(typeof(SystemHandler))!;
        foreach (var playingSound in systemHandler.PlayingSounds)
        {
            playingSound.Stop();
            playingSound.Dispose();
        }

        systemHandler.PlayingSounds.Clear();
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        contentFrame.NavigationService.Navigate(new Settings());
    }
}