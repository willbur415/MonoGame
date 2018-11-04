// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using Bridge;
using static Retyped.dom;
using static WebHelper;
using glc = Retyped.webgl2.WebGL2RenderingContext;

namespace Microsoft.Xna.Framework.Graphics
{
    internal static partial class GraphicsExtensions
    {
		public static int OpenGLNumberOfElements(this VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                    return 1;

                case VertexElementFormat.Vector2:
                    return 2;

                case VertexElementFormat.Vector3:
                    return 3;

                case VertexElementFormat.Vector4:
                    return 4;

                case VertexElementFormat.Color:
                    return 4;

                case VertexElementFormat.Byte4:
                    return 4;

                case VertexElementFormat.Short2:
                    return 2;

                case VertexElementFormat.Short4:
                    return 4;

                case VertexElementFormat.NormalizedShort2:
                    return 2;

                case VertexElementFormat.NormalizedShort4:
                    return 4;

                case VertexElementFormat.HalfVector2:
                    return 2;

                case VertexElementFormat.HalfVector4:
                    return 4;
            }

            throw new ArgumentException();
        }

		public static int OpenGLVertexAttribPointerType(this VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                case VertexElementFormat.Vector2:
                case VertexElementFormat.Vector3:
                case VertexElementFormat.Vector4:
                    return (int)glc.FLOAT;

                case VertexElementFormat.Color:
                case VertexElementFormat.Byte4:
					return (int)glc.UNSIGNED_BYTE;

                case VertexElementFormat.Short2:
                case VertexElementFormat.Short4:
                case VertexElementFormat.NormalizedShort2:
                case VertexElementFormat.NormalizedShort4:
                    return (int)glc.SHORT;
            }

            throw new ArgumentException();
        }

        public static bool OpenGLVertexAttribNormalized(this VertexElement element)
        {
            // TODO: This may or may not be the right behavor.  
            //
            // For instance the VertexElementFormat.Byte4 format is not supposed
            // to be normalized, but this line makes it so.
            //
            // The question is in MS XNA are types normalized based on usage or
            // normalized based to their format?
            //
            if (element.VertexElementUsage == VertexElementUsage.Color)
                return true;

            switch (element.VertexElementFormat)
            {
                case VertexElementFormat.NormalizedShort2:
                case VertexElementFormat.NormalizedShort4:
                    return true;

                default:
                    return false;
            }
        }
		
		public static double GetBlendEquationMode (this BlendFunction function)
		{
			switch (function)
            {
                case BlendFunction.Add:
                    return glc.FUNC_ADD;
                case BlendFunction.ReverseSubtract:
                    return glc.FUNC_REVERSE_SUBTRACT;
                case BlendFunction.Subtract:
                    return glc.FUNC_SUBTRACT;

                default:
                    throw new ArgumentException();
			}
		}

		public static double GetBlendFactorSrc (this Blend blend)
		{
			switch (blend)
            {
                case Blend.BlendFactor:
                    return glc.CONSTANT_COLOR;
                case Blend.DestinationAlpha:
                    return glc.DST_ALPHA;
                case Blend.DestinationColor:
                    return glc.DST_COLOR;
                case Blend.InverseBlendFactor:
                    return glc.ONE_MINUS_CONSTANT_COLOR;
                case Blend.InverseDestinationAlpha:
                    return glc.ONE_MINUS_DST_ALPHA;
                case Blend.InverseDestinationColor:
                    return glc.ONE_MINUS_DST_COLOR;
                case Blend.InverseSourceAlpha:
                    return glc.ONE_MINUS_SRC_ALPHA;
                case Blend.InverseSourceColor:
                    return glc.ONE_MINUS_SRC_COLOR;
                case Blend.One:
                    return glc.ONE;
                case Blend.SourceAlpha:
                    return glc.SRC_ALPHA;
                case Blend.SourceAlphaSaturation:
                    return glc.SRC_ALPHA_SATURATE;
                case Blend.SourceColor:
                    return glc.SRC_COLOR;
                case Blend.Zero:
                    return glc.ZERO;
                default:
                    throw new ArgumentOutOfRangeException("blend", "The specified blend function is not implemented.");
            }

		}

		public static double GetBlendFactorDest (this Blend blend)
		{
			switch (blend)
            {
                case Blend.BlendFactor:
                    return glc.CONSTANT_COLOR;
                case Blend.DestinationAlpha:
                    return glc.DST_ALPHA;
                case Blend.DestinationColor:
                    return glc.DST_COLOR;
                case Blend.InverseBlendFactor:
                    return glc.ONE_MINUS_CONSTANT_COLOR;
                case Blend.InverseDestinationAlpha:
                    return glc.ONE_MINUS_DST_ALPHA;
                case Blend.InverseDestinationColor:
                    return glc.ONE_MINUS_DST_COLOR;
                case Blend.InverseSourceAlpha:
                    return glc.ONE_MINUS_SRC_ALPHA;
                case Blend.InverseSourceColor:
                    return glc.ONE_MINUS_SRC_COLOR;
                case Blend.One:
                    return glc.ONE;
                case Blend.SourceAlpha:
                    return glc.SRC_ALPHA;
                case Blend.SourceAlphaSaturation:
                    return glc.SRC_ALPHA_SATURATE;
                case Blend.SourceColor:
                    return glc.SRC_COLOR;
                case Blend.Zero:
                    return glc.ZERO;
                default:
				    throw new ArgumentOutOfRangeException("blend", "The specified blend function is not implemented.");
			}
		}

        public static double GetDepthFunction(this CompareFunction compare)
        {
            switch (compare)
            {
                default:
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
            }
        }

        public static void GetGLFormat (this SurfaceFormat format,
            GraphicsDevice graphicsDevice,
            out double glInternalFormat,
            out double glFormat,
            out double glType)
		{
			glInternalFormat = (int)glc.RGBA;
			glFormat = (int)glc.RGBA;
			glType = (int)glc.UNSIGNED_BYTE;

		    var supportsSRgb = graphicsDevice.GraphicsCapabilities.SupportsSRgb;
			
			switch (format)
            {
                case SurfaceFormat.Color:
                    glInternalFormat = glc.RGBA;
                    glFormat = glc.RGBA;
                    glType = glc.UNSIGNED_BYTE;
                    break;
                case SurfaceFormat.ColorSRgb:
                    glInternalFormat = gl.SRGB;
                    glFormat = glc.RGBA;
                    glType = glc.UNSIGNED_BYTE;
                    break;
                case SurfaceFormat.Bgr565:
                    glInternalFormat = glc.RGB;
                    glFormat = glc.RGB;
                    glType = glc.UNSIGNED_SHORT_5_6_5;
                    break;
                case SurfaceFormat.Bgra4444:
                    glInternalFormat = glc.RGBA;
                    glFormat = glc.RGBA;
                    glType = glc.UNSIGNED_SHORT_4_4_4_4;
                    break;
                case SurfaceFormat.Bgra5551:
                    glInternalFormat = glc.RGBA;
                    glFormat = glc.RGBA;
                    glType = glc.UNSIGNED_SHORT_5_5_5_1;
                    break;
                case SurfaceFormat.Alpha8:
                    glInternalFormat = glc.LUMINANCE;
                    glFormat = glc.LUMINANCE;
                    glType = glc.UNSIGNED_BYTE;
                    break;
                    break;
                default:
                    throw new NotSupportedException();
			}
		}

        public static int GetPrimitiveTypeGL(PrimitiveType primitiveType)
        {
            switch (primitiveType)
            {
                case PrimitiveType.LineList:
                    return (int)glc.LINES;
                case PrimitiveType.LineStrip:
                    return (int)glc.LINE_STRIP;
                case PrimitiveType.TriangleList:
                    return (int)glc.TRIANGLES;
                case PrimitiveType.TriangleStrip:
                    return (int)glc.TRIANGLE_STRIP;
            }

            throw new ArgumentException();
        }

		public static WebGLTexture GetBoundTexture2D()
        {
			var ret = gl.getParameter(glc.TEXTURE_BINDING_2D);
            GraphicsExtensions.LogGLError("GraphicsExtensions.GetBoundTexture2D() GL.GetInteger");

            return ret.As<WebGLTexture>();
        }
    }
}