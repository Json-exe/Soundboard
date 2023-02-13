using System.Linq;
using System.Runtime.InteropServices;
using Soundboard.Codes;

namespace Soundboard.Classes;

public class GlobalHotkey
{
    private readonly nint _hookHandle;
    private readonly HookProc _hookProc;
    private readonly SystemHandler _systemHandler;
    
    public GlobalHotkey(SystemHandler systemHandler)
    {
        _systemHandler = systemHandler;
        _hookProc = HookCallback;
        _hookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _hookProc, GetModuleHandle(nint.Zero), 0);
    }
    
    private delegate nint HookProc(int nCode, nint wParam, nint lParam);

    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;

    private nint HookCallback(int nCode, nint wParam, nint lParam)
    {
        if (nCode < 0 || wParam is not (WM_KEYDOWN or WM_SYSKEYDOWN))
            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        var vkCode = Marshal.ReadInt32(lParam);
        foreach (var sound in _systemHandler.Sounds.Where(sound => (int)sound.ShortcutKey == vkCode))
        {
            sound.Play();
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
}