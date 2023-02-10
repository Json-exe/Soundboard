using System.Windows;
using System.Windows.Controls;

namespace Soundboard.Components;

public partial class Navigation : Page
{
    public Navigation()
    {
        InitializeComponent();
    }

    private void Button_AddSoundClick(object sender, RoutedEventArgs e)
    {
        contentFrame.NavigationService.Navigate(new Pages.AddSound());
    }

    private void Button_SoundsClick(object sender, RoutedEventArgs e)
    {
        contentFrame.NavigationService.Navigate(new Pages.Sounds());
    }
}