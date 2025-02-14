﻿#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009-2012 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software,
you accept this license. If you do not accept the license, do not use the
software.

1. Definitions

The terms "reproduce," "reproduction," "derivative works," and "distribution"
have the same meaning here as under U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the
software.

A "contributor" is any person that distributes its contribution under this
license.

"Licensed patents" are a contributor's patent claims that read directly on its
contribution.

2. Grant of Rights

(A) Copyright Grant- Subject to the terms of this license, including the
license conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free copyright license to reproduce its
contribution, prepare derivative works of its contribution, and distribute its
contribution or any derivative works that you create.

(B) Patent Grant- Subject to the terms of this license, including the license
conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free license under its licensed patents to
make, have made, use, sell, offer for sale, import, and/or otherwise dispose of
its contribution in the software or derivative works of the contribution in the
software.

3. Conditions and Limitations

(A) No Trademark License- This license does not grant you rights to use any
contributors' name, logo, or trademarks.

(B) If you bring a patent claim against any contributor over patents that you
claim are infringed by the software, your patent license from such contributor
to the software ends automatically.

(C) If you distribute any portion of the software, you must retain all
copyright, patent, trademark, and attribution notices that are present in the
software.

(D) If you distribute any portion of the software in source code form, you may
do so only under this license by including a complete copy of this license with
your distribution. If you distribute any portion of the software in compiled or
object code form, you may only do so under a license that complies with this
license.

(E) The software is licensed "as-is." You bear the risk of using it. The
contributors give no express warranties, guarantees or conditions. You may have
additional consumer rights under your local laws which this license cannot
change. To the extent permitted under your local laws, the contributors exclude
the implied warranties of merchantability, fitness for a particular purpose and
non-infringement.
*/


#endregion License

using System;
using System.Drawing;

using CoreAnimation;
using Foundation;
using ObjCRuntime;
using OpenGLES;
using UIKit;
using CoreGraphics;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Platform;
using Microsoft.Xna.Platform.Graphics.OpenGL;
using Microsoft.Xna.Platform.Graphics;


namespace Microsoft.Xna.Framework
{

    [Register("iOSGameView")]
    partial class iOSGameView : UIView
    {
        private readonly ConcreteGame _concreteGame;
        private int _colorbuffer;
        private int _depthbuffer;
        private int _framebuffer;

        #region Construction/Destruction
        public iOSGameView(ConcreteGame concreteGame, CGRect frame) : base(frame)
        {
            if (concreteGame == null)
                throw new ArgumentNullException("concreteGame");

            _concreteGame = concreteGame;

            #if !TVOS
            MultipleTouchEnabled = true;
            #endif

            Opaque = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_eaglContext != null)
                {
                    AssertNotDisposed();

                    _eaglContext.Dispose();
                    _eaglContext = null;
                    _glapi = null;
                }
            }

            base.Dispose(disposing);
            _isDisposed = true;
        }

        #endregion Construction/Destruction

        #region Properties

        private bool _isDisposed;

        public bool IsDisposed { get { return _isDisposed; } }

        #endregion Properties

        [Export("layerClass")]
        public static Class GetLayerClass()
        {
            return new Class(typeof(CAEAGLLayer));
        }

        public override bool CanBecomeFirstResponder
        {
            get { return true; }
        }

        private new CAEAGLLayer Layer
        {
            get { return base.Layer as CAEAGLLayer; }
        }

        //TODO: Move _eaglContext into an iOS-specific GraphicsContext
        internal EAGLContext _eaglContext;
        private OGL _glapi;

        private void CreateGLContext()
        {
            AssertNotDisposed();

            // RetainedBacking controls if the content of the colorbuffer should be preserved after being displayed
            // This is the XNA equivalent to set PreserveContent when initializing the GraphicsDevice
            // (should be false by default for better performance)
            Layer.DrawableProperties = NSDictionary.FromObjectsAndKeys(
                new NSObject[]
                {
                    NSNumber.FromBoolean(false),
                    EAGLColorFormat.RGBA8
                },
                new NSObject[]
                {
                    EAGLDrawableProperty.RetainedBacking,
                    EAGLDrawableProperty.ColorFormat
                });

            Layer.ContentsScale = Window.Screen.Scale;

            //var strVersion = OpenTK.Graphics.ES11.GL.GetString(OpenTK.Graphics.ES11.All.Version);
            //strVersion = OpenTK.Graphics.ES20.GL.GetString(OpenTK.Graphics.ES20.All.Version);
            //var version = Version.Parse(strVersion);

            try 
            {
                try
                {
                    _eaglContext = new EAGLContext(EAGLRenderingAPI.OpenGLES3);
                }
                catch
                {
                    // Fall back to GLES 2.0
                    _eaglContext = new EAGLContext(EAGLRenderingAPI.OpenGLES2);
                }
                //new GraphicsContext(null, null, 2, 0, GraphicsContextFlags.Embedded)
            } 
            catch (Exception ex)
            {
                throw new Exception("Device not Supported. GLES 2.0 or above is required!");
            }

            this.MakeCurrent();

            OGL_IOS.Initialize();
            OGL_IOS.Current.InitExtensions();
            _glapi = OGL.Current;
        }

        [Export("doTick")]
        void DoTick()
        {
            _concreteGame.iOSTick();
        }

        private void CreateFramebuffer()
        {
            this.MakeCurrent();
            
            var GL = OGL.Current;

            // HACK:  GraphicsDevice itself should be calling
            //        glViewport, so we shouldn't need to do it
            //        here and then force the state into
            //        GraphicsDevice.  However, that change is a
            //        ways off, yet.
            int viewportHeight = (int)Math.Round(Layer.Bounds.Size.Height * Layer.ContentsScale);
            int viewportWidth = (int)Math.Round(Layer.Bounds.Size.Width * Layer.ContentsScale);

            _framebuffer = _glapi.GenFramebuffer();
            _glapi.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);

            // Create our Depth buffer. Color buffer must be the last one bound
            GraphicsDeviceManager gdm = _concreteGame.Services.GetService(typeof(IGraphicsDeviceManager)) as GraphicsDeviceManager;
            if (gdm != null)
            {
                DepthFormat preferredDepthFormat = gdm.PreferredDepthStencilFormat;
                if (preferredDepthFormat != DepthFormat.None)
                {
                    _depthbuffer = GL.GenRenderbuffer();
                    GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _depthbuffer);
                    RenderbufferStorage internalFormat = RenderbufferStorage.DepthComponent16;
                    if (preferredDepthFormat == DepthFormat.Depth24)
                        internalFormat = RenderbufferStorage.DepthComponent24oes;
                    else if (preferredDepthFormat == DepthFormat.Depth24Stencil8)
                        internalFormat = RenderbufferStorage.Depth24Stencil8oes;
                    GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, internalFormat, viewportWidth, viewportHeight);
                    GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _depthbuffer);
                    if (preferredDepthFormat == DepthFormat.Depth24Stencil8)
                        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.StencilAttachment, RenderbufferTarget.Renderbuffer, _depthbuffer);
                }
            }

            _colorbuffer = _glapi.GenRenderbuffer();
            _glapi.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _colorbuffer);

            // TODO: EAGLContext.RenderBufferStorage returns false
            //       on all but the first call.  Nevertheless, it
            //       works.  Still, it would be nice to know why it
            //       claims to have failed.
            _eaglContext.RenderBufferStorage((uint)RenderbufferTarget.Renderbuffer, Layer);
            
            _glapi.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, _colorbuffer);

            FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
                throw new InvalidOperationException("Framebuffer was not created correctly: " + status);

            _glapi.Viewport(0, 0, viewportWidth, viewportHeight);
            _glapi.Scissor(0, 0, viewportWidth, viewportHeight);

            IGraphicsDeviceService gds = _concreteGame.Services.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService;
            if (gds != null && gds.GraphicsDevice != null)
            {
                PresentationParameters pp = gds.GraphicsDevice.PresentationParameters;
                int height = viewportHeight;
                int width = viewportWidth;

                if (this.NextResponder is iOSGameViewController)
                {
                    DisplayOrientation displayOrientation = _concreteGame.Game.Window.CurrentOrientation;
                    if (displayOrientation == DisplayOrientation.LandscapeLeft || displayOrientation == DisplayOrientation.LandscapeRight)
                    {
                        height = Math.Min(viewportHeight, viewportWidth);
                        width = Math.Max(viewportHeight, viewportWidth);
                    }
                    else
                    {
                        height = Math.Max(viewportHeight, viewportWidth);
                        width = Math.Min(viewportHeight, viewportWidth);
                    }
                }

                pp.BackBufferWidth = width;
                pp.BackBufferHeight = height;

                gds.GraphicsDevice.Viewport = new Viewport(0, 0, pp.BackBufferWidth, pp.BackBufferHeight);
                
                ((IPlatformGraphicsDevice)gds.GraphicsDevice).Strategy.ToConcrete<ConcreteGraphicsDevice>()._glDefaultFramebuffer = _framebuffer;
            }
        }

        private void DestroyFramebuffer()
        {
            AssertNotDisposed();
            AssertValidContext();

            if (!EAGLContext.SetCurrentContext(_eaglContext))
                throw new InvalidOperationException("Unable to change current EAGLContext.");

            _glapi.DeleteFramebuffer(_framebuffer);
            _framebuffer = 0;

            _glapi.DeleteRenderbuffer(_colorbuffer);
            _colorbuffer = 0;
            
            if (_depthbuffer != 0)
            {
                _glapi.DeleteRenderbuffer(_depthbuffer);
                _depthbuffer = 0;
            }
        }

        private static readonly FramebufferAttachment[] attachements = new FramebufferAttachment[]
        {
            FramebufferAttachment.DepthAttachment, 
            FramebufferAttachment.StencilAttachment 
        };

        // FIXME: This logic belongs in GraphicsDevice.Present, not
        //        here.  If it can someday be moved there, then the
        //        normal call to Present in Game.Tick should cover
        //        this.  For now, ConcreteGame will call Present
        //        in the Draw/Update loop handler.
        public void Present()
        {
            var GL = OGL.Current;

            AssertNotDisposed();
            AssertValidContext();

            this.MakeCurrent();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, this._colorbuffer);
            GL.InvalidateFramebuffer(FramebufferTarget.Framebuffer, 2, attachements);

            if (!_eaglContext.PresentRenderBuffer(36161u))
                throw new InvalidOperationException("EAGLContext.PresentRenderbuffer failed.");
        }

        // FIXME: This functionality belongs in GraphicsDevice.
        public void MakeCurrent()
        {
            AssertNotDisposed();
            AssertValidContext();

            if (EAGLContext.CurrentContext != _eaglContext)
            {
                if (!EAGLContext.SetCurrentContext(_eaglContext))
                    throw new InvalidOperationException("Unable to change current EAGLContext.");
            }
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            IGraphicsDeviceService gds = _concreteGame.Services.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService;
            if (gds != null && gds.GraphicsDevice != null)
            {
                if (_framebuffer != 0)
                    DestroyFramebuffer();
                if (_eaglContext == null)
                    CreateGLContext();

                CreateFramebuffer();
            }
        }

        #region UIWindow Notifications

        [Export("didMoveToWindow")]
        public virtual void DidMoveToWindow()
        {
            if (Window != null) 
            {
                if (_eaglContext == null)
                    CreateGLContext();
                if (_framebuffer == 0)
                    CreateFramebuffer();
            }
        }

        #endregion UIWindow Notifications

        private void AssertNotDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        private void AssertValidContext()
        {
            if (_eaglContext == null)
                throw new InvalidOperationException("_eaglContext must be created for this operation to succeed.");
        }
    }
}
