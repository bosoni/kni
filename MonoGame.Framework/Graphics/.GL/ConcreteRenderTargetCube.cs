﻿// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.OpenGL;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteRenderTargetCube : ConcreteTextureCube, IRenderTargetCubeStrategy, IRenderTargetStrategy,
        IRenderTargetStrategyGL
    {
        private readonly DepthFormat _depthStencilFormat;
        internal int _multiSampleCount;
        private readonly RenderTargetUsage _renderTargetUsage;

        internal ConcreteRenderTargetCube(GraphicsContextStrategy contextStrategy, int size, bool mipMap, RenderTargetUsage usage,
            SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
            : base(contextStrategy, size, mipMap, preferredSurfaceFormat,
                   isRenderTarget: true)
        {
            this._renderTargetUsage = usage;
            this._depthStencilFormat = preferredDepthFormat;

            PlatformConstructTextureCube_rt(contextStrategy, size, mipMap, preferredSurfaceFormat);
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
        #endregion IRenderTarget2DStrategy


        int _glColorBuffer;
        int _glDepthBuffer;
        int _glStencilBuffer;

        #region IRenderTargetStrategyGL
        int IRenderTargetStrategyGL.GLTexture { get { return _glTexture; } }
        TextureTarget IRenderTargetStrategyGL.GLTarget { get { return _glTarget; } }
        int IRenderTargetStrategyGL.GLColorBuffer
        {
            get { return _glColorBuffer; }
            set { _glColorBuffer = value; }
        }
        int IRenderTargetStrategyGL.GLDepthBuffer
        {
            get { return _glDepthBuffer; }
            set { _glDepthBuffer = value; }
        }
        int IRenderTargetStrategyGL.GLStencilBuffer
        {
            get { return _glStencilBuffer; }
            set { _glStencilBuffer = value; }
        }

        TextureTarget IRenderTargetStrategyGL.GetFramebufferTarget(int arraySlice)
        {
            return TextureTarget.TextureCubeMapPositiveX + arraySlice;
        }
        #endregion IRenderTargetStrategyGL


        private void PlatformConstructTextureCube_rt(GraphicsContextStrategy contextStrategy, int size, bool mipMap, SurfaceFormat format)
        {
            base.PlatformConstructTextureCube(contextStrategy, size, mipMap, format);
        }

    }
}
