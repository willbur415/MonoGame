// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using Bridge.WebGL;
using Bridge;

namespace Microsoft.Xna.Framework.Graphics
{
    internal static partial class GraphicsExtensions
    {
        public static void GetGLFormat (this SurfaceFormat format,
            GraphicsDevice graphicsDevice,
            out int glInternalFormat,
            out int glFormat,
            out int glType)
		{
			glInternalFormat = Web.GL.RGBA;
			glFormat = Web.GL.RGBA;
			glType = Web.GL.UNSIGNED_BYTE;

		    var supportsSRgb = graphicsDevice.GraphicsCapabilities.SupportsSRgb;
			
			switch (format) {
			case SurfaceFormat.Color:
				glInternalFormat = Web.GL.RGBA;
				glFormat = Web.GL.RGBA;
				glType = Web.GL.UNSIGNED_BYTE;
				break;
            case SurfaceFormat.ColorSRgb:
                // TODO: WebGL 2 supports this, need to do some checks for this case.
                goto case SurfaceFormat.Color;
			case SurfaceFormat.Bgr565:
				glInternalFormat =  Web.GL.RGB;
				glFormat = Web.GL.RGB;
				glType = Web.GL.UNSIGNED_SHORT_5_6_5;
				break;
			case SurfaceFormat.Bgra4444:
				glInternalFormat = Web.GL.RGBA;
				glFormat = Web.GL.RGBA;
				glType = Web.GL.UNSIGNED_SHORT_4_4_4_4;
				break;
			case SurfaceFormat.Bgra5551:
				glInternalFormat = Web.GL.RGBA;
				glFormat = Web.GL.RGBA;
				glType = Web.GL.UNSIGNED_SHORT_5_5_5_1;
				break;
			case SurfaceFormat.Alpha8:
				glInternalFormat = Web.GL.LUMINANCE;
				glFormat = Web.GL.LUMINANCE;
				glType = Web.GL.UNSIGNED_BYTE;
				break;
            default:
				throw new NotSupportedException();
			}
		}

		public static WebGLTexture GetBoundTexture2D()
        {
			//WebGLTexture ret = Web.GL.GetParameter(Web.GL.TEXTURE_BINDING_2D).Cast<WebGLTexture>();
            //GraphicsExtensions.LogGLError("GraphicsExtensions.GetBoundTexture2D() GL.GetInteger");
			return null;
        }
    }
}