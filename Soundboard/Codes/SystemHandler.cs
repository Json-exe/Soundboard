using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using Soundboard.Classes;
using Soundboard.Components;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;

namespace Soundboard.Codes;

public class SystemHandler
{
    public readonly GlobalHotkey GlobalHotkey;
    public readonly List<IWavePlayer> PlayingSounds = new();
    public Grid DialogBlockingGrid = new();
    public Navigation? Navigation;

    public readonly Notifier Notifier = new(cfg =>
    {
        cfg.PositionProvider = new WindowPositionProvider(
            Application.Current.MainWindow,
            Corner.TopRight,
            10,
            10);

        cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
            TimeSpan.FromSeconds(5),
            MaximumNotificationCount.FromCount(5));

        cfg.Dispatcher = Application.Current.Dispatcher;
    });

    public MMDevice? SelectedAudioDevice;
    public List<SoundClass> Sounds = new();

    public SystemHandler()
    {
        GlobalHotkey = new GlobalHotkey(this);
    }
}