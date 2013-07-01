using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SWENG.UserInterface
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class LoadingScreen : Screen
    {
        private readonly Rectangle _viewableArea;
        private const float MARGIN = 10f;

        private bool _isInitialized;
        private Texture2D _blankTexture;
        private SpriteFont _spriteFont;

        public LoadingScreen(Game game, Rectangle viewableArea, ScreenState startingState)
            : base(game)
        {
            ScreenState = startingState;
            _viewableArea = viewableArea;

            Title = "Loading";

            _isInitialized = false;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            _isInitialized = true;

            base.Initialize();
        }

        public override void LoadContent()
        {
            if (null == contentManager)
            {
                contentManager = new ContentManager(Game.Services, "Content");
            }

            _blankTexture = contentManager.Load<Texture2D>(@"blank");
            _spriteFont = contentManager.Load<SpriteFont>(@"Arial16");

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
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (_isInitialized)
            {
                var spriteBatch = SharedSpriteBatch;
                spriteBatch.GraphicsDevice.Clear(Color.Black);
                spriteBatch.Begin();

                spriteBatch.Draw(
                    _blankTexture,
                    _viewableArea,
                    Color.White
                );

                spriteBatch.DrawString(
                    _spriteFont,
                    "Loading...",
                    Vector2.Zero,
                    Color.White
                );

                spriteBatch.End();
            }
            base.Draw(gameTime);
        }

        public void CloseScreen(object sender, EventArgs e)
        {
            ScreenState = UserInterface.ScreenState.Hidden;
            OnTransition(new TransitionEventArgs(Title, "LoadingIsDone"));
        }
    }
}
