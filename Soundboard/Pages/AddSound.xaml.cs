using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Newtonsoft.Json;
using NLog;
using Soundboard.Classes;
using Soundboard.Codes;

namespace Soundboard.Pages;

public partial class AddSound : Page
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private readonly string _dataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                                        "\\JDS\\Soundboard\\Data";

    public AddSound()
    {
        InitializeComponent();
    }

    private void AddButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(NameBox.Text) || string.IsNullOrEmpty(PathBox.Text))
        {
            MessageBox.Show("Please fill in all fields!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var sound = new SoundClass
        {
            Name = NameBox.Text,
            PathToFile = PathBox.Text,
            Volume = Properties.Settings.Default.StandardVolume,
            Playlist = PlaylistBox.Text,
            Loop = false
        };
        var serviceProvider = (IServiceProvider)Application.Current.Resources["ServiceProvider"];
        var systemHandler = (SystemHandler)serviceProvider.GetService(typeof(SystemHandler))!;
        systemHandler.Sounds.Add(sound);
        var json = JsonConvert.SerializeObject(systemHandler.Sounds, Formatting.Indented);
        File.WriteAllText(_dataPath + "\\data.json", json);
        Log.Info("Added sound " + sound.Name);
        NavigationService?.Navigate(new Sounds());
    }

    private void BrowseButton_OnClick(object sender, RoutedEventArgs e)
    {
        // Open a file dialog to select a mp3 file
        // Set the file path to the file path text box
        var dialog = new OpenFileDialog
        {
            Filter = "MP3 Files (*.mp3)|*.mp3",
            InitialDirectory = "C:\\",
            Title = "Select a sound file"
        };
        var result = dialog.ShowDialog();
        if (result != true) return;
        // Check if the file exists
        if (File.Exists(dialog.FileName))
            // Set the file path to the file path text box
            PathBox.Text = dialog.FileName;
        else
            // Show an error message
            MessageBox.Show("The file does not exist!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}