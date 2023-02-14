using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Soundboard.Codes;
using Soundboard.Components;

namespace Soundboard.Pages;

public partial class Sounds : Page
{
    public Sounds()
    {
        InitializeComponent();
        var serviceProvider = (IServiceProvider)Application.Current.Resources["ServiceProvider"];
        var systemHandler = (SystemHandler)serviceProvider.GetService(typeof(SystemHandler))!;
        foreach (var sound in systemHandler.Sounds)
        {
            if (!string.IsNullOrEmpty(sound.Playlist))
            {
                var playlistControl = new PlaylistControl(sound.Playlist)
                {
                    PlaylistTitle = sound.Playlist,
                    PlaylistCount = systemHandler.Sounds.Count(x => x.Playlist == sound.Playlist)
                };
                SoundPad.Children.Add(playlistControl);
            }
            else
            {
                var soundPadControl = new SoundPadControl(sound)
                {
                    ThisSound = sound,
                    SoundName = sound.Name,
                    SoundFilePath = sound.PathToFile,
                    SoundVolume = sound.Volume,
                };
                SoundPad.Children.Add(soundPadControl);
            }
        }
    }

    public Sounds(string PlaylistDelimeter)
    {
        InitializeComponent();
        var serviceProvider = (IServiceProvider)Application.Current.Resources["ServiceProvider"];
        var systemHandler = (SystemHandler)serviceProvider.GetService(typeof(SystemHandler))!;
        foreach (var sound in systemHandler.Sounds.Where(x => x.Playlist == PlaylistDelimeter))
        {
            var soundPadControl = new SoundPadControl(sound)
            {
                ThisSound = sound,
                SoundName = sound.Name,
                SoundFilePath = sound.PathToFile,
                SoundVolume = sound.Volume,
            };
            SoundPad.Children.Add(soundPadControl);
        }
    }

    private void Sounds_OnUnloaded(object sender, RoutedEventArgs e)
    {
        SoundPad.Children.Clear();
    }
}