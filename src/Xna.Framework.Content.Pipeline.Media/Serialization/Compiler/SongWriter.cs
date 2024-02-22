﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    [ContentTypeWriter]
    class SongWriter : ContentTypeWriter<SongContent>
    {
        /// <summary>
        /// Writes the value to the output.
        /// </summary>
        /// <param name="output">The output writer object.</param>
        /// <param name="value">The value to write to the output.</param>
        protected override void Write(ContentWriter output, SongContent value)
        {
            output.Write(value.fileName);
            output.WriteObject((int)value.duration.TotalMilliseconds);
        }

        /// <inheritdoc/>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            string readerNamespace = "Microsoft.Xna.Framework.Content";
            string readerName = ".SongReader";
            // From looking at XNA-produced XNBs, it appears built-in
            // type readers don't need assembly qualification.
            string readerAssembly = String.Empty;

            string runtimeReader = readerNamespace + readerName + readerAssembly;
            return runtimeReader;
        }

        /// <inheritdoc/>
        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            string typeNamespace = "Microsoft.Xna.Framework.Media";
            string typeName = ".Song";
            string typeAssembly = ", Microsoft.Xna.Framework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=842cf8be1de50553";

            string runtimeType = typeNamespace + typeName + typeAssembly;
            return runtimeType;
        }
    }
}
