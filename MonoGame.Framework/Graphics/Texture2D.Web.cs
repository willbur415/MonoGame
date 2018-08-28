// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Threading.Tasks;
using static Retyped.dom;
using static Retyped.es5;
using static WebHelper;
using Math = System.Math;
using glc = Retyped.webgl2.WebGL2RenderingContext;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture2D : Texture
    {
        private void PlatformConstruct(int width, int height, bool mipmap, SurfaceFormat format, SurfaceType type, bool shared)
        {
            this.glTarget = glc.TEXTURE_2D;
            format.GetGLFormat(GraphicsDevice, out glInternalFormat, out glFormat, out glType);
            
            GenerateGLTextureIfRequired();
            int w = width;
            int h = height;
            int level = 0;
            while (true)
            {

                if (glFormat == glc.COMPRESSED_TEXTURE_FORMATS)
                {
                    int imageSize = 0;
                    // PVRTC has explicit calculations for imageSize
                    // https://www.khronos.org/registry/OpenGL/extensions/IMG/IMG_texture_compression_pvrtc.txt
                    if (format == SurfaceFormat.RgbPvrtc2Bpp || format == SurfaceFormat.RgbaPvrtc2Bpp)
                    {
                        imageSize = (Math.Max(w, 16) * Math.Max(h, 8) * 2 + 7) / 8;
                    }
                    else if (format == SurfaceFormat.RgbPvrtc4Bpp || format == SurfaceFormat.RgbaPvrtc4Bpp)
                    {
                        imageSize = (Math.Max(w, 8) * Math.Max(h, 8) * 4 + 7) / 8;
                    }
                    else
                    {
                        int blockSize = format.GetSize();
                        int blockWidth, blockHeight;
                        format.GetBlockSize(out blockWidth, out blockHeight);
                        int wBlocks = (w + (blockWidth - 1)) / blockWidth;
                        int hBlocks = (h + (blockHeight - 1)) / blockHeight;
                        imageSize = wBlocks * hBlocks * blockSize;
                    }
                }

                gl.texImage2D(glc.TEXTURE_2D, level, glInternalFormat, glFormat, glType, (new ImageData(w, h).As<Retyped.webgl2.ImageBitmap>()));
                GraphicsExtensions.CheckGLError();

                if ((w == 1 && h == 1) || !mipmap)
                    break;
                if (w > 1)
                    w = w / 2;
                if (h > 1)
                    h = h / 2;
                ++level;
            }
        }

        private void GenerateGLTextureIfRequired()
        {
            if (this.glTexture == null)
            {
                glTexture = gl.createTexture();
                GraphicsExtensions.CheckGLError();

                // For best compatibility and to keep the default wrap mode of XNA, only set ClampToEdge if either
                // dimension is not a power of two.
                var wrap = glc.REPEAT;
                if (((width & (width - 1)) != 0) || ((height & (height - 1)) != 0))
                    wrap = glc.CLAMP_TO_EDGE;

                gl.bindTexture(glc.TEXTURE_2D, glTexture);
                GraphicsExtensions.CheckGLError();
                gl.texParameteri(glc.TEXTURE_2D, glc.TEXTURE_MIN_FILTER, (_levelCount > 1) ? glc.LINEAR_MIPMAP_LINEAR : glc.LINEAR);
                GraphicsExtensions.CheckGLError();
                gl.texParameteri(glc.TEXTURE_2D, glc.TEXTURE_MAG_FILTER, glc.LINEAR);
                GraphicsExtensions.CheckGLError();
                gl.texParameteri(glc.TEXTURE_2D, glc.TEXTURE_WRAP_S, wrap);
                GraphicsExtensions.CheckGLError();
                gl.texParameteri(glc.TEXTURE_2D, glc.TEXTURE_WRAP_T, wrap);
                GraphicsExtensions.CheckGLError();
            }
        }

        private void PlatformSetData<T>(int level, T[] data, int startIndex, int elementCount) where T : struct
        {
            int w, h;
            GetSizeForLevel(Width, Height, level, out w, out h);

            // var elementSizeInByte = ReflectionHelpers.SizeOf<T>.Get();
            // var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            // Use try..finally to make sure dataHandle is freed in case of an error

            // var startBytes = startIndex * elementSizeInByte;
            // var dataPtr = new IntPtr(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);
            // Store the current bound texture.
            var prevTexture = GraphicsExtensions.GetBoundTexture2D();

            if (prevTexture != glTexture)
            {
                gl.bindTexture(glc.TEXTURE_2D, glTexture);
                GraphicsExtensions.CheckGLError();
            }

            GenerateGLTextureIfRequired();
            gl.pixelStorei(glc.UNPACK_ALIGNMENT, Math.Min(_format.GetSize(), 8));

            if (glFormat == glc.COMPRESSED_TEXTURE_FORMATS)
            {
                if (LastTSize == 1)
                {
                    var arr2 = new Uint8Array((uint)elementCount);
                    for (uint i = 0; i < elementCount; i++)
                        arr2[i] = data[i + startIndex].As<byte>();
                    gl.compressedTexImage2D(glc.TEXTURE_2D, level, glInternalFormat, w, h, 0, arr2);
                }
                else if (LastTSize == 2)
                {
                    var arr2 = new Uint16Array((uint)elementCount);
                    for (uint i = 0; i < elementCount; i++)
                        arr2[i] = data[i + startIndex].As<ushort>();
                    gl.compressedTexImage2D(glc.TEXTURE_2D, level, glInternalFormat, w, h, 0, arr2);
                }
                else if (LastTSize == 4)
                {
                    var arr2 = new Uint32Array((uint)elementCount);
                    for (uint i = 0; i < elementCount; i++)
                        arr2[i] = data[i + startIndex].As<uint>();
                    gl.compressedTexImage2D(glc.TEXTURE_2D, level, glInternalFormat, w, h, 0, arr2);
                }
            }
            else
            {
                if (LastTSize == 1)
                {
                    var arr = new Uint8Array((uint)elementCount);
                    for (uint i = 0; i < elementCount; i++)
                        arr[i] = data[i + startIndex].As<byte>();
                    gl.texImage2D(glc.TEXTURE_2D, level, glInternalFormat, w, h, 0, glFormat, glType, arr.As<ArrayBufferView>());
                }
                else if (LastTSize == 2)
                {
                    var arr = new Uint16Array((uint)elementCount);
                    for (uint i = 0; i < elementCount; i++)
                        arr[i] = data[i + startIndex].As<ushort>();
                    gl.texImage2D(glc.TEXTURE_2D, level, glInternalFormat, w, h, 0, glFormat, glType, arr.As<ArrayBufferView>());
                }
                else if (LastTSize == 4)
                {
                    var arr = new Uint32Array((uint)elementCount);
                    for (uint i = 0; i < elementCount; i++)
                        arr[i] = data[i + startIndex].As<uint>();
                    gl.texImage2D(glc.TEXTURE_2D, level, glInternalFormat, w, h, 0, glFormat, glType, arr.As<ArrayBufferView>());
                }
            }
            GraphicsExtensions.CheckGLError();
            
            // Restore the bound texture.
            if (prevTexture != glTexture)
            {
                gl.bindTexture(glc.TEXTURE_2D, prevTexture);
                GraphicsExtensions.CheckGLError();
            }
        }

        private void PlatformSetData<T>(int level, int arraySlice, Rectangle rect, T[] data, int startIndex, int elementCount) where T : struct
        {
            throw new NotImplementedException();
        }

        private void PlatformSetData(HTMLImageElement image)
        {
            var prevTexture = GraphicsExtensions.GetBoundTexture2D();
            if (prevTexture != glTexture)
            {
                gl.bindTexture(glc.TEXTURE_2D, glTexture);
                GraphicsExtensions.CheckGLError();
            }

            gl.texImage2D(glc.TEXTURE_2D, 0, glc.RGBA, glc.RGBA, glc.UNSIGNED_BYTE, image.As<Retyped.webgl2.ImageBitmap>());
            GraphicsExtensions.CheckGLError();
            
            // Restore the bound texture.
            if (prevTexture != glTexture)
            {
                gl.bindTexture(glc.TEXTURE_2D, prevTexture);
                GraphicsExtensions.CheckGLError();
            }
        }

        private void PlatformGetData<T>(int level, int arraySlice, Rectangle rect, T[] data, int startIndex, int elementCount) where T : struct
        {
            throw new NotImplementedException();
        }

        private static Texture2D PlatformFromStream(GraphicsDevice graphicsDevice, Stream stream)
        {
            throw new NotImplementedException();
        }

        public static async Task<Texture2D> FromURL(GraphicsDevice graphicsDevice, string url)
        {
            var loaded = false;
            var image = new HTMLImageElement();

            image.onload += (e) => 
            {
                loaded = true;
            };
            image.src = url;

            while (!loaded)
                await Task.Delay(10);

            var ret = new Texture2D(graphicsDevice, (int)image.width, (int)image.height);
            ret.PlatformSetData(image);

            return ret;
        }

        private void PlatformSaveAsJpeg(Stream stream, int width, int height)
        {
            throw new NotImplementedException();
        }

        private void PlatformSaveAsPng(Stream stream, int width, int height)
        {
            throw new NotImplementedException();
        }

        private void PlatformReload(Stream textureStream)
        {
            throw new NotImplementedException();
        }
	}
}

