// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input.Touch;
using System.Diagnostics;
using Bridge.WebGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class GraphicsDevice
    {
        // List<ResourceHandle> _disposeThisFrame = new List<ResourceHandle>();
        // List<ResourceHandle> _disposeNextFrame = new List<ResourceHandle>();
        object _disposeActionsLock = new object();

        static List<IntPtr> _disposeContexts = new List<IntPtr>();
        static object _disposeContextsLock = new object();

        private ShaderProgramCache _programCache;
        private ShaderProgram _shaderProgram = null;

        static readonly float[] _posFixup = new float[4];

        // private static BufferBindingInfo[] _bufferBindingInfos;
        private static bool[] _newEnabledVertexAttributes;
        internal static readonly List<int> _enabledVertexAttributes = new List<int>();
        internal static bool _attribsDirty;

        internal FramebufferHelper framebufferHelper;

        internal int glFramebuffer = 0;
        internal int MaxVertexAttributes;
        internal int _maxTextureSize = 0;

        // Keeps track of last applied state to avoid redundant OpenGL calls
        internal bool _lastBlendEnable = false;
        internal BlendState _lastBlendState = new BlendState();
        internal DepthStencilState _lastDepthStencilState = new DepthStencilState();
        internal RasterizerState _lastRasterizerState = new RasterizerState();
        private Vector4 _lastClearColor = Vector4.Zero;
        private float _lastClearDepth = 1.0f;
        private int _lastClearStencil = 0;
        private DepthStencilState clearDepthStencilState = new DepthStencilState { StencilEnable = true };

        // Get a hashed value based on the currently bound shaders
        // throws an exception if no shaders are bound
        private int ShaderProgramHash
        {
            get
            {
                if (_vertexShader == null && _pixelShader == null)
                    throw new InvalidOperationException("There is no shader bound!");
                if (_vertexShader == null)
                    return _pixelShader.HashKey;
                if (_pixelShader == null)
                    return _vertexShader.HashKey;
                return _vertexShader.HashKey ^ _pixelShader.HashKey;
            }
        }


        internal void SetVertexAttributeArray(bool[] attrs)
        {
            for (var x = 0; x < attrs.Length; x++)
            {
                if (attrs[x] && !_enabledVertexAttributes.Contains(x))
                {
                    _enabledVertexAttributes.Add(x);
                    Web.GL.EnableVertexAttribArray(x);
                    GraphicsExtensions.CheckGLError();
                }
                else if (!attrs[x] && _enabledVertexAttributes.Contains(x))
                {
                    _enabledVertexAttributes.Remove(x);
                    Web.GL.DisableVertexAttribArray(x);
                    GraphicsExtensions.CheckGLError();
                }
            }
        }

        private void PlatformSetup()
        {
            _programCache = new ShaderProgramCache(this);

            MaxTextureSlots = (int)Web.GL.GetParameter(Web.GL.MAX_TEXTURE_IMAGE_UNITS);
            GraphicsExtensions.CheckGLError();

            _maxTextureSize = (int)Web.GL.GetParameter(Web.GL.MAX_TEXTURE_SIZE);
            GraphicsExtensions.CheckGLError();

            MaxVertexAttributes = (int)Web.GL.GetParameter(Web.GL.MAX_VERTEX_ATTRIBS);
            GraphicsExtensions.CheckGLError();

            _maxVertexBufferSlots = MaxVertexAttributes;
            _newEnabledVertexAttributes = new bool[MaxVertexAttributes];

            Console.WriteLine(Web.GL.GetParameter(Web.GL.VERSION).ToString());
        }

        private void PlatformInitialize()
        {
            _viewport = new Viewport(0, 0, PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight);

            // Ensure the vertex attributes are reset
            _enabledVertexAttributes.Clear();

            // Free all the cached shader programs. 
            _programCache.Clear();
            _shaderProgram = null;

            framebufferHelper = FramebufferHelper.Create(this);

            // Force resetting states
            this.PlatformApplyBlend(true);
        }

        internal void OnPresentationChanged()
        {
            ApplyRenderTargets(null);
        }

        public void PlatformClear(ClearOptions options, Vector4 color, float depth, int stencil)
        {
            // TODO: We need to figure out how to detect if we have a
            // depth stencil buffer or not, and clear options relating
            // to them if not attached.

            // Unlike with XNA and DirectX...  GL.Clear() obeys several
            // different render states:
            //
            //  - The color write flags.
            //  - The scissor rectangle.
            //  - The depth/stencil state.
            //
            // So overwrite these states with what is needed to perform
            // the clear correctly and restore it afterwards.
            //
		    var prevScissorRect = ScissorRectangle;
		    var prevDepthStencilState = DepthStencilState;
            var prevBlendState = BlendState;
            ScissorRectangle = _viewport.Bounds;
            // DepthStencilState.Default has the Stencil Test disabled; 
            // make sure stencil test is enabled before we clear since
            // some drivers won't clear with stencil test disabled
            DepthStencilState = this.clearDepthStencilState;
		    BlendState = BlendState.Opaque;
            ApplyState(false);

            int bufferMask = 0;
            if ((options & ClearOptions.Target) == ClearOptions.Target)
            {
                if (color != _lastClearColor)
                {
                    Web.GL.ClearColor(color.X, color.Y, color.Z, color.W);
                    GraphicsExtensions.CheckGLError();
                    _lastClearColor = color;
                }

                bufferMask = bufferMask | Web.GL.COLOR_BUFFER_BIT;
            }
			if ((options & ClearOptions.Stencil) == ClearOptions.Stencil)
            {
                if (stencil != _lastClearStencil)
                {
				    Web.GL.ClearStencil(stencil);
                    GraphicsExtensions.CheckGLError();
                    _lastClearStencil = stencil;
                }
                bufferMask = bufferMask | Web.GL.STENCIL_BUFFER_BIT;
			}

			if ((options & ClearOptions.DepthBuffer) == ClearOptions.DepthBuffer) 
            {
                if (depth != _lastClearDepth)
                {
                    Web.GL.ClearDepth(depth);
                    GraphicsExtensions.CheckGLError();
                    _lastClearDepth = depth;
                }
				bufferMask = bufferMask | Web.GL.DEPTH_BUFFER_BIT;
			}

            Web.GL.Clear(bufferMask);
            GraphicsExtensions.CheckGLError();
           		
            // Restore the previous render state.
		    ScissorRectangle = prevScissorRect;
		    DepthStencilState = prevDepthStencilState;
		    BlendState = prevBlendState;
        }

        private void PlatformDispose()
        {

        }

        internal void DisposeShader(WebGLShader handle)
        {
        }

        internal void DisposeTexture(WebGLTexture handle)
        {
        }

        internal void DisposeProgram(WebGLProgram handle)
        {
            /*if (!_isDisposed)
            {
                lock (_disposeActionsLock)
                {
                    _disposeNextFrame.Add(ResourceHandle.Program(handle));
                }
            }*/
        }

        public void PlatformPresent()
        {
        }

        private void PlatformSetViewport(ref Viewport value)
        {
            if (IsRenderTargetBound)
                Web.GL.Viewport(value.X, value.Y, value.Width, value.Height);
            else
                Web.GL.Viewport(value.X, PresentationParameters.BackBufferHeight - value.Y - value.Height, value.Width, value.Height);
            GraphicsExtensions.LogGLError("GraphicsDevice.Viewport_set() GL.Viewport");

            Web.GL.DepthRange(value.MinDepth, value.MaxDepth);
            GraphicsExtensions.LogGLError("GraphicsDevice.Viewport_set() GL.DepthRange");
                
            // In OpenGL we have to re-apply the special "posFixup"
            // vertex shader uniform if the viewport changes.
            _vertexShaderDirty = true;
        }

        private void PlatformApplyDefaultRenderTarget()
        {
        }

        internal void PlatformResolveRenderTargets()
        {
            // Resolving MSAA render targets should be done here.
        }

        private IRenderTarget PlatformApplyRenderTargets()
        {
            return null;
        }
		
        internal void PlatformBeginApplyState()
        {
        }

        private void PlatformApplyBlend(bool force = false)
        {
            _actualBlendState.PlatformApplyState(this, force);

            if (force || BlendFactor != _lastBlendState.BlendFactor)
            {
                Web.GL.BlendColor(
                    this.BlendFactor.R/255.0f,
                    this.BlendFactor.G/255.0f,
                    this.BlendFactor.B/255.0f,
                    this.BlendFactor.A/255.0f);
                GraphicsExtensions.CheckGLError();
                _lastBlendState.BlendFactor = this.BlendFactor;
            }
        }

        internal void PlatformApplyState(bool applyShaders)
        {
        }

        private void PlatformDrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount)
        {
        }

        private void PlatformDrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, VertexDeclaration vertexDeclaration, int vertexCount) where T : struct
        {
        }

        private void PlatformDrawPrimitives(PrimitiveType primitiveType, int vertexStart, int vertexCount)
        {
        }

        private void PlatformDrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
        {
            ApplyState(true);

            // Unbind current VBOs.
            Web.GL.BindBuffer(Web.GL.ARRAY_BUFFER, null);
            GraphicsExtensions.CheckGLError();
            Web.GL.BindBuffer(Web.GL.ELEMENT_ARRAY_BUFFER, null);
            GraphicsExtensions.CheckGLError();
            _indexBufferDirty = true;

            // Setup the vertex declaration to point at the VB data.
            vertexDeclaration.GraphicsDevice = this;
            /* vertexDeclaration.Apply(_vertexShader, 0, ShaderProgramHash);

            //Draw
            /*Web.GL.DrawElements(GraphicsExtensions.GetPrimitiveTypeGL(primitiveType),
                                GetElementCountArray(primitiveType, primitiveCount),
                                Web.GL.UNSIGNED_SHORT,
                                (indexOffset * 2));

            GraphicsExtensions.CheckGLError();*/
        }

        private void PlatformDrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, int[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
        {
        }

        private void PlatformDrawInstancedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount, int instanceCount)
        {
        }

        private void PlatformGetBackBufferData<T>(Rectangle? rect, T[] data, int startIndex, int count) where T : struct
        {
            throw new NotImplementedException();
        }

        private static Rectangle PlatformGetTitleSafeArea(int x, int y, int width, int height)
        {
            return new Rectangle(x, y, width, height);
        }
        
        internal void PlatformSetMultiSamplingToMaximum(PresentationParameters presentationParameters, out int quality)
        {
            presentationParameters.MultiSampleCount = 4;
            quality = 0;
        }
    }
}
