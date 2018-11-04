// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using static Retyped.dom;
using static Retyped.es5;
using static WebHelper;
using Math = System.Math;
using glc = Retyped.webgl2.WebGL2RenderingContext;

namespace Microsoft.Xna.Framework.Graphics
{
	public partial class TextureCube
	{
        private void PlatformConstruct(GraphicsDevice graphicsDevice, int size, bool mipMap, SurfaceFormat format, bool renderTarget)
        {
            this.glTarget = glc.TEXTURE_CUBE_MAP;

            this.glTexture = gl.createTexture();
            GraphicsExtensions.CheckGLError();
            gl.bindTexture(glc.TEXTURE_CUBE_MAP, this.glTexture);
            GraphicsExtensions.CheckGLError();
            gl.texParameteri(glc.TEXTURE_CUBE_MAP, glc.TEXTURE_MIN_FILTER, mipMap ? glc.LINEAR_MIPMAP_LINEAR : glc.LINEAR);
            GraphicsExtensions.CheckGLError();
            gl.texParameteri(glc.TEXTURE_CUBE_MAP, glc.TEXTURE_MAG_FILTER, glc.LINEAR);
            GraphicsExtensions.CheckGLError();
            gl.texParameteri(glc.TEXTURE_CUBE_MAP, glc.TEXTURE_WRAP_S, glc.CLAMP_TO_EDGE);
            GraphicsExtensions.CheckGLError();
            gl.texParameteri(glc.TEXTURE_CUBE_MAP, glc.TEXTURE_WRAP_T, glc.CLAMP_TO_EDGE);
            GraphicsExtensions.CheckGLError();


            format.GetGLFormat(GraphicsDevice, out glInternalFormat, out glFormat, out glType);

            for (var i = 0; i < 6; i++)
            {
                var target = GetGLCubeFace((CubeMapFace)i);

                if (glFormat == glc.COMPRESSED_TEXTURE_FORMATS)
                {
                    var imageSize = 0;
                    switch (format)
                    {
                        case SurfaceFormat.RgbPvrtc2Bpp:
                        case SurfaceFormat.RgbaPvrtc2Bpp:
                            imageSize = (Math.Max(size, 16) * Math.Max(size, 8) * 2 + 7) / 8;
                            break;
                        case SurfaceFormat.RgbPvrtc4Bpp:
                        case SurfaceFormat.RgbaPvrtc4Bpp:
                            imageSize = (Math.Max(size, 8) * Math.Max(size, 8) * 4 + 7) / 8;
                            break;
                        case SurfaceFormat.Dxt1:
                        case SurfaceFormat.Dxt1a:
                        case SurfaceFormat.Dxt1SRgb:
                        case SurfaceFormat.Dxt3:
                        case SurfaceFormat.Dxt3SRgb:
                        case SurfaceFormat.Dxt5:
                        case SurfaceFormat.Dxt5SRgb:
                        case SurfaceFormat.RgbEtc1:
                        case SurfaceFormat.RgbaAtcExplicitAlpha:
                        case SurfaceFormat.RgbaAtcInterpolatedAlpha:
                            imageSize = (size + 3) / 4 * ((size + 3) / 4) * format.GetSize();
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                    gl.compressedTexImage2D(target, 0, glInternalFormat, size, size, 0, null);
                    GraphicsExtensions.CheckGLError();
                }
                else
                {
                    gl.texImage2D(target, 0, glInternalFormat, size, size, 0, glFormat, glType, (new ImageData(size, size).As<Retyped.webgl2.ImageBitmap>()));
                    GraphicsExtensions.CheckGLError();
                }
            }

            if (mipMap)
            {
                gl.generateMipmap(glc.TEXTURE_CUBE_MAP);
                GraphicsExtensions.CheckGLError();
            }
        }

        private void PlatformGetData<T>(CubeMapFace cubeMapFace, int level, Rectangle rect, T[] data, int startIndex, int elementCount) where T : struct
        {
            throw new NotImplementedException();
        }

        private void PlatformSetData<T>(CubeMapFace face, int level, Rectangle rect, T[] data, int startIndex, int elementCount)
        {
            var subarr = new Uint8Array(data.As<ArrayBuffer>(), startIndex.As<uint>(), elementCount.As<uint>());

            gl.bindTexture(glc.TEXTURE_CUBE_MAP, this.glTexture);
            GraphicsExtensions.CheckGLError();

            var target = GetGLCubeFace(face);
            if (glFormat == glc.COMPRESSED_TEXTURE_FORMATS)
            {
                gl.compressedTexSubImage2D(target, level, rect.X, rect.Y, rect.Width, rect.Height, glInternalFormat, subarr.As<ArrayBufferView>());
                GraphicsExtensions.CheckGLError();
            }
            else
            {
                gl.texSubImage2D(target, level, rect.X, rect.Y, rect.Width, rect.Height, glFormat, glType, subarr.As<ArrayBufferView>());
                GraphicsExtensions.CheckGLError();
            }
        }

		private double GetGLCubeFace(CubeMapFace face)
        {
			switch (face)
            {
                case CubeMapFace.PositiveX:
                    return glc.TEXTURE_CUBE_MAP_POSITIVE_X;
                case CubeMapFace.NegativeX:
                    return glc.TEXTURE_CUBE_MAP_NEGATIVE_X;
                case CubeMapFace.PositiveY:
                    return glc.TEXTURE_CUBE_MAP_POSITIVE_Y;
                case CubeMapFace.NegativeY:
                    return glc.TEXTURE_CUBE_MAP_NEGATIVE_Y;
                case CubeMapFace.PositiveZ:
                    return glc.TEXTURE_CUBE_MAP_POSITIVE_Z;
                case CubeMapFace.NegativeZ:
                    return glc.TEXTURE_CUBE_MAP_NEGATIVE_Z;
			}
			throw new ArgumentException();
		}
	}
}

