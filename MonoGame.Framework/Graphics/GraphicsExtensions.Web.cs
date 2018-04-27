// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using Bridge;
using static Retyped.dom;
using static WebHelper;

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
                    return (int)gl.FLOAT;

                case VertexElementFormat.Color:
                case VertexElementFormat.Byte4:
					return (int)gl.UNSIGNED_BYTE;

                case VertexElementFormat.Short2:
                case VertexElementFormat.Short4:
                case VertexElementFormat.NormalizedShort2:
                case VertexElementFormat.NormalizedShort4:
                    return (int)gl.SHORT;
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

        public static void GetGLFormat (this SurfaceFormat format,
            GraphicsDevice graphicsDevice,
            out int glInternalFormat,
            out int glFormat,
            out int glType)
		{
			glInternalFormat = (int)gl.RGBA;
			glFormat = (int)gl.RGBA;
			glType = (int)gl.UNSIGNED_BYTE;

		    var supportsSRgb = graphicsDevice.GraphicsCapabilities.SupportsSRgb;
			
			switch (format) {
			case SurfaceFormat.Color:
				glInternalFormat = (int)gl.RGBA;
				glFormat = (int)gl.RGBA;
				glType = (int)gl.UNSIGNED_BYTE;
				break;
            case SurfaceFormat.ColorSRgb:
                // TODO: WebGL 2 supports this, need to do some checks for this case.
                goto case SurfaceFormat.Color;
			case SurfaceFormat.Bgr565:
				glInternalFormat = (int)gl.RGB;
				glFormat = (int)gl.RGB;
				glType = (int)gl.UNSIGNED_SHORT_5_6_5;
				break;
			case SurfaceFormat.Bgra4444:
				glInternalFormat = (int)gl.RGBA;
				glFormat = (int)gl.RGBA;
				glType = (int)gl.UNSIGNED_SHORT_4_4_4_4;
				break;
			case SurfaceFormat.Bgra5551:
				glInternalFormat = (int)gl.RGBA;
				glFormat = (int)gl.RGBA;
				glType = (int)gl.UNSIGNED_SHORT_5_5_5_1;
				break;
			case SurfaceFormat.Alpha8:
				glInternalFormat = (int)gl.LUMINANCE;
				glFormat = (int)gl.LUMINANCE;
				glType = (int)gl.UNSIGNED_BYTE;
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
                    return (int)gl.LINES;
                case PrimitiveType.LineStrip:
                    return (int)gl.LINE_STRIP;
                case PrimitiveType.TriangleList:
                    return (int)gl.TRIANGLES;
                case PrimitiveType.TriangleStrip:
                    return (int)gl.TRIANGLE_STRIP;
            }

            throw new ArgumentException();
        }

		public static WebGLTexture GetBoundTexture2D()
        {
			var ret = gl.getParameter(gl.TEXTURE_BINDING_2D);
            GraphicsExtensions.LogGLError("GraphicsExtensions.GetBoundTexture2D() GL.GetInteger");

            return ret.As<WebGLTexture>();
        }
    }
}