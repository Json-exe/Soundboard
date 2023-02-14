using System;
using System.Windows;
using System.Windows.Controls;
using Soundboard.Codes;

namespace Soundboard.Components;

public partial class PlaylistControl : UserControl
{
    private string PlaylistDelimeter { get; set; }

    public PlaylistControl(string playlistDelimeter)
    {
        InitializeComponent();
        PlaylistDelimeter = playlistDelimeter;
    }

    public static readonly DependencyProperty PlaylistTitleProperty = DependencyProperty.Register(
        nameof(PlaylistTitle), typeof(string), typeof(PlaylistControl), new PropertyMetadata(default(string)));

    public string PlaylistTitle
    {
        get => (string)GetValue(PlaylistTitleProperty);
        set => SetValue(PlaylistTitleProperty, value);
    }

    public static readonly DependencyProperty PlaylistCountProperty = DependencyProperty.Register(
        nameof(PlaylistCount), typeof(int), typeof(PlaylistControl), new PropertyMetadata(default(int)));

    public int PlaylistCount
    {
        get => (int)GetValue(PlaylistCountProperty);
        set => SetValue(PlaylistCountProperty, value);
    }

    private void OpenPlaylist_OnClick(object sender, RoutedEventArgs e)
    {
        var serviceProvider = (IServiceProvider)Application.Current.Resources["ServiceProvider"];
        var systemHandler = (SystemHandler)serviceProvider.GetService(typeof(SystemHandler))!;
        systemHandler.Navigation.contentFrame.Navigate(new Pages.Sounds(PlaylistDelimeter));
    }
}