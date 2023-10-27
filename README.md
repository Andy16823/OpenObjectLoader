OpenObjectLoader
================

After years i rewritten the whole libary. Its now possible to load an model more easy.Its also possible
to load multiple shapes (objects) within one model. This shapes can have multiple materials with multiple textures. At the
moment every shape has own materials. So there are no shared materials yet.

Example
==
``` C#
using NetGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GenesisMath.Math;
using GlmSharp;
using OpenObjectLoader;

namespace WindowsFormsApp1
{
    /// <summary>
    /// Model Loading Example
    /// </summary>
    public partial class Form1 : Form
    {
        private NetGL.OpenGL gl;
        private float rotate;
        private List<Model> _models;

        /// <summary>
        /// Initial the Windows Form
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            DoubleBuffered = false;
        }       

        /// <summary>
        /// Our rendering thread
        /// </summary>
        private void loop()
        {
            //First we need to define where our models located in
            String modelspath = new System.IO.FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).Directory + "\\Models";

            //Then we create a new instance from the wavefront loader and read the .obj file
            WavefrontLoader loader = new WavefrontLoader();

            //After this we load the model
            Model model = loader.LoadModel(modelspath + "\\Test3.obj");

            //Now we create our shader
            string vertexShaderCode = @"
                #version 330 core

                layout(location = 0) in vec3 inPosition;
                layout(location = 1) in vec3 inColor;
                layout(location = 2) in vec2 inTexCoord;

                out vec3 color;
                out vec2 texCoord;

                uniform mat4 mvp;

                void main()
                {
                    gl_Position = mvp * vec4(inPosition, 1.0);
                    color = inColor;
                    texCoord = inTexCoord;
                }
            ";

            //Creating the fragment shader
            string fragmentShaderCode = @"
                #version 330 core

                in vec3 color;
                in vec2 texCoord;

                out vec4 fragColor;
                uniform sampler2D textureSampler;

                void main()
                {
                    fragColor = texture(textureSampler, texCoord) * vec4(1.0, 1.0, 1.0, 1.0);
                }
            ";

            //Create a new instance from netgl
            gl = new NetGL.OpenGL();
            gl.modernGL = true;
            gl.Initial(this.panel1.Handle);
            gl.SwapIntervalEXT(0);
            gl.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            
            //Enable depthtest
            gl.Enable(OpenGL.DepthTest);
            gl.DepthFunc(OpenGL.Less);
            
            //Compile the vertex shader
            int vertexShader = gl.CreateShader(OpenGL.VertexShader);
            gl.SetShaderSource(vertexShader, 1, vertexShaderCode);
            gl.CompileShader(vertexShader);

            //Compile the fragment shader
            int fragmentShader = gl.CreateShader(OpenGL.FragmentShader);
            gl.SetShaderSource(fragmentShader, 1, fragmentShaderCode);
            gl.CompileShader(fragmentShader);

            //Create the shader program
            int program = gl.CreateProgram();
            gl.AttachShader(program, vertexShader);
            gl.AttachShader(program, fragmentShader);
            gl.LinkProgram(program);

            //After linking the program, we can delete the shader source
            gl.DeleteShader(vertexShader);
            gl.DeleteShader(fragmentShader);

            //Now we initial the model
            foreach (var shape in model.Shapes)
            {
                foreach (var material in shape.Materials)
                {
                    //First we load the texture
                    this.LoadTexture(material);
                    

                    //Then we load the vertex buffer
                    float[] verticies = material.IndexVerticies();
                    int vertexBuffer = gl.GenBuffer(1);
                    material.Propeterys.Add("vbo", vertexBuffer);
                    gl.BindBuffer(OpenGL.ArrayBuffer, vertexBuffer);
                    gl.BufferData(OpenGL.ArrayBuffer, verticies.Length * sizeof(float), verticies, OpenGL.StaticDraw);

                    //Then we load the texture coords
                    //Generate the texture buffer from the index data
                    float[] texcoords = material.IndexTexCoords();
                    int tbo = gl.GenBuffer(1);
                    material.Propeterys.Add("tbo", tbo);
                    gl.BindBuffer(OpenGL.ArrayBuffer, tbo);
                    gl.BufferData(OpenGL.ArrayBuffer, texcoords.Length * sizeof(float), texcoords, OpenGL.StaticDraw);

                    material.Propeterys.Add("tris", verticies.Length / 3);
                }
            }
            
            //Now we create the projection and the view matrix
            mat4 p_mat = mat4.Perspective(Matrix4x4.DegreesToRadians(45.0f), (float)this.ClientSize.Width / (float)this.ClientSize.Height, 0.1f, 100f);
            mat4 v_mat = mat4.LookAt(new vec3(0f, 0f, 1f), new vec3(0f, 0f, 0f), new vec3(0f, 1f, 0f));

            while (true) {
                Thread.Sleep(10);

                //Clear the render context
                gl.Clear(NetGL.OpenGL.ColorBufferBit | NetGL.OpenGL.DepthBufferBit);

                //First we create the mvp for the model
                mat4 mt_mat = mat4.Translate(new vec3(0f, -1f, -5.0f));
                mat4 mr_mat = mat4.RotateX(0f) * mat4.RotateY(rotate) * mat4.RotateZ(0f);
                mat4 ms_mat = mat4.Scale(new vec3(1f, 1f, 1f));
                mat4 m_mat = mt_mat * mr_mat * ms_mat;
                mat4 mvp = p_mat * v_mat * m_mat;

                //Now we draw the materials
                foreach (var shape in model.Shapes)
                {
                    foreach (var material in shape.Materials)
                    {
                        //We need to load the shader for every material because the textures
                        gl.UseProgram(program);
                        gl.UniformMatrix4fv(gl.GetUniformLocation(program, "mvp"), 1, false, mvp.ToArray());

                        gl.BindTexture(OpenGL.Texture2D, (int)material.Propeterys["tex_id"]);
                        gl.Uniform1I(gl.GetUniformLocation(program, "textureSampler"), 0);

                        //Send the vertex buffer to the shader
                        gl.EnableVertexAttribArray(0);
                        gl.BindBuffer(OpenGL.ArrayBuffer, (int)material.Propeterys["vbo"]);
                        gl.VertexAttribPointer(0, 3, OpenGL.Float, false, 0, 0);

                        //Send the tex coord buffer to the shader
                        gl.EnableVertexAttribArray(2);
                        gl.BindBuffer(OpenGL.ArrayBuffer, (int)material.Propeterys["tbo"]);
                        gl.VertexAttribPointer(2, 2, OpenGL.Float, false, 0, 0);

                        //Render the model as Triangles v.Length / 3 is the amount of triangles you need to render
                        gl.DrawArrays(OpenGL.Triangles, 0, (int)material.Propeterys["tris"]);
                    }
                }

                gl.Flush();
                gl.SwapLayerBuffers(NetGL.OpenGL.SwapMainPlane);
                Console.WriteLine(gl.GetError());
                rotate += 0.05f;
            }
        }

        /// <summary>
        /// With this function we load the material texture
        /// </summary>
        /// <param name="material"></param>
        public void LoadTexture(Material material)
        {
            //We create a texture id for the texture and set them as propetery
            int texid = gl.GenTextures(1);
            material.Propeterys.Add("tex_id", texid);

            //Now we load the materials texture. If there is no texture we load a default texture.
            Bitmap bitmap = Properties.Resources.Smiley;
            if (!String.IsNullOrEmpty(material.TexturePath))
            {
                bitmap = (Bitmap)Bitmap.FromFile(material.TexturePath);
            }

            //We bind the texture and load it into the vram
            gl.BindTexture(NetGL.OpenGL.Texture2D, texid);
            gl.TexParameteri(NetGL.OpenGL.Texture2D, NetGL.OpenGL.TextureMinFilter, NetGL.OpenGL.Nearest);
            gl.TexParameteri(NetGL.OpenGL.Texture2D, NetGL.OpenGL.TextureMagFilter, NetGL.OpenGL.Linear);
            gl.TexParameteri(NetGL.OpenGL.Texture2D, NetGL.OpenGL.TextureWrapS, NetGL.OpenGL.Repeate);
            gl.TexParameteri(NetGL.OpenGL.Texture2D, NetGL.OpenGL.TextureWrapT, NetGL.OpenGL.Repeate);
            gl.TexImage2D(NetGL.OpenGL.Texture2D, 0, NetGL.OpenGL.RGBA, bitmap.Width, bitmap.Height, 0, NetGL.OpenGL.BGRAExt, NetGL.OpenGL.UnsignedByte, bitmap);
            //gl.Uniform1I(gl.GetUniformLocation(program, "textureSampler"), 0);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            Thread renderThread = new Thread(new ThreadStart(loop));
            renderThread.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}

```
