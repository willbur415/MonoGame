// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Diagnostics;
using Bridge.WebGL;

namespace Microsoft.Xna.Framework.Graphics
{
    internal partial class Shader
    {
        // The shader handle.
        private WebGLShader _shaderHandle;
        private bool _shaderCreated;

        // We keep this around for recompiling on context lost and debugging.
        private string _glslCode;

        private static int PlatformProfile()
        {
            return 0;
        }

        private void PlatformConstruct(bool isVertexShader, byte[] shaderBytecode)
        {
            _glslCode = System.Text.Encoding.ASCII.GetString(shaderBytecode);

            HashKey = MonoGame.Utilities.Hash.ComputeHash(shaderBytecode);
        }

        internal WebGLShader GetShaderHandle()
        {
            // If the shader has already been created then return it.
            if (_shaderCreated)
                return _shaderHandle;

            _shaderHandle = Web.GL.CreateShader(Stage == ShaderStage.Vertex ? Web.GL.VERTEX_SHADER : Web.GL.FRAGMENT_SHADER);
            GraphicsExtensions.CheckGLError();
            Web.GL.ShaderSource(_shaderHandle, _glslCode);
            GraphicsExtensions.CheckGLError();
            Web.GL.CompileShader(_shaderHandle);
            GraphicsExtensions.CheckGLError();
            var compiled = (int)Web.GL.GetShaderParameter(_shaderHandle, Web.GL.COMPILE_STATUS);
            GraphicsExtensions.CheckGLError();
            if (compiled != 1)
            {
                var log = Web.GL.GetShaderInfoLog(_shaderHandle);
                Debug.WriteLine(log);

                GraphicsDevice.DisposeShader(_shaderHandle);

                throw new InvalidOperationException("Shader Compilation Failed");
            }

            _shaderCreated = true;
            return _shaderHandle;
        }

        internal void GetVertexAttributeLocations(WebGLProgram program)
        {
            for (int i = 0; i < Attributes.Length; ++i)
            {
                Attributes[i].location = Web.GL.GetAttribLocation(program, Attributes[i].name);
                GraphicsExtensions.CheckGLError();
            }
        }

        internal int GetAttribLocation(VertexElementUsage usage, int index)
        {
            for (int i = 0; i < Attributes.Length; ++i)
            {
                if ((Attributes[i].usage == usage) && (Attributes[i].index == index))
                    return Attributes[i].location;
            }
            return -1;
        }

        internal void ApplySamplerTextureUnits(WebGLProgram program)
        {
            // Assign the texture unit index to the sampler uniforms.
            foreach (var sampler in Samplers)
            {
                var loc = Web.GL.GetUniformLocation(program, sampler.name);
                GraphicsExtensions.CheckGLError();
                
                Web.GL.Uniform1f(loc, sampler.textureSlot);
                GraphicsExtensions.CheckGLError();
            }
        }

        private void PlatformGraphicsDeviceResetting()
        {
            if (_shaderCreated)
            {
                GraphicsDevice.DisposeShader(_shaderHandle);
                _shaderCreated = false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed && _shaderCreated)
            {
                GraphicsDevice.DisposeShader(_shaderHandle);
                _shaderCreated = false;
            }

            base.Dispose(disposing);
        }
    }
}
