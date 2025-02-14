// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public class PresentationParameters
    {
        #region Constants

        public const int DefaultPresentRate = 60;

        #endregion Constants

        #region Private Fields

        private SurfaceFormat _backBufferFormat = SurfaceFormat.Color;
        private DepthFormat _depthStencilFormat = DepthFormat.None;
        private int _backBufferWidth;
        private int _backBufferHeight;
        private int _multiSampleCount;
        private RenderTargetUsage _renderTargetUsage = RenderTargetUsage.DiscardContents;
        private PresentInterval _presentationInterval = PresentInterval.Default;
        private DisplayOrientation _displayOrientation= DisplayOrientation.Default;
        private bool _isFullScreen;
        private bool _hardwareModeSwitch = true;
        private IntPtr _deviceWindowHandle;

        private bool _useDebugLayers;
        private DriverType _useDriverType;

        #endregion Private Fields

        #region Constructors

        /// <summary>
        /// Create a <see cref="PresentationParameters"/> instance with default values for all properties.
        /// </summary>
        public PresentationParameters()
        {
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Get or set the format of the back buffer.
        /// </summary>
        public SurfaceFormat BackBufferFormat
        {
            get { return _backBufferFormat; }
            set { _backBufferFormat = value; }
        }

        /// <summary>
        /// Get or set the height of the back buffer.
        /// </summary>
        public int BackBufferHeight
        {
            get { return _backBufferHeight; }
            set { _backBufferHeight = value; }
        }

        /// <summary>
        /// Get or set the width of the back buffer.
        /// </summary>
        public int BackBufferWidth
        {
            get { return _backBufferWidth; }
            set { _backBufferWidth = value; }
        }

        /// <summary>
        /// Get the bounds of the back buffer.
        /// </summary>
        public Rectangle Bounds
        {
            get { return new Rectangle(0, 0, _backBufferWidth, _backBufferHeight); }
        }

        /// <summary>
        /// Get or set the handle of the window that will present the back buffer.
        /// </summary>
        public IntPtr DeviceWindowHandle
        {
            get { return _deviceWindowHandle; }
            set { _deviceWindowHandle = value; }
        }

        /// <summary>
        /// Get or set the depth stencil format for the back buffer.
        /// </summary>
        public DepthFormat DepthStencilFormat
        {
            get { return _depthStencilFormat; }
            set { _depthStencilFormat = value; }
        }

        /// <summary>
        /// Get or set a value indicating if we are in full screen mode.
        /// </summary>
        public bool IsFullScreen
        {
            get { return _isFullScreen; }
            set { _isFullScreen = value; }
        }
        
        /// <summary>
        /// If <code>true</code> the <see cref="GraphicsDevice"/> will do a mode switch
        /// when going to full screen mode. If <code>false</code> it will instead do a
        /// soft full screen by maximizing the window and making it borderless.
        /// </summary>
        public bool HardwareModeSwitch
        {
            get { return _hardwareModeSwitch; }
            set { _hardwareModeSwitch = value; }
        }

        /// <summary>
        /// Get or set the multisample count for the back buffer.
        /// </summary>
        public int MultiSampleCount
        {
            get { return _multiSampleCount; }
            set { _multiSampleCount = value; }
        }
        
        /// <summary>
        /// Get or set the presentation interval.
        /// </summary>
        public PresentInterval PresentationInterval
        {
            get { return _presentationInterval; }
            set { _presentationInterval = value; }
        }

        /// <summary>
        /// Get or set the display orientation.
        /// </summary>
        public DisplayOrientation DisplayOrientation
        {
            get { return _displayOrientation; }
            set { _displayOrientation = value; }
        }
        
        /// <summary>
        /// Get or set the RenderTargetUsage for the back buffer.
        /// Determines if the back buffer is cleared when it is set as the
        /// render target by the <see cref="GraphicsDevice"/>.
        /// <see cref="GraphicsDevice"/> target.
        /// </summary>
        public RenderTargetUsage RenderTargetUsage
        {
            get { return _renderTargetUsage; }
            set { _renderTargetUsage = value; }
        }


        /// <summary>
        /// Used to request the graphics device should be created
        /// with debugging features enabled.
        /// </summary>
        /// <remarks>Usable only on DirectX platforms.</remarks>
        public bool UseDebugLayers
        {
            get { return _useDebugLayers; }
            set { _useDebugLayers = value; }
        }

        /// <summary>
        /// Used to request creation of a specific kind of driver.
        /// </summary>
        /// <remarks>
        /// These values only work on DirectX platforms and must be defined before the graphics device
        /// is created. <see cref="DriverType.Hardware"/> by default.
        /// </remarks>
        public DriverType UseDriverType
        {
            get { return _useDriverType; }
            set { _useDriverType = value; }
        }


        /// <summary>
        /// Defines the driver type for graphics adapter.
        /// </summary>
        /// <remarks>Usable only on DirectX platforms.</remarks>
        public enum DriverType
        {
            /// <summary>
            /// Hardware device been used for rendering. Maximum speed and performance.
            /// </summary>
            Hardware,
            /// <summary>
            /// Emulates the hardware device on CPU. Slowly, only for testing.
            /// </summary>
            Reference,
            /// <summary>
            /// Useful when <see cref="DriverType.Hardware"/> acceleration does not work.
            /// </summary>
            FastSoftware
        }

        #endregion Properties


        #region Methods

        /// <summary>
        /// Create a copy of this <see cref="PresentationParameters"/> instance.
        /// </summary>
        /// <returns></returns>
        public PresentationParameters Clone()
        {
            PresentationParameters clone = new PresentationParameters();
            clone._backBufferFormat = this._backBufferFormat;
            clone._depthStencilFormat = this._depthStencilFormat;
            clone._backBufferHeight = this._backBufferHeight;
            clone._backBufferWidth = this._backBufferWidth;
            clone._multiSampleCount = this._multiSampleCount;
            clone._renderTargetUsage = this._renderTargetUsage;
            clone._presentationInterval = this._presentationInterval;
            clone._displayOrientation = this._displayOrientation;
            clone._isFullScreen = this._isFullScreen;
            clone._hardwareModeSwitch = this._hardwareModeSwitch;
            clone._deviceWindowHandle = this._deviceWindowHandle;

            clone._useDebugLayers = this._useDebugLayers;
            clone._useDriverType = this._useDriverType;

            return clone;
        }

        #endregion Methods

    }
}
