// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Audio;

namespace OpenAL
{
    public enum ALFormat
    {
        Mono8 = 0x1100,
        Mono16 = 0x1101,
        Stereo8 = 0x1102,
        Stereo16 = 0x1103,
        MonoMSADPCM =0x1302,
        StereoMSADPCM =0x1303,
    }

    public enum ALError
    {
        NoError = 0,
        InvalidName = 0xA001,
        InvalidEnum = 0xA002,
        InvalidValue = 0xA003,
        InvalidOperation = 0xA004,
        OutOfMemory = 0xA005,
    }

    public enum ALGetString
    {
        Extensions = 0xB004,
    }

    public enum ALBufferi
    {
        UnpackBlockAlignmentSoft = 0x200C,
        LoopSoftPointsExt = 0x2015,
    }

    public enum ALGetBufferi
    {
        Bits = 0x2002,
        Channels = 0x2003,
        Size = 0x2004,
    }

    public enum ALSourceb
    {
        Looping = 0x1007,
    }

    public enum ALSourcei
    {
        Buffer = 0x1009,
        EfxDirectFilter = 0x20005,
        EfxAuxilarySendFilter = 0x20006,
    }

    public enum ALSourcef
    {
        Pitch = 0x1003,
        Gain = 0x100A,
    }

    public enum ALGetSourcei
    {
        SampleOffset = 0x1025,
        SourceState = 0x1010,
        BuffersQueued = 0x1015,
        BuffersProcessed = 0x1016,
    }

    public enum ALSourceState
    {
        Initial = 0x1011,
        Playing = 0x1012,
        Paused = 0x1013,
        Stopped = 0x1014,
    }

    public enum ALSourcefv
    {
        Position = 0x1004,
        Velocity = 0x1006,
        Direction = 0x1005,
    }

    public enum ALListenerfv
    {
        Position = 0x1004,
        Velocity = 0x1006,
        Direction = 0x1005,
    }

    public enum ALDistanceModel
    {
        InverseDistanceClamped = 0xD002,
    }

    public enum AlcError
    {
        NoError = 0,
    }

    public enum AlcGetString
    {
        Extensions = 0x1006,
    }

    public enum EfxFilteri
    {
        FilterType = 0x8001,
    }

    public enum EfxFilterf
    {
        LowpassGain = 0x0001,
        LowpassGainHF = 0x0002,
        HighpassGain = 0x0001,
        HighpassGainLF = 0x0002,
        BandpassGain = 0x0001,
        BandpassGainLF = 0x0002,
        BandpassGainHF = 0x0003,
    }

    public enum EfxFilterType
    {
        None = 0x0000,
        Lowpass = 0x0001,
        Highpass = 0x0002,
        Bandpass = 0x0003,
    }

    public enum EfxEffecti
    {
        EffectType = 0x8001,
        SlotEffect = 0x0001,
    }

    public enum EfxEffectSlotf
    {
        EffectSlotGain = 0x0002,
    }

    public enum EfxEffectf
    {
        EaxReverbDensity = 0x0001,
        EaxReverbDiffusion = 0x0002,
        EaxReverbGain = 0x0003,
        EaxReverbGainHF = 0x0004,
        EaxReverbGainLF = 0x0005,
        DecayTime = 0x0006,
        DecayHighFrequencyRatio = 0x0007,
        DecayLowFrequencyRation = 0x0008,
        EaxReverbReflectionsGain = 0x0009,
        EaxReverbReflectionsDelay = 0x000A,
        ReflectionsPain = 0x000B,
        LateReverbGain = 0x000C,
        LateReverbDelay = 0x000D,
        LateRevertPain = 0x000E,
        EchoTime = 0x000F,
        EchoDepth = 0x0010,
        ModulationTime = 0x0011,
        ModulationDepth = 0x0012,
        AirAbsorbsionHighFrequency = 0x0013,
        EaxReverbHFReference = 0x0014,
        EaxReverbLFReference = 0x0015,
        RoomRolloffFactor = 0x0016,
        DecayHighFrequencyLimit = 0x0017,
    }

    public enum EfxEffectType
    {
        Reverb = 0x8000,
    }

    public class AL
    {
#if IOS || MONOMAC
        public const string NativeLibName = "/System/Library/Frameworks/OpenAL.framework/OpenAL";
#elif ANDROID
        public const string NativeLibName = "libopenal32.so";
#else
        public const string NativeLibName = "soft_oal.dll";
#endif

        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alEnable")]
        public static extern void pEnable (int cap);

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alBufferData")]
        public static extern void pBufferData(uint bid, int format, IntPtr data, int size, int freq);

        public static  void Enable (int cap)
        {
            Android.Util.Log.Debug ("ropo", "Enable tid: " + Environment.CurrentManagedThreadId);
            pEnable (cap);
        }

         public static  void BufferData (uint bid, int format, IntPtr data, int size, int freq)
        {
            Android.Util.Log.Debug ("ropo", "BufferData tid: " + Environment.CurrentManagedThreadId);
            pBufferData (bid, format, data, size, freq);
        }

        public static void BufferData(int bid, ALFormat format, byte[] data, int size, int freq)
        {
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            BufferData((uint)bid, (int)format, handle.AddrOfPinnedObject(), size, freq);
            handle.Free();
        }

        public static void BufferData(int bid, ALFormat format, short[] data, int size, int freq)
        {
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            BufferData((uint)bid, (int)format, handle.AddrOfPinnedObject(), size, freq);
            handle.Free();
        }

        [CLSCompliant (false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alDeleteBuffers")]
        public static unsafe extern void pDeleteBuffers(int n, int* buffers);


        public static unsafe  void DeleteBuffers (int n, int* buffers)
        {
            Android.Util.Log.Debug ("ropo", "DeleteBuffers tid: " + Environment.CurrentManagedThreadId);

            pDeleteBuffers (n, buffers);
        }

        public static void DeleteBuffers(int[] buffers)
        {
            DeleteBuffers (buffers.Length, ref buffers [0]);
        }

        public unsafe static void DeleteBuffers(int n, ref int buffers)
        {
            fixed (int* pbuffers = &buffers)
            {
                DeleteBuffers (n, pbuffers);
            }
        }

        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alBufferi")]
        public static extern void pBufferi (int buffer, ALBufferi param, int value);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alGetBufferi")]
        public static extern void pGetBufferi(int bid, ALGetBufferi param, out int value);

        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alBufferiv")]
        public static extern void pBufferiv (int bid, ALBufferi param, int[] values);

      public static  void Bufferi (int buffer, ALBufferi param, int value)
        {
            Android.Util.Log.Debug ("ropo", "Bufferi tid: " + Environment.CurrentManagedThreadId);

            pBufferi (buffer, param, value);
        }

        public static  void GetBufferi (int bid, ALGetBufferi param, out int value)
        {
            Android.Util.Log.Debug ("ropo", "GetBufferi tid: " + Environment.CurrentManagedThreadId);

            pGetBufferi (bid, param,out value);
        }

        public static  void Bufferiv (int bid, ALBufferi param, int[] values)
        {
            Android.Util.Log.Debug ("ropo", "Bufferiv tid: " + Environment.CurrentManagedThreadId);

            pBufferiv (bid, param, values);
        }


        public static void GetBuffer(int bid, ALGetBufferi param, out int value)
        {
            Android.Util.Log.Debug ("ropo", "GetBuffer tid: " + Environment.CurrentManagedThreadId);

            GetBufferi (bid, param, out value);
        }

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alGenBuffers")]
        public static unsafe extern void pGenBuffers(int count, int* buffers);


        public static unsafe  void GenBuffers (int count, int* buffers)
        {
            Android.Util.Log.Debug ("ropo", "GenBuffers tid: " + Environment.CurrentManagedThreadId);

            pGenBuffers (count, buffers);
        }


        internal unsafe static void GenBuffers (int count,out int[] buffers)
        {
            buffers = new int[count];
            fixed (int* ptr = &buffers[0])
            {
                GenBuffers (count, ptr);
            }
        }

        public static void GenBuffers(int count, out int buffer)
        {
            int[] ret;
            GenBuffers(count, out ret);
            buffer = ret[0];
        }

        public static int[] GenBuffers(int count)
        {
            int[] ret;
            GenBuffers(count, out ret);
            return ret;
        }

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alGenSources")]
        public static extern void pGenSources(int n, uint[] sources);


        public static void GenSources(int[] sources)
        {
            uint[] temp = new uint[sources.Length];
            GenSources(temp.Length, temp);
            for (int i = 0; i < temp.Length; i++)
            {
                sources[i] = (int)temp[i];
            }
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alGetError")]
        public static extern ALError pGetError();

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alIsBuffer")]
        public static extern bool pIsBuffer(uint buffer);

        public static void GenSources (int n, uint[] sources)
        {
            Android.Util.Log.Debug ("ropo", "GenSources tid: " + Environment.CurrentManagedThreadId);

            pGenSources (n, sources);
        }


        public static ALError GetError ()
        {
            Android.Util.Log.Debug ("ropo", "GetError tid: " + Environment.CurrentManagedThreadId);

            return pGetError ();
        }

        public static bool IsBuffer (uint buffer)
        {
            Android.Util.Log.Debug ("ropo", "IsBuffer tid: " + Environment.CurrentManagedThreadId);

            return pIsBuffer (buffer);
        }


        public static bool IsBuffer(int buffer)
        {
            Android.Util.Log.Debug ("ropo", "IsBuffer tid: " + Environment.CurrentManagedThreadId);

            return IsBuffer ((uint)buffer);
        }

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alSourcePause")]
        public static extern void pSourcePause(uint source);

        public static  void SourcePause (uint source)
        {
            Android.Util.Log.Debug ("ropo", "SourcePause tid: " + Environment.CurrentManagedThreadId);

            pSourcePause (source);
        }

        public static void SourcePause(int source)
        {
            SourcePause((uint)source);
        }

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alSourcePlay")]
        public static extern void pSourcePlay(uint source);

      public static  void SourcePlay (uint source)
        {
            Android.Util.Log.Debug ("ropo", "SourcePlay tid: " + Environment.CurrentManagedThreadId);

            pSourcePlay (source);
        }

        public static void SourcePlay(int source)
        {
            SourcePlay((uint)source);
        }

        public static string GetErrorString(ALError errorCode)
        {
            return errorCode.ToString (); 
        }

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alIsSource")]
        public static extern bool pIsSource(int source);

        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alDeleteSources")]
        public static extern void pDeleteSources(int n, ref int sources);

        public static bool IsSource (int source)
        {
            Android.Util.Log.Debug ("ropo", "IsSource tid: " + Environment.CurrentManagedThreadId);

            return pIsSource (source);
        }

        public static void DeleteSources (int n, ref int sources)
        {
            Android.Util.Log.Debug ("ropo", "DeleteSources tid: " + Environment.CurrentManagedThreadId);

            pDeleteSources (n, ref sources);
        }

        public static void DeleteSource(int source)
        {
            DeleteSources (1, ref source);
        }

        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alSourceStop")]
        public static extern void pSourceStop (int sourceId);

        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alSourcei")]
        internal static extern void pSource (int sourceId, int i, int a);

        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alSource3i")]
        public static extern void pSource (int sourceId, ALSourcei i, int a, int b, int c);

        public static void SourceStop (int sourceId)
        {
            Android.Util.Log.Debug ("ropo", "SourceStop tid: " + Environment.CurrentManagedThreadId);

            pSourceStop (sourceId);
        }
       internal static void Source (int sourceId, int i, int a)
        {
            Android.Util.Log.Debug ("ropo", "Source A tid: " + Environment.CurrentManagedThreadId);

            pSource (sourceId, i, a);
        }

        public static void Source (int sourceId, ALSourcei i, int a, int b, int c)
        {
            Android.Util.Log.Debug ("ropo", "Source B tid: " + Environment.CurrentManagedThreadId);

            pSource (sourceId, i, a, b, c);
        }

        public static void Source (int sourceId, ALSourcei i, int a)
        {
            Source (sourceId, (int)i, a);
        }

        public static void Source (int sourceId, ALSourceb i, bool a) {
            Source (sourceId, (int)i, a ? 1 : 0);
        }

        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alSourcef")]
        public static extern void pSource (int sourceId, ALSourcef i, float a);

        public static  void Source (int sourceId, ALSourcef i, float a)
        {
            Android.Util.Log.Debug ("ropo", "Source C tid: " + Environment.CurrentManagedThreadId);

            pSource (sourceId, i, a);
        }


        /* MUST NOT USE THIS VERSION BECAUSE OF WHAT SEEMS AS A XAMARIN BUG WHERE MULTIPLIE FLOAT PARAMS PASSED BY VALUE CORRUPT FUNCTION PARAMETERS,
         SO WE MUST USE THE VERSION WHICH PASSES AN ARRAY (POINTER) OF FLOATS. 
        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alSource3f")]
        public static extern void Source (int sourceId, ALSource3f i, float x, float y, float z);
        */

        public static void Source (int sourceId, ALSourcefv i, float x, float y, float z)
        {
            float[] v = { x, y, z };
            Source (sourceId, i, ref v);
        }

        public static void Source (int sourceId, ALSourcefv i, ref float[] values)
        {
            unsafe
            {
                fixed (float* ptr = &values[0])
                {
                    SourcePrivate (sourceId, i, ptr);
                }
            }
        }

        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alSourcefv", ExactSpelling = true)]
        unsafe private static extern void pSourcePrivate (int sid, ALSourcefv param, float* values);

        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alGetSourcei")]
        public static extern void pGetSource (int sourceId, ALGetSourcei i, out int state);

       unsafe private static  void SourcePrivate (int sid, ALSourcefv param, float* values)
        {
            Android.Util.Log.Debug ("ropo", "SourcePrivate tid: " + Environment.CurrentManagedThreadId);

            pSourcePrivate (sid, param, values);
        }

        public static  void GetSource (int sourceId, ALGetSourcei i, out int state)
        {
            Android.Util.Log.Debug ("ropo", "GetSource tid: " + Environment.CurrentManagedThreadId);

            pGetSource (sourceId, i, out state);
        }

        public static ALSourceState GetSourceState(int sourceId) {
            int state = (int)ALSourceState.Stopped;
            GetSource (sourceId, ALGetSourcei.SourceState, out state);
            return (ALSourceState)state;
        }

        /* MUST NOT USE THIS VERSION BECAUSE OF WHAT SEEMS AS A XAMARIN BUG WHERE MULTIPLIE FLOAT PARAMS PASSED BY VALUE CORRUPT FUNCTION PARAMETERS,
         SO WE MUST USE THE VERSION WHICH PASSES AN ARRAY (POINTER) OF FLOATS. 
       [CLSCompliant (false)]
       [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alGetListener3f")]
       public static extern void GetListener (ALListener3f param, out float value1, out float value2, out float value3);
       */

        public static void GetListener (ALListenerfv i, ref float[] outValues)
        {
            unsafe
            {
                fixed (float* ptr = &outValues[0])
                {
                    GetListener (i, ptr);
                }
            }
        }

       // public static void DistanceModel (ALDistanceModel model) { }

        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alGetListenerfv", ExactSpelling = true)]
        unsafe private static extern void pGetListener (ALListenerfv i, float* values);
       
        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alSourceQueueBuffers")]
        public unsafe static extern void pSourceQueueBuffers (int sourceId, int numEntries, [In] int* buffers);

        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alSourceUnqueueBuffers")]
        public unsafe static extern void pSourceUnqueueBuffers (int sourceId, int numEntries, [In] int* salvaged);

        unsafe private static void GetListener (ALListenerfv i, float* values)
        {
            Android.Util.Log.Debug ("ropo", "GetListener B tid: " + Environment.CurrentManagedThreadId);

            pGetListener (i, values);
        }

        public unsafe static void SourceQueueBuffers (int sourceId, int numEntries, [In] int* buffers)
        {
            Android.Util.Log.Debug ("ropo", "SourceQueueBuffers tid: " + Environment.CurrentManagedThreadId);

            pSourceUnqueueBuffers (sourceId, numEntries, buffers);
        }

        public unsafe static void SourceUnqueueBuffers (int sourceId, int numEntries, [In] int* salvaged)
        {
            Android.Util.Log.Debug ("ropo", "SourceUnqueueBuffers tid: " + Environment.CurrentManagedThreadId);

            pSourceUnqueueBuffers (sourceId, numEntries, salvaged);
        }

        [CLSCompliant (false)]
        public unsafe static void SourceQueueBuffers (int sourceId, int numEntries, int [] buffers)
        {
            fixed (int* ptr = &buffers[0]) {
                AL.SourceQueueBuffers (sourceId, numEntries, ptr);
            }
        }

        public unsafe static void SourceQueueBuffer (int sourceId, int buffer)
        {
            AL.SourceQueueBuffers (sourceId, 1, &buffer);
        }

        public static unsafe int [] SourceUnqueueBuffers (int sourceId, int numEntries)
        {
            if (numEntries <= 0) {
                throw new ArgumentOutOfRangeException ("numEntries", "Must be greater than zero.");
            }
            int [] array = new int [numEntries];
            fixed (int* ptr = &array [0])
            {
                AL.SourceUnqueueBuffers (sourceId, numEntries, ptr);
            }
            return array;
        }

        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alSourceUnqueueBuffers")]
        public static extern void pSourceUnqueueBuffers (int sid, int numEntries, [Out] int[] bids);

        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alGetEnumValue")]
        public static extern int pGetEnumValue (string enumName);

        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alIsExtensionPresent")]
        public static extern bool pIsExtensionPresent (string extensionName);

        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alGetProcAddress")]
        public static extern IntPtr pGetProcAddress (string functionName);

        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alGetString")]
        private static extern IntPtr palGetString (int p);

        public static void SourceUnqueueBuffers (int sid, int numEntries, [Out] int[] bids)
        {
            Android.Util.Log.Debug ("ropo", "SourceUnqueueBuffers tid: " + Environment.CurrentManagedThreadId);

            pSourceUnqueueBuffers (sid, numEntries, bids);
        }

        public static int GetEnumValue (string enumName)
        {
            Android.Util.Log.Debug ("ropo", "GetEnumValue tid: " + Environment.CurrentManagedThreadId);

            return pGetEnumValue (enumName);
        }

        public static bool IsExtensionPresent (string extensionName)
        {
            Android.Util.Log.Debug ("ropo", "IsExtensionPresent tid: " + Environment.CurrentManagedThreadId);

            return pIsExtensionPresent (extensionName);
        }

        public static IntPtr GetProcAddress (string functionName)
        {
            Android.Util.Log.Debug ("ropo", "GetProcAddress tid: " + Environment.CurrentManagedThreadId);

            return pGetProcAddress (functionName);
        }

        private static IntPtr alGetString (int p)
        {
            Android.Util.Log.Debug ("ropo", "alGetString tid: " + Environment.CurrentManagedThreadId);

            return palGetString (p);
        }

        public static string GetString (int p)
        {
            return Marshal.PtrToStringAnsi (alGetString (p));
        }

        public static string Get (ALGetString p)
        {
            return GetString ((int)p);
        }
    }

    public partial class Alc
    {
#if IOS || MONOMAC
        public const string NativeLibName = "/System/Library/Frameworks/OpenAL.framework/OpenAL";
#elif ANDROID
        public const string NativeLibName = "libopenal32.so";
#else
        public const string NativeLibName = "soft_oal.dll";
#endif


        public static AlcError GetError ()
        {
            return GetError (IntPtr.Zero);
        }

        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcCreateContext")]
        public static extern IntPtr pCreateContext (IntPtr device, int [] attributes);

        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcGetError")]
        public static extern AlcError pGetError (IntPtr device);

        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcGetCurrentContext")]
        public static extern IntPtr pGetCurrentContext ();

        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcMakeContextCurrent")]
        public static extern void pMakeContextCurrent (IntPtr context);

        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcDestroyContext")]
        public static extern void pDestroyContext (IntPtr context);

        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcCloseDevice")]
        public static extern void pCloseDevice (IntPtr device);

        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcOpenDevice")]
        public static extern IntPtr pOpenDevice ([MarshalAs (UnmanagedType.LPStr)]  string device);

        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcIsExtensionPresent")]
        public static extern bool pIsExtensionPresent (IntPtr device, [MarshalAs (UnmanagedType.LPStr)] string extensionName);

        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcGetString")]
        internal static extern IntPtr palGetString (IntPtr device, int p);

        ////

        public static IntPtr CreateContext (IntPtr device, int[] attributes)
        {
            Android.Util.Log.Debug ("ropo", "CreateContext tid: " + Environment.CurrentManagedThreadId);

            return pCreateContext (device, attributes);
        }

        public static AlcError GetError (IntPtr device)
        {
            Android.Util.Log.Debug ("ropo", "GetError tid: " + Environment.CurrentManagedThreadId);

            return pGetError (device);
        }

        public static IntPtr GetCurrentContext ()
        {
            Android.Util.Log.Debug ("ropo", "GetCurrentContext tid: " + Environment.CurrentManagedThreadId);

            return pGetCurrentContext ();
        }

        public static void MakeContextCurrent (IntPtr context)
        {
            Android.Util.Log.Debug ("ropo", "MakeContextCurrent tid: " + Environment.CurrentManagedThreadId);

            pMakeContextCurrent (context);
        }

        public static void DestroyContext (IntPtr context)
        {
            Android.Util.Log.Debug ("ropo", "DestroyContext tid: " + Environment.CurrentManagedThreadId);

            pDestroyContext (context);
        }

        public static void CloseDevice (IntPtr device)
        {
            Android.Util.Log.Debug ("ropo", "CloseDevice tid: " + Environment.CurrentManagedThreadId);

            pCloseDevice (device);
        }

        public static IntPtr OpenDevice ([MarshalAs (UnmanagedType.LPStr)]  string device)
        {
            Android.Util.Log.Debug ("ropo", "OpenDevice tid: " + Environment.CurrentManagedThreadId);

            return pOpenDevice (device);
        }

        public static bool IsExtensionPresent (IntPtr device, [MarshalAs (UnmanagedType.LPStr)] string extensionName)
        {
            Android.Util.Log.Debug ("ropo", "IsExtensionPresent tid: " + Environment.CurrentManagedThreadId);

            return pIsExtensionPresent (device, extensionName);
        }

        internal static IntPtr alGetString (IntPtr device, int p)
        {
            Android.Util.Log.Debug ("ropo", "alGetString tid: " + Environment.CurrentManagedThreadId);

            return palGetString (device, p);
        }

        ////

        public static string GetString (IntPtr device, int p)
        {
            return Marshal.PtrToStringAnsi (alGetString (device, p));
        }

        public static string GetString (IntPtr device, AlcGetString p)
        {
            return GetString (device, (int)p);
        }

        public static void SuspendContext (IntPtr context)
        {
        }

        public static void ProcessContext (IntPtr context)
        {
        }
    }

}

