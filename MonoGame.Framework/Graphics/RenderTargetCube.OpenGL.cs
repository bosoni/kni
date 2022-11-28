﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Platform.Graphics;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTargetCube : IRenderTargetGL
    {
        int IRenderTargetGL.GLTexture { get { return glTexture; } }
        TextureTarget IRenderTargetGL.GLTarget { get { return glTarget; } }
        int IRenderTargetGL.GLColorBuffer { get; set; }
        int IRenderTargetGL.GLDepthBuffer { get; set; }
        int IRenderTargetGL.GLStencilBuffer { get; set; }

        TextureTarget IRenderTargetGL.GetFramebufferTarget(int arraySlice)
        {
            return TextureTarget.TextureCubeMapPositiveX + arraySlice;
        }

        private void PlatformConstruct(
            GraphicsDevice graphicsDevice, bool mipMap, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage)
        {
            Threading.EnsureUIThread();
            {
                graphicsDevice.PlatformCreateRenderTarget(
                    this, size, size, mipMap, this.Format, preferredDepthFormat, preferredMultiSampleCount, usage);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (GraphicsDevice != null)
                {
                    GraphicsDevice.PlatformDeleteRenderTarget(this);
                }
            }

            base.Dispose(disposing);
        }
    }
}
