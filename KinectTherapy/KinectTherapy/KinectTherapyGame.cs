using Microsoft.Kinect;
using Microsoft.Samples.Kinect.XnaBasics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SWENG;
using SWENG.Service;
using SWENG.UserInterface;

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
        private const int WIDTH = 1024;

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
        private readonly CatalogManager catalogManager;

        private readonly SummaryScreen _summaryScreen;
        private readonly ExerciseScreen _exerciseScreen;
        private readonly CatalogScreen _catalogScreen;
        private readonly LogInScreen _logInScreen;
        private readonly HomeScreen _homeScreen;
        private readonly CatalogTileEditScreen _catalogTileEditScreen;
        private readonly LoadingScreen _loadingScreen;
        private readonly SensorTileEditScreen _sensorTileEditScreen;

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
            graphics.PreferredBackBufferHeight = (WIDTH / 4) * 3;
            graphics.PreparingDeviceSettings += this.GraphicsDevicePreparingDeviceSettings;
            graphics.SynchronizeWithVerticalRetrace = true;
            graphics.IsFullScreen = false;
            // this will give the viewport a border
            viewPortRectangle = new Rectangle(10, 80, WIDTH - 20, ((WIDTH / 4) * 3) - 90);

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

            catalogManager = new CatalogManager();
            Services.AddService(typeof(CatalogManager), catalogManager);
            #endregion

            #region Components
            userInterfaceManager = new Manager(this);
            #endregion

            #region Screens
            _homeScreen = new HomeScreen(this, viewPortRectangle, ScreenState.Active);
            _summaryScreen = new SummaryScreen(this, viewPortRectangle, ScreenState.Hidden);
            _exerciseScreen = new ExerciseScreen(this, viewPortRectangle, ScreenState.Hidden);
            _catalogScreen = new CatalogScreen(this, viewPortRectangle, ScreenState.Hidden);
            _logInScreen = new LogInScreen(this, viewPortRectangle, ScreenState.Hidden);
            _catalogTileEditScreen = new CatalogTileEditScreen(
                this, 
                new Rectangle(
                    0, 
                    0, 
                    graphics.PreferredBackBufferWidth, 
                    graphics.PreferredBackBufferHeight
                ), 
                ScreenState.Hidden
            );
            _loadingScreen = new LoadingScreen(
                this,
                new Rectangle(
                    0,
                    0,
                    graphics.PreferredBackBufferWidth,
                    graphics.PreferredBackBufferHeight
                ),
                ScreenState.Hidden
            );
            _sensorTileEditScreen = new SensorTileEditScreen(
                this,
                new Rectangle(
                    0,
                    0,
                    graphics.PreferredBackBufferWidth,
                    graphics.PreferredBackBufferHeight
                ),
                ScreenState.Hidden
            );

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
            #region Attaching Events/Adding to Event Lists
            exerciseQueue.RepetitionStartedListener.Add(new StartedRepetitionEventHandler(recordingManager.StartRecording));
            exerciseQueue.QueueIsDone += recordingManager.StopRecording;

            catalogManager.CatalogCompleteEventHandler += exerciseQueue.LoadExercises;
            exerciseQueue.LoadIsDone += _loadingScreen.CloseScreen;

            recordingManager.RecordingStatusChanged += exerciseQueue.AssociateFiles;

            exerciseQueue.QueueIsDone += _exerciseScreen.QueueIsDone;
            exerciseQueue.QueueIsDone += _summaryScreen.QueueIsDone;
            #endregion

            #region Adding Screens
            //TODO: This needs to be refitted as the actual home screen.
            userInterfaceManager.AddScreen(_homeScreen);
            userInterfaceManager.AddScreen(_logInScreen);
            userInterfaceManager.AddScreen(_exerciseScreen);
            userInterfaceManager.AddScreen(_summaryScreen);
            userInterfaceManager.AddScreen(_catalogScreen);
            userInterfaceManager.AddScreen(_catalogTileEditScreen);
            userInterfaceManager.AddScreen(_loadingScreen);
            userInterfaceManager.AddScreen(_sensorTileEditScreen);

            #endregion

            #region Manual Initialization
            skeletonPool.Initialize();
            recordingManager.Initialize();
            exerciseQueue.Initialize();
            catalogManager.Initialize();
            #endregion

            #region Adding Components
            Components.Add(userInterfaceManager);
            #endregion

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
