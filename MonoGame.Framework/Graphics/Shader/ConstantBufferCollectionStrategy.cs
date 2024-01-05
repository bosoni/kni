﻿// Copyright (C)2023 Nick Kastellanos

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Platform.Graphics
{
    public abstract class ConstantBufferCollectionStrategy
    {
        protected ConstantBufferCollectionStrategy(int capacity)
        {
        }

        public abstract ConstantBuffer this[int index]
        {
            get;
            set;
        }

        public abstract void Clear();


        internal T ToConcrete<T>() where T : ConstantBufferCollectionStrategy
        {
            return (T)this;
        }
    }
}
