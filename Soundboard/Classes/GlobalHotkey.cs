using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Input;
using Soundboard.Codes;

namespace Soundboard.Classes;

public class GlobalHotkey
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;
    private readonly nint _hookHandle;
    private readonly HookProc _hookProc;
    private readonly SystemHandler _systemHandler;

    public GlobalHotkey(SystemHandler systemHandler)
    {
        _systemHandler = systemHandler;
        _hookProc = HookCallback;
        _hookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _hookProc, GetModuleHandle(nint.Zero), 0);
    }

    private nint HookCallback(int nCode, nint wParam, nint lParam)
    {
        if (Properties.Settings.Default.ActivateHotkeys && (nCode < 0 || wParam is not (WM_KEYDOWN or WM_SYSKEYDOWN)))
            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        var vkCode = Marshal.ReadInt32(lParam);
        var hotKeyConverter = new KeysConverter();
        foreach (var sound in _systemHandler.Sounds)
        {
            // Remove all spaces from the hotkey
            sound.HotKey = sound.HotKey.Replace(" ", "");
            // If the hotkey contains a +, it's a combination of keys (e.g. Ctrl + F) or (e.g. Ctrl + Shift + F)
            if (sound.HotKey.Contains('+'))
            {
                // Split the hotkey into a list of keys
                var hotKeyList = sound.HotKey.Split("+").ToList();
                // Remove the last key from the list
                var lastKey = hotKeyList[^1];
                hotKeyList.RemoveAt(hotKeyList.Count - 1);
                // Convert the last key to a Keys enum
                var lastKeyEnum = (Keys)(hotKeyConverter.ConvertFromString(lastKey) ?? Keys.None);
                // If the last key is not the key that was pressed, continue
                if ((int)lastKeyEnum != vkCode)
                    continue;
                // If the last key is the key that was pressed, check if the other keys are pressed
                var otherKeysPressed = true;
                foreach (var hotKey in hotKeyList)
                    if (hotKey == "Ctrl")
                    {
                        if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                        {
                            otherKeysPressed = false;
                            break;
                        }
                    }
                    else if (hotKey == "Shift")
                    {
                        if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                        {
                            otherKeysPressed = false;
                            break;
                        }
                    }
                    else if (hotKey == "Alt")
                    {
                        if (!Keyboard.IsKeyDown(Key.LeftAlt) && !Keyboard.IsKeyDown(Key.RightAlt))
                        {
                            otherKeysPressed = false;
                            break;
                        }
                    }
                    else
                    {
                        // Convert the key to a Keys enum
                        var hotKeyEnum = (Key)(hotKeyConverter.ConvertFromString(hotKey) ?? Key.None);
                        // If the key is not pressed, set otherKeysPressed to false and break the loop
                        if (!Keyboard.IsKeyDown(hotKeyEnum))
                        {
                            otherKeysPressed = false;
                            break;
                        }
                    }

                // If all keys are pressed, play the sound
                if (otherKeysPressed) sound.Play();
            }
            else
            {
                // If the hotkey does not contain a +, it's a single key
                var hotKey = (Keys)(hotKeyConverter.ConvertFromString(sound.HotKey) ?? Keys.None);
                if ((int)hotKey == vkCode) sound.Play();
            }

            // var hotKey = (Keys)(hotKeyConverter.ConvertFromString(sound.HotKey) ?? Keys.None);
            // if ((int)hotKey == vkCode)
            // {
            //     sound.Play();
            // }
        }

        return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern nint SetWindowsHookEx(int idHook, HookProc lpfn, nint hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(nint hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern nint CallNextHookEx(nint hhk, int nCode, nint wParam, nint lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern nint GetModuleHandle(nint lpModuleName);

    public void Dispose()
    {
        UnhookWindowsHookEx(_hookHandle);
    }

    private delegate nint HookProc(int nCode, nint wParam, nint lParam);
}