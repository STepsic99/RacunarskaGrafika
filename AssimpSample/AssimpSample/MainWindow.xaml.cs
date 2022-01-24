using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using SharpGL.SceneGraph;
using SharpGL;
using Microsoft.Win32;


namespace AssimpSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Atributi

        /// <summary>
        ///	 Instanca OpenGL "sveta" - klase koja je zaduzena za iscrtavanje koriscenjem OpenGL-a.
        /// </summary>
        World m_world = null;

        int intervalAnimacije = 1000;

        #endregion Atributi

        #region Konstruktori

        public MainWindow()
        {
            // Inicijalizacija komponenti
            InitializeComponent();

            // Kreiranje OpenGL sveta
            try
            {
                m_world = new World(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "3D Models\\Human"), "baby.3ds", (int)openGLControl.ActualWidth, (int)openGLControl.ActualHeight, openGLControl.OpenGL);
            }
            catch (Exception e)
            {
                MessageBox.Show("Neuspesno kreirana instanca OpenGL sveta. Poruka greške: " + e.Message, "Poruka", MessageBoxButton.OK);
                this.Close();
            }
        }

        #endregion Konstruktori

        /// <summary>
        /// Handles the OpenGLDraw event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            m_world.Draw(args.OpenGL);
        }

        /// <summary>
        /// Handles the OpenGLInitialized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            m_world.Initialize(args.OpenGL);
        }

        /// <summary>
        /// Handles the Resized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_Resized(object sender, OpenGLEventArgs args)
        {
            m_world.Resize(args.OpenGL, (int)openGLControl.ActualWidth, (int)openGLControl.ActualHeight);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!World.animationStarted)
            {
                switch (e.Key)
                {
                    case Key.F4: this.Close(); break;
                    case Key.E:
                        if (m_world.RotationX - 5f < -20f)
                        {
                            m_world.RotationX = -20f;
                        }
                        else
                        {
                            m_world.RotationX -= 5.0f;
                        }
                        break;
                    case Key.D:
                        if (m_world.RotationX + 5f > 60f)
                        {
                            m_world.RotationX = 60f;
                        }
                        else
                        {
                            m_world.RotationX += 5.0f;
                        }
                        break;
                    case Key.S: m_world.RotationY -= 5.0f; break;
                    case Key.F: m_world.RotationY += 5.0f; break;
                    case Key.Add: m_world.SceneDistance -= 50.0f; break;
                    case Key.Subtract: m_world.SceneDistance += 50.0f; break;
                    case Key.F2:
                        OpenFileDialog opfModel = new OpenFileDialog();
                        bool result = (bool)opfModel.ShowDialog();
                        if (result)
                        {

                            try
                            {
                                World newWorld = new World(Directory.GetParent(opfModel.FileName).ToString(), Path.GetFileName(opfModel.FileName), (int)openGLControl.Width, (int)openGLControl.Height, openGLControl.OpenGL);
                                m_world.Dispose();
                                m_world = newWorld;
                                m_world.Initialize(openGLControl.OpenGL);
                            }
                            catch (Exception exp)
                            {
                                MessageBox.Show("Neuspesno kreirana instanca OpenGL sveta:\n" + exp.Message, "GRESKA", MessageBoxButton.OK);
                            }
                        }
                        break;
                    case Key.V:
                        World.animationStarted = true;
                        World.timer = new System.Windows.Threading.DispatcherTimer();
                        World.timer.Tick += new EventHandler(World.Animation);
                        World.timer.Interval = TimeSpan.FromMilliseconds(intervalAnimacije);
                        World.modelTranslateX = 0.0f;
                        World.modelTranslateY = -2.0f;
                        World.modelTranslateZ = 4.0f;
                        World.timer.Start();
                        break;
                }
            }
        }

        private void sliderVisina_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (World.animationStarted == false)
                World.heightFactor = (int)e.NewValue;
            else sliderVisina.Value = World.heightFactor;
        }

        private void sliderBrzina_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (World.animationStarted == false)
                intervalAnimacije = -(int)e.NewValue;
            else sliderBrzina.Value = -intervalAnimacije;
        }

        private void sliderR_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //World.redAmb = (float)e.NewValue;
            if (m_world != null && World.animationStarted == false)
                //m_world.redAmb = (float)e.NewValue;
                World.redAmb = (float)e.NewValue / 255;
            else if (m_world != null) sliderR.Value = World.redAmb*255;
        }

        private void sliderG_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // World.greenAmb = (float)e.NewValue;
            if (m_world != null && World.animationStarted == false)
                //m_world.greenAmb = (float)e.NewValue;
                World.greenAmb = (float)e.NewValue/255;
            else if (m_world != null) sliderG.Value = World.greenAmb * 255;
        }

        private void sliderB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //World.blueAmb = (float)e.NewValue;
            if (m_world != null && World.animationStarted==false)
                // m_world.blueAmb = (float)e.NewValue;
                World.blueAmb = (float)e.NewValue/255;
            else if (m_world != null) sliderB.Value = World.blueAmb * 255;
        }
    }
}
