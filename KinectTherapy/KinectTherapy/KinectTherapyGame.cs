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
        /// preloading assets
        /// </summary>
        static readonly string[] preloadGraphics = 
        {
            "gradient",
            "blank",
        };

        /// <summary>
        /// From XnaBasics; This control selects a sensor, and displays a notice if one is
        /// not connected.
        /// </summary>
        private readonly KinectChooser _chooser;

        /// <summary>
        /// This is the viewport of the streams.
        /// </summary>
        private readonly Rectangle _viewPortRectangle;

        /// <summary>
        /// This is the queue of SkeletonStamps
        /// </summary>
        private readonly SkeletonPool _skeletonPool;

        /// <summary>
        /// This manages the queue of exercises as well as the current exercise being performed
        /// </summary>
        private readonly ExerciseQueue _exerciseQueue;

        /// <summary>
        /// The exercise screen
        /// </summary>
        private readonly Manager _userInterfaceManager;
        private readonly RecordingManager _recordingManager;
        private readonly CatalogManager _catalogManager;
        private readonly SummaryScreen _summaryScreen;
        private readonly ExerciseScreen _exerciseScreen;
        private readonly CatalogScreen _catalogScreen;
        private readonly HomeScreen _homeScreen;
        private readonly CatalogTileEditScreen _catalogTileEditScreen;
        private readonly LoadingScreen _loadingScreen;
        private readonly SensorTileEditScreen _sensorTileEditScreen;

        private const int SKELETON_POOL_SIZE = 100;
        private const int WIDTH = 1024; // window size

        /// <summary>
        /// This tracks the previous keyboard state.
        /// </summary>
        private KeyboardState _previousKeyboard;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public KinectTherapyGame()
        {
            IsMouseVisible = true;
            Window.Title = "Kinect Therapy";
            _previousKeyboard = Keyboard.GetState();

            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = WIDTH;
            _graphics.PreferredBackBufferHeight = (WIDTH / 4) * 3;
            _graphics.PreparingDeviceSettings += this.GraphicsDevicePreparingDeviceSettings;
            _graphics.SynchronizeWithVerticalRetrace = true;
            _graphics.IsFullScreen = false;
            // this will give the viewport a border
            _viewPortRectangle = new Rectangle(10, 80, WIDTH - 20, ((WIDTH / 4) * 3) - 90);

            Content.RootDirectory = "Content";

            #region Services
            _chooser = new KinectChooser(this, ColorImageFormat.RgbResolution640x480Fps30, DepthImageFormat.Resolution640x480Fps30);
            Services.AddService(typeof(KinectChooser), this._chooser);

            _skeletonPool = new SkeletonPool(this, SKELETON_POOL_SIZE);
            Services.AddService(typeof(SkeletonPool), _skeletonPool);

            _exerciseQueue = new ExerciseQueue(this);
            Services.AddService(typeof(ExerciseQueue), _exerciseQueue);

            _recordingManager = new RecordingManager();
            Services.AddService(typeof(RecordingManager), _recordingManager);

            _catalogManager = new CatalogManager();
            Services.AddService(typeof(CatalogManager), _catalogManager);
            #endregion

            #region Components
            _userInterfaceManager = new Manager(this);
            #endregion

            #region Screens
            _homeScreen = new HomeScreen(this, _viewPortRectangle, ScreenState.Active);
            _summaryScreen = new SummaryScreen(this, _viewPortRectangle, ScreenState.Hidden);
            _exerciseScreen = new ExerciseScreen(this, _viewPortRectangle, ScreenState.Hidden);
            _catalogScreen = new CatalogScreen(this, _viewPortRectangle, ScreenState.Hidden);
            _catalogTileEditScreen = new CatalogTileEditScreen(
                this, 
                new Rectangle(
                    0, 
                    0, 
                    _graphics.PreferredBackBufferWidth, 
                    _graphics.PreferredBackBufferHeight
                ), 
                ScreenState.Hidden
            );
            _loadingScreen = new LoadingScreen(
                this,
                new Rectangle(
                    0,
                    0,
                    _graphics.PreferredBackBufferWidth,
                    _graphics.PreferredBackBufferHeight
                ),
                ScreenState.Hidden
            );
            _sensorTileEditScreen = new SensorTileEditScreen(
                this,
                new Rectangle(
                    0,
                    0,
                    _graphics.PreferredBackBufferWidth,
                    _graphics.PreferredBackBufferHeight
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
            _exerciseQueue.RepetitionStartedListener.Add(new StartedRepetitionEventHandler(_recordingManager.StartRecording));
            _exerciseQueue.QueueIsDone += _recordingManager.StopRecording;
            _exerciseQueue.QueueIsDone += _exerciseScreen.QueueIsDone;
            _exerciseQueue.QueueIsDone += _summaryScreen.QueueIsDone;
            _exerciseQueue.LoadIsDone += _loadingScreen.CloseScreen;

            _catalogManager.CatalogCompleteEventHandler += _exerciseQueue.LoadExercises;

            _recordingManager.RecordingStatusChanged += _exerciseQueue.AssociateFiles;
            #endregion

            #region Adding Screens
            //TODO: This needs to be refitted as the actual home screen.
            _userInterfaceManager.AddScreen(_homeScreen);
            _userInterfaceManager.AddScreen(_exerciseScreen);
            _userInterfaceManager.AddScreen(_summaryScreen);
            _userInterfaceManager.AddScreen(_catalogScreen);
            _userInterfaceManager.AddScreen(_catalogTileEditScreen);
            _userInterfaceManager.AddScreen(_loadingScreen);
            _userInterfaceManager.AddScreen(_sensorTileEditScreen);

            #endregion

            #region Manual Initialization
            _skeletonPool.Initialize();
            _recordingManager.Initialize();
            _catalogManager.Initialize();
            #endregion

            #region Adding Components
            Components.Add(_userInterfaceManager);
            #endregion

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService(typeof(SpriteBatch), this._spriteBatch);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
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
            _previousKeyboard = newState;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
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
