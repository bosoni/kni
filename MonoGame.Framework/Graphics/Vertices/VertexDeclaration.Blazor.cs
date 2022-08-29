// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using nkast.Wasm.Canvas.WebGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class VertexDeclaration
    {
        private IWebGLRenderingContext GL { get { return GraphicsDevice._glContext; } }

        private readonly Dictionary<int, VertexDeclarationAttributeInfo> _shaderAttributeInfo = new Dictionary<int, VertexDeclarationAttributeInfo>();

        internal VertexDeclarationAttributeInfo GetAttributeInfo(Shader shader, int programHash)
        {
            VertexDeclarationAttributeInfo attrInfo;
            if (_shaderAttributeInfo.TryGetValue(programHash, out attrInfo))
                return attrInfo;

            // Get the vertex attribute info and cache it
            attrInfo = new VertexDeclarationAttributeInfo(GraphicsDevice.MaxVertexAttributes);

            foreach (var ve in InternalVertexElements)
            {
                var attributeLocation = shader.GetAttribLocation(ve.VertexElementUsage, ve.UsageIndex);
                // XNA appears to ignore usages it can't find a match for, so we will do the same
                if (attributeLocation < 0)
                    continue;

                attrInfo.Elements.Add(new VertexDeclarationAttributeInfo.Element
                {
                    Offset = ve.Offset,
                    AttributeLocation = attributeLocation,
                    NumberOfElements = OpenGLNumberOfElements(ve.VertexElementFormat),
                    VertexAttribPointerType = OpenGLVertexAttribPointerType(ve.VertexElementFormat),
                    Normalized = OpenGLVertexAttribNormalized(ve),
                });
                attrInfo.EnabledAttributes[attributeLocation] = true;
            }

            _shaderAttributeInfo.Add(programHash, attrInfo);
            return attrInfo;
        }


		internal void Apply(Shader shader, int offset, int programHash)
		{
            var attrInfo = GetAttributeInfo(shader, programHash);

            // Apply the vertex attribute info
            for (int i=0; i< attrInfo.Elements.Count; i++)
            {
                var element = attrInfo.Elements[i];
                GL.VertexAttribPointer(element.AttributeLocation,
                    element.NumberOfElements,
                    element.VertexAttribPointerType,
                    element.Normalized,
                    VertexStride,
                    (offset + element.Offset));
                GraphicsExtensions.CheckGLError();

                if (GraphicsDevice.GraphicsCapabilities.SupportsInstancing)
                {
                    //GL2.VertexAttribDivisor(element.AttributeLocation, 0);
                    GraphicsExtensions.CheckGLError();
                }
            }
            GraphicsDevice.SetVertexAttributeArray(attrInfo.EnabledAttributes);
		    GraphicsDevice._attribsDirty = true;
		}

        private static int OpenGLNumberOfElements(VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                    return 1;
                case VertexElementFormat.Vector2:
                    return 2;
                case VertexElementFormat.Vector3:
                    return 3;
                case VertexElementFormat.Vector4:
                    return 4;
                case VertexElementFormat.Color:
                    return 4;
                case VertexElementFormat.Byte4:
                    return 4;
                case VertexElementFormat.Short2:
                    return 2;
                case VertexElementFormat.Short4:
                    return 4;
                case VertexElementFormat.NormalizedShort2:
                    return 2;
                case VertexElementFormat.NormalizedShort4:
                    return 4;
                case VertexElementFormat.HalfVector2:
                    return 2;
                case VertexElementFormat.HalfVector4:
                    return 4;
                default:
                    throw new ArgumentException();
            }
        }

        private static WebGLDataType OpenGLVertexAttribPointerType(VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                    return WebGLDataType.FLOAT;

                case VertexElementFormat.Vector2:
                    return WebGLDataType.FLOAT;

                case VertexElementFormat.Vector3:
                    return WebGLDataType.FLOAT;

                case VertexElementFormat.Vector4:
                    return WebGLDataType.FLOAT;

                case VertexElementFormat.Color:
                    return WebGLDataType.UBYTE;

                case VertexElementFormat.Byte4:
                    return WebGLDataType.UBYTE;

                case VertexElementFormat.Short2:
                    return WebGLDataType.SHORT;

                case VertexElementFormat.Short4:
                    return WebGLDataType.SHORT;

                case VertexElementFormat.NormalizedShort2:
                    return WebGLDataType.SHORT;

                case VertexElementFormat.NormalizedShort4:
                    return WebGLDataType.SHORT;

                default:
                    throw new ArgumentException();
            }
        }

        private static bool OpenGLVertexAttribNormalized(VertexElement element)
        {
            // TODO: This may or may not be the right behavor.  
            //
            // For instance the VertexElementFormat.Byte4 format is not supposed
            // to be normalized, but this line makes it so.
            //
            // The question is in MS XNA are types normalized based on usage or
            // normalized based to their format?
            //
            if (element.VertexElementUsage == VertexElementUsage.Color)
                return true;

            switch (element.VertexElementFormat)
            {
                case VertexElementFormat.NormalizedShort2:
                case VertexElementFormat.NormalizedShort4:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Vertex attribute information for a particular shader/vertex declaration combination.
        /// </summary>
        internal class VertexDeclarationAttributeInfo
        {
            internal bool[] EnabledAttributes;

            internal class Element
            {
                public int Offset;
                public int AttributeLocation;
                public int NumberOfElements;
                public WebGLDataType VertexAttribPointerType;
                public bool Normalized;
            }

            internal List<Element> Elements;

            internal VertexDeclarationAttributeInfo(int maxVertexAttributes)
            {
                EnabledAttributes = new bool[maxVertexAttributes];
                Elements = new List<Element>();
            }
        }
    }
}
