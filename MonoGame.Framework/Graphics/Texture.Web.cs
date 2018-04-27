// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Bridge.WebGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public abstract partial class Texture
    {
        internal WebGLTexture glTexture;
        internal int glTarget;
        internal int glTextureUnit = Web.GL.TEXTURE0;
        internal int glInternalFormat;
        internal int glFormat;
        internal int glType;
        internal SamplerState glLastSamplerState;

        private void PlatformGraphicsDeviceResetting()
        {
            DeleteGLTexture();
            glLastSamplerState = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                DeleteGLTexture();
                glLastSamplerState = null;
            }

            base.Dispose(disposing);
        }

        private void DeleteGLTexture()
        {
            if (glTexture != null)
                GraphicsDevice.DisposeTexture(glTexture);
            glTexture = null;
        }
    }
}

