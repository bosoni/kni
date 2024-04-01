﻿// Copyright (C)2021-2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Platform.Input
{
    public sealed class ConcreteMouseCursor : MouseCursorStrategy
    {

        public ConcreteMouseCursor(MouseCursorStrategy.MouseCursorType cursorType)
        {
            this._cursorType = cursorType;
            this._handle = IntPtr.Zero;
        }


        public ConcreteMouseCursor(byte[] data, int w, int h, int originx, int originy)
        {
            throw new PlatformNotSupportedException();
        }

        protected override void Dispose(bool dispose)
        {
            if (dispose)
            {
            }

            base.Dispose(dispose);
        }

    }
}
