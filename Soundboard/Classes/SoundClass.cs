using System;
using System.IO;
using System.Windows;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NLog;
using Soundboard.Codes;

namespace Soundboard.Classes;

public class SoundClass
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    private readonly WaveOutEvent _waveOut = new();
    private Mp3FileReader? _reader;

    public string Name { get; set; } = "";
    public string PathToFile { get; set; } = "";
    public double Volume { get; set; } = 100;
    public string Playlist { get; set; } = "";
    public bool Loop { get; set; }
    public string HotKey { get; set; } = "";

    public void SetVolume(double newVolume)
    {
        // Change the volume of the sound
        if (_waveOut is { PlaybackState: PlaybackState.Playing }) _waveOut.Volume = (float)Volume / 100;

        Volume = newVolume;
    }

    public void Play()
    {
        var serviceProvider = (IServiceProvider)Application.Current.Resources["ServiceProvider"];
        var systemHandler = (SystemHandler)serviceProvider.GetService(typeof(SystemHandler))!;
        Log.Debug("Playing sound: " + Name);
        if (systemHandler.SelectedAudioDevice == null)
        {
            MessageBox.Show("Please select an audio device!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Check if the sound file exists
        if (File.Exists(PathToFile))
        {
            // Check if the sound is already playing
            if (_waveOut is { PlaybackState: PlaybackState.Playing })
            {
                _waveOut.Stop();
                _reader?.Close();
            }

            // Check if there are already sounds playing
            if (systemHandler.PlayingSounds.Count > 0)
            {
                // Stop all sounds
                foreach (var playingSound in systemHandler.PlayingSounds)
                {
                    playingSound.Stop();
                    playingSound.Dispose();
                }

                // Clear the list
                systemHandler.PlayingSounds.Clear();
            }

            _reader = new Mp3FileReader(PathToFile);
            _waveOut.DeviceNumber = GetDeviceNumber(systemHandler.SelectedAudioDevice);
            _waveOut.Volume = (float)Volume / 100;
            _waveOut.Init(_reader);
            _waveOut.PlaybackStopped += WaveOutOnPlaybackStopped;
            _waveOut.Play();
            systemHandler.PlayingSounds.Add(_waveOut);
        }
        else
        {
            // Show an error message
            MessageBox.Show("The sound file does not exist!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void StopSound()
    {
        if (_waveOut is not { PlaybackState: PlaybackState.Playing }) return;
        _waveOut.Stop();
        _reader?.Close();
    }

    private void WaveOutOnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        if (Loop)
        {
            if (_reader != null) _reader.Position = 0;
            _waveOut.Play();
            return;
        }

        // Remove the sound from the list of playing sounds
        var serviceProvider = (IServiceProvider)Application.Current.Resources["ServiceProvider"];
        var systemHandler = (SystemHandler)serviceProvider.GetService(typeof(SystemHandler))!;
        systemHandler.PlayingSounds.Remove(_waveOut);
        if (_waveOut is not { PlaybackState: PlaybackState.Playing }) return;
        _waveOut.Stop();
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
}