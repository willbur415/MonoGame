// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Audio;
using Bridge.Html5;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Song : IEquatable<Song>, IDisposable
    {
        private HTMLAudioElement _audio;

        private void PlatformInitialize(string fileName)
        {
            _audio = new HTMLAudioElement(fileName);
            _audio.Load();

            _duration = TimeSpan.FromSeconds(_audio.Duration);

            _audio.OnEnded += (e) => MediaPlayer.OnSongFinishedPlaying(null, null);
        }
        
        internal void SetEventHandler(FinishedPlayingHandler handler) { }
		
        void PlatformDispose(bool disposing)
        {

        }

        internal void Play(TimeSpan? startPosition)
        {
            if (startPosition.HasValue)
                _audio.FastSeek(startPosition.Value.Seconds);

            _audio.Play();
            _playCount++;
        }

        internal void Resume()
        {
            _audio.Play();
        }

        internal void Pause()
        {
            _audio.Pause();
        }

        internal void Stop()
        {
            _audio.FastSeek(0);
            _audio.Pause();
            _playCount = 0;
        }

        internal float Volume
        {
            get => (float)_audio.Volume;
            set => _audio.Volume = value;
        }

        public TimeSpan Position
        {
            get => TimeSpan.FromSeconds(_audio.CurrentTime);
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