using System;
using System.Collections.Generic;
using Bridge.WebGL;

namespace Microsoft.Xna.Framework.Graphics
{
    internal class ShaderProgram
    {
        public readonly WebGLProgram Program;

        private readonly Dictionary<string, WebGLUniformLocation> _uniformLocations = new Dictionary<string, WebGLUniformLocation>();

        public ShaderProgram(WebGLProgram program)
        {
            Program = program;
        }

        public WebGLUniformLocation GetUniformLocation(string name)
        {
            if (_uniformLocations.ContainsKey(name))
                return _uniformLocations[name];

            var location = Web.GL.GetUniformLocation(Program, name);
            GraphicsExtensions.CheckGLError();
            _uniformLocations[name] = location;
            return location;
        }
    }

    /// <summary>
    /// This class is used to Cache the links between Vertex/Pixel Shaders and Constant Buffers.
    /// It will be responsible for linking the programs under OpenGL if they have not been linked
    /// before. If an existing link exists it will be resused.
    /// </summary>
    internal class ShaderProgramCache : IDisposable
    {
        private readonly Dictionary<int, ShaderProgram> _programCache = new Dictionary<int, ShaderProgram>();
        GraphicsDevice _graphicsDevice;
        bool disposed;

        public ShaderProgramCache(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }

        /// <summary>
        /// Clear the program cache releasing all shader programs.
        /// </summary>
        public void Clear()
        {
            foreach (var pair in _programCache)
            {
                _graphicsDevice.DisposeProgram(pair.Value.Program);
            }
            _programCache.Clear();
        }

        public ShaderProgram GetProgram(Shader vertexShader, Shader pixelShader)
        {
            // TODO: We should be hashing in the mix of constant 
            // buffers here as well.  This would allow us to optimize
            // setting uniforms to only when a constant buffer changes.

            var key = vertexShader.HashKey | pixelShader.HashKey;
            if (!_programCache.ContainsKey(key))
            {
                // the key does not exist so we need to link the programs
                _programCache.Add(key, Link(vertexShader, pixelShader));
            }

            return _programCache[key];
        }

        private ShaderProgram Link(Shader vertexShader, Shader pixelShader)
        {
            // NOTE: No need to worry about background threads here
            // as this is only called at draw time when we're in the
            // main drawing thread.
            var program = (WebGLProgram)Web.GL.CreateProgram();
            GraphicsExtensions.CheckGLError();

            Web.GL.AttachShader(program, vertexShader.GetShaderHandle());
            GraphicsExtensions.CheckGLError();

            Web.GL.AttachShader(program, pixelShader.GetShaderHandle());
            GraphicsExtensions.CheckGLError();

            //vertexShader.BindVertexAttributes(program);

            Web.GL.LinkProgram(program);
            GraphicsExtensions.CheckGLError();

            Web.GL.UseProgram(program);
            GraphicsExtensions.CheckGLError();

            vertexShader.GetVertexAttributeLocations(program);

            pixelShader.ApplySamplerTextureUnits(program);

            var linked = (int)Web.GL.GetProgramParameter(program, Web.GL.LINK_STATUS);
            GraphicsExtensions.LogGLError("VertexShaderCache.Link(), GL.GetProgram");

            if (linked == 0)
            {
                var log = Web.GL.GetProgramInfoLog(program);
                Console.WriteLine(log);
                Web.GL.DetachShader(program, vertexShader.GetShaderHandle());
                Web.GL.DetachShader(program, pixelShader.GetShaderHandle());
                _graphicsDevice.DisposeProgram(program);
                throw new InvalidOperationException("Unable to link effect program");
            }

            return new ShaderProgram(program);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                    Clear();
                disposed = true;
            }
        }
    }
}
