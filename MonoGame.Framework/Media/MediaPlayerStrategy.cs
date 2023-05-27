// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Media;


namespace Microsoft.Xna.Platform.Media
{
    abstract public class MediaPlayerStrategy : IDisposable
    {
        private float _volume = 1.0f;
        private bool _isMuted;
        private bool _isRepeating;
        private bool _isShuffled;
        private bool _isVisualizationEnabled;

        internal MediaState _state = MediaState.Stopped;

        private readonly MediaQueue _queue = new MediaQueue();
        // Need to hold onto this to keep track of how many songs
        // have played when in shuffle mode
        internal int _numSongsInQueuePlayed = 0;
        

        internal event EventHandler<EventArgs> PlatformActiveSongChanged;
        internal event EventHandler<EventArgs> PlatformMediaStateChanged;
        
        ~MediaPlayerStrategy()
        {
            Dispose(false);
        }
        
        internal MediaState State
        {
            get
            {
                PlatformUpdateState(ref _state);
                return _state;
            }
        }

        internal MediaQueue Queue { get {return _queue; }}

        internal virtual float PlatformVolume
        {
            get { return _volume; }
            set { _volume = value; }
        }
        internal virtual bool PlatformIsMuted
        {
            get { return _isMuted; }
            set { _isMuted = value; }
        }
        internal virtual bool PlatformIsRepeating
        {
            get { return _isRepeating; }
            set { _isRepeating = value; }
        }

        internal virtual bool PlatformIsShuffled
        {
            get { return _isShuffled; }
            set { _isShuffled = value; }
        }
        internal virtual bool PlatformIsVisualizationEnabled
        {
            get { return _isVisualizationEnabled; }
            set { _isVisualizationEnabled = value; }
        }

        internal abstract bool PlatformGameHasControl { get; }
        internal abstract TimeSpan PlatformPlayPosition { get; }

        protected abstract bool PlatformUpdateState(ref MediaState state);
        internal abstract void PlatformPlaySong(Song song);
        internal abstract void PlatformPause();
        internal abstract void PlatformResume();
        internal abstract void PlatformStop();


        internal void OnPlatformActiveSongChanged(EventArgs args)
        {
            var handler = PlatformActiveSongChanged;
            if (handler != null)
                handler(null, args);
        }

        internal void OnPlatformMediaStateChanged(EventArgs args)
        {
            var handler = PlatformMediaStateChanged;
            if (handler != null)
                handler(null, args);
        }

        internal virtual void PlatformClearQueue()
        {
            while (_queue.Count > 0)
            {
                Song song = _queue[0];
                _queue.Remove(song);
            }

            _numSongsInQueuePlayed = 0;
        }


        internal void Play(Song song)
        {
            if (song == null)
                throw new ArgumentNullException("song", "This method does not accept null for this parameter.");

            var previousSong = _queue.Count > 0 ? _queue[0] : null;

            PlatformClearQueue();
            _queue.Add(song);
            _queue.ActiveSongIndex = 0;

            if (song.IsDisposed)
                throw new ObjectDisposedException("song");

            PlatformPlaySong(song);
            _state = MediaState.Playing;
            OnPlatformMediaStateChanged(EventArgs.Empty);

            if (previousSong != song)
                OnPlatformActiveSongChanged(EventArgs.Empty);
        }

        internal void Play(SongCollection collection, int index)
        {
            if (collection == null)
                throw new ArgumentNullException("collection", "This method does not accept null for this parameter.");

            PlatformClearQueue();

            foreach (var song in collection)
                _queue.Add(song);

            _queue.ActiveSongIndex = index;

            Song activeSong = _queue.ActiveSong;
            if (activeSong.IsDisposed)
                throw new ObjectDisposedException("activeSong");

            PlatformPlaySong(activeSong);
            _state = MediaState.Playing;
            OnPlatformMediaStateChanged(EventArgs.Empty);
        }

        internal void Pause()
        {
            MediaState state = State;
            switch (state)
            {
                case MediaState.Playing:
                    if (_queue.ActiveSong != null)
                    {
                        PlatformPause();
                        _state =  MediaState.Paused;
                        OnPlatformMediaStateChanged(EventArgs.Empty);
                    }
                    break;
            }
        }

        internal void Resume()
        {
            MediaState state = State;
            switch (state)
            {
                case MediaState.Paused:
                    {
                        PlatformResume();
                        _state = MediaState.Playing;
                        OnPlatformMediaStateChanged(EventArgs.Empty);
                    }
                    break;
            }
        }

        internal void Stop()
        {
            MediaState state = State;
            switch (state)
            {
                case MediaState.Playing:
                case MediaState.Paused:
                    {
                        PlatformStop();
                        _state = MediaState.Stopped;
                        OnPlatformMediaStateChanged(EventArgs.Empty);
                    }
                    break;
            }
        }

        internal void MoveNext()
        {
            Stop();
            NextSong(1);
        }

        internal void MovePrevious()
        {
            Stop();
            NextSong(-1);
        }

        private void NextSong(int direction)
        {
            if (PlatformIsRepeating && _queue.ActiveSongIndex >= _queue.Count - 1)
            {
                _queue.ActiveSongIndex = 0;
                
                // Setting direction to 0 will force the first song
                // in the queue to be played.
                // if we're on "shuffle", then it'll pick a random one
                // anyway, regardless of the "direction".
                direction = 0;
            }

            var nextSong = _queue.GetNextSong(direction, PlatformIsShuffled);
            if (nextSong != null)
            {
                if (nextSong.IsDisposed)
                    throw new ObjectDisposedException("nextSong");

                PlatformPlaySong(nextSong);
                _state = MediaState.Playing;
                OnPlatformMediaStateChanged(EventArgs.Empty);
            }

            OnPlatformActiveSongChanged(EventArgs.Empty);
        }

        internal void OnSongFinishedPlaying(object sender, EventArgs args)
        {
            Song song = (Song)sender;
            Song activeSong = Queue.ActiveSong;
            System.Diagnostics.Debug.Assert(song == activeSong);

            _numSongsInQueuePlayed++;
            
            if (_numSongsInQueuePlayed >= _queue.Count)
            {
                _numSongsInQueuePlayed = 0;
                if (!PlatformIsRepeating)
                {
                    // Stop
                    MediaState state = State;
                    switch (state)
                    {
                        case MediaState.Playing:
                        case MediaState.Paused:
                            {
                                PlatformStop();
                                _state = MediaState.Stopped;
                                OnPlatformMediaStateChanged(EventArgs.Empty);
                            }
                            break;
                    }

                    OnPlatformActiveSongChanged(EventArgs.Empty);
                    return;
                }
            }

            // Stop
            {
                MediaState state = State;
                switch (state)
                {
                    case MediaState.Playing:
                    case MediaState.Paused:
                        {
                            PlatformStop();
                            _state = MediaState.Stopped;
                            OnPlatformMediaStateChanged(EventArgs.Empty);
                        }
                        break;
                }
            }

            NextSong(1);
        }


        #region IDisposable Members
        
        /// <summary>Releases the resources held by this <see cref="Microsoft.Xna.Framework.Audio.SoundEffect"/>.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {               
                
            }
            
        }

        #endregion
    }
}
