using System;

namespace Microsoft.Xna.Framework.Input
{
    static partial class Joystick
    {
        private static bool UseSdl = true;
        private const bool PlatformIsSupported = true;

        private static JoystickCapabilities PlatformGetCapabilities(int index)
        {
            if (UseSdl)
                return SdlPlatformGetCapabilities(index);
            
            return LinuxPlatformGetCapabilities(index);
        }

        private static JoystickState PlatformGetState(int index)
        {
            if (UseSdl)
                return SdlPlatformGetState(index);

            return LinuxPlatformGetState(index);
        }
    }
}
