using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SWENG.UserInterface
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class HomeScreen : Screen
    {
        private Texture2D blankTexture;
        private SpriteFont spriteFont;
        private Rectangle viewableArea;
        private GuiButton[] buttonList;
        private bool isInitialized;
        private MouseState oldMouseState;

        /// <summary>
        /// Initialize a new instance of the HomeScreen class.
        /// </summary>
        /// <param name="game">The related game object.</param>
        /// <param name="viewableArea">The desired canvas size to draw on.</param>
        /// <param name="startingState">The desired starting Screen State</param>
        public HomeScreen(Game game, Rectangle viewableArea, ScreenState startingState)
            : base(game)
        {
            this.ScreenState = startingState;
            this.viewableArea = viewableArea;

            this.Title = "The Hub";

            #region Laying out the positions
            // Keep the buttons the same size
            Vector2 buttonSize = new Vector2(100, 30f);
            
            // Starting button position
            Vector2 buttonPosition = new Vector2(
                (viewableArea.Width / 2) - (buttonSize.X / 2),
                (viewableArea.Height / 2) - (buttonSize.Y / 2)
            );

            this.buttonList = new GuiButton[]{
                new GuiButton("Exercise", buttonSize, buttonPosition)
            };
            #endregion

            this.isInitialized = false;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            this.isInitialized = true;

            base.Initialize();
        }

        /// <summary>
        /// This method creates a new ContentManager 
        /// and loads the textures and fonts.
        /// </summary>
        public override void LoadContent()
        {
            if (null == this.contentManager)
            {
                this.contentManager = new ContentManager(this.Game.Services, "Content");
            }

            this.spriteFont = this.contentManager.Load<SpriteFont>("Arial16");
            this.blankTexture = this.contentManager.Load<Texture2D>("blank");

            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            Rectangle mouseBoundingBox = new Rectangle(mouseState.X, mouseState.Y, 1, 1);

            foreach (GuiButton button in this.buttonList)
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

            this.oldMouseState = mouseState;
            
            base.Update(gameTime);
        }

        /// <summary>
        /// This method renders the current state of the HomeScreen.
        /// </summary>
        /// <param name="gameTime">The elapsed game time.</param>
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
