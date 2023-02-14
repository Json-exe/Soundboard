using System;
using System.Windows;
using System.Windows.Controls;
using NLog;
using Soundboard.Classes;
using Soundboard.Codes;

namespace Soundboard.Pages;

public partial class EditSound : Page
{
    private readonly string _dataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                                        "\\JDS\\Soundboard\\Data";

    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    private SoundClass _sound;

    public EditSound(SoundClass sound)
    {
        InitializeComponent();
        Title.Content = Title.Content.ToString()?.Replace("$Sound", sound.Name);
        _sound = sound;
        PathBox.Text = sound.PathToFile;
        VolumeSlider.Value = sound.Volume;
        NameBox.Text = sound.Name;
    }

    private void BrowseButton_OnClick(object sender, RoutedEventArgs e)
    {
        // Open a file dialog to select a mp3 file
        // Set the file path to the file path text box
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "MP3 Files (*.mp3)|*.mp3",
            InitialDirectory = "C:\\",
            Title = "Select a sound file"
        };
        var result = dialog.ShowDialog();
        if (result != true) return;
        // Check if the file exists
        if (System.IO.File.Exists(dialog.FileName))
        {
            // Set the file path to the file path text box
            PathBox.Text = dialog.FileName;
        }
        else
        {
            // Show an error message
            MessageBox.Show("The file does not exist!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SaveButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(NameBox.Text) || string.IsNullOrEmpty(PathBox.Text))
        {
            MessageBox.Show("Please fill in all fields!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var serviceProvider = (IServiceProvider)Application.Current.Resources["ServiceProvider"];
        var systemHandler = (SystemHandler)serviceProvider.GetService(typeof(SystemHandler))!;
        systemHandler.Sounds.Remove(systemHandler.Sounds.Find(x => x.Name == _sound.Name)!);
        _sound.Name = NameBox.Text;
        _sound.PathToFile = PathBox.Text;
        _sound.Volume = VolumeSlider.Value;
        systemHandler.Sounds.Add(_sound);
        Log.Info("Saved sound " + _sound.Name);
        NavigationService?.Navigate(new Sounds());
    }
}