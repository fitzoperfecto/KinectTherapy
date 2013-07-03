using System;
using System.Collections.Generic;
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
    public class SummaryScreen : Screen
    {
        #region Non-Specific Variables
        private Texture2D _blankTexture;
        private SpriteFont _spriteFont;
        private Rectangle _viewableArea;
        private bool _isInitialized;
        private string _fileId;
        private const float _ANIMATION_RATE = 0.25f;
        private const float MARGIN = 10f;
        private readonly GuiHeader _header;
        private readonly GuiSensorStatus _sensorStatus;
        private bool _isReplaying;
        #endregion

        #region ColorStreamRenderer Variables
        private readonly ColorStreamRenderer _colorStream;
        private Vector2 _colorStreamPosition;
        private Vector2 _colorStreamSize;
        private Vector2 _colorStreamMaxSize;
        #endregion

        #region ExerciseQueue Variables
        // queue of exercises
        // reference to the exercise queue service
        private ExerciseQueue _exerciseQueue
        {
            get
            {
                return (ExerciseQueue)Game.Services.GetService(typeof(ExerciseQueue));
            }
        }
        #endregion

        #region Recording & Replay Variables
        private RecordingManager _recordingManager
        {
            get
            {
                return (RecordingManager)Game.Services.GetService(typeof(RecordingManager));
            }
        }
        private ReplayTile[] _replayTiles;
        #endregion

        #region Button Variables
        private GuiButtonCollection _buttonListSelect;
        private GuiButtonCollection _buttonListReplay;
        private Vector2 _startingPosition;
        private MouseState _oldMouseState;
        #endregion

        /// <summary>
        /// Initialize a new instance of the ExerciseScreen class.
        /// </summary>
        /// <param name="game">The related game object.</param>
        /// <param name="viewableArea">The desired canvas size to draw on.</param>
        /// <param name="startingState">The desired starting Screen State</param>
        public SummaryScreen(Game game, Rectangle viewableArea, ScreenState startingState)
            : base(game)
        {
            ScreenState = startingState;
            _viewableArea = viewableArea;
            _colorStream = new ColorStreamRenderer(game);

            Title = "Summary";

            _replayTiles = new ReplayTile[0];

            _colorStreamPosition = new Vector2(
                    (float)(_viewableArea.X),
                    (float)(_viewableArea.Y)
                );

            _colorStreamMaxSize = new Vector2(
                    (float)(0.7 * _viewableArea.Width),
                    (float)(0.7 * _viewableArea.Height)
                );

            _colorStreamSize = Vector2.Zero;

            Vector2 buttonSize = new Vector2(240f, 60f);
            Vector2 buttonBottom = new Vector2(
                _viewableArea.Right - buttonSize.X + MARGIN,
                _viewableArea.Bottom - buttonSize.Y);

            #region Laying out positions
            _buttonListSelect = new GuiButtonCollection();
            _buttonListSelect.Collection.Add(
                new GuiButton("Finished", buttonSize,
                    buttonBottom
                    - new Vector2(0f, MARGIN)
                    - new Vector2(0f, buttonSize.Y)
                ));
            _buttonListSelect.Collection.Add(new GuiButton("Exit Program", buttonSize, buttonBottom));

            _buttonListReplay = new GuiButtonCollection();
            _buttonListReplay.Collection.Add(new GuiButton("Replay", buttonSize,
                    buttonBottom
                    - (new Vector2(0f, 3 * MARGIN))
                    - (new Vector2(0f, 3 * buttonSize.Y))
                ));
            _buttonListReplay.Collection.Add(new GuiButton("Change", buttonSize,
                    buttonBottom
                    - new Vector2(0f, 2 * MARGIN)
                    - new Vector2(0f, 2 * buttonSize.Y)
                ));
            _buttonListReplay.Collection.Add(new GuiButton("Finished", buttonSize,
                    buttonBottom
                    - new Vector2(0f, MARGIN)
                    - new Vector2(0f, buttonSize.Y)
                ));
            _buttonListReplay.Collection.Add(new GuiButton("Exit Program", buttonSize, buttonBottom));

            _sensorStatus = new GuiSensorStatus("Sensor Status",
                new Vector2(99f, 32f),
                new Vector2(
                (_viewableArea.Right / 2) - (99f / 2),
                _viewableArea.Bottom - 32f
            ));

            _header = new GuiHeader("Kinect Therapy: Home Screen",
                new Vector2(326f, 52f),
                new Vector2(
                _viewableArea.Left,
                _viewableArea.Top - MARGIN - 52f
            ));
            #endregion

            _isInitialized = false;
            _isReplaying = false;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            _colorStream.Position = _colorStreamPosition;
            _colorStream.Size = Vector2.Zero;
            _colorStream.Initialize();

            _startingPosition = new Vector2(
                _colorStreamPosition.X,
                _colorStreamPosition.Y
            );

            _buttonListSelect.ClickEventForAll(GuiButtonWasClicked);
            _buttonListReplay.ClickEventForAll(GuiButtonWasClicked);

            foreach (ReplayTile replayTile in _replayTiles)
            {
                replayTile.Initialize();
            }

            _isInitialized = true;

            base.Initialize();
        }

        private void GuiButtonWasClicked(object sender, GuiButtonClickedArgs e)
        {
            switch (e.ClickedOn)
            {
                case "Exit Program":
                case "Finished":
                    ScreenState = UserInterface.ScreenState.Hidden;
                    OnTransition(new TransitionEventArgs(Title, e.ClickedOn));
                    break;
                case "Replay":
                    _recordingManager.RestartReplay();
                    break;
                case "Change":
                    closeReplay();
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

            _spriteFont = contentManager.Load<SpriteFont>("Arial16");
            _blankTexture = contentManager.Load<Texture2D>("blank");

            _buttonListSelect.Collection[0].Texture2D = contentManager.Load<Texture2D>(@"UI\Finished");
            _buttonListSelect.Collection[1].Texture2D = contentManager.Load<Texture2D>(@"UI\ExitProgram");

            _buttonListReplay.Collection[0].Texture2D = contentManager.Load<Texture2D>(@"UI\Replay");
            _buttonListReplay.Collection[1].Texture2D = contentManager.Load<Texture2D>(@"UI\Change");
            _buttonListReplay.Collection[2].Texture2D = contentManager.Load<Texture2D>(@"UI\Finished");
            _buttonListReplay.Collection[3].Texture2D = contentManager.Load<Texture2D>(@"UI\ExitProgram");

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
                Rectangle mouseBoundingBox = new Rectangle(currentState.X, currentState.Y, 1, 1);

                if (_isReplaying)
                {
                    _buttonListReplay.Update(currentState, _oldMouseState);
                }
                else
                {
                    _buttonListSelect.Update(currentState, _oldMouseState);
                    foreach (ReplayTile replayTile in _replayTiles)
                    {
                        if (mouseBoundingBox.Intersects(replayTile.Rectangle))
                        {
                            replayTile.Hovered = true;
                            if (currentState.LeftButton == ButtonState.Pressed
                                && _oldMouseState.LeftButton == ButtonState.Released)
                            {
                                openReplay(replayTile.FileId);
                            }
                        }
                        else
                        {
                            replayTile.Hovered = false;
                        }

                        replayTile.Update(gameTime);
                    }
                }

                if (Math.Ceiling(_colorStream.Size.X) == Math.Ceiling(_colorStreamSize.X) 
                    && !string.IsNullOrEmpty(_fileId))
                {
                    _recordingManager.StartReplaying(_fileId);
                    _fileId = string.Empty;
                }

                _colorStream.Size = Vector2.SmoothStep(
                    _colorStream.Size,
                    _colorStreamSize,
                    _ANIMATION_RATE
                );
                
                _colorStream.Update(gameTime);

                _oldMouseState = currentState;
            }
            
            base.Update(gameTime);
        }

        /// <summary>
        /// Move all the UI elements to their "open" state positions
        /// </summary>
        /// <param name="fileId">The ID of the file to play</param>
        private void openReplay(string fileId)
        {
            if (_recordingManager.Status == RecordingManagerStatus.Replaying)
            {
                _recordingManager.StopReplaying();
            }
            else
            {
                _colorStreamSize = _colorStreamMaxSize;
                _isReplaying = true;
            }

            _fileId = fileId;
        }

        /// <summary>
        /// Move all the UI elements back to the original positioning and size
        /// </summary>
        private void closeReplay()
        {
            _fileId = string.Empty;
            _recordingManager.StopReplaying();
            _colorStreamSize = Vector2.Zero;

            _isReplaying = false;

            /** adjust replay tile positions (and size possibly) */
            foreach (ReplayTile replayTile in _replayTiles)
            {
                replayTile.ToPosition = new Vector2(
                    _startingPosition.X,
                    replayTile.Position.Y
                );
            }
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

                if (!_isReplaying)
                {
                    _buttonListSelect.Draw(spriteBatch);
                }
                else
                {
                    _buttonListReplay.Draw(spriteBatch);
                }

                _header.Draw(spriteBatch);

                _sensorStatus.Draw(spriteBatch);

                spriteBatch.End();

                _colorStream.Draw(gameTime);

                if (!_isReplaying)
                {
                    foreach (ReplayTile replayTile in _replayTiles)
                    {
                        replayTile.Draw(gameTime);
                    }
                }
            }

            base.Draw(gameTime);
        }

        /// <summary>
        /// The method to use when the summary screen
        /// should be triggered to open when an event occurs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args">Generic event arguments</param>
        public void QueueIsDone(object sender, EventArgs args)
        {
            Vector2 tileStartingPosition = new Vector2(
                _viewableArea.Left,
                _viewableArea.Top
            );

            Vector2 tileSize = new Vector2(
                0.25f * _viewableArea.Width,
                0.25f * _viewableArea.Height
            );
            
            ExerciseGameComponent[] exercises = _exerciseQueue.Exercises;
            List<ReplayTile> replayTilesList = new List<ReplayTile>();

            int replayCount = 0;
            foreach (ExerciseGameComponent exercise in exercises)
            {
                for (int i = 0; i < exercise.RepetitionToFileId.Count; i = i + 1)
                {
                    replayTilesList.Add(
                        new ReplayTile(
                            Game,
                            exercise.RepetitionToFileId[i],
                            exercise.Name,
                            i
                        )
                    );

                    replayTilesList[replayCount].Position = new Vector2(tileStartingPosition.X, tileStartingPosition.Y);
                    /** 
                     * initial position is the position we want, otherwise
                     * it will immediately start shrinking 
                     */
                    replayTilesList[replayCount].ToPosition = new Vector2(tileStartingPosition.X, tileStartingPosition.Y);
                    replayTilesList[replayCount].Size = tileSize;
                    replayTilesList[replayCount].Initialize();

                    /** bump the next tile right by the size of the tile and a x margin */
                    if (tileStartingPosition.X + 2 * (tileSize.X + MARGIN) < _viewableArea.Right)
                    {
                        tileStartingPosition = new Vector2(
                            tileStartingPosition.X + tileSize.X + MARGIN,
                            tileStartingPosition.Y
                        );
                    }
                    /**
                     * bump the next tile down by the size of the tile and a y margin.
                     * reset to the left side
                     */
                    else
                    {
                        tileStartingPosition = new Vector2(
                            _viewableArea.Left,
                            tileStartingPosition.Y + tileSize.Y + MARGIN
                        );
                    }

                    replayCount = replayCount + 1;
                }
            }

            _replayTiles = replayTilesList.ToArray();

            ScreenState = UserInterface.ScreenState.Active;
            base.Transition();
        }
    }
}
