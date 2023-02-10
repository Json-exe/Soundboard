using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using NLog;
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
        services.AddSingleton<SystemHandler>();
        var serviceProvider = services.BuildServiceProvider();
        // Store the service provider in the application's resources
        Application.Current.Resources["ServiceProvider"] = serviceProvider;

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
        }
        else
        {
            Log.Info("Data file does not exist! Creating...");
            File.Create(dataPath + "\\data.json");
            Log.Debug("Data file created");
        }

        mainFrame.NavigationService.Navigate(new Navigation());
    }
}