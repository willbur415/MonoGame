// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Text;
using System.Runtime.InteropServices;

internal static class SDL
{
    private const string nativeLibName = "SDL2.dll";

    private unsafe static string GetString (IntPtr handle)
    {
        if (handle == IntPtr.Zero)
            return "";

        var ptr = (byte*)handle;
        while (*ptr != 0)
            ptr++;

        var bytes = new byte [ptr - (byte*)handle];
        Marshal.Copy (handle, bytes, 0, bytes.Length);

        return Encoding.UTF8.GetString (bytes);
    }

    [Flags]
    public enum InitFlags
    {
        Video = 0x00000020,
        Joystick = 0x00000200,
        Haptic = 0x00001000,
        GameController = 0x00002000,
    }

    public enum EventType
    {
        Quit = 0x100,
        WindowEvent = 0x200,
        KeyDown = 0x300,
        KeyUp = 0x301,
        TextInput = 0x303,
        JoyDeviceAdded = 0x605,
        JoyDeviceRemoved = 0x606,
        MouseWheel = 0x403,
    }

    [StructLayout (LayoutKind.Explicit)]
    public struct Event
    {
        [FieldOffset (0)]
        public EventType Type;
        [FieldOffset (0)]
        public Window.Event Window;
        [FieldOffset (0)]
        public Keyboard.Event Key;
        [FieldOffset (0)]
        public Mouse.WheelEvent Wheel;
        [FieldOffset (0)]
        public Joystick.DeviceEvent JoystickDevice;
    }

    public struct Rectangle
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
    }

    public struct Version
    {
        public byte Major;
        public byte Minor;
        public byte Patch;
    }

    [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_Init")]
    public static extern int SDL_Init (int flags);

    public static void Init (int flags)
    {
        GetError (SDL_Init (flags));
    }

    [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_DisableScreenSaver")]
    public static extern void DisableScreenSaver ();

    [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetVersion")]
    public static extern void GetVersion (out Version version);

    [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_PollEvent")]
    public static extern int PollEvent (out Event _event);

    [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetError")]
    private static extern IntPtr SDL_GetError ();

    public static string GetError ()
    {
        return GetString (SDL_GetError ());
    }

    public static int GetError (int value)
    {
        if (value < 0)
            throw new Exception (GetError ());

        return value;
    }

    public static IntPtr GetError (IntPtr pointer)
    {
        if (pointer == IntPtr.Zero)
            throw new Exception (GetError ());

        return pointer;
    }

    [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetHint")]
    public static extern IntPtr SDL_GetHint (string name);

    public static string GetHint (string name)
    {
        return GetString (SDL_GetHint (name));
    }

    [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_LoadBMP_RW")]
    private static extern IntPtr SDL_LoadBMP_RW (IntPtr src, int freesrc);

    public static IntPtr LoadBMP_RW (IntPtr src, int freesrc)
    {
        return GetError (SDL_LoadBMP_RW (src, freesrc));
    }

    [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_Quit")]
    public static extern void Quit ();

    [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_RWFromMem")]
    private static extern IntPtr SDL_RWFromMem (byte [] mem, int size);

    public static IntPtr RWFromMem (byte [] mem, int size)
    {
        return GetError (SDL_RWFromMem (mem, size));
    }

    [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_SetHint")]
    public static extern int SetHint (string name, string value);

    public static class Window
    {
        public const int PosCentered = 0x2FFF0000;

        public enum EventID : byte
        {
            None,
            Shown,
            Hidden,
            Exposed,
            Moved,
            Resized,
            SizeChanged,
            Minimized,
            Maximized,
            Restored,
            Enter,
            Leave,
            FocusGained,
            FocusLost,
            Close,
        }

        [Flags]
        public enum State
        {
            Fullscreen = 0x00000001,
            OpenGL = 0x00000002,
            Shown = 0x00000004,
            Hidden = 0x00000008,
            Boderless = 0x00000010,
            Resizable = 0x00000020,
            Minimized = 0x00000040,
            Maximized = 0x00000080,
            Grabbed = 0x00000100,
            InputFocus = 0x00000200,
            MouseFocus = 0x00000400,
            FullscreenDesktop = 0x00001001,
            Foreign = 0x00000800,
            AllowHighDPI = 0x00002000,
            MouseCapture = 0x00004000,
        }

        [StructLayout (LayoutKind.Sequential)]
        public struct Event
        {
            public EventType Type;
            public uint TimeStamp;
            public uint WindowID;
            public EventID EventID;
            private byte padding1;
            private byte padding2;
            private byte padding3;
            public int Data1;
            public int Data2;
        }

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_CreateWindow")]
        private static extern IntPtr SDL_CreateWindow (string title, int x, int y, int w, int h, State flags);

        public static IntPtr Create (string title, int x, int y, int w, int h, State flags)
        {
            return GetError (SDL_CreateWindow (title, x, y, w, h, flags));
        }

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_DestroyWindow")]
        public static extern void Destroy (IntPtr window);

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetWindowDisplayIndex")]
        private static extern int SDL_GetWindowDisplayIndex (IntPtr window);

        public static int GetDisplayIndex (IntPtr window)
        {
            return GetError (SDL_GetWindowDisplayIndex (window));
        }

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_SetWindowIcon")]
        public static extern void SetIcon (IntPtr window, IntPtr icon);

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetWindowPosition")]
        public static extern void GetPosition (IntPtr window, out int x, out int y);

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetWindowSize")]
        public static extern void GetSize (IntPtr window, out int w, out int h);

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_SetWindowBordered")]
        public static extern void SetBordered (IntPtr window, int bordered);

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_SetWindowFullscreen")]
        private static extern int SDL_SetWindowFullscreen (IntPtr window, State flags);

        public static void SetFullscreen (IntPtr window, State flags)
        {
            GetError (SDL_SetWindowFullscreen (window, flags));
        }

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_SetWindowPosition")]
        public static extern void SetPosition (IntPtr window, int x, int y);

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_SetWindowSize")]
        public static extern void SetSize (IntPtr window, int w, int h);

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_SetWindowTitle")]
        public static extern void SetTitle (IntPtr window, string title);

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_ShowWindow")]
        public static extern void Show (IntPtr window);
    }

    public static class Display
    {
        public struct Mode
        {
            public uint Format;
            public int Width;
            public int Height;
            public int RefreshRate;
            public IntPtr DriverData;
        }

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetDisplayBounds")]
        private static extern int SDL_GetDisplayBounds (int displayIndex, out Rectangle rect);

        public static void GetBounds (int displayIndex, out Rectangle rect)
        {
            GetError (SDL_GetDisplayBounds (displayIndex, out rect));
        }

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetCurrentDisplayMode")]
        private static extern int SDL_GetCurrentDisplayMode (int displayIndex, out Mode mode);

        public static void GetCurrentDisplayMode (int displayIndex, out Mode mode)
        {
            GetError (SDL_GetCurrentDisplayMode (displayIndex, out mode));
        }

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetDisplayMode")]
        private static extern int SDL_GetDisplayMode (int displayIndex, int modeIndex, out Mode mode);

        public static void GetDisplayMode (int displayIndex, int modeIndex, out Mode mode)
        {
            GetError (SDL_GetDisplayMode (displayIndex, modeIndex, out mode));
        }

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetDisplayName")]
        private static extern IntPtr SDL_GetDisplayName (int index);

        public static string GetDisplayName (int index)
        {
            return GetString (GetError (SDL_GetDisplayName (index)));
        }

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetNumDisplayModes")]
        private static extern int SDL_GetNumDisplayModes (int displayIndex);

        public static int GetNumDisplayModes (int displayIndex)
        {
            return GetError (SDL_GetNumDisplayModes (displayIndex));
        }

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetNumVideoDisplays")]
        private static extern int SDL_GetNumVideoDisplays ();

        public static int GetNumVideoDisplays ()
        {
            return GetError (SDL_GetNumVideoDisplays ());
        }

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetWindowDisplayIndex")]
        private static extern int SDL_GetWindowDisplayIndex (IntPtr window);

        public static int GetWindowDisplayIndex (IntPtr window)
        {
            return GetError (SDL_GetWindowDisplayIndex (window));
        }
    }

    public static class Mouse
    {
        [Flags]
        public enum Button
        {
            Left = 1 << 0,
            Middle = 1 << 1,
            Right = 1 << 2,
            X1Mask = 1 << 3,
            X2Mask = 1 << 4
        }

        public struct WheelEvent
        {
            public EventType Type;
            public uint TimeStamp;
            public uint WindowID;
            public uint Which;
            public int X;
            public int Y;
            public uint Direction;
        }

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetGlobalMouseState")]
        public static extern Button GetGlobalState (out int x, out int y);

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetMouseState")]
        public static extern Button GetState (out int x, out int y);

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_ShowCursor")]
        public static extern int ShowCursor (int toggle);

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_WarpMouseInWindow")]
        public static extern void WarpInWindow (IntPtr window, int x, int y);
    }

    public static class Keyboard
    {
        public struct Keysym
        {
            public int Scancode;
            public int Sym;
            public ushort Mod;
            public uint Unicode;
        }

        public struct Event
        {
            public EventType Type;
            public uint TimeStamp;
            public uint WindowID;
            public byte State;
            public byte Repeat;
            private byte padding2;
            private byte padding3;
            public Keysym Keysym;
        }
    }

    public static class Joystick
    {
        [Flags]
        public enum Hat : byte
        {
            Centered,
            Up,
            Right,
            Down,
            Left,
        }

        public struct DeviceEvent
        {
            public EventType Type;
            public uint TimeStamp;
            public int Which;
        }

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_JoystickClose")]
        public static extern void Close (IntPtr joystick);

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_JoystickGetAxis")]
        public static extern short GetAxis (IntPtr joystick, int axis);

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_JoystickGetButton")]
        public static extern byte GetButton (IntPtr joystick, int button);

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_JoystickGetGUID")]
        public static extern Guid GetGUID (IntPtr joystick);

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_JoystickGetHat")]
        public static extern Hat GetHat (IntPtr joystick, int hat);

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_JoystickOpen")]
        private static extern IntPtr SDL_JoystickOpen (int device_index);

        public static IntPtr Open (int device_index)
        {
            return GetError (SDL_JoystickOpen (device_index));
        }

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_JoystickNumAxes")]
        private static extern int SDL_JoystickNumAxes (IntPtr joystick);

        public static int NumAxes (IntPtr joystick)
        {
            return GetError (SDL_JoystickNumAxes (joystick));
        }

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_JoystickNumButtons")]
        private static extern int SDL_JoystickNumButtons (IntPtr joystick);

        public static int NumButtons (IntPtr joystick)
        {
            return GetError (SDL_JoystickNumButtons (joystick));
        }

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_JoystickNumHats")]
        private static extern int SDL_JoystickNumHats (IntPtr joystick);

        public static int NumHats (IntPtr joystick)
        {
            return GetError (SDL_JoystickNumHats (joystick));
        }

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_NumJoysticks")]
        private static extern int SDL_NumJoysticks ();

        public static int NumJoysticks ()
        {
            return GetError (SDL_NumJoysticks ());
        }
    }

    public static class GameController
    {
        public enum Axis
        {
            Invalid = -1,
            LeftX,
            LeftY,
            RightX,
            RightY,
            TriggerLeft,
            TriggerRight,
            Max,
        }

        public enum Button
        {
            Invalid = -1,
            A,
            B,
            X,
            Y,
            Back,
            Guide,
            Start,
            LeftStick,
            RightStick,
            LeftShoulder,
            RightShoulder,
            DpadUp,
            DpadDown,
            DpadLeft,
            DpadRight,
            Max,
        }

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GameControllerAddMapping")]
        public static extern int AddMapping (string mappingString);

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GameControllerClose")]
        public static extern void Close (IntPtr gamecontroller);

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GameControllerGetAxis")]
        public static extern short GetAxis (IntPtr gamecontroller, Axis axis);

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GameControllerGetButton")]
        public static extern byte GetButton (IntPtr gamecontroller, Button button);

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GameControllerGetJoystick")]
        private static extern IntPtr SDL_GameControllerGetJoystick (IntPtr gamecontroller);

        public static IntPtr GetJoystick (IntPtr gamecontroller)
        {
            return GetError (SDL_GameControllerGetJoystick (gamecontroller));
        }

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_IsGameController")]
        public static extern byte IsGameController (int joystick_index);

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GameControllerOpen")]
        private static extern IntPtr SDL_GameControllerOpen (int joystick_index);

        public static IntPtr Open (int joystick_index)
        {
            return GetError (SDL_GameControllerOpen (joystick_index));
        }

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GameControllerName")]
        private static extern IntPtr SDL_GameControllerName (IntPtr gamecontroller);

        public static string GetName (IntPtr gamecontroller)
        {
            return GetString (SDL_GameControllerName (gamecontroller));
        }
    }

    public static class Haptic
    {
        public const uint Infinity = uint.MaxValue;

        public enum EffectID : ushort
        {
            LeftRight = 1 << 2,
        }

        public struct LeftRight
        {
            public EffectID Type;
            public uint Length;
            public ushort LargeMagnitude;
            public ushort SmallMagnitude;
        }

        [StructLayout (LayoutKind.Explicit)]
        public struct Effect
        {
            [FieldOffset (0)]
            public EffectID type;
            [FieldOffset (0)]
            public LeftRight leftright;
        }

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_HapticClose")]
        public static extern void Close (IntPtr haptic);

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_HapticEffectSupported")]
        private static extern int SDL_HapticEffectSupported (IntPtr haptic, ref Effect effect);

        public static int EffectSupported (IntPtr haptic, ref Effect effect)
        {
            return GetError (SDL_HapticEffectSupported (haptic, ref effect));
        }

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_JoystickIsHaptic")]
        private static extern int SDL_JoystickIsHaptic (IntPtr joystick);

        public static int IsHaptic (IntPtr joystick)
        {
            return GetError (SDL_JoystickIsHaptic (joystick));
        }

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_HapticNewEffect")]
        private static extern int SDL_HapticNewEffect (IntPtr haptic, ref Effect effect);

        public static void NewEffect (IntPtr haptic, ref Effect effect)
        {
            GetError (SDL_HapticNewEffect (haptic, ref effect));
        }

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_HapticOpenFromJoystick")]
        private static extern IntPtr SDL_HapticOpenFromJoystick (IntPtr joystick);

        public static IntPtr OpenFromJoystick (IntPtr joystick)
        {
            return GetError (SDL_HapticOpenFromJoystick (joystick));
        }

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_HapticRumbleInit")]
        private static extern int SDL_HapticRumbleInit (IntPtr haptic);

        public static void RumbleInit (IntPtr haptic)
        {
            GetError (SDL_HapticRumbleInit (haptic));
        }

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_HapticRumblePlay")]
        private static extern int SDL_HapticRumblePlay (IntPtr haptic, float strength, uint length);

        public static void RumblePlay (IntPtr haptic, float strength, uint length)
        {
            GetError (SDL_HapticRumblePlay (haptic, strength, length));
        }

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_HapticRumbleSupported")]
        private static extern int SDL_HapticRumbleSupported (IntPtr haptic);

        public static int RumbleSupported (IntPtr haptic)
        {
            return GetError (SDL_HapticRumbleSupported (haptic));
        }

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_HapticRunEffect")]
        private static extern int SDL_HapticRunEffect (IntPtr haptic, int effect, uint iterations);

        public static void RunEffect (IntPtr haptic, int effect, uint iterations)
        {
            GetError (SDL_HapticRunEffect (haptic, effect, iterations));
        }

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_HapticStopAll")]
        private static extern int SDL_HapticStopAll (IntPtr haptic);

        public static void StopAll (IntPtr haptic)
        {
            GetError (SDL_HapticStopAll (haptic));
        }

        [DllImport (nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_HapticUpdateEffect")]
        private static extern int SDL_HapticUpdateEffect (IntPtr haptic, int effect, ref Effect data);

        public static void UpdateEffect (IntPtr haptic, int effect, ref Effect data)
        {
            GetError (SDL_HapticUpdateEffect (haptic, effect, ref data));
        }
    }
}
