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

using Microsoft.Samples.Kinect.XnaBasics;

using SWENG;
using SWENG.UserInterface;
using Microsoft.Kinect;
using SWENG.Service;

namespace KinectTherapy
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class KinectTherapyGame : Microsoft.Xna.Framework.Game
    {
        /// <summary>
        /// This is used to adjust the window size.
        /// </summary>
        private const int WIDTH = 800;

        /// <summary>
        /// From XnaBasics; This control selects a sensor, and displays a notice if one is
        /// not connected.
        /// </summary>
        private readonly KinectChooser chooser;

        /// <summary>
        /// This is the viewport of the streams.
        /// </summary>
        private readonly Rectangle viewPortRectangle;

        /// <summary>
        /// This tracks the previous keyboard state.
        /// </summary>
        private KeyboardState previousKeyboard;

        /// <summary>
        /// This is the queue of SkeletonStamps
        /// </summary>
        private readonly SkeletonPool skeletonPool;
        private const int SKELETON_POOL_SIZE = 100;

        /// <summary>
        /// This manages the queue of exercises as well as the current exercise being performed
        /// </summary>
        private readonly ExerciseQueue exerciseQueue;

        /// <summary>
        /// The exercise screen
        /// </summary>
        private readonly Manager userInterfaceManager;

        private readonly RecordingManager recordingManager;

        /// <summary>
        /// preloading assets
        /// </summary>
        static readonly string[] preloadGraphics = 
        {
            "gradient",
            "blank",
        };


        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public KinectTherapyGame()
        {
            IsMouseVisible = true;
            Window.Title = "Kinect Therapy";
            previousKeyboard = Keyboard.GetState();

            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = WIDTH;
            // keep the 110 header for now.
            // exercise stuff is being written here.
            graphics.PreferredBackBufferHeight = ((WIDTH / 4) * 3) + 110;
            graphics.PreparingDeviceSettings += this.GraphicsDevicePreparingDeviceSettings;
            graphics.SynchronizeWithVerticalRetrace = true;
            // this will give the viewport a border
            viewPortRectangle = new Rectangle(10, 80, WIDTH - 20, ((WIDTH - 2) / 4) * 3);

            Content.RootDirectory = "Content";

            #region Services
            chooser = new KinectChooser(this, ColorImageFormat.RgbResolution640x480Fps30, DepthImageFormat.Resolution640x480Fps30);
            Services.AddService(typeof(KinectChooser), this.chooser);

            skeletonPool = new SkeletonPool(this, SKELETON_POOL_SIZE);
            Services.AddService(typeof(SkeletonPool), skeletonPool);

            exerciseQueue = new ExerciseQueue(this);
            Services.AddService(typeof(ExerciseQueue), exerciseQueue);

            recordingManager = new RecordingManager();
            Services.AddService(typeof(RecordingManager), recordingManager);
            #endregion

            #region Components
            userInterfaceManager = new Manager(this);
            #endregion

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
            skeletonPool.Initialize();
            exerciseQueue.Initialize();
            recordingManager.Initialize();
            Components.Add(this.exerciseQueue);
            Components.Add(this.userInterfaceManager);

            this.userInterfaceManager.AddScreen(new HomeScreen(this, viewPortRectangle, ScreenState.Active));
            this.userInterfaceManager.AddScreen(new ExerciseScreen(this, viewPortRectangle, ScreenState.Hidden));

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService(typeof(SpriteBatch), this.spriteBatch);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState newState = Keyboard.GetState();
            // Allows the game to exit
            if (newState.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }

            // TODO: Add your update logic here
            previousKeyboard = newState;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        /// <summary>
        /// from XnaBasics
        /// This method ensures that we can render to the back buffer without
        /// losing the data we already had in our previous back buffer.  This
        /// is necessary for the SkeletonStreamRenderer.
        /// </summary>
        /// <param name="sender">The sending object.</param>
        /// <param name="e">The event args.</param>
        private void GraphicsDevicePreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            // This is necessary because we are rendering to back buffer/render targets and we need to preserve the data
            e.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
        }
    }
}
