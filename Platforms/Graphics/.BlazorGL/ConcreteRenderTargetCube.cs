﻿// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.Utilities;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteRenderTargetCube : ConcreteTextureCube, IRenderTargetCubeStrategy, IRenderTargetStrategy,
        IRenderTargetStrategyGL
    {
        private readonly DepthFormat _depthStencilFormat;
        internal int _multiSampleCount;
        private readonly RenderTargetUsage _renderTargetUsage;
        private bool _isContentLost;


        internal ConcreteRenderTargetCube(GraphicsContextStrategy contextStrategy, int size, bool mipMap, RenderTargetUsage usage,
            SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
            : base(contextStrategy, size, mipMap, preferredSurfaceFormat,
                   isRenderTarget: true)
        {
            this._renderTargetUsage = usage;
            this._depthStencilFormat = preferredDepthFormat;

            var GL = contextStrategy.ToConcrete<ConcreteGraphicsContext>().GL;

            int maxMultiSampleCount = ((IPlatformGraphicsContext)contextStrategy.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().GetMaxMultiSampleCount(preferredSurfaceFormat);
            this._multiSampleCount = TextureHelpers.GetClampedMultiSampleCount(this.Format, preferredMultiSampleCount, maxMultiSampleCount);

            PlatformConstructTextureCube_rt(contextStrategy, size, mipMap, preferredSurfaceFormat);

            PlatformConstructRenderTargetCube(contextStrategy, mipMap, preferredDepthFormat, _multiSampleCount);
        }


        #region IRenderTargetStrategy
        public DepthFormat DepthStencilFormat
        {
            get { return _depthStencilFormat; }
        }

        public int MultiSampleCount
        {
            get { return _multiSampleCount; }
        }

        public RenderTargetUsage RenderTargetUsage
        {
            get { return _renderTargetUsage; }
        }

        public bool IsContentLost
        {
            get { return _isContentLost; }
        }

        public void ResolveSubresource(GraphicsContextStrategy graphicsContextStrategy)
        {
            if (this.MultiSampleCount > 1)
            {
            }
        }
        #endregion IRenderTargetStrategy


        WebGLRenderbuffer _glColorBuffer;
        WebGLRenderbuffer _glDepthBuffer;
        WebGLRenderbuffer _glStencilBuffer;

        #region IRenderTargetStrategyGL
        WebGLTexture IRenderTargetStrategyGL.GLTexture { get { return _glTexture; } }
        WebGLTextureTarget IRenderTargetStrategyGL.GLTarget { get { return _glTarget; } }
        WebGLRenderbuffer IRenderTargetStrategyGL.GLColorBuffer
        {
            get { return _glColorBuffer; }
            set { _glColorBuffer = value; }
        }
        WebGLRenderbuffer IRenderTargetStrategyGL.GLDepthBuffer
        {
            get { return _glDepthBuffer; }
            set { _glDepthBuffer = value; }
        }
        WebGLRenderbuffer IRenderTargetStrategyGL.GLStencilBuffer
        {
            get { return _glStencilBuffer; }
            set { _glStencilBuffer = value; }
        }

        WebGLTextureTarget IRenderTargetStrategyGL.GetFramebufferTarget(int arraySlice)
        {
            return WebGLTextureTarget.TEXTURE_CUBE_MAP_POSITIVE_X + arraySlice;
        }
        #endregion IRenderTargetStrategyGL


        private void PlatformConstructTextureCube_rt(GraphicsContextStrategy contextStrategy, int size, bool mipMap, SurfaceFormat format)
        {
            base.PlatformConstructTextureCube(contextStrategy, size, mipMap, format);
        }

        private void PlatformConstructRenderTargetCube(GraphicsContextStrategy contextStrategy, bool mipMap,
            DepthFormat preferredDepthFormat, int multiSampleCount)
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

            }

            ConcreteTexture.PlatformDeleteRenderTarget((IRenderTargetStrategyGL)this, ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy);

            base.Dispose(disposing);
        }
    }
}
