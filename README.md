OpenObjectLoader
================

After years i rewritten the whole libary. Its now possible to load an model more easy.Its also possible
to load multiple shapes (objects) within one model. This shapes can have multiple materials with multiple textures. At the
moment every shape has own materials. So there are no shared materials yet.

Example
==

Within this example the famous "sponza" scene from https://github.com/jimmiebergmann/Sponza gets rendered. For the matrices
the GlmSharp libary is used https://github.com/Philip-Trettner/GlmSharp. And for the rendering with OpenGL the NetGL wrapper
is used https://github.com/Andy16823/NetGL-2023/tree/main

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
using static System.Net.Mime.MediaTypeNames;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private NetGL.OpenGL gl;
        private String modelspath;
        private float rotate;
        private List<Model> _models;
        private float _camRotX = 0f;
        private float _camRotY = 0f;
        private float _camX = 0f;
        private float _camY = 0f;
        private float _camZ = 0f;

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
            modelspath = new System.IO.FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).Directory + "\\Models";

            //Then we create a new instance from the wavefront loader and read the .obj file
            WavefrontLoader loader = new WavefrontLoader();

            //After this we load the model
            Model model = loader.LoadModel(modelspath + "\\sponza.obj");

            //Now we create our shader
            string vertexShaderCode = @"
                #version 330 core

                layout(location = 0) in vec3 inPosition;
                layout(location = 1) in vec3 inColor;
                layout(location = 2) in vec2 inTexCoord;
                layout(location = 3) in vec3 inNormal;

                out vec3 fragPos;
                out vec3 fragNormal;
                out vec3 color;
                out vec2 texCoord;

                uniform mat4 mvp;

                void main()
                {
                    gl_Position = mvp * vec4(inPosition, 1.0);
                    fragPos = inPosition;
                    fragNormal = inNormal;
                    color = inColor;
                    texCoord = inTexCoord;
                }
            ";

            string fragmentShaderCode = @"
                #version 330 core

                in vec3 fragPos;
                in vec3 fragNormal;
                in vec3 color;
                in vec2 texCoord;

                out vec4 fragColor;
                uniform sampler2D textureSampler;
                uniform sampler2D normalMap;

                void main()
                {
                    vec2 flippedTexCoord = vec2(texCoord.x, 1.0 - texCoord.y);
                    vec3 norm = texture(normalMap, flippedTexCoord).xyz * 2.0 - 1.0;
                    norm = normalize(norm);

                    vec3 lightDir = normalize(vec3(0.0, 1.0, 1.0));
                    float diff = max(dot(norm, lightDir), 0.0);

                    vec4 texColor = texture(textureSampler, flippedTexCoord);
                    float alpha;
                    if (texColor == vec4(0.0, 0.0, 0.0, 1.0)) {
                        alpha = 0.0;
                    } else {
                        alpha = 1.0;
                    }

                    vec3 diffuse = diff * texture(textureSampler, flippedTexCoord).rgb * vec3(1.0, 1.0, 1.0);
                    fragColor = vec4(diffuse, alpha);
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

            //Enable Alpha blending
            gl.Enable(OpenGL.Blend);
            gl.BlendFunc(OpenGL.SrcAlpha, OpenGL.OneMinusSrcAlpha);
            
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
            foreach (var material in model.Materials)
            {
                //First we load the texture and the normal map
                this.LoadTexture(material);
                this.LoadNormal(material);

                //Then we load the vertex buffer
                float[] verticies = material.IndexVerticies();
                int vertexBuffer = gl.GenBuffer(1);
                material.Propeterys.Add("vbo", vertexBuffer);
                gl.BindBuffer(OpenGL.ArrayBuffer, vertexBuffer);
                gl.BufferData(OpenGL.ArrayBuffer, verticies.Length * sizeof(float), verticies, OpenGL.StaticDraw);

                //Then we load the texture coords
                float[] texcoords = material.IndexTexCoords();
                int tbo = gl.GenBuffer(1);
                material.Propeterys.Add("tbo", tbo);
                gl.BindBuffer(OpenGL.ArrayBuffer, tbo);
                gl.BufferData(OpenGL.ArrayBuffer, texcoords.Length * sizeof(float), texcoords, OpenGL.StaticDraw);

                //And last we load the normal coords
                float[] normals = material.IndexNormals();
                int nbo = gl.GenBuffer(1);
                material.Propeterys.Add("nbo", nbo);
                gl.BindBuffer(OpenGL.ArrayBuffer, nbo);
                gl.BufferData(OpenGL.ArrayBuffer, normals.Length * sizeof(float), normals, OpenGL.StaticDraw);
                
                //Then we calculate how many triangles we have
                material.Propeterys.Add("tris", verticies.Length / 3);
            }

            //Now we create the projection and the view matrix
            mat4 p_mat = mat4.Perspective(Matrix4x4.DegreesToRadians(45.0f), (float)this.ClientSize.Width / (float)this.ClientSize.Height, 0.1f, 100f);
            mat4 v_mat = mat4.LookAt(new vec3(0f, 0f, 1f), new vec3(0f, 0f, 0f), new vec3(0f, 1f, 0f));
            
            while (true) {
                Thread.Sleep(10);

                //Clear the render context
                gl.Clear(NetGL.OpenGL.ColorBufferBit | NetGL.OpenGL.DepthBufferBit);

                //Recreate the view matrix for the camera. 
                p_mat = mat4.Perspective(Matrix4x4.DegreesToRadians(45.0f), (float)this.ClientSize.Width / (float)this.ClientSize.Height, 0.1f, 100f);
                mat4 vr_mat = mat4.RotateX(_camRotX) * mat4.RotateY(_camRotY); // Camera Rotation
                v_mat = mat4.Translate(_camX, _camY, _camZ) * vr_mat * mat4.LookAt(new vec3(0f, 0f, 1f), new vec3(0f, 0f, 0f), new vec3(0f, 1f, 0f));


                //Then we create the modelview matrix
                mat4 mt_mat = mat4.Translate(new vec3(0f, 0f, -1.0f));
                mat4 mr_mat = mat4.RotateX(0f) * mat4.RotateY(0f) * mat4.RotateZ(0f);
                mat4 ms_mat = mat4.Scale(new vec3(0.01f, 0.01f, 0.01f));
                mat4 m_mat = mt_mat * mr_mat * ms_mat;

                //Now we have all matrices we need for the mvp matrix
                mat4 mvp = p_mat * v_mat * m_mat;

                //Rendering the materials
                foreach (var material in model.Materials)
                {
                    //Load the shader programm for every material
                    gl.UseProgram(program);
                    gl.UniformMatrix4fv(gl.GetUniformLocation(program, "mvp"), 1, false, mvp.ToArray());

                    //Activate the first texture slot (its actualy slot 0) and send the texture to the shader
                    gl.ActiveTexture(OpenGL.Texture0);
                    gl.BindTexture(OpenGL.Texture2D, (int)material.Propeterys["tex_id"]);
                    gl.Uniform1I(gl.GetUniformLocation(program, "textureSampler"), 0);

                    //Activate the second texture slot (its the slot 1) and send it also to the shader
                    gl.ActiveTexture(OpenGL.Texture1);
                    gl.BindTexture(OpenGL.Texture2D, (int)material.Propeterys["normal_id"]);
                    gl.Uniform1I(gl.GetUniformLocation(program, "normalMap"), 1);

                    //Send the vertex buffer to the shader
                    gl.EnableVertexAttribArray(0);
                    gl.BindBuffer(OpenGL.ArrayBuffer, (int)material.Propeterys["vbo"]);
                    gl.VertexAttribPointer(0, 3, OpenGL.Float, false, 0, 0);

                    //Send the tex coord buffer to the shader
                    gl.EnableVertexAttribArray(2);
                    gl.BindBuffer(OpenGL.ArrayBuffer, (int)material.Propeterys["tbo"]);
                    gl.VertexAttribPointer(2, 2, OpenGL.Float, false, 0, 0);

                    //Send the normals to the shader
                    gl.EnableVertexAttribArray(3);
                    gl.BindBuffer(OpenGL.ArrayBuffer, (int)material.Propeterys["nbo"]);
                    gl.VertexAttribPointer(3, 3, OpenGL.Float, false, 0, 0);

                    //Render the model as Triangles v.Length / 3 is the amount of triangles you need to render
                    gl.DrawArrays(OpenGL.Triangles, 0, (int)material.Propeterys["tris"]);
                }

                gl.Flush();
                gl.SwapLayerBuffers(NetGL.OpenGL.SwapMainPlane);
                Console.WriteLine(gl.GetError());
                //rotate += 0.05f;
            }
        }

        /// <summary>
        /// Load the textures
        /// </summary>
        /// <param name="material"></param>
        public void LoadTexture(Material material)
        {
            int texid = gl.GenTextures(1);
            material.Propeterys.Add("tex_id", texid);

            Bitmap bitmap = Properties.Resources.Smiley;
            if (!String.IsNullOrEmpty(material.TexturePath))
            {
                bitmap = (Bitmap)Bitmap.FromFile(modelspath + "\\" + material.TexturePath);
            }

            gl.BindTexture(NetGL.OpenGL.Texture2D, texid);
            gl.TexParameteri(NetGL.OpenGL.Texture2D, NetGL.OpenGL.TextureMinFilter, NetGL.OpenGL.Nearest);
            gl.TexParameteri(NetGL.OpenGL.Texture2D, NetGL.OpenGL.TextureMagFilter, NetGL.OpenGL.Linear);
            gl.TexParameteri(NetGL.OpenGL.Texture2D, NetGL.OpenGL.TextureWrapS, NetGL.OpenGL.Repeate);
            gl.TexParameteri(NetGL.OpenGL.Texture2D, NetGL.OpenGL.TextureWrapT, NetGL.OpenGL.Repeate);
            gl.TexImage2D(NetGL.OpenGL.Texture2D, 0, NetGL.OpenGL.RGBA, bitmap.Width, bitmap.Height, 0, NetGL.OpenGL.BGRAExt, NetGL.OpenGL.UnsignedByte, bitmap);
        }

        /// <summary>
        /// Load the normal maps
        /// </summary>
        /// <param name="material"></param>
        public void LoadNormal(Material material)
        {
            int texID = gl.GenTextures(1);
            material.Propeterys.Add("normal_id", texID);

            Bitmap bitmap = Properties.Resources.Smiley;
            if (!String.IsNullOrEmpty(material.NormalPath))
            {
                bitmap = (Bitmap)Bitmap.FromFile(modelspath + "\\" + material.NormalPath);
            }

            gl.BindTexture(NetGL.OpenGL.Texture2D, texID);
            gl.TexParameteri(NetGL.OpenGL.Texture2D, NetGL.OpenGL.TextureMinFilter, NetGL.OpenGL.Nearest);
            gl.TexParameteri(NetGL.OpenGL.Texture2D, NetGL.OpenGL.TextureMagFilter, NetGL.OpenGL.Linear);
            gl.TexParameteri(NetGL.OpenGL.Texture2D, NetGL.OpenGL.TextureWrapS, NetGL.OpenGL.Repeate);
            gl.TexParameteri(NetGL.OpenGL.Texture2D, NetGL.OpenGL.TextureWrapT, NetGL.OpenGL.Repeate);
            gl.TexImage2D(NetGL.OpenGL.Texture2D, 0, NetGL.OpenGL.RGBA, bitmap.Width, bitmap.Height, 0, NetGL.OpenGL.BGRAExt, NetGL.OpenGL.UnsignedByte, bitmap);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            Thread renderThread = new Thread(new ThreadStart(loop));
            renderThread.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Up)
            {
                this._camRotX += 0.1f;
            }
            else if(e.KeyCode == Keys.Down)
            {
                this._camRotX -= 0.1f;
            }
            else if (e.KeyCode == Keys.W)
            {
                this._camZ += 0.1f;
            }
            else if (e.KeyCode == Keys.S)
            {
                this._camZ -= 0.1f;
            }
            else if (e.KeyCode == Keys.A)
            {
                this._camX -= 0.1f;
            }
            else if (e.KeyCode == Keys.D)
            {
                this._camX += 0.1f;
            }
            else if (e.KeyCode == Keys.PageUp)
            {
                this._camY -= 0.1f;
            }
            else if (e.KeyCode == Keys.PageDown)
            {
                this._camY += 0.1f;
            }
            else if(e.KeyCode == Keys.Q)
            {
                this._camRotY += 0.1f;
            }
            else if (e.KeyCode == Keys.E)
            {
                this._camRotY -= 0.1f;
            }
        }
    }
}

```
