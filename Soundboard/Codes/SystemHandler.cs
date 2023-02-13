using System.Collections.Generic;
using NAudio.CoreAudioApi;
using Soundboard.Classes;
using NAudio.Wave;
using Soundboard.Components;

namespace Soundboard.Codes;

public class SystemHandler
{
    public List<SoundClass> Sounds = new();
    public readonly List<IWavePlayer> PlayingSounds = new();
    public MMDevice? SelectedAudioDevice;
    public Navigation? Navigation;
    public readonly GlobalHotkey GlobalHotkey;

    public SystemHandler()
    {
        GlobalHotkey = new GlobalHotkey(this);
    }
}