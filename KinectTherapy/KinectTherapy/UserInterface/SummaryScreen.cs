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
using Microsoft.Kinect;
using Microsoft.Samples.Kinect.XnaBasics;
using System.Diagnostics;
using SWENG.Service;
using Kinect.Toolbox.Record;

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
        private bool _isInitialized = false;
        private const int _MARGIN = 10;
        private MouseState _oldMouseState;
        private string _fileId;
        private const float _ANIMATION_RATE = 0.25f;
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
        private GuiButton[] _buttonList;
        private Vector2 _startingPosition;
        private Vector2 _endingPosition;
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
            _buttonList = new GuiButton[0];

            _colorStreamPosition = new Vector2(
                    (float)(viewableArea.X),
                    (float)(viewableArea.Y)
                );

            _colorStreamMaxSize = new Vector2(
                    (float)(0.7 * viewableArea.Width),
                    (float)(0.7 * viewableArea.Height)
                );

            _colorStreamSize = Vector2.Zero;
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

            foreach (ReplayTile replayTile in _replayTiles)
            {
                replayTile.Initialize();
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
            if (null == contentManager)
            {
                contentManager = new ContentManager(Game.Services, "Content");
            }

            _spriteFont = contentManager.Load<SpriteFont>("Arial16");
            _blankTexture = contentManager.Load<Texture2D>("blank");

            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            if (_isInitialized)
            {
                MouseState mouseState = Mouse.GetState();
                Rectangle mouseBoundingBox = new Rectangle(mouseState.X, mouseState.Y, 1, 1);

                foreach (GuiButton button in _buttonList)
                {
                    if (mouseBoundingBox.Intersects(button.Rectangle))
                    {
                        button.Hovered = true;

                        if (mouseState.LeftButton == ButtonState.Pressed 
                            && mouseState.LeftButton != _oldMouseState.LeftButton)
                        {
                            switch (button.Text)
                            {
                                case "The Hub":
                                    Transition();
                                    Manager.CallOpen("The Hub");
                                    break;
                                case "Replay":
                                    _recordingManager.RestartReplay();
                                    break;
                                case "Stop":
                                    closeReplay();
                                    break;
                            }
                        }
                    }
                    else
                    {
                        button.Hovered = false;
                    }
                }

                foreach (ReplayTile replayTile in _replayTiles)
                {
                    if (mouseBoundingBox.Intersects(replayTile.Rectangle))
                    {
                        replayTile.Hovered = true;
                        if (mouseState.LeftButton == ButtonState.Pressed
                            && mouseState.LeftButton != _oldMouseState.LeftButton)
                        {
                            openReplay(replayTile.FileId);
                        }
                    }
                    else
                    {
                        replayTile.Hovered = false;
                    }

                    replayTile.Position = Vector2.SmoothStep(
                            replayTile.Position,
                            replayTile.ToPosition,
                            _ANIMATION_RATE
                        );

                    replayTile.Update(gameTime);
                }

                _oldMouseState = mouseState;

                if (Math.Ceiling(_colorStream.Size.X) == _colorStreamSize.X && !string.IsNullOrEmpty(_fileId))
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

                // adjust replay tile positions (and size possibly)
                foreach (ReplayTile replayTile in _replayTiles)
                {
                    replayTile.ToPosition = new Vector2(
                        _endingPosition.X,
                        replayTile.Position.Y
                    );
                    /*
                    replayTile.ToPosition = new Vector2(
                        replayTile.Position.X + _colorStreamMaxSize.X + _MARGIN,
                        replayTile.Position.Y
                    );
                    */
                }

                Vector2 buttonSize = new Vector2(100, 30f);
                Vector2 buttonPosition = new Vector2(
                    (
                        (_colorStreamPosition.X + // get the far left position
                        (_colorStreamSize.X / 2)) - // add half of the width of the stream
                        (buttonSize.X / 2) // and then get rid of half the button width... now we are centered
                    ),
                    (
                        (_colorStreamPosition.Y + _colorStreamSize.Y) + // get the bottom of the stream
                        ((
                            (_viewableArea.Height) - // get the entire viewable area 
                            (_colorStreamPosition.Y + _colorStreamSize.Y) // remove what the stream has taken
                        ) / 2) - // get the center of the remaining space
                        (buttonSize.Y / 2) // and then get rid of half of the button height... again, centered
                    )
                );

                _buttonList = new GuiButton[] {
                    new GuiButton("The Hub", buttonSize, buttonPosition),
                    new GuiButton("Replay", buttonSize, new Vector2(buttonPosition.X, buttonPosition.Y + buttonSize.Y + _MARGIN)),
                    new GuiButton("Stop", buttonSize, new Vector2(buttonPosition.X + buttonSize.X + _MARGIN, buttonPosition.Y + buttonSize.Y + _MARGIN)),
                };
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

            // adjust replay tile positions (and size possibly)
            foreach (ReplayTile replayTile in _replayTiles)
            {
                replayTile.ToPosition = new Vector2(
                    _startingPosition.X,
                    replayTile.Position.Y
                );
                /*
                replayTile.ToPosition = new Vector2(
                    replayTile.Position.X - (_colorStreamMaxSize.X + _MARGIN),
                    replayTile.Position.Y
                );
                */
            }

            _buttonList = new GuiButton[0];
        }

        /// <summary>
        /// This method renders the current state of the ExerciseScreen.
        /// </summary>
        /// <param name="gameTime">The elapsed game time.</param>
        public override void Draw(GameTime gameTime)
        {
            if (_isInitialized)
            {
                GraphicsDevice.Clear(Color.White);
                SharedSpriteBatch.Begin();

                foreach (GuiButton button in _buttonList)
                {
                    if (!button.Hovered)
                    {
                        SharedSpriteBatch.Draw(
                            _blankTexture,
                            button.Rectangle,
                            Color.Magenta
                        );
                    }
                    else
                    {
                        SharedSpriteBatch.Draw(
                            _blankTexture,
                            button.Rectangle,
                            Color.DarkMagenta
                        );
                    }

                    SharedSpriteBatch.DrawString(
                        _spriteFont,
                        button.Text,
                        button.Position,
                        Color.White
                    );
                }

                SharedSpriteBatch.End();
                _colorStream.Draw(gameTime);
                foreach (ReplayTile replayTile in _replayTiles)
                {
                    replayTile.Draw(gameTime);
                }
            }

            base.Draw(gameTime);
        }

        /// <summary>
        /// The method to use when the summary screen
        /// should be triggered to open when an event occurs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void QueueIsDone(object sender, EventArgs args)
        {
            _startingPosition = new Vector2(
                (float)(_colorStreamPosition.X),
                (float)(_colorStreamPosition.Y)
            );

            _endingPosition = new Vector2(
                (float)(_colorStreamPosition.X + _colorStreamMaxSize.X + _MARGIN),
                (float)(_colorStreamPosition.Y)
            );

            Vector2 tileStartingPosition = new Vector2(
                (float)_colorStreamPosition.X,
                //(float)(_colorStreamPosition.X + _colorStreamSize.X + (_MARGIN * 2)),
                (float)(_colorStreamPosition.Y)
            );

            Vector2 tileSize = new Vector2(
                (float)(0.25 * _viewableArea.Width),
                (float)(0.25 * _viewableArea.Height)
            );
            
            ExerciseGameComponent[] exercises = _exerciseQueue.Exercises;
            List<ReplayTile> replayTilesList = new List<ReplayTile>();
            _replayTiles = new ReplayTile[exercises.Length];

            foreach (ExerciseGameComponent exercise in exercises)
            {
                for (int i = 0; i < exercise.RepetitionToFileId.Count; i++)
                {
                    replayTilesList.Add(
                        new ReplayTile(
                            Game,
                            exercise.RepetitionToFileId[i],
                            exercise.Name,
                            i
                        )
                    );

                    replayTilesList[i].Position = tileStartingPosition;
                    // initial position is the position we want, otherwise
                    // it will immediately start shrinking
                    replayTilesList[i].ToPosition = tileStartingPosition;
                    replayTilesList[i].Size = tileSize;
                    replayTilesList[i].Initialize();

                    // bump the next tile down by the size of the tile and a y margin
                    tileStartingPosition = new Vector2(
                        tileStartingPosition.X,
                        tileStartingPosition.Y + tileSize.Y + _MARGIN
                    );
                }
            }

            _replayTiles = replayTilesList.ToArray();

            base.Transition();
        }
    }
}
