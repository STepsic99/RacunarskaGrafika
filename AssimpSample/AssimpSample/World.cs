// -----------------------------------------------------------------------
// <file>World.cs</file>
// <copyright>Grupa za Grafiku, Interakciju i Multimediju 2013.</copyright>
// <author>Srđan Mihić</author>
// <author>Aleksandar Josić</author>
// <summary>Klasa koja enkapsulira OpenGL programski kod.</summary>
// -----------------------------------------------------------------------
using System;
using Assimp;
using System.IO;
using System.Reflection;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph.Quadrics;
using SharpGL.SceneGraph.Core;
using SharpGL;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Threading;

namespace AssimpSample
{


    /// <summary>
    ///  Klasa enkapsulira OpenGL kod i omogucava njegovo iscrtavanje i azuriranje.
    /// </summary>
    public class World : IDisposable
    {
        #region Atributi

        private enum TextureObjects { PLOCICE = 0, METAL };
        private readonly int m_textureCount = Enum.GetNames(typeof(TextureObjects)).Length;
        private uint[] m_textures = null;
        private string[] m_textureFiles = { "..//..//Images//plocice.jpg", "..//..//Images//metal.jpg"};

        public static float redAmb = 1.0f;
        public static float greenAmb = 1.0f;
        public static float blueAmb = 0.8f;
        public static bool animationStarted = false;
        public static float modelTranslateX = 0.0f;
        public static float modelTranslateY = -2.0f;
        public static float modelTranslateZ = 4.0f;
        public static DispatcherTimer timer;

        public static int heightFactor = 1;
        /// <summary>
        ///	 Ugao rotacije Meseca
        /// </summary>
        private float m_moonRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije Zemlje
        /// </summary>
        private float m_earthRotation = 0.0f;

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        private AssimpScene m_scene;

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        private float m_xRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        private float m_yRotation = 0.0f;

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        private float m_sceneDistance = 20.0f;

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_width;

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_height;

        #endregion Atributi

        #region Properties

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        public AssimpScene Scene
        {
            get { return m_scene; }
            set { m_scene = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        public float RotationX
        {
            get { return m_xRotation; }
            set { m_xRotation = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        public float RotationY
        {
            get { return m_yRotation; }
            set { m_yRotation = value; }
        }

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        public float SceneDistance
        {
            get { return m_sceneDistance; }
            set { m_sceneDistance = value; }
        }

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        public int Width
        {
            get { return m_width; }
            set { m_width = value; }
        }

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        public int Height
        {
            get { return m_height; }
            set { m_height = value; }
        }

        #endregion Properties

        #region Konstruktori

        /// <summary>
        ///  Konstruktor klase World.
        /// </summary>
        public World(String scenePath, String sceneFileName, int width, int height, OpenGL gl)
        {
            this.m_scene = new AssimpScene(scenePath, sceneFileName, gl);
            this.m_width = width;
            this.m_height = height;
            m_textures = new uint[m_textureCount];
        }

        /// <summary>
        ///  Destruktor klase World.
        /// </summary>
        ~World()
        {
            this.Dispose(false);
        }

        #endregion Konstruktori

        #region Metode

        /// <summary>
        ///  Korisnicka inicijalizacija i podesavanje OpenGL parametara.
        /// </summary>
        public void Initialize(OpenGL gl)
        {
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            gl.Color(1f, 0f, 0f);
            // Model sencenja na flat (konstantno)
            gl.ShadeModel(OpenGL.GL_FLAT);
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.Enable(OpenGL.GL_CULL_FACE);
            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT_AND_DIFFUSE);
            gl.Enable(OpenGL.GL_NORMALIZE);
            gl.Enable(OpenGL.GL_AUTO_NORMAL);
            // gl.Enable(OpenGL.GL_CCW);
            m_scene.LoadScene();
            m_scene.Initialize();
            SetupLighting(gl);
            SetupTextures(gl);
        }


        public void SetupTextures(OpenGL gl)
        {
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
           
           

            gl.GenTextures(m_textureCount, m_textures);
            for (int i = 0; i < m_textureCount; ++i)
            {
                // Pridruzi teksturu odgovarajucem identifikatoru
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[i]);
                // Ucitaj sliku i podesi parametre teksture
                Bitmap image = new Bitmap(m_textureFiles[i]);
                // rotiramo sliku zbog koordinantog sistema opengl-a
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                // RGBA format (dozvoljena providnost slike tj. alfa kanal)
                BitmapData imageData = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                                                      System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                gl.Build2DMipmaps(OpenGL.GL_TEXTURE_2D, (int)OpenGL.GL_RGBA8, image.Width, image.Height, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);


                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);

                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_NEAREST);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_NEAREST);

                image.UnlockBits(imageData);
                image.Dispose();
            }

        }

        private void SetupLighting(OpenGL gl)
        {

            gl.Enable(OpenGL.GL_LIGHTING);
             
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_CUTOFF, 180.0f); 
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, new float[] { redAmb, greenAmb, blueAmb, 1.0f });
            gl.Enable(OpenGL.GL_LIGHT0);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, new float[] { 30.0f, 10.0f, 0f, 1.0f });
            


         

            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_DIFFUSE, new float[] { 1.0f,0.0f,0.0f,1.0f });
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_DIRECTION, new float[] { 0.0f, -1.0f, 0.0f });
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_CUTOFF, 40.0f);
            gl.Enable(OpenGL.GL_LIGHT1);


        }

        /// <summary>
        ///  Iscrtavanje OpenGL kontrole.
        /// </summary>
        public void Draw(OpenGL gl)
        {
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.LoadIdentity();

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, new float[] { redAmb, greenAmb, blueAmb, 1.0f });

            gl.PushMatrix();

            

            gl.Translate(0.0f, 0.0f, -m_sceneDistance);
            gl.LookAt(0.0f, 5.0f, -8.0f, 0.0f, -10.0f, 0.0f, 0.0f, 1.0f, 0.0f);
            gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);

            



            DrawPodloga(gl);

           
            gl.Color(0.36f, 0.36f, 0.36f);
            gl.Translate(0f, 0f, 2f);

            

            DrawStepenice(gl);

            gl.PushMatrix();
            gl.Color(1.0f, 1.0f, 1.0f);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            //gl.Translate(0f, -1.9f, 4f);
            gl.Translate(modelTranslateX, modelTranslateY, modelTranslateZ);
            gl.Scale(0.03f, 0.03f, 0.03f);

            if(heightFactor == 2)
            {
                gl.Scale(1.0f, 2.0f, 1.0f);
                gl.Translate(0f, -5f, 0f);
            }
            if (heightFactor == 3)
            {
                gl.Scale(1.0f, 3.0f, 1.0f);
                gl.Translate(0f, -6f, 0f);
            }
            m_scene.Draw();
            gl.PopMatrix();
            
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
            gl.PopMatrix();


            





            // Oznaci kraj iscrtavanja

            gl.PushMatrix();
            gl.Viewport(m_width - 180, 0, m_width / 2, m_height);
            gl.DrawText3D("Verdana Italic",10,10,10,"");
            gl.DrawText(10, 50, 1.0f, 0.0f, 0.0f, "Verdana Italic", 10, "Predmet: Racunarska grafika");
            gl.DrawText(10, 40, 1.0f, 0.0f, 0.0f, "Verdana Italic", 10, "Sk.god: 2021/22.");
            gl.DrawText(10, 30, 1.0f, 0.0f, 0.0f, "Verdana Italic", 10, "Ime: Silvija");
            gl.DrawText(10, 20, 1.0f, 0.0f, 0.0f, "Verdana Italic", 10, "Prezime: Tepsic");
            gl.DrawText(10, 10, 1.0f, 0.0f, 0.0f, "Verdana Italic", 10, "Sifra zad: 16.1");
            gl.Viewport(0, 0, m_width, m_height);
            gl.PopMatrix();

            gl.Flush();
        }

        public void DrawPodloga(OpenGL gl)
        {
           
            gl.PushMatrix();
            
            gl.Translate(-0.2f, -6f, 0f);
          
            gl.Rotate(-90f, 1.0f, 0.0f, 0.0f);
            
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.PLOCICE]);
            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.LoadIdentity();
            gl.Scale(1f, 1f, 1f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.Begin(OpenGL.GL_QUADS);
            gl.Normal(0, 1, 0);

            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(-12f, -12f);
            gl.TexCoord(0.0f, 1.0f);
            gl.Vertex(12f, -12f);
           gl.TexCoord(1.0f, 1.0f);
            gl.Vertex(12f, 12f);
           gl.TexCoord(1.0f, 0.0f);
            gl.Vertex(-12f, 12f);








            gl.End();
           
            gl.Flush();
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.PopMatrix();
            
        }

        public void DrawStepenice(OpenGL gl)
        {
            gl.PushMatrix();
            gl.Rotate(0f, 90f, 0f);
            gl.Translate(0f, -3.2f, 0f);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_DIRECTION, new float[] { 0.0f, -1.0f, 0.0f });
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, new float[] { 0.0f, 100.0f, 0.0f, 1.0f });
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.METAL]);

           /* gl.PushMatrix();
            gl.Scale(1f, 1.6f, 1f);
            Cube cube = new Cube();
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();*/

            gl.PushMatrix();
            gl.Translate(-1.5f, -2f, 0f);
            gl.Scale(2.5f, 0.5f, 1f);
            Cube cube2 = new Cube();
            cube2.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(-1f, -1f, 0f);
            gl.Scale(2f, 0.5f, 1f);
            Cube cube3 = new Cube();
            cube3.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(-0.5f, 0f, 0f);
            gl.Scale(1.5f, 0.5f, 1f);
            Cube cube4 = new Cube();
            cube4.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(-0.0f, 1f, 0f);
            gl.Scale(1.0f, 0.5f, 1f);
            Cube cube5 = new Cube();
            cube4.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Color(0.0f, 0.0f, 1.0f);
            gl.Translate(0.0f, 0.0f, 1f);
            Cylinder cil = new Cylinder();
            cil.BaseRadius = 2f;
            cil.TopRadius = 2f;
            cil.Height = 0.2f;
            cil.CreateInContext(gl);
            cil.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Color(0.0f, 0.0f, 1.0f);
            gl.Translate(0.0f, 0.0f, -1.2f);
            gl.Rotate(0f, 0f, 0f);
            Cylinder cil2 = new Cylinder();
            cil2.BaseRadius = 2f;
            cil2.TopRadius = 2f;
            cil2.Height = 0.2f;
            cil2.CreateInContext(gl);
            cil2.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();

            gl.PopMatrix();
        }

        public static void Animation(object sender, EventArgs e)
        {
            if (modelTranslateY == -2.0f)
            {
                
                modelTranslateY -= 1f;
                modelTranslateZ += 1.7f;
            }
            else if (modelTranslateY == -3.0f)
            {
                modelTranslateY -= 1.0f;
                modelTranslateZ += 1.0f;
            }
            else if (modelTranslateY == -4.0f)
            {
                modelTranslateY -= 1.0f;
                modelTranslateZ += 0.9f;
            }
            else if (modelTranslateY == -5.0f)
            {
                modelTranslateY -= 0.9f;
                modelTranslateZ += 1.2f;
            }
            else
            {
                animationStarted = false;
                timer.Stop();
            }
        }


        /// <summary>
        /// Podesava viewport i projekciju za OpenGL kontrolu.
        /// </summary>
        public void Resize(OpenGL gl, int width, int height)
        {
            m_width = width;
            m_height = height;
            //m_width = 0;
            //m_height = 0;
            gl.Viewport(0, 0, m_width, m_height);
            gl.MatrixMode(OpenGL.GL_PROJECTION);      // selektuj Projection Matrix
            gl.LoadIdentity();
            gl.Perspective(50f, (double)width / height, 0.5f, 0f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();                // resetuj ModelView Matrix
        }

        /// <summary>
        ///  Implementacija IDisposable interfejsa.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_scene.Dispose();
            }
        }

        #endregion Metode

        #region IDisposable metode

        /// <summary>
        ///  Dispose metoda.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable metode
    }
}
