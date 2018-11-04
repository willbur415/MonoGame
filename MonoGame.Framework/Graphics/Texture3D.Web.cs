// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using static Retyped.dom;
using static Retyped.es5;
using static WebHelper;
using glc = Retyped.webgl2.WebGL2RenderingContext;

namespace Microsoft.Xna.Framework.Graphics
{
	public partial class Texture3D : Texture
	{
        private void PlatformConstruct(
            GraphicsDevice graphicsDevice, 
            int width,
            int height, 
            int depth, 
            bool mipMap, 
            SurfaceFormat format, 
            bool renderTarget)
        {
            this.glTarget = gl.TEXTURE_3D;

            glTexture = gl.createTexture();
            GraphicsExtensions.CheckGLError();

            gl.bindTexture(glTarget, glTexture);
            GraphicsExtensions.CheckGLError();

            format.GetGLFormat(GraphicsDevice, out glInternalFormat, out glFormat, out glType);

            gl.texImage3D(glTarget, 0, glInternalFormat, width, height, depth, 0, glFormat, glType, new ImageData(width, height));

            GraphicsExtensions.CheckGLError();

            if (mipMap)
                throw new NotImplementedException("Texture3D does not yet support mipmaps.");
        }

        private void PlatformSetData<T>(
            int level,
            int left, 
            int top, 
            int right, 
            int bottom, 
            int front, 
            int back,
            T[] data,
            int startIndex,
            int elementCount,
            int width, 
            int height, 
            int depth)
        {
            var subarr = new Uint8Array(data.As<ArrayBuffer>(), startIndex.As<uint>(), elementCount.As<uint>());

            gl.bindTexture(glTarget, glTexture);
            GraphicsExtensions.CheckGLError();

            gl.texSubImage3D(glTarget, level, left, top, front, width, height, depth, glFormat, glType, subarr.As<ArrayBufferView>());
            GraphicsExtensions.CheckGLError();
        }

        private void PlatformGetData<T>(
            int level,
            int left,
            int top,
            int right,
            int bottom,
            int front,
            int back, 
            T[] data, 
            int startIndex, 
            int elementCount) where T : struct
        {
            throw new NotImplementedException();
        }
	}
}

