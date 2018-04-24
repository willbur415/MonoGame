// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Song : IEquatable<Song>, IDisposable
    {
        private float _volume = 1f;

        private void PlatformInitialize(string fileName)
        {
            Console.WriteLine(fileName);
            _duration = TimeSpan.FromSeconds(0.0);
        }
        
        internal void SetEventHandler(FinishedPlayingHandler handler) { }

        internal void OnFinishedPlaying()
        {
            MediaPlayer.OnSongFinishedPlaying(null, null);
        }
		
        void PlatformDispose(bool disposing)
        {

        }

        internal void Play(TimeSpan? startPosition)
        {

            _playCount++;
        }

        internal void Resume()
        {
        }

        internal void Pause()
        {

        }

        internal void Stop()
        {
            _playCount = 0;
        }

        internal float Volume
        {
            get
            {
                return _volume; 
            }
            set
            {
                _volume = value;
            }
        }

        public TimeSpan Position
        {
            get
            {
                return TimeSpan.FromSeconds(0.0);
            }
        }

        private Album PlatformGetAlbum()
        {
            return null;
        }

        private Artist PlatformGetArtist()
        {
            return null;
        }

        private Genre PlatformGetGenre()
        {
            return null;
        }

        private TimeSpan PlatformGetDuration()
        {
            return _duration;
        }

        private bool PlatformIsProtected()
        {
            return false;
        }

        private bool PlatformIsRated()
        {
            return false;
        }

        private string PlatformGetName()
        {
            return _name;
        }

        private int PlatformGetPlayCount()
        {
            return _playCount;
        }

        private int PlatformGetRating()
        {
            return 0;
        }

        private int PlatformGetTrackNumber()
        {
            return 0;
        }
    }
}