using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace SWENG.UserInterface
{
    public class HomeScreen : Screen
    {
        private readonly Rectangle _viewableArea;
        private readonly GuiDrawable[] _guiDrawable;

        private const float MARGIN = 10f;

        private bool _isInitialized;

        private MouseState _oldMouseState;

        /// <summary>
        /// Initialize a new instance of the ExerciseScreen class.
        /// </summary>
        /// <param name="game">The related game object.</param>
        /// <param name="viewableArea">The desired canvas size to draw on.</param>
        /// <param name="startingState">The desired starting Screen State</param>
        public HomeScreen(Game game, Rectangle viewableArea, ScreenState startingState)
            : base(game)
        {
            ScreenState = startingState;
            _viewableArea = viewableArea;

            Title = "Home";

            Vector2 buttonSize = new Vector2(240f, 60f);
            Vector2 buttonBottom = new Vector2(
                _viewableArea.Right - buttonSize.X + MARGIN,
                _viewableArea.Bottom - buttonSize.Y);

            #region Laying out the positions
            Dictionary<string, GuiDrawable> _buttonDct = new Dictionary<string,GuiDrawable>();
            _buttonDct.Add(
                "LogIn",
                new GuiButton(
                    "LogIn", 
                    buttonSize,
                    buttonBottom
                    - (new Vector2(0f, 2 * MARGIN))
                    - (new Vector2(0f, 2 * buttonSize.Y))
                )
            );
            _buttonDct.Add(
                "SensorSetup",
                new GuiButton(
                    "SensorSetup", 
                    buttonSize,
                    buttonBottom
                    - new Vector2(0f, MARGIN)
                    - new Vector2(0f, buttonSize.Y)
                )
            );
            _buttonDct.Add(
                "ExitProgram",
                new GuiButton(
                    "ExitProgram", 
                    buttonSize, 
                    buttonBottom
                )
            );

            _buttonDct.Add(
                "SensorStatus",
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

            _buttonDct.Add(
                "KinectTherapy",
                new GuiHeader(
                    "KinectTherapy",
                        new Vector2(326f, 52f),
                        new Vector2(
                            _viewableArea.Left,
                            _viewableArea.Top - MARGIN - 52f
                    )
                )
            );
            #endregion

            _guiDrawable = new GuiDrawable[_buttonDct.Count];
            _buttonDct.Values.CopyTo(_guiDrawable, 0);

            _isInitialized = false;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            _isInitialized = true;

            foreach (GuiDrawable guiDrawable in _guiDrawable)
            {
                if (guiDrawable.GetType() == typeof(GuiButton))
                {
                    ((GuiButton)guiDrawable).ClickEvent += GuiButtonWasClicked;
                }
            }

            base.Initialize();
        }

        public override void LoadContent()
        {
            if (contentManager == null)
            {
                contentManager = new ContentManager(Game.Services, "Content");
            }

            foreach (GuiDrawable guiDrawable in _guiDrawable)
            {
                guiDrawable.LoadContent(Game, contentManager, SharedSpriteBatch);
            }

            base.LoadContent();
        }

        public override void UnloadContent()
        {
            contentManager.Unload();
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

                foreach (GuiDrawable guiDrawable in _guiDrawable)
                {
                    guiDrawable.Update(currentState, _oldMouseState, mouseBoundingBox, gameTime);
                }

                _oldMouseState = currentState;
            }
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (_isInitialized)
            {
                GraphicsDevice.Clear(Color.WhiteSmoke);
                var spriteBatch = SharedSpriteBatch;
                spriteBatch.Begin();

                foreach (GuiDrawable guiDrawable in _guiDrawable)
                {
                    guiDrawable.Draw(spriteBatch);
                }

                spriteBatch.End();
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
                case "LogIn":
                    ScreenState = UserInterface.ScreenState.Hidden;
                    OnTransition(new TransitionEventArgs(Title, e.ClickedOn));
                    break;
                case "SensorSetup":
                    ScreenState = UserInterface.ScreenState.Active | UserInterface.ScreenState.NonInteractive;
                    OnTransition(new TransitionEventArgs(Title, e.ClickedOn));
                    break;
                case "ExitProgram":
                    OnTransition(new TransitionEventArgs(Title, e.ClickedOn));
                    break;
            }
        }
    }
}
