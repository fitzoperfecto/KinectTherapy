using System;
using Microsoft.Samples.Kinect.XnaBasics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SWENG.Service;

namespace SWENG.UserInterface
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class ExerciseScreen : Screen
    {
        private readonly Rectangle _viewableArea;
        private readonly GuiButtonCollection _buttonList;
        private readonly GuiHeader _header;
        private readonly GuiSensorStatus _sensorStatus;

        private const float MARGIN = 10f;

        private bool _isInitialized;

        #region ColorStreamRenderer Variables
        private readonly ColorStreamRenderer colorStream;
        private Vector2 colorStreamPosition;
        private Vector2 colorStreamSize;
        #endregion

        #region ExerciseQueue Variables
        // queue of exercises
        // reference to the exercise queue service
        private ExerciseQueue ExerciseQueue
        {
            get
            {
                return (ExerciseQueue)Game.Services.GetService(typeof(ExerciseQueue));
            }
        }
        private ExerciseTile[] _exerciseTiles;
        private MouseState _oldMouseState;
        #endregion

        private RecordingManager recordingManager
        {
            get
            {
                return (RecordingManager)Game.Services.GetService(typeof(RecordingManager));
            }
        }

        /// <summary>
        /// Initialize a new instance of the ExerciseScreen class.
        /// </summary>
        /// <param name="game">The related game object.</param>
        /// <param name="viewableArea">The desired canvas size to draw on.</param>
        /// <param name="startingState">The desired starting Screen State</param>
        public ExerciseScreen(Game game, Rectangle viewableArea, ScreenState startingState)
            : base(game)
        {
            ScreenState = startingState;
            _viewableArea = viewableArea;
            colorStream = new ColorStreamRenderer(game);

            Title = "Exercise";

            _exerciseTiles = new ExerciseTile[0];

            #region Laying out the positions
            colorStreamPosition = new Vector2(
                    (float)(viewableArea.X),
                    (float)(viewableArea.Y)
                );

            colorStreamSize = new Vector2(
                    (float)(0.7 * viewableArea.Width),
                    (float)(0.7 * viewableArea.Height)
                );

            Vector2 buttonSize = new Vector2(240f, 60f);
            Vector2 buttonBottom = new Vector2(
                _viewableArea.Right - buttonSize.X + MARGIN,
                _viewableArea.Bottom - buttonSize.Y);

            _buttonList = new GuiButtonCollection();
            _buttonList.Collection.Add(new GuiButton("Menu", buttonSize,                     
                    buttonBottom 
                    - (new Vector2(0f, 2 * MARGIN)) 
                    - (new Vector2(0f, 2 * buttonSize.Y))
                ));
            _buttonList.Collection.Add(new GuiButton("Skip", buttonSize, 
                    buttonBottom 
                    - new Vector2(0f , MARGIN) 
                    - new Vector2(0f, buttonSize.Y)
                ));
            _buttonList.Collection.Add(new GuiButton("End Queue", buttonSize, buttonBottom));


            _sensorStatus = new GuiSensorStatus("Sensor Status",
                new Vector2(99f, 32f),
                new Vector2(
                (_viewableArea.Right / 2) - (99f / 2),
                _viewableArea.Bottom - 32f
            ));

            _header = new GuiHeader("Kinect Therapy: Exercise Screen",
                new Vector2(326f, 52f),
                new Vector2(
                _viewableArea.Left,
                _viewableArea.Top - MARGIN - 52f
            ));

            #endregion

            _isInitialized = false;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            colorStream.Position = colorStreamPosition;
            colorStream.Size = colorStreamSize;
            colorStream.Initialize();

            _buttonList.ClickEventForAll(GuiButtonWasClicked);

            foreach (ExerciseTile exerciseTile in _exerciseTiles)
            {
                exerciseTile.Initialize();
            }

            _isInitialized = true;

            base.Initialize();
        }

        private void GuiButtonWasClicked(object sender, GuiButtonClickedArgs e)
        {
            switch (e.ClickedOn)
            {
                case "Menu":
                    ScreenState = UserInterface.ScreenState.Hidden;
                    OnTransition(new TransitionEventArgs(Title, e.ClickedOn));
                    break;
                case "Skip":
                    break;
                case "End Queue":
                    break;
            }
        }

        /// <summary>
        /// This method creates a new ContentManager 
        /// and loads the textures and fonts.
        /// </summary>
        public override void LoadContent()
        {
            if (null == contentManager)
            {
                contentManager = new ContentManager(Game.Services, "Content");
            }

            _buttonList.Collection[0].Texture2D = contentManager.Load<Texture2D>(@"UI\Menu");
            _buttonList.Collection[1].Texture2D = contentManager.Load<Texture2D>(@"UI\Skip");
            _buttonList.Collection[2].Texture2D = contentManager.Load<Texture2D>(@"UI\EndQueue");

            _sensorStatus.Texture2D = contentManager.Load<Texture2D>(@"UI\KinectSensorGood");

            _header.Texture2D = contentManager.Load<Texture2D>(@"UI\KinectTherapy");

            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            if (_isInitialized
                && (ScreenState & UserInterface.ScreenState.NonInteractive) == 0)
            {
                MouseState currentState = Mouse.GetState();
                _buttonList.Update(currentState, _oldMouseState);

                colorStream.Update(gameTime);
                foreach (ExerciseTile exerciseTile in _exerciseTiles)
                {
                    exerciseTile.Update(gameTime);
                }
                _oldMouseState = currentState;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This method renders the current state of the ExerciseScreen.
        /// </summary>
        /// <param name="gameTime">The elapsed game time.</param>
        public override void Draw(GameTime gameTime)
        {
            if (_isInitialized)
            {
                GraphicsDevice.Clear(Color.WhiteSmoke);
                var spriteBatch = SharedSpriteBatch;
                spriteBatch.Begin();

                _header.Draw(spriteBatch);

                _buttonList.Draw(spriteBatch);

                _sensorStatus.Draw(spriteBatch);

                spriteBatch.End();

                foreach (ExerciseTile exerciseTile in _exerciseTiles)
                {
                    if (exerciseTile.IsCurrentTile)
                    {
                        exerciseTile.Draw(gameTime);
                    }
                }

                colorStream.Draw(gameTime);
            }

            base.Draw(gameTime);
        }

        /// <summary>
        /// The method to use when the exercise screen
        /// should be triggered to close when an event occurs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void QueueIsDone(object sender, EventArgs args)
        {
            ScreenState = UserInterface.ScreenState.Hidden;
            base.Transition();
        }

        public override void OpenScreen()
        {
            Vector2 tileStartingPosition = new Vector2(
                (float)(colorStreamPosition.X + colorStreamSize.X + (MARGIN * 2)),
                (float)(colorStreamPosition.Y)
            );

            Vector2 tileSize = new Vector2(
                    (float)(0.25 * _viewableArea.Width),
                    (float)(0.25 * _viewableArea.Height)
                );

            // for now we'll generate hardcoded exercises....
            // note the location of this will need to change if we load these from an external file. 
            ExerciseGameComponent[] exercises = ExerciseQueue.Exercises;
            _exerciseTiles = new ExerciseTile[exercises.Length];
            
            // draw at the same height... going to "cycle" through them
            for (int i = 0; i < exercises.Length; i++)
            {
                _exerciseTiles[i] = new ExerciseTile(Game, exercises[i], i);
                _exerciseTiles[i].Position = tileStartingPosition;
                _exerciseTiles[i].Size = tileSize;
                _exerciseTiles[i].Initialize();

                // bump the next tile down by the size of the tile and a y margin
                //tileStartingPosition = new Vector2(
                //    tileStartingPosition.X,
                //    tileStartingPosition.Y + tileSize.Y + MARGIN
                //);
            }

            base.Transition();
        }
    }
}
