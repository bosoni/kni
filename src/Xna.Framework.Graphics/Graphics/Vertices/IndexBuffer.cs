﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

﻿// Copyright (C)2023 Nick Kastellanos

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Platform.Graphics;
using Microsoft.Xna.Platform.Graphics.Utilities;

namespace Microsoft.Xna.Framework.Graphics
{
    public class IndexBuffer : GraphicsResource
        , IPlatformIndexBuffer
    {
        internal IndexBufferStrategy _strategy;

        IndexBufferStrategy IPlatformIndexBuffer.Strategy { get { return _strategy; } }

        public BufferUsage BufferUsage
        {
            get { return _strategy.BufferUsage; }
        }

        public int IndexCount
        {
            get { return _strategy.IndexCount; }
        }

        public IndexElementSize IndexElementSize
        {
            get { return _strategy.IndexElementSize; }
        }


        public IndexBuffer(GraphicsDevice graphicsDevice, Type indexType, int indexCount, BufferUsage usage) :
            this(graphicsDevice, SizeForType(graphicsDevice, indexType), indexCount, usage)
        {
        }

        public IndexBuffer(GraphicsDevice graphicsDevice, IndexElementSize indexElementSize, int indexCount, BufferUsage usage)
            : base()
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException("graphicsDevice");
            if (graphicsDevice.GraphicsProfile == GraphicsProfile.Reach && indexElementSize == IndexElementSize.ThirtyTwoBits)
                throw new NotSupportedException("Reach profile does not support 32 bit indices");

            _strategy = ((IPlatformGraphicsContext)graphicsDevice.CurrentContext).Strategy.CreateIndexBufferStrategy(indexElementSize, indexCount, usage);
            SetResourceStrategy((IGraphicsResourceStrategy)_strategy);
        }


        protected IndexBuffer()
            : base()
        {
        }


        /// <summary>
        /// Gets the relevant IndexElementSize enum value for the given type.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="type">The type to use for the index buffer</param>
        /// <returns>The IndexElementSize enum value that matches the type</returns>
        internal static IndexElementSize SizeForType(GraphicsDevice graphicsDevice, Type type)
        {
            switch (Marshal.SizeOf(type))
            {
                case 2:
                    return IndexElementSize.SixteenBits;
                case 4:
                    if (graphicsDevice.GraphicsProfile == GraphicsProfile.Reach)
                        throw new NotSupportedException("The profile does not support an elementSize of IndexElementSize.ThirtyTwoBits; use IndexElementSize.SixteenBits or a type that has a size of two bytes.");
                    return IndexElementSize.ThirtyTwoBits;
                default:
                    throw new ArgumentOutOfRangeException("type","Index buffers can only be created for types that are sixteen or thirty two bits in length");
            }
        }

        public void GetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (data.Length < (startIndex + elementCount))
                throw new InvalidOperationException("The array specified in the data parameter is not the correct size for the amount of data requested.");
            if (BufferUsage == BufferUsage.WriteOnly)
                throw new NotSupportedException("This IndexBuffer was created with a usage type of BufferUsage.WriteOnly. Calling GetData on a resource that was created with BufferUsage.WriteOnly is not supported.");

            _strategy.GetData<T>(offsetInBytes, data, startIndex, elementCount);
        }

        public void GetData<T>(T[] data, int startIndex, int elementCount) where T : struct
        {
            this.GetData<T>(0, data, startIndex, elementCount);
        }

        public void GetData<T>(T[] data) where T : struct
        {
            this.GetData<T>(0, data, 0, data.Length);
        }

        public void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct
        {
            SetDataInternal<T>(offsetInBytes, data, startIndex, elementCount, SetDataOptions.None);
        }
                
        public void SetData<T>(T[] data, int startIndex, int elementCount) where T : struct
        {
            SetDataInternal<T>(0, data, startIndex, elementCount, SetDataOptions.None);
        }
        
        public void SetData<T>(T[] data) where T : struct
        {
            SetDataInternal<T>(0, data, 0, data.Length, SetDataOptions.None);
        }

        protected void SetDataInternal<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (data.Length < (startIndex + elementCount))
                throw new InvalidOperationException("The array specified in the data parameter is not the correct size for the amount of data requested.");

            _strategy.SetData<T>(offsetInBytes, data, startIndex, elementCount, options);
        }
    }
}
