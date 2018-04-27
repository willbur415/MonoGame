// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Bridge.WebGL;
using Bridge.Html5;
using Bridge;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture2D : Texture
    {
        private void PlatformConstruct(int width, int height, bool mipmap, SurfaceFormat format, SurfaceType type, bool shared)
        {
            this.glTarget = Web.GL.TEXTURE_2D;
            format.GetGLFormat(GraphicsDevice, out glInternalFormat, out glFormat, out glType);
            
            GenerateGLTextureIfRequired();
            int w = width;
            int h = height;
            int level = 0;
            while (true)
            {

                if (glFormat == Web.GL.COMPRESSED_TEXTURE_FORMATS)
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
                    // GL.CompressedTexImage2D(TextureTarget.Texture2D, level, glInternalFormat, w, h, 0, imageSize, IntPtr.Zero);
                    // Web.GL.TexImage2D(Web.GL.TEXTURE_2D, level, glInternalFormat, w, h, 0, glFormat, glType, new Bridge.Html5.Int8Array(0));
                    // GraphicsExtensions.CheckGLError();
                }
                else
                {
                    // Web.GL.TexImage2D(Web.GL.TEXTURE_2D, level, glInternalFormat, w, h, 0, glFormat, glType, new Bridge.Html5.Int8Array(0));
                    // GraphicsExtensions.CheckGLError();
                }

                Web.GL.TexImage2D(Web.GL.TEXTURE_2D, level, glInternalFormat, glFormat, glType, new Bridge.Html5.ImageData(1, 1));
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
                glTexture = Web.GL.CreateTexture();
                GraphicsExtensions.CheckGLError();

                // For best compatibility and to keep the default wrap mode of XNA, only set ClampToEdge if either
                // dimension is not a power of two.
                var wrap = Web.GL.REPEAT;
                if (((width & (width - 1)) != 0) || ((height & (height - 1)) != 0))
                    wrap = Web.GL.CLAMP_TO_EDGE;

                Web.GL.BindTexture(Web.GL.TEXTURE_2D, glTexture);
                GraphicsExtensions.CheckGLError();
                Web.GL.TexParameteri(Web.GL.TEXTURE_2D, Web.GL.TEXTURE_MIN_FILTER, (_levelCount > 1) ? Web.GL.LINEAR_MIPMAP_LINEAR : Web.GL.LINEAR);
                GraphicsExtensions.CheckGLError();
                Web.GL.TexParameteri(Web.GL.TEXTURE_2D, Web.GL.TEXTURE_MAG_FILTER, Web.GL.LINEAR);
                GraphicsExtensions.CheckGLError();
                Web.GL.TexParameteri(Web.GL.TEXTURE_2D, Web.GL.TEXTURE_WRAP_S, wrap);
                GraphicsExtensions.CheckGLError();
                Web.GL.TexParameteri(Web.GL.TEXTURE_2D, Web.GL.TEXTURE_WRAP_T, wrap);
                GraphicsExtensions.CheckGLError();
                // Set mipmap levels
                // WebGL 2
                // Web.GL.TexParameteri(Web.GL.TEXTURE_BASE_LEVEL, Web.GL.texture_base, 0);
                // GraphicsExtensions.CheckGLError();
                // WebGL 2
                /*if (GraphicsDevice.GraphicsCapabilities.SupportsTextureMaxLevel)
                {
                    if (_levelCount > 0)
                    {
                        GL.TexParameter(TextureTarget.Texture2D, SamplerState.TextureParameterNameTextureMaxLevel, _levelCount - 1);
                    }
                    else
                    {
                        GL.TexParameter(TextureTarget.Texture2D, SamplerState.TextureParameterNameTextureMaxLevel, 1000);
                    }
                    GraphicsExtensions.CheckGLError();
                }*/
            }
        }

        

        private void PlatformSetData<T>(int level, T[] data, int startIndex, int elementCount) where T : struct
        {
            Console.WriteLine(LastTSize);

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
                Web.GL.BindTexture(Web.GL.TEXTURE_2D, glTexture);
                GraphicsExtensions.CheckGLError();
            }

            GenerateGLTextureIfRequired();
            Web.GL.PixelStorei(Web.GL.UNPACK_ALIGNMENT, Math.Min(_format.GetSize(), 8));

            if (glFormat == Web.GL.COMPRESSED_TEXTURE_FORMATS)
            {
                throw new NotImplementedException();
                /*ArrayBufferView arr = new Uint32Array(elementCount);

                Web.GL.CompressedTexImage2D(Web.GL.TEXTURE_2D, level, glInternalFormat, w, h, 0, arr);*/
            }
            else
            {
                Uint8Array arr = new Uint8Array(elementCount);
                for (int i = startIndex; i < elementCount; i++)
                    arr[i] = Convert.ToByte(data[i]);

                Web.GL.TexImage2D(Web.GL.TEXTURE_2D, level, glInternalFormat, w, h, 0, glFormat, glType, arr);
            }
            GraphicsExtensions.CheckGLError();
            
            // Restore the bound texture.
            if (prevTexture != glTexture)
            {
                Script.InvokeMethod(Web.GL, "bindTexture", Web.GL.TEXTURE_2D, prevTexture);
                GraphicsExtensions.CheckGLError();
            }
        }

        private void PlatformSetData<T>(int level, int arraySlice, Rectangle rect, T[] data, int startIndex, int elementCount) where T : struct
        {
            // var startBytes = startIndex * elementSizeInByte;
            // var dataPtr = new IntPtr(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);
            // Store the current bound texture.
            /* var prevTexture = GraphicsExtensions.GetBoundTexture2D();

            if (prevTexture != glTexture)
            {
                Web.GL.BindTexture(Web.GL.TEXTURE_2D, glTexture);
                GraphicsExtensions.CheckGLError();
            }

            GenerateGLTextureIfRequired();
            Web.GL.PixelStorei(Web.GL.UNPACK_ALIGNMENT, Math.Min(_format.GetSize(), 8));

            if (glFormat == (PixelFormat)GLPixelFormat.CompressedTextureFormats)
            {
                GL.CompressedTexSubImage2D(TextureTarget.Texture2D, level, rect.X, rect.Y, rect.Width, rect.Height,
                    (PixelInternalFormat)glInternalFormat, elementCount * elementSizeInByte, dataPtr);
            }
            else
            {
                GL.TexSubImage2D(TextureTarget.Texture2D, level, rect.X, rect.Y,
                    rect.Width, rect.Height, glFormat, glType, dataPtr);
            }
            GraphicsExtensions.CheckGLError();

            // Restore the bound texture.
            if (prevTexture != glTexture)
            {
                Web.GL.BindTexture(Web.GL.TEXTURE_2D, prevTexture);
                GraphicsExtensions.CheckGLError();
            }*/
            throw new NotImplementedException();
        }

        private void PlatformGetData<T>(int level, int arraySlice, Rectangle rect, T[] data, int startIndex, int elementCount) where T : struct
        {
            throw new NotImplementedException();
        }

        private static Texture2D PlatformFromStream(GraphicsDevice graphicsDevice, Stream stream)
        {
            throw new NotImplementedException();
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

