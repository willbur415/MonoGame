// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using static WebHelper;
using glc = Retyped.webgl2.WebGL2RenderingContext;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class DepthStencilState
    {
        internal void PlatformApplyState(GraphicsDevice device, bool force = false)
        {
            if (force || this.DepthBufferEnable != device._lastDepthStencilState.DepthBufferEnable)
            {
                if (!DepthBufferEnable)
                {
                    gl.disable(glc.DEPTH_TEST);
                    GraphicsExtensions.CheckGLError();
                }
                else
                {
                    // enable Depth Buffer
                    gl.enable(glc.DEPTH_TEST);
                    GraphicsExtensions.CheckGLError();
                }
                device._lastDepthStencilState.DepthBufferEnable = this.DepthBufferEnable;
            }

            if (force || this.DepthBufferFunction != device._lastDepthStencilState.DepthBufferFunction)
            {
                gl.depthFunc(DepthBufferFunction.GetDepthFunction());
                GraphicsExtensions.CheckGLError();
                device._lastDepthStencilState.DepthBufferFunction = this.DepthBufferFunction;
            }

            if (force || this.DepthBufferWriteEnable != device._lastDepthStencilState.DepthBufferWriteEnable)
            {
                gl.depthMask(DepthBufferWriteEnable);
                GraphicsExtensions.CheckGLError();
                device._lastDepthStencilState.DepthBufferWriteEnable = this.DepthBufferWriteEnable;
            }

            if (force || this.StencilEnable != device._lastDepthStencilState.StencilEnable)
            {
                if (!StencilEnable)
                {
                    gl.disable(glc.STENCIL_TEST);
                    GraphicsExtensions.CheckGLError();
                }
                else
                {
                    // enable Stencil
                    gl.enable(glc.STENCIL_TEST);
                    GraphicsExtensions.CheckGLError();
                }
                device._lastDepthStencilState.StencilEnable = this.StencilEnable;
            }

            // set function
            if (this.TwoSidedStencilMode)
            {
                var cullFaceModeFront = glc.FRONT;
                var cullFaceModeBack = glc.BACK;
                var stencilFaceFront = glc.FRONT;
                var stencilFaceBack = glc.BACK;

                if (force ||
					this.TwoSidedStencilMode != device._lastDepthStencilState.TwoSidedStencilMode ||
					this.StencilFunction != device._lastDepthStencilState.StencilFunction ||
					this.ReferenceStencil != device._lastDepthStencilState.ReferenceStencil ||
					this.StencilMask != device._lastDepthStencilState.StencilMask)
				{
                    gl.stencilFuncSeparate(cullFaceModeFront, GetStencilFunc(this.StencilFunction),
                                           this.ReferenceStencil, this.StencilMask);
                    GraphicsExtensions.CheckGLError();
                    device._lastDepthStencilState.StencilFunction = this.StencilFunction;
                    device._lastDepthStencilState.ReferenceStencil = this.ReferenceStencil;
                    device._lastDepthStencilState.StencilMask = this.StencilMask;
                }

                if (force ||
                    this.TwoSidedStencilMode != device._lastDepthStencilState.TwoSidedStencilMode ||
                    this.CounterClockwiseStencilFunction != device._lastDepthStencilState.CounterClockwiseStencilFunction ||
                    this.ReferenceStencil != device._lastDepthStencilState.ReferenceStencil ||
                    this.StencilMask != device._lastDepthStencilState.StencilMask)
			    {
                    gl.stencilFuncSeparate(cullFaceModeBack, GetStencilFunc(this.CounterClockwiseStencilFunction),
                                           this.ReferenceStencil, this.StencilMask);
                    GraphicsExtensions.CheckGLError();
                    device._lastDepthStencilState.CounterClockwiseStencilFunction = this.CounterClockwiseStencilFunction;
                    device._lastDepthStencilState.ReferenceStencil = this.ReferenceStencil;
                    device._lastDepthStencilState.StencilMask = this.StencilMask;
                }

                
                if (force ||
					this.TwoSidedStencilMode != device._lastDepthStencilState.TwoSidedStencilMode ||
					this.StencilFail != device._lastDepthStencilState.StencilFail ||
					this.StencilDepthBufferFail != device._lastDepthStencilState.StencilDepthBufferFail ||
					this.StencilPass != device._lastDepthStencilState.StencilPass)
                {
                    gl.stencilOpSeparate(stencilFaceFront, GetStencilOp(this.StencilFail),
                                         GetStencilOp(this.StencilDepthBufferFail),
                                         GetStencilOp(this.StencilPass));
                    GraphicsExtensions.CheckGLError();
                    device._lastDepthStencilState.StencilFail = this.StencilFail;
                    device._lastDepthStencilState.StencilDepthBufferFail = this.StencilDepthBufferFail;
                    device._lastDepthStencilState.StencilPass = this.StencilPass;
                }

                if (force ||
                    this.TwoSidedStencilMode != device._lastDepthStencilState.TwoSidedStencilMode ||
                    this.CounterClockwiseStencilFail != device._lastDepthStencilState.CounterClockwiseStencilFail ||
                    this.CounterClockwiseStencilDepthBufferFail != device._lastDepthStencilState.CounterClockwiseStencilDepthBufferFail ||
                    this.CounterClockwiseStencilPass != device._lastDepthStencilState.CounterClockwiseStencilPass)
			    {
                    gl.stencilOpSeparate(stencilFaceBack, GetStencilOp(this.CounterClockwiseStencilFail),
                                         GetStencilOp(this.CounterClockwiseStencilDepthBufferFail),
                                         GetStencilOp(this.CounterClockwiseStencilPass));
                    GraphicsExtensions.CheckGLError();
                    device._lastDepthStencilState.CounterClockwiseStencilFail = this.CounterClockwiseStencilFail;
                    device._lastDepthStencilState.CounterClockwiseStencilDepthBufferFail = this.CounterClockwiseStencilDepthBufferFail;
                    device._lastDepthStencilState.CounterClockwiseStencilPass = this.CounterClockwiseStencilPass;
                }
            }
            else
            {
                if (force ||
					this.TwoSidedStencilMode != device._lastDepthStencilState.TwoSidedStencilMode ||
					this.StencilFunction != device._lastDepthStencilState.StencilFunction ||
					this.ReferenceStencil != device._lastDepthStencilState.ReferenceStencil ||
					this.StencilMask != device._lastDepthStencilState.StencilMask)
				{
                    gl.stencilFunc(GetStencilFunc(this.StencilFunction), ReferenceStencil, StencilMask);
                    GraphicsExtensions.CheckGLError();
                    device._lastDepthStencilState.StencilFunction = this.StencilFunction;
                    device._lastDepthStencilState.ReferenceStencil = this.ReferenceStencil;
                    device._lastDepthStencilState.StencilMask = this.StencilMask;
                }

                if (force ||
                    this.TwoSidedStencilMode != device._lastDepthStencilState.TwoSidedStencilMode ||
                    this.StencilFail != device._lastDepthStencilState.StencilFail ||
                    this.StencilDepthBufferFail != device._lastDepthStencilState.StencilDepthBufferFail ||
                    this.StencilPass != device._lastDepthStencilState.StencilPass)
                {
                    gl.stencilOp(GetStencilOp(StencilFail),
                                 GetStencilOp(StencilDepthBufferFail),
                                 GetStencilOp(StencilPass));
                    GraphicsExtensions.CheckGLError();
                    device._lastDepthStencilState.StencilFail = this.StencilFail;
                    device._lastDepthStencilState.StencilDepthBufferFail = this.StencilDepthBufferFail;
                    device._lastDepthStencilState.StencilPass = this.StencilPass;
                }
            }

            device._lastDepthStencilState.TwoSidedStencilMode = this.TwoSidedStencilMode;

            if (force || this.StencilWriteMask != device._lastDepthStencilState.StencilWriteMask)
            {
                gl.stencilMask(this.StencilWriteMask);
                GraphicsExtensions.CheckGLError();
                device._lastDepthStencilState.StencilWriteMask = this.StencilWriteMask;
            }
        }

        private static double GetStencilFunc(CompareFunction function)
        {
            switch (function)
            {
                case CompareFunction.Always: 
                    return glc.ALWAYS;
                case CompareFunction.Equal:
                    return glc.EQUAL;
                case CompareFunction.Greater:
                    return glc.GREATER;
                case CompareFunction.GreaterEqual:
                    return glc.GEQUAL;
                case CompareFunction.Less:
                    return glc.LESS;
                case CompareFunction.LessEqual:
                    return glc.LEQUAL;
                case CompareFunction.Never:
                    return glc.NEVER;
                case CompareFunction.NotEqual:
                    return glc.NOTEQUAL;
                default:
                    return glc.ALWAYS;
            }
        }

        private static double GetStencilOp(StencilOperation operation)
        {
            switch (operation)
            {
                case StencilOperation.Keep:
                    return glc.KEEP;
                case StencilOperation.Decrement:
                    return glc.DECR_WRAP;
                case StencilOperation.DecrementSaturation:
                    return glc.DECR;
                case StencilOperation.IncrementSaturation:
                    return glc.INCR;
                case StencilOperation.Increment:
                    return glc.INCR_WRAP;
                case StencilOperation.Invert:
                    return glc.INVERT;
                case StencilOperation.Replace:
                    return glc.REPLACE;
                case StencilOperation.Zero:
                    return glc.ZERO;
                default:
                    return glc.KEEP;
            }
        }
    }
}

