﻿// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteRenderTargetSwapChain : IRenderTarget2DStrategy, IRenderTargetStrategy, ITexture2DStrategy,
        IRenderTargetStrategyDX11
    {
        private readonly DepthFormat _depthStencilFormat;
        internal int _multiSampleCount;
        private readonly RenderTargetUsage _renderTargetUsage;

        internal ConcreteRenderTargetSwapChain(GraphicsContextStrategy contextStrategy, int width, int height, bool mipMap, int arraySize, RenderTargetUsage usage,
            DepthFormat preferredDepthFormat)
        {
            this._renderTargetUsage = usage;
            this._depthStencilFormat = preferredDepthFormat;
        }


        #region ITextureStrategy
        public SurfaceFormat Format
        {
            get { throw new NotImplementedException(); }
        }

        public int LevelCount
        {
            get { throw new NotImplementedException(); }
        }
        #endregion ITextureStrategy


        #region ITexture2DStrategy
        public int Width
        {
            get { throw new NotImplementedException(); }
        }

        public int Height
        {
            get { throw new NotImplementedException(); }
        }

        public int ArraySize
        {
            get { throw new NotImplementedException(); }
        }

        public Rectangle Bounds
        {
            get { throw new NotImplementedException(); }
        }

        public IntPtr GetSharedHandle()
        {
            throw new NotImplementedException();
        }

        public void SetData<T>(int level, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            throw new NotImplementedException();
        }

        public void SetData<T>(int level, int arraySlice, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            throw new NotImplementedException();
        }

        public void GetData<T>(int level, int arraySlice, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            throw new NotImplementedException();
        }

        void ITexture2DStrategy.SaveAsJpeg(Stream stream, int width, int height)
        {
            throw new NotImplementedException();
        }

        void ITexture2DStrategy.SaveAsPng(Stream stream, int width, int height)
        {
            throw new NotImplementedException();
        }
        #endregion ITexture2DStrategy


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


        internal D3D11.RenderTargetView[] _renderTargetViews;
        internal D3D11.DepthStencilView[] _depthStencilViews;


        D3D11.RenderTargetView IRenderTargetStrategyDX11.GetRenderTargetView(int arraySlice)
        {
            return _renderTargetViews[arraySlice];
        }

        D3D11.DepthStencilView IRenderTargetStrategyDX11.GetDepthStencilView(int arraySlice)
        {
            return _depthStencilViews[0];
        }

    }
}
