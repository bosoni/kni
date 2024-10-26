﻿// Copyright (C)2024 Nick Kastellanos

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    public enum ContentCompression
    {
        Uncompressed = 0,
        //ZLX        = 1,
        LZ4          = 2,
        
        LegacyLZ4    = 255,
    }
}