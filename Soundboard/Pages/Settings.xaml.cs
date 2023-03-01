using System;
using System.Windows;
using System.Windows.Controls;
using NAudio.CoreAudioApi;
using Soundboard.Codes;

namespace Soundboard.Pages;

public partial class Settings : Page
{
    private readonly SystemHandler _systemHandler;

    public Settings()
    {
        InitializeComponent();
        var serviceProvider = (IServiceProvider)Application.Current.Resources["ServiceProvider"];
        _systemHandler = (SystemHandler)serviceProvider.GetService(typeof(SystemHandler))!;
        ActivateHotkeys.IsChecked = Properties.Settings.Default.ActivateHotkeys;
    }

    private void Settings_OnLoaded(object sender, RoutedEventArgs e)
    {
        // Enumerate the audio devices and add them to the combo box
        var deviceEnumerator = new MMDeviceEnumerator();
        foreach (var device in deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            AudioDevices.Items.Add(device);

        if (_systemHandler.SelectedAudioDevice == null) return;
        {
            foreach (var item in AudioDevices.Items)
            {
                if (item is not MMDevice device || device.ID != _systemHandler.SelectedAudioDevice.ID) continue;
                AudioDevices.SelectedItem = item;
                break;
            }
        }
        StandardVolumeSlider.Value = Properties.Settings.Default.StandardVolume;
    }

    private void AudioDevices_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _systemHandler.SelectedAudioDevice = (MMDevice)AudioDevices.SelectedItem;
        // Save the ID of the selected audio device to the settings file
        Properties.Settings.Default.StandardAudioDeviceID = _systemHandler.SelectedAudioDevice.ID;
        Properties.Settings.Default.Save();
    }

    private void StandardVolumeSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        Properties.Settings.Default.StandardVolume = StandardVolumeSlider.Value;
        Properties.Settings.Default.Save();
    }

    private void ActivateHotkeys_OnClick(object sender, RoutedEventArgs e)
    {
        Properties.Settings.Default.ActivateHotkeys = !Properties.Settings.Default.ActivateHotkeys;
        Properties.Settings.Default.Save();
    }
}