// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using static WebHelper;
using glc = Retyped.webgl2.WebGL2RenderingContext;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class SamplerState
    {
        internal const double TextureParameterNameTextureMaxAnisotropy = 0x84FE;
        private readonly float[] _openGLBorderColor = new float[4];

        internal void Activate(GraphicsDevice device, double target, bool useMipmaps = false)
        {
            if (GraphicsDevice == null)
            {
                // We're now bound to a device... no one should
                // be changing the state of this object now!
                GraphicsDevice = device;
            }
            Debug.Assert(GraphicsDevice == device, "The state was created for a different device!");

            switch (Filter)
            {
                case TextureFilter.Point:
                    if (GraphicsDevice.GraphicsCapabilities.SupportsTextureFilterAnisotropic)
                    {
                        gl.texParameteri(target, TextureParameterNameTextureMaxAnisotropy, 1.0f);
                        GraphicsExtensions.CheckGLError();
                    }
                    gl.texParameteri(target, glc.TEXTURE_MIN_FILTER, useMipmaps ? glc.NEAREST_MIPMAP_NEAREST : glc.NEAREST);
                    GraphicsExtensions.CheckGLError();
                    gl.texParameteri(target, glc.TEXTURE_MAG_FILTER, glc.NEAREST);
                    GraphicsExtensions.CheckGLError();
                    break;
                case TextureFilter.Linear:
                    if (GraphicsDevice.GraphicsCapabilities.SupportsTextureFilterAnisotropic)
                    {
                        gl.texParameteri(target, TextureParameterNameTextureMaxAnisotropy, 1.0f);
                        GraphicsExtensions.CheckGLError();
                    }
                    gl.texParameteri(target, glc.TEXTURE_MIN_FILTER, useMipmaps ? glc.LINEAR_MIPMAP_LINEAR : glc.LINEAR);
                    GraphicsExtensions.CheckGLError();
                    gl.texParameteri(target, glc.TEXTURE_MAG_FILTER, glc.LINEAR);
                    GraphicsExtensions.CheckGLError();
                    break;
                case TextureFilter.Anisotropic:
                    if (GraphicsDevice.GraphicsCapabilities.SupportsTextureFilterAnisotropic)
                    {
                        gl.texParameteri(target, TextureParameterNameTextureMaxAnisotropy, MathHelper.Clamp(this.MaxAnisotropy, 1.0f, GraphicsDevice.GraphicsCapabilities.MaxTextureAnisotropy));
                        GraphicsExtensions.CheckGLError();
                    }
                    gl.texParameteri(target, glc.TEXTURE_MIN_FILTER, useMipmaps ? glc.LINEAR_MIPMAP_LINEAR : glc.LINEAR);
                    GraphicsExtensions.CheckGLError();
                    gl.texParameteri(target, glc.TEXTURE_MAG_FILTER, glc.LINEAR);
                    GraphicsExtensions.CheckGLError();
                    break;
                case TextureFilter.PointMipLinear:
                    if (GraphicsDevice.GraphicsCapabilities.SupportsTextureFilterAnisotropic)
                    {
                        gl.texParameteri(target, TextureParameterNameTextureMaxAnisotropy, 1.0f);
                        GraphicsExtensions.CheckGLError();
                    }
                    gl.texParameteri(target, glc.TEXTURE_MIN_FILTER, useMipmaps ? glc.NEAREST_MIPMAP_LINEAR : glc.NEAREST);
                    GraphicsExtensions.CheckGLError();
                    gl.texParameteri(target, glc.TEXTURE_MAG_FILTER, glc.NEAREST);
                    GraphicsExtensions.CheckGLError();
                    break;
                case TextureFilter.LinearMipPoint:
                    if (GraphicsDevice.GraphicsCapabilities.SupportsTextureFilterAnisotropic)
                    {
                        gl.texParameteri(target, TextureParameterNameTextureMaxAnisotropy, 1.0f);
                        GraphicsExtensions.CheckGLError();
                    }
                    gl.texParameteri(target, glc.TEXTURE_MIN_FILTER, useMipmaps ? glc.LINEAR_MIPMAP_NEAREST : glc.LINEAR);
                    GraphicsExtensions.CheckGLError();
                    gl.texParameteri(target, glc.TEXTURE_MAG_FILTER, glc.LINEAR);
                    GraphicsExtensions.CheckGLError();
                    break;
                case TextureFilter.MinLinearMagPointMipLinear:
                    if (GraphicsDevice.GraphicsCapabilities.SupportsTextureFilterAnisotropic)
                    {
                        gl.texParameteri(target, TextureParameterNameTextureMaxAnisotropy, 1.0f);
                        GraphicsExtensions.CheckGLError();
                    }
                    gl.texParameteri(target, glc.TEXTURE_MIN_FILTER, useMipmaps ? glc.LINEAR_MIPMAP_LINEAR : glc.LINEAR);
                    GraphicsExtensions.CheckGLError();
                    gl.texParameteri(target, glc.TEXTURE_MAG_FILTER, glc.NEAREST);
                    GraphicsExtensions.CheckGLError();
                    break;
                case TextureFilter.MinLinearMagPointMipPoint:
                    if (GraphicsDevice.GraphicsCapabilities.SupportsTextureFilterAnisotropic)
                    {
                        gl.texParameteri(target, TextureParameterNameTextureMaxAnisotropy, 1.0f);
                        GraphicsExtensions.CheckGLError();
                    }
                    gl.texParameteri(target, glc.TEXTURE_MIN_FILTER, useMipmaps ? glc.LINEAR_MIPMAP_NEAREST : glc.LINEAR);
                    GraphicsExtensions.CheckGLError();
                    gl.texParameteri(target, glc.TEXTURE_MAG_FILTER, glc.NEAREST);
                    GraphicsExtensions.CheckGLError();
                    break;
                case TextureFilter.MinPointMagLinearMipLinear:
                    if (GraphicsDevice.GraphicsCapabilities.SupportsTextureFilterAnisotropic)
                    {
                        gl.texParameteri(target, TextureParameterNameTextureMaxAnisotropy, 1.0f);
                        GraphicsExtensions.CheckGLError();
                    }
                    gl.texParameteri(target, glc.TEXTURE_MIN_FILTER, useMipmaps ? glc.NEAREST_MIPMAP_LINEAR : glc.NEAREST);
                    GraphicsExtensions.CheckGLError();
                    gl.texParameteri(target, glc.TEXTURE_MAG_FILTER, glc.LINEAR);
                    GraphicsExtensions.CheckGLError();
                    break;
                case TextureFilter.MinPointMagLinearMipPoint:
                    if (GraphicsDevice.GraphicsCapabilities.SupportsTextureFilterAnisotropic)
                    {
                        gl.texParameteri(target, TextureParameterNameTextureMaxAnisotropy, 1.0f);
                        GraphicsExtensions.CheckGLError();
                    }
                    gl.texParameteri(target, glc.TEXTURE_MIN_FILTER, useMipmaps ? glc.NEAREST_MIPMAP_NEAREST : glc.NEAREST);
                    GraphicsExtensions.CheckGLError();
                    gl.texParameteri(target, glc.TEXTURE_MAG_FILTER, glc.LINEAR);
                    GraphicsExtensions.CheckGLError();
                    break;
                default:
                    throw new NotSupportedException();
            }

            // Set up texture addressing.
            gl.texParameteri(target, glc.TEXTURE_WRAP_S, (int)GetWrapMode(AddressU));
            GraphicsExtensions.CheckGLError();
            gl.texParameteri(target, glc.TEXTURE_WRAP_T, (int)GetWrapMode(AddressV));
            GraphicsExtensions.CheckGLError();

            // LOD bias is not supported by glTexParameter in OpenGL ES 2.0
            gl.texParameterf(target, gl.MAX_TEXTURE_LOD_BIAS, MipMapLevelOfDetailBias);
            GraphicsExtensions.CheckGLError();

            // Comparison samplers are not supported in OpenGL ES 2.0 (without an extension, anyway)
            switch (FilterMode)
            {
                case TextureFilterMode.Comparison:
                    gl.texParameteri(target, gl.TEXTURE_COMPARE_MODE, gl.COMPARE_REF_TO_TEXTURE);
                    GraphicsExtensions.CheckGLError();
                    gl.texParameteri(target, gl.TEXTURE_COMPARE_MODE, ComparisonFunction.GetDepthFunction());
                    GraphicsExtensions.CheckGLError();
                    break;
                case TextureFilterMode.Default:
                    gl.texParameteri(target, gl.TEXTURE_COMPARE_MODE, glc.NONE);
                    GraphicsExtensions.CheckGLError();
                    break;
                default:
                    throw new InvalidOperationException("Invalid filter mode!");
            }

            if (GraphicsDevice.GraphicsCapabilities.SupportsTextureMaxLevel)
            {
                if (this.MaxMipLevel > 0)
                {
                    gl.texParameteri(glc.TEXTURE_2D, gl.TEXTURE_MAX_LEVEL, this.MaxMipLevel);
                }
                else
                {
                    gl.texParameteri(glc.TEXTURE_2D, gl.TEXTURE_MAX_LEVEL, 1000);
                }
                GraphicsExtensions.CheckGLError();
            }
        }

        private double GetWrapMode(TextureAddressMode textureAddressMode)
        {
            switch (textureAddressMode)
            {
                case TextureAddressMode.Clamp:
                    return glc.CLAMP_TO_EDGE;
                case TextureAddressMode.Wrap:
                    return glc.REPEAT;
                case TextureAddressMode.Mirror:
                    return glc.MIRRORED_REPEAT;
                default:
                    throw new ArgumentException("No support for " + textureAddressMode);
            }
        }
    }
}

