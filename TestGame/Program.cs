using System;
using WebAssembly;
using WebGLDotNET;

namespace TestGame
{
    class Program
    {
        static async void Main()
        {
            JSObject canvas;

            using (var document = (JSObject)Runtime.GetGlobalObject("document"))
            using (var body = (JSObject)document.GetObjectProperty("body"))
            {
                canvas = (JSObject)document.Invoke("createElement", "canvas");
                body.Invoke("appendChild", canvas);
            }

            var gl = new WebGL2RenderingContext(canvas);

            var vertices = new float[]
            {
                -0.5f,  0.5f, 0.0f,
                -0.5f, -0.5f, 0.0f,
                0.5f, -0.5f, 0.0f
            };
            var vertexBuffer = gl.CreateBuffer();
            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, vertexBuffer);
            gl.BufferData(WebGLRenderingContextBase.ARRAY_BUFFER, vertices, WebGLRenderingContextBase.STATIC_DRAW);
            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, null);

            var indices = new ushort[] { 0, 1, 2 };
            var indexBuffer = gl.CreateBuffer();
            gl.BindBuffer(WebGLRenderingContextBase.ELEMENT_ARRAY_BUFFER, indexBuffer);
            gl.BufferData(WebGLRenderingContextBase.ELEMENT_ARRAY_BUFFER, indices, WebGLRenderingContextBase.STATIC_DRAW);
            gl.BindBuffer(WebGLRenderingContextBase.ELEMENT_ARRAY_BUFFER, null);

            var shaderProgram = gl.CreateProgram();

            var vertexShader = gl.CreateShader(WebGLRenderingContextBase.VERTEX_SHADER);
            gl.ShaderSource(vertexShader,
            @"attribute vec3 position;

void main(void) {
    gl_Position = vec4(position, 1.0);
}");
            gl.CompileShader(vertexShader);

            var fragmentShader = gl.CreateShader(WebGLRenderingContextBase.FRAGMENT_SHADER);
            gl.ShaderSource(fragmentShader,
            @"void main(void) {
    gl_FragColor = vec4(1.0, 0.0, 0.0, 1.0);
}");
            gl.CompileShader(fragmentShader);

            gl.AttachShader(shaderProgram, vertexShader);
            gl.AttachShader(shaderProgram, fragmentShader);

            gl.LinkProgram(shaderProgram);

            gl.UseProgram(shaderProgram);

            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, vertexBuffer);
            gl.BindBuffer(WebGLRenderingContextBase.ELEMENT_ARRAY_BUFFER, indexBuffer);

            var positionAttribute = (uint)gl.GetAttribLocation(shaderProgram, "position");
            gl.VertexAttribPointer(positionAttribute, 3, WebGLRenderingContextBase.FLOAT, false, 0, 0);
            gl.EnableVertexAttribArray(positionAttribute);

            gl.Enable(WebGLRenderingContextBase.DEPTH_TEST);

            gl.Viewport(0, 0, (int)canvas.GetObjectProperty("width"), (int)canvas.GetObjectProperty("height"));

            gl.ClearColor(0, 0, 0, 0);
            gl.Clear(WebGLRenderingContextBase.COLOR_BUFFER_BIT);

            gl.DrawElements(
                WebGLRenderingContextBase.TRIANGLES,
                indices.Length,
                WebGLRenderingContextBase.UNSIGNED_SHORT,
                0);
        }
    }
}
