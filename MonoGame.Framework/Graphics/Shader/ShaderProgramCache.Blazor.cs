
using System;
using System.Collections.Generic;
using Microsoft.Xna.Platform.Graphics;
using nkast.Wasm.Canvas.WebGL;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// This class is used to Cache the links between Vertex/Pixel Shaders and Constant Buffers.
    /// It will be responsible for linking the programs under OpenGL if they have not been linked
    /// before. If an existing link exists it will be resused.
    /// </summary>
    internal class ShaderProgramCache : IDisposable
    {
        GraphicsDevice _device;

        private readonly Dictionary<int, ShaderProgram> _programCache = new Dictionary<int, ShaderProgram>();
        bool _isDisposed;

        public ShaderProgramCache(GraphicsDevice device)
        {
            _device = device;
        }


        /// <summary>
        /// Clear the program cache releasing all shader programs.
        /// </summary>
        public void Clear()
        {
            foreach (ShaderProgram shaderProgram in _programCache.Values)
            {
                shaderProgram.Program.Dispose();
            }
            _programCache.Clear();
        }

        public ShaderProgram GetProgram(Shader vertexShader, Shader pixelShader, int shaderProgramHash)
        {
            // TODO: We should be hashing in the mix of constant
            // buffers here as well.  This would allow us to optimize
            // setting uniforms to only when a constant buffer changes.

            ShaderProgram program;
            if(_programCache.TryGetValue(shaderProgramHash, out program))
                return program;

            // the key does not exist so we need to link the programs
            program = CreateProgram(vertexShader, pixelShader);
            _programCache.Add(shaderProgramHash, program);
            return program;
        }

        private ShaderProgram CreateProgram(Shader vertexShader, Shader pixelShader)
        {
            var GL = ((ConcreteGraphicsContext)_device.Strategy.CurrentContext.Strategy).GL;

            var program = GL.CreateProgram();
            GraphicsExtensions.CheckGLError();

            GL.AttachShader(program, vertexShader.GetShaderHandle());
            GraphicsExtensions.CheckGLError();

            GL.AttachShader(program, pixelShader.GetShaderHandle());
            GraphicsExtensions.CheckGLError();

            //vertexShader.BindVertexAttributes(program);

            GL.LinkProgram(program);
            GraphicsExtensions.CheckGLError();

            GL.UseProgram(program);
            GraphicsExtensions.CheckGLError();

            vertexShader.GetVertexAttributeLocations(program);

            pixelShader.ApplySamplerTextureUnits(program);

            bool linkStatus;
            linkStatus = GL.GetProgramParameter(program, WebGLProgramStatus.LINK);

            if (linkStatus == true)
            {
                return new ShaderProgram(program, _device.Strategy);
            }
            else
            { 
                var log = GL.GetProgramInfoLog(program);
                vertexShader.Dispose();
                pixelShader.Dispose();
                program.Dispose();
                throw new InvalidOperationException("Unable to link effect program");
            }
        }

        ~ShaderProgramCache()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    foreach (ShaderProgram shaderProgram in _programCache.Values)
                    {
                        shaderProgram.Program.Dispose();
                    }
                    _programCache.Clear();

                    _device = null;
                }

                _isDisposed = true;
            }
        }

    }
}
