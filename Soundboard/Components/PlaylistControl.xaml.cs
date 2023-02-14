using System;
using System.Windows;
using System.Windows.Controls;
using Soundboard.Codes;
using Soundboard.Pages;

namespace Soundboard.Components;

public partial class PlaylistControl : UserControl
{
    public static readonly DependencyProperty PlaylistTitleProperty = DependencyProperty.Register(
        nameof(PlaylistTitle), typeof(string), typeof(PlaylistControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty PlaylistCountProperty = DependencyProperty.Register(
        nameof(PlaylistCount), typeof(int), typeof(PlaylistControl), new PropertyMetadata(default(int)));

    public PlaylistControl(string playlistDelimeter)
    {
        InitializeComponent();
        PlaylistDelimeter = playlistDelimeter;
    }

    private string PlaylistDelimeter { get; }

    public string PlaylistTitle
    {
        get => (string)GetValue(PlaylistTitleProperty);
        set => SetValue(PlaylistTitleProperty, value);
    }

    public int PlaylistCount
    {
        get => (int)GetValue(PlaylistCountProperty);
        set => SetValue(PlaylistCountProperty, value);
    }

    private void OpenPlaylist_OnClick(object sender, RoutedEventArgs e)
    {
        var serviceProvider = (IServiceProvider)Application.Current.Resources["ServiceProvider"];
        var systemHandler = (SystemHandler)serviceProvider.GetService(typeof(SystemHandler))!;
        systemHandler.Navigation.contentFrame.Navigate(new Sounds(PlaylistDelimeter));
    }
}