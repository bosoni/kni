﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Platform.Graphics;
using MonoGame.Framework.Utilities;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class IndexBuffer
    {
        private SharpDX.Direct3D11.Buffer _buffer;

        internal SharpDX.Direct3D11.Buffer Buffer
        {
            get
            {
                GenerateIfRequired();
                return _buffer;
            }
        }

        private void PlatformConstructIndexBuffer(IndexElementSize indexElementSize, int indexCount)
        {
            GenerateIfRequired();
        }

        private void PlatformGraphicsDeviceResetting()
        {
            SharpDX.Utilities.Dispose(ref _buffer);
        }

        void GenerateIfRequired()
        {
            if (_buffer != null)
                return;

            // TODO: To use true Immutable resources we would need to delay creation of 
            // the Buffer until SetData() and recreate them if set more than once.

            var sizeInBytes = IndexCount * (this.IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4);

            var accessflags = SharpDX.Direct3D11.CpuAccessFlags.None;
            var resUsage = SharpDX.Direct3D11.ResourceUsage.Default;

            if (_isDynamic)
            {
                accessflags |= SharpDX.Direct3D11.CpuAccessFlags.Write;
                resUsage = SharpDX.Direct3D11.ResourceUsage.Dynamic;
            }

            _buffer = new SharpDX.Direct3D11.Buffer(((ConcreteGraphicsDevice)GraphicsDevice.Strategy).D3DDevice,
                                                        sizeInBytes,
                                                        resUsage,
                                                        SharpDX.Direct3D11.BindFlags.IndexBuffer,
                                                        accessflags,
                                                        SharpDX.Direct3D11.ResourceOptionFlags.None,
                                                        0  // StructureSizeInBytes
                                                        );
        }

        private void PlatformGetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct
        {
            GenerateIfRequired();

            if (_isDynamic)
            {
                throw new NotImplementedException();
            }
            else
            {

                // Copy the texture to a staging resource
                var stagingDesc = _buffer.Description;
                stagingDesc.BindFlags = SharpDX.Direct3D11.BindFlags.None;
                stagingDesc.CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.Read | SharpDX.Direct3D11.CpuAccessFlags.Write;
                stagingDesc.Usage = SharpDX.Direct3D11.ResourceUsage.Staging;
                stagingDesc.OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None;
                using (var stagingBuffer = new SharpDX.Direct3D11.Buffer(((ConcreteGraphicsDevice)GraphicsDevice.Strategy).D3DDevice, stagingDesc))
                {
                    lock (GraphicsDevice.CurrentD3DContext)
                        GraphicsDevice.CurrentD3DContext.CopyResource(_buffer, stagingBuffer);

                    int TsizeInBytes = SharpDX.Utilities.SizeOf<T>();
                    var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                    try
                    {
                        var startBytes = startIndex * TsizeInBytes;
                        var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);
                        SharpDX.DataPointer DataPointer = new SharpDX.DataPointer(dataPtr, elementCount * TsizeInBytes);

                        lock (GraphicsDevice.CurrentD3DContext)
                        {
                            // Map the staging resource to a CPU accessible memory
                            var box = GraphicsDevice.CurrentD3DContext.MapSubresource(stagingBuffer, 0, SharpDX.Direct3D11.MapMode.Read, SharpDX.Direct3D11.MapFlags.None);

                            SharpDX.Utilities.CopyMemory(dataPtr, box.DataPointer + offsetInBytes, elementCount * TsizeInBytes);

                            // Make sure that we unmap the resource in case of an exception
                            GraphicsDevice.CurrentD3DContext.UnmapSubresource(stagingBuffer, 0);
                        }
                    }
                    finally
                    {
                        dataHandle.Free();
                    }
                }
            }
        }

        private void PlatformSetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct
        {
            GenerateIfRequired();

            if (_isDynamic)
            {
                // We assume discard by default.
                var mode = SharpDX.Direct3D11.MapMode.WriteDiscard;
                if ((options & SetDataOptions.NoOverwrite) == SetDataOptions.NoOverwrite)
                    mode = SharpDX.Direct3D11.MapMode.WriteNoOverwrite;

                lock (GraphicsDevice.CurrentD3DContext)
                {
                    var dataBox = GraphicsDevice.CurrentD3DContext.MapSubresource(_buffer, 0, mode, SharpDX.Direct3D11.MapFlags.None);
                    SharpDX.Utilities.Write(IntPtr.Add(dataBox.DataPointer, offsetInBytes), data, startIndex,
                                            elementCount);
                    GraphicsDevice.CurrentD3DContext.UnmapSubresource(_buffer, 0);
                }
            }
            else
            {
                var elementSizeInBytes = ReflectionHelpers.SizeOf<T>();
                var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                try
                {
                    var startBytes = startIndex * elementSizeInBytes;
                    var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

                    var box = new SharpDX.DataBox(dataPtr, elementCount * elementSizeInBytes, 0);

                    var region = new SharpDX.Direct3D11.ResourceRegion();
                    region.Top = 0;
                    region.Front = 0;
                    region.Back = 1;
                    region.Bottom = 1;
                    region.Left = offsetInBytes;
                    region.Right = offsetInBytes + (elementCount * elementSizeInBytes);

                    lock (GraphicsDevice.CurrentD3DContext)
                        GraphicsDevice.CurrentD3DContext.UpdateSubresource(box, _buffer, 0, region);
                }
                finally
                {
                    dataHandle.Free();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                SharpDX.Utilities.Dispose(ref _buffer);

            base.Dispose(disposing);
        }
	}
}
