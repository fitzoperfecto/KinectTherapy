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
    /// This class implements the screen for its use with the Manager
    /// </summary>
    public class SummaryScreen : Screen
    {
        private readonly ColorStreamRenderer _colorStream;

        private const float ANIMATION_RATE = 0.25f;
        private const float MARGIN = 10f;

        private string _fileId;
        private int _catalogLocation;
        private bool _isInitialized;
        private bool _isReplaying;

        private Texture2D _blankTexture;
        private SpriteFont _spriteFont;
        private Rectangle _viewableArea;
        private Vector2 _colorStreamPosition;
        private Vector2 _colorStreamSize;
        private Vector2 _colorStreamMaxSize;
        private ReplayTile[] _replayTiles;
        private GuiDrawable[] _guiDrawable;
        private GuiDrawable[] _guiDrawableSelect;
        private GuiDrawable[] _guiDrawableReplay;
        private Vector2 _startingPosition;
        private MouseState _oldMouseState;

        private ExerciseQueue _exerciseQueue
        {
            get
            {
                return (ExerciseQueue)Game.Services.GetService(typeof(ExerciseQueue));
            }
        }

        private RecordingManager _recordingManager
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
        public SummaryScreen(Game game, Rectangle viewableArea, ScreenState startingState)
            : base(game)
        {
            ScreenState = startingState;
            _viewableArea = viewableArea;
            _colorStream = new ColorStreamRenderer(game);

            Title = "Summary";

            _replayTiles = new ReplayTile[0];

            #region Laying out positions
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

            Vector2 catalogPosition = new Vector2(
                _viewableArea.Left + MARGIN,
                _viewableArea.Top + buttonSize.Y
            );

            Vector2 catalogSize = new Vector2(
                _viewableArea.Width - buttonSize.X - MARGIN,
                _viewableArea.Height - buttonSize.Y
            );

            List<GuiDrawable> guiDrawableSelect = new List<GuiDrawable>();
            guiDrawableSelect.Add(
                new GuiButton(
                    "Finished", 
                    buttonSize,
                    buttonBottom
                    - new Vector2(0f, MARGIN)
                    - new Vector2(0f, buttonSize.Y)
            ));

            guiDrawableSelect.Add(
                new GuiButton(
                    "ExitProgram",
                    buttonSize,
                    buttonBottom)
            );

            List<GuiDrawable> guiDrawableReplay = new List<GuiDrawable>();
            guiDrawableReplay.Add(
                new GuiButton(
                    "Replay", 
                    buttonSize,
                    buttonBottom
                    - (new Vector2(0f, 3 * MARGIN))
                    - (new Vector2(0f, 3 * buttonSize.Y))
            ));
            guiDrawableReplay.Add(
                new GuiButton(
                    "Change", 
                    buttonSize,
                    buttonBottom
                    - new Vector2(0f, 2 * MARGIN)
                    - new Vector2(0f, 2 * buttonSize.Y)
                ));
            guiDrawableReplay.Add(
                new GuiButton(
                    "Finished", 
                    buttonSize,
                    buttonBottom
                    - new Vector2(0f, MARGIN)
                    - new Vector2(0f, buttonSize.Y)
                ));
            guiDrawableReplay.Add(
                new GuiButton(
                    "ExitProgram",
                    buttonSize,
                    buttonBottom
                )
            );

            List<GuiDrawable> guiDrawable = new List<GuiDrawable>();

            guiDrawable.Add(
                new GuiSensorStatus(
                    "SensorStatus",
                    new Vector2(99f, 32f),
                    new Vector2(
                        (_viewableArea.Right / 2) - (99f / 2),
                        _viewableArea.Bottom - 32f
                    ),
                    game
                )
            );

            guiDrawable.Add(
                new GuiHeader("KinectTherapy",
                    new Vector2(326f, 52f),
                    new Vector2(
                        _viewableArea.Left,
                        _viewableArea.Top - MARGIN - 52f
                    )
                )
            );

            guiDrawable.Add(
                new GuiScrollableCollection(
                    "Catalog",
                    catalogSize,
                    catalogPosition,
                    2,
                    //115f,
                    230f,
                    650f
                )
            );

            _catalogLocation = guiDrawable.Count - 1;

            #endregion

            _guiDrawable = guiDrawable.ToArray();
            _guiDrawableReplay = guiDrawableReplay.ToArray();
            _guiDrawableSelect = guiDrawableSelect.ToArray();

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

            foreach (GuiDrawable guiDrawable in _guiDrawableReplay)
            {
                ((GuiButton)guiDrawable).ClickEvent += GuiButtonWasClicked;
            }

            foreach (GuiDrawable guiDrawable in _guiDrawableSelect)
            {
                ((GuiButton)guiDrawable).ClickEvent += GuiButtonWasClicked;
            }

            _isInitialized = true;

            base.Initialize();
        }

        /// <summary>
        /// This method creates a new ContentManager 
        /// and loads the textures and fonts.
        /// </summary>
        public override void LoadContent()
        {
            if (contentManager == null)
            {
                contentManager = new ContentManager(Game.Services, "Content");
            }

            _spriteFont = contentManager.Load<SpriteFont>("Arial16");
            _blankTexture = contentManager.Load<Texture2D>("blank");

            foreach (GuiDrawable guiDrawable in _guiDrawableReplay)
            {
                guiDrawable.LoadContent(Game, contentManager, SharedSpriteBatch);
            }

            foreach (GuiDrawable guiDrawable in _guiDrawableSelect)
            {
                guiDrawable.LoadContent(Game, contentManager, SharedSpriteBatch);
            }

            foreach (GuiDrawable guiDrawable in _guiDrawable)
            {
                guiDrawable.LoadContent(Game, contentManager, SharedSpriteBatch);
            }

            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            /** 
             * The exercise game component still "ups" the number of reps in its last update cycle
             * SO, this makes sure that the recording stops
             */
            if (_recordingManager.Status == RecordingManagerStatus.Recording)
            {
                _recordingManager.StopRecording();
            }

            if (_isInitialized
                && (ScreenState & UserInterface.ScreenState.NonInteractive) == 0)
            {
                MouseState currentState = Mouse.GetState();
                Rectangle mouseBoundingBox = new Rectangle(currentState.X, currentState.Y, 1, 1);

                /** Update only the items pertinent to a RUNNING replay */
                if (_isReplaying)
                {
                    foreach (GuiDrawable guiDrawable in _guiDrawableReplay)
                    {
                        guiDrawable.Update(currentState, _oldMouseState, mouseBoundingBox, gameTime);
                    }

                    foreach (GuiDrawable guiDrawable in _guiDrawable)
                    {
                        if (guiDrawable.Text != "Catalog")
                        {
                            guiDrawable.Update(currentState, _oldMouseState, mouseBoundingBox, gameTime);
                        }
                    }
                }
                /** Update only the items pertinent to a SELECTING replay */
                else
                {
                    foreach (GuiDrawable guiDrawable in _guiDrawableSelect)
                    {
                        guiDrawable.Update(currentState, _oldMouseState, mouseBoundingBox, gameTime);
                    }

                    foreach (GuiDrawable guiDrawable in _guiDrawable)
                    {
                        guiDrawable.Update(currentState, _oldMouseState, mouseBoundingBox, gameTime);
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
                    ANIMATION_RATE
                );
                
                _colorStream.Update(gameTime);

                _oldMouseState = currentState;
            }
            
            base.Update(gameTime);
        }

        /// <summary>
        /// This method renders the current state of the SummaryScreen to the screen.
        /// </summary>
        /// <param name="gameTime">The elapsed game time.</param>
        public override void Draw(GameTime gameTime)
        {
            if (_isInitialized)
            {
                GraphicsDevice.Clear(Color.WhiteSmoke);
                var spriteBatch = SharedSpriteBatch;
                spriteBatch.Begin();

                /** Draw only the items pertinent to a running replay */
                if (_isReplaying)
                {
                    foreach (GuiDrawable guiDrawable in _guiDrawableReplay)
                    {
                        guiDrawable.Draw(spriteBatch);
                    }

                    foreach (GuiDrawable guiDrawable in _guiDrawable)
                    {
                        if (guiDrawable.Text != "Catalog")
                        {
                            guiDrawable.Draw(spriteBatch);
                        }
                    }
                }
                /** Draw only the items pertinent to a selecting replay */
                else
                {
                    foreach (GuiDrawable guiDrawable in _guiDrawableSelect)
                    {
                        guiDrawable.Draw(spriteBatch);
                    }

                    foreach (GuiDrawable guiDrawable in _guiDrawable)
                    {
                        guiDrawable.Draw(spriteBatch);
                    }
                }

                spriteBatch.End();

                _colorStream.Draw(gameTime);
            }

            base.Draw(gameTime);
        }

        /// <summary>
        /// Central button click management.
        /// </summary>
        private void GuiButtonWasClicked(object sender, GuiButtonClickedArgs e)
        {
            switch (e.ClickedOn)
            {
                case "ExitProgram":
                case "Finished":
                    ScreenState = UserInterface.ScreenState.Hidden;
                    CloseReplay();
                    OnTransition(new TransitionEventArgs(Title, e.ClickedOn));
                    break;
                case "Replay":
                    _recordingManager.RestartReplay();
                    break;
                case "Change":
                    CloseReplay();
                    break;
            }

        }

        private void ReplayTileSelected(object sender, ReplaySelectedEventArgs e)
        {
            OpenReplay(e.ID);
        }

        /// <summary>
        /// Move all the UI elements to their "open" state positions
        /// </summary>
        /// <param name="fileId">The ID of the file to play</param>
        private void OpenReplay(string fileId)
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
        private void CloseReplay()
        {
            _fileId = string.Empty;
            _recordingManager.StopReplaying();
            _colorStreamSize = Vector2.Zero;

            _isReplaying = false;
        }

        /// <summary>
        /// The method to use when the summary screen
        /// should be triggered to open when an event occurs
        /// </summary>
        public void QueueIsDone(object sender, EventArgs args)
        {
            GuiScrollableCollection scrollableCollection = (GuiScrollableCollection)_guiDrawable[_catalogLocation];

            scrollableCollection.ClearCollection();

            ExerciseGameComponent[] exercises = _exerciseQueue.Exercises;
            foreach (ExerciseGameComponent exercise in exercises)
            {
                for (int i = 0; i < exercise.RepetitionToFileId.Count; i = i + 1)
                {
                    ReplayTile replayTile = new ReplayTile(
                            scrollableCollection.ItemSize,
                            scrollableCollection.GetNextPosition(),
                            exercise.RepetitionToFileId[i],
                            exercise.Name,
                            i
                        );

                    replayTile.OnSelected += ReplayTileSelected;

                    scrollableCollection.AddCatalogItem(replayTile);
                }
            }

            scrollableCollection.LoadContent(Game, contentManager, SharedSpriteBatch);

            ScreenState = UserInterface.ScreenState.Active;
            base.Transition();
        }
    }
}
