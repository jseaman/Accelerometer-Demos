using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using IphoneComm;

namespace IphoneDemo
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Model myModel;
        float aspectRatio;

        Vector3 modelPosition = Vector3.Zero;
        Vector3 cameraPosition = new Vector3(0.0f, 0.0f, 1000.0f);

        CoordListener Listener = null;
        float FirstAngle = 0;

        Matrix IphoneMatrix = Matrix.Identity;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
            Listener = new CoordListener(666);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            myModel = Content.Load<Model>("Models\\iphone2");
            aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            Listener.Stop();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            IphoneMatrix = GetIphoneMatrix();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Copy any parent transforms.
            Matrix[] transforms = new Matrix[myModel.Bones.Count];
            myModel.CopyAbsoluteBoneTransformsTo(transforms);

            // Draw the model. A model can have multiple meshes, so loop.
            foreach (ModelMesh mesh in myModel.Meshes)
            {
                // This is where the mesh orientation is set, as well as our camera and projection.
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = transforms[mesh.ParentBone.Index] * IphoneMatrix
                        * Matrix.CreateTranslation(modelPosition);
                    effect.View = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);
                    effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),
                        aspectRatio, 1.0f, 10000.0f);
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }

            base.Draw(gameTime);
        }

        private double MiddleAngle(Vector3 v1, Vector3 v2)
        {
            double val1 = Vector3.Dot(v1, v2);
            double val2 = v1.Length() * v2.Length();

            if (val2 == 0)
                val2 = 0.0001;

            double val3 = val1 / val2;
            double val4 = Math.Acos(val3);

            double angle = val4;

            if (angle.Equals(double.NaN))
                angle = 0;

            return angle;
        }

        protected Matrix GetIphoneMatrix()
        {
            Vector3 v1 = new Vector3(0, -1, 0);
            IphoneComm.Vector iAccelVector = Listener.AccelerometerVector;

            Vector3 v2 = new Vector3(-(float)iAccelVector.x, (float)iAccelVector.y, -(float)iAccelVector.z);

            Vector3 zAxis = Vector3.Cross(v1, v2);
            zAxis.Normalize();

            Vector3 xAxis = Vector3.Cross(zAxis, Vector3.Up);
            xAxis.Normalize();

            Vector3 yAxis = Vector3.Cross(zAxis, xAxis);
            yAxis.Normalize();

            Matrix m = new Matrix(xAxis.X, xAxis.Y, xAxis.Z, 0, yAxis.X, yAxis.Y, yAxis.Z, 0, zAxis.X, zAxis.Y, zAxis.Z, 0, 0, 0, 0, 1);

            v1 = Vector3.Transform(v1, m);
            v2 = Vector3.Transform(v2, m);

            float Ang = (float)MiddleAngle(v1, v2);

            // compass matrix:

            Matrix CompassMatrix = Matrix.Identity;

            IphoneComm.Orientation iCompass = Listener.CompassData;
            float CompassAngle = (float)(iCompass.Angle * MathHelper.Pi / 180);

            if (Listener.Connected && iCompass.Angle != 0 && iCompass.Accuracy != 0 && FirstAngle == 0)
                FirstAngle = CompassAngle;

            CompassAngle = FirstAngle - CompassAngle;

           
            
            //adjust x axis
            CompassAngle += (float)(MathHelper.Pi / 2 * iAccelVector.x);

            
            //adjust z axis
            if (iAccelVector.z > 0.1)
                CompassAngle += (float)(MathHelper.Pi * iAccelVector.z * (iAccelVector.x >= 0 ? 1 : -1));

            // generate rotation matrix
            CompassMatrix = Matrix.CreateRotationY(CompassAngle);


            // finally:
            return m * Matrix.CreateRotationZ(Ang) * Matrix.Invert(m) * CompassMatrix;
        }
    }
}
