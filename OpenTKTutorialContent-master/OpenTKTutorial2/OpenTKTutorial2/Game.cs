﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.IO;

namespace OpenTKTutorial2
{
    class Game : GameWindow
    {
        /// <summary>
        /// ID of our program on the graphics card
        /// </summary>
        int pgmID;

        /// <summary>
        /// Address of the vertex shader
        /// </summary>
        int vsID;

        /// <summary>
        /// Address of the fragment shader
        /// </summary>
        int fsID;

        /// <summary>
        /// Address of the color parameter
        /// </summary>
        int attribute_vcol;

        /// <summary>
        /// Address of the position parameter
        /// </summary>
        int attribute_vpos;

        /// <summary>
        /// Address of the modelview matrix uniform
        /// </summary>
        int uniform_mview;

        /// <summary>
        /// Address of the Vertex Buffer Object for our position parameter
        /// </summary>
        int vbo_position;

        /// <summary>
        /// Address of the Vertex Buffer Object for our color parameter
        /// </summary>
        int vbo_color;

        /// <summary>
        /// Address of the Vertex Buffer Object for our modelview matrix
        /// </summary>
        int vbo_mview;

        /// <summary>
        /// Array of our vertex positions
        /// </summary>
        Vector3[] vertdata;

        /// <summary>
        /// Array of our vertex colors
        /// </summary>
        Vector3[] coldata;

        /// <summary>
        /// Array of our modelview matrices
        /// </summary>
        Matrix4[] mviewdata;

        void initProgram()
        {
            /** In this function, we'll start with a call to the GL.CreateProgram() function,
             * which returns the ID for a new program object, which we'll store in pgmID. */
            pgmID = GL.CreateProgram();

            loadShader("vs.glsl", ShaderType.VertexShader, pgmID, out vsID);
            loadShader("fs.glsl", ShaderType.FragmentShader, pgmID, out fsID);

            /** Now that the shaders are added, the program needs to be linked.
             * Like C code, the code is first compiled, then linked, so that it goes
             * from human-readable code to the machine language needed. */
            GL.LinkProgram(pgmID);
            Console.WriteLine(GL.GetProgramInfoLog(pgmID));

            /** We have multiple inputs on our vertex shader, so we need to get
             * their addresses to give the shader position and color information for our vertices.
             * 
             * To get the addresses for each variable, we use the 
             * GL.GetAttribLocation and GL.GetUniformLocation functions.
             * Each takes the program's ID and the name of the variable in the shader. */
            attribute_vpos = GL.GetAttribLocation(pgmID, "vPosition");
            attribute_vcol = GL.GetAttribLocation(pgmID, "vColor");
            uniform_mview = GL.GetUniformLocation(pgmID, "modelview");

            /** Now our shaders and program are set up, but we need to give them something to draw.
             * To do this, we'll be using a Vertex Buffer Object (VBO).
             * When you use a VBO, first you need to have the graphics card create
             * one, then bind to it and send your information. 
             * Then, when the DrawArrays function is called, the information in
             * the buffers will be sent to the shaders and drawn to the screen. */
            GL.GenBuffers(1, out vbo_position);
            GL.GenBuffers(1, out vbo_color);
            GL.GenBuffers(1, out vbo_mview);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            initProgram();

            vertdata = new Vector3[] { new Vector3(-0.8f, -0.8f, 0f),
                new Vector3( 0.8f, -0.8f, 0f), 
                new Vector3( 0f,  0.8f, 0f)};


            coldata = new Vector3[] { new Vector3(1f, 0f, 0f),
                new Vector3( 0f, 0f, 1f), 
                new Vector3( 0f,  1f, 0f)};

            mviewdata = new Matrix4[]{
                Matrix4.Identity
            };

            Title = "Hello OpenTK!";
            GL.ClearColor(Color.CornflowerBlue);
            GL.PointSize(5f);
        }

        /// <summary>
        /// This creates a new shader (using a value from the ShaderType enum), loads code for it, compiles it, and adds it to our program.
        /// It also prints any errors it found to the console, which is really nice for when you make a mistake in a shader (it will also yell at you if you use deprecated code).
        /// </summary>
        /// <param name="filename">File to load the shader from</param>
        /// <param name="type">Type of shader to load</param>
        /// <param name="program">ID of the program to use the shader with</param>
        /// <param name="address">Address of the compiled shader</param>
        void loadShader(String filename, ShaderType type, int program, out int address)
        {
            address = GL.CreateShader(type);
            using (StreamReader sr = new StreamReader(filename))
            {
                GL.ShaderSource(address, sr.ReadToEnd());
            }
            GL.CompileShader(address);
            GL.AttachShader(program, address);
            Console.WriteLine(GL.GetShaderInfoLog(address));
        }



        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);

            GL.EnableVertexAttribArray(attribute_vpos);
            GL.EnableVertexAttribArray(attribute_vcol);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            GL.DisableVertexAttribArray(attribute_vpos);
            GL.DisableVertexAttribArray(attribute_vcol);

            GL.Flush();
            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_position);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(vertdata.Length * Vector3.SizeInBytes), vertdata, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(attribute_vpos, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_color);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(coldata.Length * Vector3.SizeInBytes), coldata, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(attribute_vcol, 3, VertexAttribPointerType.Float, true, 0, 0);

            GL.UniformMatrix4(uniform_mview, false, ref mviewdata[0]);

            GL.UseProgram(pgmID);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
    }
}
