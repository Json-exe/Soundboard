using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using Microsoft.Extensions.DependencyInjection;
using NAudio.CoreAudioApi;
using Newtonsoft.Json;
using NLog;
using Soundboard.Classes;
using Soundboard.Codes;
using Soundboard.Components;

namespace Soundboard;

public partial class MainWindow : Window
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public MainWindow()
    {
        InitializeComponent();
    }

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        // Build the service provider
        var services = new ServiceCollection();
        services.AddSingleton(typeof(SystemHandler));
        var serviceProvider = services.BuildServiceProvider();
        // Store the service provider in the application's resources
        Application.Current.Resources["ServiceProvider"] = serviceProvider;
        var systemHandler = serviceProvider.GetService<SystemHandler>()!;
        systemHandler.DialogBlockingGrid = DialogBlockingGrid;

        // Here comes initial startup code
        Log.Info("Application started");
        var dataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                       "\\JDS\\Soundboard\\Data";
        if (Directory.Exists(dataPath))
        {
            Log.Debug("Data directory exists");
        }
        else
        {
            Log.Info("Data directory does not exist! Creating...");
            Directory.CreateDirectory(dataPath);
            Log.Debug("Data directory created");
        }

        if (File.Exists(dataPath + "\\data.json"))
        {
            Log.Debug("Data file exists");
            var sounds = JsonConvert.DeserializeObject<List<SoundClass>>(File.ReadAllText(dataPath + "\\data.json")) ??
                         new List<SoundClass>();
            systemHandler.Sounds = sounds;
        }
        else
        {
            Log.Info("Data file does not exist! Creating...");
            File.Create(dataPath + "\\data.json");
            Log.Debug("Data file created");
        }

        if (!string.IsNullOrEmpty(Properties.Settings.Default.StandardAudioDeviceID))
        {
            // Enumerate the audio devices and add them to the combo box
            var deviceEnumerator = new MMDeviceEnumerator();
            foreach (var device in deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                if (device.ID != Properties.Settings.Default.StandardAudioDeviceID) continue;
                systemHandler.SelectedAudioDevice = device;
                break;
            }

            // If the selected audio device is not found clear the ID from the settings file
            if (systemHandler.SelectedAudioDevice == null)
            {
                Properties.Settings.Default.StandardAudioDeviceID = string.Empty;
                Properties.Settings.Default.Save();
            }
        }

        MainFrame.NavigationService.Navigate(new Navigation());
    }

    private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        // Save all Sounds
        Log.Info("Saving data");
        var serviceProvider = (IServiceProvider)Application.Current.Resources["ServiceProvider"];
        var systemHandler = (SystemHandler)serviceProvider.GetService(typeof(SystemHandler))!;
        systemHandler.GlobalHotkey.Dispose();
        var dataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                       "\\JDS\\Soundboard\\Data";
        var sounds = systemHandler.Sounds;
        File.WriteAllText(dataPath + "\\data.json", JsonConvert.SerializeObject(sounds, Formatting.Indented));
        Log.Info("Application closed");
    }
}