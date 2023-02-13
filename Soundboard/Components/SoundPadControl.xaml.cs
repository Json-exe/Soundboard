using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
        systemHandler.Navigation?.contentFrame.Navigate(new Pages.Sounds());
    }

    private void SoundVolume_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        ThisSound.SetVolume(SoundVolume);
    }

    private void EditSound_OnClick(object sender, RoutedEventArgs e)
    {
        // Open the edit sound page
        var serviceProvider = (IServiceProvider)Application.Current.Resources["ServiceProvider"];
        var systemHandler = (SystemHandler)serviceProvider.GetService(typeof(SystemHandler))!;
        systemHandler.Navigation?.contentFrame.Navigate(new Pages.EditSound(ThisSound));
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