// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Platform.Graphics;
using SharpDX.Direct3D11;


namespace Microsoft.Xna.Framework.Graphics
{
    public abstract partial class Texture
    {
        private Resource _texture;

        internal void SetTextureInternal_DX(Resource textureResource)
        {
            _texture = textureResource;
        }

        private ShaderResourceView _resourceView;


        /// <summary>
        /// Returns a handle to internal device object. Valid only on DirectX platforms.
        /// For usage, convert this to SharpDX.Direct3D11.Resource.
        /// </summary>
        public object Handle
        {
            get
            {
                return this.GetTexture();
            }
        }

        /// <summary>
        /// Gets the handle to a shared resource.
        /// </summary>
        /// <returns>
        /// The handle of the shared resource, or <see cref="IntPtr.Zero"/> if the texture was not
        /// created as a shared resource.
        /// </returns>
        public IntPtr GetSharedHandle()
        {
            using (var resource = GetTexture().QueryInterface<SharpDX.DXGI.Resource>())
                return resource.SharedHandle;
        }

        internal abstract Resource CreateTexture();

        internal Resource GetTexture()
        {
            if (_texture != null)
                return _texture;

            _texture = CreateTexture();
            return _texture;
        }

        internal ShaderResourceView GetShaderResourceView()
        {
            if (_resourceView != null)
                return _resourceView;

            _resourceView = new ShaderResourceView(((ConcreteGraphicsDevice)GraphicsDevice.Strategy).D3DDevice, GetTexture());
            return _resourceView;
        }

        private void PlatformGraphicsDeviceResetting()
        {
            SharpDX.Utilities.Dispose(ref _resourceView);
            SharpDX.Utilities.Dispose(ref _texture);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                SharpDX.Utilities.Dispose(ref _resourceView);
                SharpDX.Utilities.Dispose(ref _texture);
            }

            base.Dispose(disposing);
        }
    }
}

