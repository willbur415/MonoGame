using System;
using System.IO;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Input
{
    static partial class Joystick
    {
        class JoystickInitData
        {
            // Temp variables for initialization
            public int AxisCount, ButtonCount;
            public ButtonState[] Buttons = new ButtonState[256];
            public short[] Axes = new short[256];
        }
        
        class JoystickData
        {
            public bool Init;
            public JoystickInitData InitData = new JoystickInitData();

            public int Fd;
            public string Name;
            public JoystickState State = new JoystickState
            {
                IsConnected = true,
                Axes = new int[256],
                Buttons = new ButtonState[256],
                Hats = new JoystickHat[0]
            };
        }

        private const int EventButton = 0x01;
        private const int EventAxis = 0x02;
        private const int EventInit = 0x80;
        
        private static Dictionary<int, JoystickData> _joysticks = new Dictionary<int, JoystickData>();
        private static byte[] _jsevent = new byte[8];
        
        private static JoystickCapabilities LinuxPlatformGetCapabilities(int index)
        {
            JoystickData data; // TODO: Use C# 7 syntax

            if (_joysticks.TryGetValue(index, out data))
            {
                return new JoystickCapabilities
                {
                    IsConnected = true,
                    AxisCount = data.State.Axes.Length,
                    ButtonCount = data.State.Buttons.Length,
                    HatCount = 0,
                    IsGamepad = false,
                    Identifier = data.Name
                };
            }
            else
                return JoystickCapabilities.Default;
        }

        private static JoystickState LinuxPlatformGetState(int index)
        {
            JoystickData data; // TODO: Use C# 7 syntax

            if (_joysticks.TryGetValue(index, out data))
                return data.State;
            else
                return JoystickState.Default;
        }

        internal static unsafe bool PollEvents(int id)
        {
            JoystickData data;

            if (!_joysticks.TryGetValue(id, out data))
            {
                data = new JoystickData();
                data.Fd = Libc.open("/dev/input/js" + id, 0x0800);

                // An error occured during open
                if (data.Fd < 0)
                    return false;
            }
            
            fixed (void* buff = &_jsevent[0])
            {
                int readbytes = Libc.read(data.Fd, buff, 8);

                while (readbytes > 0)
                {
                    ProcessData(data);
                    readbytes = Libc.read(data.Fd, buff, 8);
                }

                if (readbytes < 0)
                {
                    // An error occured, do something?
                }
            }

            if (!data.Init)
            {
                data.Init = true;

                // Copy over the init data
                data.State.Buttons = new ButtonState[data.InitData.ButtonCount];
                for (int i = 0; i < data.InitData.Buttons.Length; i++)
                    data.State.Buttons[i] = data.InitData.Buttons[i];

                data.State.Axes = new int[data.InitData.AxisCount];
                for (int i = 0; i < data.InitData.AxisCount; i++)
                    data.State.Axes[i] = data.InitData.Axes[i] * 2;

                // Delete the init data
                data.InitData = null;

                // Figure out the device name
                var lines = File.ReadAllLines("/proc/bus/input/devices");
                var devicename = Path.GetFileName("/dev/input/js" + id);

                foreach (var line in lines)
                {
                    if (line.Contains("Name"))
                        data.Name = line.Split('=')[1].Trim('"');
                    else if (line.Contains(devicename))
                        break;
                }
            }

            return true;
        }

        private static unsafe void ProcessData(JoystickData data)
        {
            // joystick event structure:
            // 4 timestamp  0, 1, 2, 3
            // 2 value      4, 5
            // 1 type       6
            // 1 number     7

            var type = _jsevent[6];
            var number = _jsevent[7];

            if (!data.Init)
            {
                // each button and axis gets their initial value
                // set on the first read of the main device file
                if ((type & EventInit) == EventInit)
                {
                    if ((type & EventButton) == EventButton)
                        data.InitData.ButtonCount++;
                    else if ((type & EventAxis) == EventAxis)
                        data.InitData.AxisCount++;
                }

                if ((type & EventButton) == EventButton)
                    data.InitData.Buttons[number] = (ButtonState)_jsevent[4];
                else if ((type & EventAxis) == EventAxis)
                {
                    fixed (byte* adata = &_jsevent[4])
                        data.InitData.Axes[number] = *(short*)adata;
                }
            }
            else
            {
                if ((type & EventButton) == EventButton)
                    data.State.Buttons[number] = (ButtonState)_jsevent[4];
                else if ((type & EventAxis) == EventAxis)
                {
                    fixed (byte* adata = &_jsevent[4])
                        data.State.Axes[number] = (*(short*)adata) * 2;
                }
            }
        }
    }
}
