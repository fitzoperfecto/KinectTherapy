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


namespace SWENG.UserInterface
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class HomeScreen : Screen
    {
        private ContentManager contentManager;
        private Texture2D blankTexture;
        private SpriteFont spriteFont;
        private Rectangle viewableArea;
        private GuiButton[] buttonList;
        private bool isInitialized;
        private MouseState oldMouseState;

        public HomeScreen(Game game, Rectangle viewableArea, ScreenState startingState)
            : base(game)
        {
            this.ScreenState = startingState;
            this.viewableArea = viewableArea;

            this.Title = "The Hub";

            Vector2 buttonSize = new Vector2(100, 30f);
            Vector2 buttonPosition = new Vector2(
                (viewableArea.Width / 2) - (buttonSize.X / 2),
                (viewableArea.Height / 2) - (buttonSize.Y / 2)
            );

            this.buttonList = new GuiButton[]{
                new GuiButton("Exercise", buttonSize, buttonPosition)
            };

            this.isInitialized = false;
            // TODO: Construct any child components here
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            isInitialized = true;

            base.Initialize();
        }

        public override void LoadContent()
        {
            if (null == contentManager)
            {
                contentManager = new ContentManager(this.Game.Services, "Content");
            }

            spriteFont = contentManager.Load<SpriteFont>("Arial16");
            blankTexture = contentManager.Load<Texture2D>("blank");

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
            MouseState mouseState = Mouse.GetState();
            Rectangle mouseBoundingBox = new Rectangle(mouseState.X, mouseState.Y, 1, 1);

            foreach (GuiButton button in buttonList)
            {
                if (mouseBoundingBox.Intersects(button.Rectangle))
                {
                    button.Hovered = true;

                    if (mouseState.LeftButton == ButtonState.Pressed && mouseState.LeftButton != oldMouseState.LeftButton)
                    {
                        this.Transition();
                        this.Manager.CallOpen("Exercise");
                    }
                }
                else
                {
                    button.Hovered = false;
                }
            }

            oldMouseState = mouseState;
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (this.isInitialized)
            {
                this.GraphicsDevice.Clear(Color.White);
                this.SharedSpriteBatch.Begin();

                foreach (GuiButton button in buttonList)
                {
                    if (!button.Hovered)
                    {
                        this.SharedSpriteBatch.Draw(
                            this.blankTexture,
                            button.Rectangle,
                            Color.Magenta
                        );
                    }
                    else
                    {
                        this.SharedSpriteBatch.Draw(
                            this.blankTexture,
                            button.Rectangle,
                            Color.DarkMagenta
                        );
                    }

                    this.SharedSpriteBatch.DrawString(
                        this.spriteFont,
                        button.Text,
                        button.Position,
                        Color.White
                    );
                }

                this.SharedSpriteBatch.End();
            }
            base.Draw(gameTime);
        }
    }
}
