using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;

namespace SWENG.UserInterface
{
    class LogInScreen : Screen
    {
        private readonly Rectangle _viewableArea;
        private readonly GuiDrawable[] _guiDrawable;

        private const float MARGIN = 10f;

        private bool _isInitialized;

        // fake log in box
        private Texture2D _logInTexture;
        private Vector2 _logInPosition;
        private Vector2 _logInSize;
        private MouseState _oldMouseState;

        public LogInScreen(Game game, Rectangle viewableArea, ScreenState startingState) : base(game)
        {
            ScreenState = startingState;
            _viewableArea = viewableArea;

            Title = "Log In";

            Vector2 buttonSize = new Vector2(240f, 60f);
            Vector2 buttonBottom = new Vector2(
                _viewableArea.Right - buttonSize.X + MARGIN,
                _viewableArea.Bottom - buttonSize.Y
            );

            #region Laying out the positions
            _guiDrawable = new GuiDrawable[0];
            List<GuiDrawable> guiDrawableList = new List<GuiDrawable>();
            
            guiDrawableList.Add(
                new GuiButton("Submit", buttonSize, 
                    buttonBottom 
                    - new Vector2(0f , MARGIN) 
                    - new Vector2(0f, buttonSize.Y)
                ));

            guiDrawableList.Add(new GuiButton("ExitProgram", buttonSize, buttonBottom));

            guiDrawableList.Add(
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

            guiDrawableList.Add(
                new GuiHeader(
                    "KinectTherapy",
                    new Vector2(326f, 52f),
                    new Vector2(
                        _viewableArea.Left,
                        _viewableArea.Top - MARGIN - 52f
                    )
                )
            );

            _logInSize = new Vector2(410f, 195f);
            _logInPosition = new Vector2(
                    (_viewableArea.Right / 2) - (_logInSize.X / 2),
                    (_viewableArea.Bottom / 2) - (_logInSize.Y / 2)
                );

            #endregion
            _guiDrawable = guiDrawableList.ToArray();
            guiDrawableList.Clear();

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

        private void GuiButtonWasClicked(object sender, GuiButtonClickedArgs e)
        {
            switch (e.ClickedOn)
            {
                case "Submit":
                    ScreenState = UserInterface.ScreenState.Hidden;
                    OnTransition(new TransitionEventArgs(Title, e.ClickedOn));
                    break;
                case "ExitProgram":
                    OnTransition(new TransitionEventArgs(Title, e.ClickedOn));
                    break;
            }
        }

        public override void LoadContent()
        {
            if (null == contentManager)
            {
                contentManager = new ContentManager(Game.Services, "Content");
            }

            foreach (GuiDrawable guiDrawable in _guiDrawable)
            {
                guiDrawable.LoadContent(contentManager);
            }

            _logInTexture = contentManager.Load<Texture2D>(@"UI\UserPassword");

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

                spriteBatch.Draw(
                    _logInTexture,
                    _logInPosition,
                    Color.White
                );

                spriteBatch.End();
            }
            base.Draw(gameTime);
        }
    }
}
