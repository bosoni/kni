﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Platform.Media;


namespace Microsoft.Xna.Framework.Media
{
    public partial class MediaLibrary : IDisposable
        , IPlatformMediaLibrary
    {
        private MediaLibraryStrategy _strategy;
        bool _isDisposed;

        MediaLibraryStrategy IPlatformMediaLibrary.Strategy { get { return _strategy; } }
        public bool IsDisposed { get { return _isDisposed; } }

        public MediaSource MediaSource { get { return _strategy.MediaSource; } }
        public AlbumCollection Albums { get { return _strategy.Albums;  } }
        public SongCollection Songs { get { return _strategy.Songs; } }
        //public ArtistCollection Artists { get; private set; }
        //public GenreCollection Genres { get; private set; }
        //public PlaylistCollection Playlists { get; private set; }


        public MediaLibrary()
        {
            _strategy = MediaFactory.Current.CreateMediaLibraryStrategy();
        }

        public MediaLibrary(MediaSource mediaSource)
        {
            _strategy = MediaFactory.Current.CreateMediaLibraryStrategy(mediaSource);
        }

        // Trick to prevent the linker removing the code, but not actually execute the code
        static bool _trimmingFalseFlag = false;
        internal static void PreserveMediaContentTypeReaders()
        {
#pragma warning disable 0219, 0649
            // Trick to prevent the linker removing the code, but not actually execute the code
            if (_trimmingFalseFlag)
            {
                // Dummy variables required for it to work with trimming ** DO NOT DELETE **
                // This forces the classes not to be optimized out when deploying with trimming

                // Framework.Media types
                var hSongReader = new SongReader();
                var hVideoReader = new VideoReader();
            }
#pragma warning restore 0219, 0649
        }

        /// <summary>
        /// Load the contents of MediaLibrary. This blocking call might take up to a few minutes depending on the platform and the size of the user's music library.
        /// </summary>
        /// <param name="progressCallback">Callback that reports back the progress of the music library loading in percents (0-100).</param>
        public void Load(Action<int> progressCallback = null)
        {
            _strategy.Load(progressCallback);
        }
        
        
        public void SavePicture(string name, byte[] imageBuffer)
        {
            _strategy.SavePicture(name, imageBuffer);
        }

        public void SavePicture(string name, Stream source)
        {
            _strategy.SavePicture(name, source);
        }



        #region IDisposable Implementation

        ~MediaLibrary()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _strategy.Dispose();
                }

                _isDisposed = true;
            }
        }

        #endregion
    }
}

