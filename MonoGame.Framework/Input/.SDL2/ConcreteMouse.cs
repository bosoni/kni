﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021-2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Platform.Input
{
    public sealed class ConcreteMouse : MouseStrategy
    {
        private IntPtr _wndHandle = IntPtr.Zero;

        private Sdl SDL { get { return Sdl.Current; } }

        internal int ScrollX;
        internal int ScrollY;

        public override IntPtr PlatformGetWindowHandle()
        {
            return _wndHandle;
        }

        public override void PlatformSetWindowHandle(IntPtr windowHandle)
        {
            _wndHandle = windowHandle;
        }

        public override bool PlatformIsRawInputAvailable()
        {
            return true;
        }

        public override MouseState PlatformGetState()
        {
            IntPtr wndHandle = _wndHandle;
            if (wndHandle != IntPtr.Zero)
            {
                SdlGameWindow gameWindow = SdlGameWindow.FromHandle(wndHandle);

                int winFlags = SDL.WINDOW.GetWindowFlags(wndHandle);

                int x, y;
                int wndx = 0, wndy = 0;
                Sdl.Mouse.Button state = SDL.MOUSE.GetGlobalState(out x, out y);
                SDL.WINDOW.GetPosition(wndHandle, out wndx, out wndy);
                x = x - wndx;
                y = y - wndy;

                gameWindow._mouseState.LeftButton = (state & Sdl.Mouse.Button.Left) != 0 ? ButtonState.Pressed : ButtonState.Released;
                gameWindow._mouseState.MiddleButton = (state & Sdl.Mouse.Button.Middle) != 0 ? ButtonState.Pressed : ButtonState.Released;
                gameWindow._mouseState.RightButton = (state & Sdl.Mouse.Button.Right) != 0 ? ButtonState.Pressed : ButtonState.Released;
                gameWindow._mouseState.XButton1 = (state & Sdl.Mouse.Button.X1Mask) != 0 ? ButtonState.Pressed : ButtonState.Released;
                gameWindow._mouseState.XButton2 = (state & Sdl.Mouse.Button.X2Mask) != 0 ? ButtonState.Pressed : ButtonState.Released;

                gameWindow._mouseState.HorizontalScrollWheelValue = ScrollX;
                gameWindow._mouseState.ScrollWheelValue = ScrollY;

                gameWindow._mouseState.X = x;
                gameWindow._mouseState.Y = y;

                return gameWindow._mouseState;
            }
            else
                return new MouseState();
        }

        public override void PlatformSetPosition(int x, int y)
        {
            SdlGameWindow gameWindow = SdlGameWindow.FromHandle(_wndHandle);

            gameWindow._mouseState.X = x;
            gameWindow._mouseState.Y = y;

            SDL.MOUSE.WarpInWindow(gameWindow.Handle, x, y);
        }

        public override void PlatformSetCursor(MouseCursor cursor)
        {
            SDL.MOUSE.SetCursor(cursor.Handle);
        }

    }
}
