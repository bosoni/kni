﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.Utilities;
using Microsoft.Xna.Platform.Graphics.OpenGL;


namespace Microsoft.Xna.Platform.Graphics
{
    public class ConcreteIndexBuffer : ConcreteIndexBufferGL
    {
        public ConcreteIndexBuffer(GraphicsContextStrategy contextStrategy, IndexElementSize indexElementSize, int indexCount, BufferUsage usage, bool isDynamic)
            : base(contextStrategy, indexElementSize, indexCount, usage, isDynamic)
        {
        }


        public ConcreteIndexBuffer(GraphicsContextStrategy contextStrategy, IndexElementSize indexElementSize, int indexCount, BufferUsage usage)
            : base(contextStrategy, indexElementSize, indexCount, usage)
        {
        }

        public override void GetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount)
        {
            ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContextGL>().EnsureContextCurrentThread();

            Debug.Assert(GLIndexBuffer != 0);

            var GL = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

            int elementSizeInBytes = ReflectionHelpers.SizeOf<T>();
            int sizeInBytes = elementCount * elementSizeInBytes;

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, GLIndexBuffer);
            GL.CheckGLError();
            ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy._indexBufferDirty = true;

            IntPtr srcPtr = GL.MapBuffer(BufferTarget.ElementArrayBuffer, BufferAccess.ReadOnly);
            GL.CheckGLError();
            srcPtr = srcPtr + offsetInBytes;

            try
            {
                if (typeof(T) == typeof(byte))
                {
                    byte[] dataBuffer = data as byte[];
                    Marshal.Copy(srcPtr, dataBuffer, startIndex * elementSizeInBytes, sizeInBytes);
                }
                else
                {
                    byte[] tmpBuffer = new byte[sizeInBytes];
                    Marshal.Copy(srcPtr, tmpBuffer, 0, tmpBuffer.Length);
                    // TODO: BlockCopy doesn't work with struct arrays. 
                    //       throws ArgumentException: "Object must be an array of primitives. (Parameter 'dst')"
                    //       see: ShouldSetAndGetStructData() test
                    Buffer.BlockCopy(tmpBuffer, 0, data, startIndex * elementSizeInBytes, sizeInBytes);
                }
            }
            finally
            {
                GL.UnmapBuffer(BufferTarget.ElementArrayBuffer);
                GL.CheckGLError();
            }
        }
    }

}
