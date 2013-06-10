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
    public class ExerciseScreen : Screen
    {
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
        // queue of exercises
        // reference to the exercise queue service
        private ExerciseQueue ExerciseQueue
        {
            get
            {
                return (ExerciseQueue)Game.Services.GetService(typeof(ExerciseQueue));
            }
        }
        private ExerciseTile[] fakeExerciseQueue;
        #endregion

        private RecordingManager recordingManager
        {
            get
            {
                return (RecordingManager)this.Game.Services.GetService(typeof(RecordingManager));
            }
        }

        #region Button Variables
        private GuiButton[] buttonList;
        #endregion

        /// <summary>
        /// Initialize a new instance of the ExerciseScreen class.
        /// </summary>
        /// <param name="game">The related game object.</param>
        /// <param name="viewableArea">The desired canvas size to draw on.</param>
        /// <param name="startingState">The desired starting Screen State</param>
        public ExerciseScreen(Game game, Rectangle viewableArea, ScreenState startingState)
            : base(game)
        {
            this.ScreenState = startingState;
            this.viewableArea = viewableArea;
            this.colorStream = new ColorStreamRenderer(game);
            this.Title = "Exercise";

            // for now we'll generate hardcoded exercises....
            // note the location of this will need to change if we load these from an external file. 
            ExerciseGameComponent[] exercises = this.ExerciseQueue.Exercises;
            this.fakeExerciseQueue = new ExerciseTile[exercises.Length];
            for (int i = 0; i < exercises.Length; i++)
            {
                fakeExerciseQueue[i] = new ExerciseTile(game, exercises[i],i);
            }

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
                new GuiButton("The Hub", buttonSize, buttonPosition),
                new GuiButton("StartRec", buttonSize, new Vector2(buttonPosition.X, buttonPosition.Y + buttonSize.Y + MARGIN)),
                new GuiButton("StopRec", buttonSize, new Vector2(buttonPosition.X + buttonSize.X + MARGIN, buttonPosition.Y + buttonSize.Y + MARGIN)),
                new GuiButton("StartRep", buttonSize, new Vector2(buttonPosition.X, buttonPosition.Y + (2 * (buttonSize.Y + MARGIN)))),
                new GuiButton("StopRep", buttonSize, new Vector2(buttonPosition.X+ buttonSize.X + MARGIN, buttonPosition.Y + (2 * (buttonSize.Y + MARGIN)))),
            };
            #endregion
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            this.colorStream.Position = this.colorStreamPosition;
            this.colorStream.Size = this.colorStreamSize;
            this.colorStream.Initialize();

            foreach (ExerciseTile exerciseTile in this.fakeExerciseQueue)
            {
                exerciseTile.Initialize();
            }

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
            
            // load the exercise routine here
            //Exercise[] exercises = contentManager.Load<Exercise>("ArmExtensions");

            base.LoadContent();
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

                        if (mouseState.LeftButton == ButtonState.Pressed 
                            && mouseState.LeftButton != this.oldMouseState.LeftButton)
                        {
                            switch (button.Text)
                            {
                                case "The Hub":
                                    this.Transition();
                                    this.Manager.CallOpen("The Hub");
                                    break;
                                case "StartRec":
                                    recordingManager.StartRecording(KinectRecordOptions.Skeletons);
                                    break;
                                // TODO: move to stats or new "exercise replay" screen
                                // exercise replay screen should be able to traverse 
                                // a group of related recorded excercises
                                case "StartRep":
                                    recordingManager.StartReplaying(
                                        recordingManager.filesUsed.ElementAt(0).Key
                                    );
                                    break;
                                case "StopRec":
                                    recordingManager.StopRecording();
                                    break;
                                case "StopRep":
                                    recordingManager.StopReplaying();
                                    break;
                            }
                        }
                    }
                    else
                    {
                        button.Hovered = false;
                    }
                }

                this.oldMouseState = mouseState;

                this.colorStream.Update(gameTime);
                foreach (ExerciseTile exerciseTile in this.fakeExerciseQueue)
                {
                    exerciseTile.Update(gameTime);
                }
            }
            
            base.Update(gameTime);
        }

        /// <summary>
        /// This method renders the current state of the ExerciseScreen.
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
