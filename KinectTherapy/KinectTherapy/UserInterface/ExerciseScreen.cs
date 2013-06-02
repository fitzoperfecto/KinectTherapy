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

namespace SWENG.UserInterface
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class ExerciseScreen : Screen
    {
        
        private ContentManager contentManager;
        private Texture2D blankTexture;
        private SpriteFont spriteFont;
        private Rectangle viewableArea;
        private bool isInitialized = false;
        private const int MARGIN = 10;
        private MouseState oldMouseState;

        #region ColorStreamRenderer Variables
        private readonly ColorStreamRenderer colorStream;
        private Vector2 colorStreamPosition;
        private Vector2 colorStreamSize;
        #endregion

        #region ExerciseQueue Variables
        private ExerciseTile[] fakeExerciseQueue;
        #endregion

        #region Button Variables
        private GuiButton[] buttonList;
        #endregion

        public ExerciseScreen(Game game, Rectangle viewableArea, ScreenState startingState)
            : base(game)
        {
            this.ScreenState = startingState;
            this.viewableArea = viewableArea;
            this.colorStream = new ColorStreamRenderer(game);
            this.Title = "Exercise";

            this.fakeExerciseQueue = new ExerciseTile[] {
                new ExerciseTile(game, "A much much longer name than the rest"),
                new ExerciseTile(game, "Right Angle"),
                new ExerciseTile(game, "Toe Dip"),
                new ExerciseTile(game, "Push-ups"),
            };

            #region Laying out the positions
            this.colorStreamPosition = new Vector2(
                    (float)(viewableArea.X),
                    (float)(viewableArea.Y)
                );

            this.colorStreamSize = new Vector2(
                    (float)(0.7 * viewableArea.Width),
                    (float)(0.7 * viewableArea.Height)
                );

            Vector2 tileStartingPosition = new Vector2(
                    (float)(this.colorStreamPosition.X + this.colorStreamSize.X + (MARGIN * 2)),
                    (float)(this.colorStreamPosition.Y)
                );

            Vector2 tileSize = new Vector2(
                    (float)(0.25 * viewableArea.Width),
                    (float)(0.25 * viewableArea.Height)
                );

            foreach (ExerciseTile exerciseTile in this.fakeExerciseQueue)
            {
                exerciseTile.Position = tileStartingPosition;
                exerciseTile.Size = tileSize;

                // bump the next tile down by the size of the tile and a y margin
                tileStartingPosition = new Vector2(
                    tileStartingPosition.X,
                    tileStartingPosition.Y + tileSize.Y + MARGIN
                );
            }

            Vector2 buttonSize = new Vector2(100, 30f);
            Vector2 buttonPosition = new Vector2(
                (
                    (this.colorStreamPosition.X + // get the far left position
                    (this.colorStreamSize.X / 2)) - // add half of the width of the stream
                    (buttonSize.X / 2) // and then get rid of half the button width... now we are centered
                ),
                (
                    (this.colorStreamPosition.Y + this.colorStreamSize.Y) + // get the bottom of the stream
                    ((
                        (viewableArea.Height) - // get the entire viewable area 
                        (this.colorStreamPosition.Y + this.colorStreamSize.Y) // remove what the stream has taken
                    ) / 2) - // get the center of the remaining space
                    (buttonSize.Y / 2) // and then get rid of half of the button height... again, centered
                )
            );

            this.buttonList = new GuiButton[] {
                new GuiButton("The Hub", buttonSize, buttonPosition)
            };
            #endregion
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // make sure there is a margin between the top header and left side of the screen
            this.colorStream.Position = this.colorStreamPosition;
            // the color stream should only be half the viewable area to keep room for more information
            this.colorStream.Size = this.colorStreamSize;
            this.colorStream.Initialize();

            foreach (ExerciseTile exerciseTile in this.fakeExerciseQueue)
            {
                exerciseTile.Initialize();
            }

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
            if (isInitialized)
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
                            this.Manager.CallOpen("The Hub");
                        }
                    }
                    else
                    {
                        button.Hovered = false;
                    }
                }

                oldMouseState = mouseState;

                this.colorStream.Update(gameTime);
                foreach (ExerciseTile exerciseTile in this.fakeExerciseQueue)
                {
                    exerciseTile.Update(gameTime);
                }
            }
            
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (isInitialized)
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
                this.colorStream.Draw(gameTime);
                foreach (ExerciseTile exerciseTile in this.fakeExerciseQueue)
                {
                    exerciseTile.Draw(gameTime);
                }
            }

            base.Draw(gameTime);
        }
    }
}
