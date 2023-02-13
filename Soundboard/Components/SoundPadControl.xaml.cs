using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using Newtonsoft.Json;
using NLog;
using Soundboard.Classes;
using Soundboard.Codes;

namespace Soundboard.Components;

public partial class SoundPadControl : UserControl
{
    private WaveOutEvent _waveOut;
    private Mp3FileReader _reader;
    private readonly string _dataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                                        "\\JDS\\Soundboard\\Data";
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    
    public SoundPadControl(SoundClass sound)
    {
        InitializeComponent();
        LoopCheckBox.IsChecked = sound.Loop;
    }

    public static readonly DependencyProperty SoundNameProperty = DependencyProperty.Register(
        nameof(SoundName), typeof(string), typeof(SoundPadControl), new PropertyMetadata(default(string)));

    public string SoundName
    {
        get => (string)GetValue(SoundNameProperty);
        init => SetValue(SoundNameProperty, value);
    }

    public static readonly DependencyProperty SoundFilePathProperty = DependencyProperty.Register(
        nameof(SoundFilePath), typeof(string), typeof(SoundPadControl), new PropertyMetadata(default(string)));

    public string SoundFilePath
    {
        get => (string)GetValue(SoundFilePathProperty);
        init => SetValue(SoundFilePathProperty, value);
    }

    public static readonly DependencyProperty SoundVolumeProperty = DependencyProperty.Register(
        nameof(SoundVolume), typeof(double), typeof(SoundPadControl), new PropertyMetadata(default(double)));

    public double SoundVolume
    {
        get => (double)GetValue(SoundVolumeProperty);
        set => SetValue(SoundVolumeProperty, value);
    }

    public static readonly DependencyProperty ThisSoundProperty = DependencyProperty.Register(
        nameof(ThisSound), typeof(SoundClass), typeof(SoundPadControl), new PropertyMetadata(default(SoundClass)));

    public SoundClass ThisSound
    {
        get => (SoundClass)GetValue(ThisSoundProperty);
        init => SetValue(ThisSoundProperty, value);
    }

    private void Button_PlayClick (object sender, RoutedEventArgs e)
    {
        ThisSound.Play();
        
        // var serviceProvider = (IServiceProvider)Application.Current.Resources["ServiceProvider"];
        // var systemHandler = (SystemHandler)serviceProvider.GetService(typeof(SystemHandler))!;
        // Log.Debug("Playing sound: " + SoundName);
        // if (systemHandler.SelectedAudioDevice == null)
        // {
        //     MessageBox.Show("Please select an audio device!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //     return;
        // }
        // // Check if the sound file exists
        // if (File.Exists(SoundFilePath))
        // {
        //     // Check if the sound is already playing
        //     if (_waveOut is { PlaybackState: PlaybackState.Playing })
        //     {
        //         _waveOut.Stop();
        //         _waveOut.Dispose();
        //         _reader.Dispose();
        //     }
        //     // Check if there are already sounds playing
        //     if (systemHandler.PlayingSounds.Count > 0)
        //     {
        //         // Stop all sounds
        //         foreach (var playingSound in systemHandler.PlayingSounds)
        //         {
        //             playingSound.Stop();
        //             playingSound.Dispose();
        //         }
        //         // Clear the list
        //         systemHandler.PlayingSounds.Clear();
        //     }
        //     _reader = new Mp3FileReader(SoundFilePath);
        //     _waveOut = new WaveOutEvent();
        //     _waveOut.DeviceNumber = GetDeviceNumber(systemHandler.SelectedAudioDevice);
        //     _waveOut.Volume = (float)SoundVolume / 100;
        //     _waveOut.Init(_reader);
        //     _waveOut.PlaybackStopped += WaveOutOnPlaybackStopped;
        //     _waveOut.Play();
        //     systemHandler.PlayingSounds.Add(_waveOut);
        // }
        // else
        // {
        //     // Show an error message
        //     MessageBox.Show("The sound file does not exist!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        // }
    }

    private void WaveOutOnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        if (LoopCheckBox.IsChecked == true)
        {
            _reader.Position = 0;
            _waveOut.Play();
            return;
        }
        // Remove the sound from the list of playing sounds
        var serviceProvider = (IServiceProvider)Application.Current.Resources["ServiceProvider"];
        var systemHandler = (SystemHandler)serviceProvider.GetService(typeof(SystemHandler))!;
        systemHandler.PlayingSounds.Remove(_waveOut);
        _waveOut.Dispose();
        _reader.Dispose();
    }

    private static int GetDeviceNumber(MMDevice device)
    {
        // Get the device number of the selected audio device
        var deviceNumber = 0;
        for (var i = 0; i < WaveOut.DeviceCount; i++)
        {
            var capabilities = WaveOut.GetCapabilities(i);
            if (!device.FriendlyName.Contains(capabilities.ProductName) ||
                capabilities.Channels != device.AudioClient.MixFormat.Channels) continue;
            deviceNumber = i;
            break;
        }
        return deviceNumber;
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        EditContextMenu.StaysOpen = true;
        EditContextMenu.IsOpen = true;
    }

    private void DeleteSound_Click(object sender, RoutedEventArgs e)
    {
        // Remove the sound from the data.json file and reload the page
        Log.Info("Deleting sound: " + SoundName);
        // Check if the sound is already playing
        if (_waveOut is { PlaybackState: PlaybackState.Playing })
        {
            _waveOut.Stop();
            _waveOut.Dispose();
            _reader.Dispose();
        }
        var sounds = JsonConvert.DeserializeObject<List<SoundClass>>(File.ReadAllText(_dataPath + "\\data.json")) ?? new List<SoundClass>();
        var sound = sounds.FirstOrDefault(x => x.Name == SoundName);
        if (sound == null) return;
        sounds.Remove(sound);
        File.WriteAllText(_dataPath + "\\data.json", JsonConvert.SerializeObject(sounds));
        var serviceProvider = (IServiceProvider)Application.Current.Resources["ServiceProvider"];
        var systemHandler = (SystemHandler)serviceProvider.GetService(typeof(SystemHandler))!;
        systemHandler.Sounds.Remove(systemHandler.Sounds.FirstOrDefault(x => x.Name == SoundName)!);
        systemHandler.Navigation.contentFrame.Navigate(new Pages.Sounds());
    }

    private void SoundVolume_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        // Change the volume of the sound
        if (_waveOut is { PlaybackState: PlaybackState.Playing })
        {
            _waveOut.Volume = (float)SoundVolume / 100;
        }
        ThisSound.Volume = SoundVolume;
    }

    private void EditSound_OnClick(object sender, RoutedEventArgs e)
    {
        // Open the edit sound page
        var serviceProvider = (IServiceProvider)Application.Current.Resources["ServiceProvider"];
        var systemHandler = (SystemHandler)serviceProvider.GetService(typeof(SystemHandler))!;
        systemHandler.Navigation.contentFrame.Navigate(new Pages.EditSound(ThisSound));
    }

    private void LoopCheckBox_OnClick(object sender, RoutedEventArgs e)
    {
        // Change the loop state of the sound
        ThisSound.Loop = LoopCheckBox.IsChecked == true;
        // Save the sound
        var sounds = JsonConvert.DeserializeObject<List<SoundClass>>(File.ReadAllText(_dataPath + "\\data.json")) ?? new List<SoundClass>();
        var sound = sounds.FirstOrDefault(x => x.Name == SoundName);
        if (sound == null) return;
        sound.Loop = LoopCheckBox.IsChecked == true;
        File.WriteAllText(_dataPath + "\\data.json", JsonConvert.SerializeObject(sounds));
    }
}