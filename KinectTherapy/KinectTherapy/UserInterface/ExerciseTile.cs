using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SWENG.Service;
using SWENG.Criteria;
using System.Collections.Generic;

// TODO: refactor similar functionality within ReplayTile to a "tile" drawablegamecomponent
namespace SWENG.UserInterface
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class ExerciseTile : DrawableGameComponent
    {
        /// <summary>
        /// Gets the SpriteBatch from the services.
        /// </summary>
        private SpriteBatch SharedSpriteBatch
        {
            get
            {
                return (SpriteBatch)this.Game.Services.GetService(typeof(SpriteBatch));
            }
        }

        /// <summary>
        /// Gets the ExerciseQueue from the services.
        /// </summary>
        private ExerciseQueue SharedExerciseQueue
        {
            get
            {
                return (ExerciseQueue)Game.Services.GetService(typeof(ExerciseQueue));
            }
        }

        private Vector2 position;
        public Vector2 Position
        {
            get { return position; }
            set
            {
                position = value;
                if (null != size)
                {
                    Rectangle = new Rectangle(
                        (int)position.X,
                        (int)position.Y,
                        (int)size.X,
                        (int)size.Y
                    );
                }
            }
        }

        private Vector2 size;
        public Vector2 Size
        {
            get { return size; }
            set
            {
                size = value;
                if (null != position)
                {
                    Rectangle = new Rectangle(
                        (int)position.X,
                        (int)position.Y,
                        (int)size.X,
                        (int)size.Y
                    );
                }
            }
        }

        private Rectangle rectangle;
        public Rectangle Rectangle
        {
            get { return rectangle; }
            set
            {
                rectangle = value;
                position = new Vector2(value.X, value.Y);
                size = new Vector2(value.Width, value.Height);
            }
        }

        public bool Hovered { get; set; }

        private ContentManager contentManager;
        private Texture2D blankTexture;
        private SpriteFont spriteFont;
        private float TitleSize;
        private const int MARGIN = 3;
        private Vector2 drawableTitle;
        private Vector2 drawableSection;
        private const float SCROLL_RATE = -1f;
        private Rectangle header;
        private Rectangle body;
        private Rectangle border;

        private Texture2D titleTexture;
        private RenderTarget2D renderTarget2d;
        private Texture2D checkpointTexture;

        public string Title { get; internal set; }
        public int ExerciseIndex { get; internal set; }
        public string CheckpointId { get; internal set; }

        private string repetitionSentence;

        public ExerciseTile(Game game, string title)
            : base(game)
        {
            this.Title = title;
        }

        // TODO: With a reference to the exerciseIndex and the ExerciseQueue passing in 
        // the ExerciseGameComponent may be redundant.  Need to check performance measures
        public ExerciseTile(Game game, ExerciseGameComponent exercise, int exerciseIndex)
            : base(game)
        {
            this.Title = exercise.Name;
            this.ExerciseIndex = exerciseIndex;
            exercise.repetition.CheckpointChanged += HandleCheckpointChange;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// This method creates a new ContentManager 
        /// and loads the textures and fonts.
        /// </summary>
        protected override void LoadContent()
        {
            if (null == this.contentManager)
            {
                this.contentManager = new ContentManager(this.Game.Services, "Content");
            }

            this.spriteFont = contentManager.Load<SpriteFont>("Arial16");
            this.blankTexture = contentManager.Load<Texture2D>("blank");

            this.drawableTitle = spriteFont.MeasureString(Title);
            this.TitleSize = drawableTitle.Length();
            this.drawableSection = Vector2.Zero;

            this.header = new Rectangle(
                (int)this.Position.X, 
                (int)this.Position.Y, 
                (int)this.Size.X, 
                (int)(0.2 * this.Size.Y)
            );

            this.border = new Rectangle(
                (int)this.Position.X,
                (int)this.Position.Y + this.header.Height,
                (int)this.Size.X,
                (int)(0.8 * this.Size.Y)
            );

            this.body = new Rectangle(
                this.border.X + MARGIN,
                this.border.Y + MARGIN,
                this.border.Width - (2 * MARGIN),
                this.border.Height - (2 * MARGIN)
            );

            this.renderTarget2d = new RenderTarget2D(Game.GraphicsDevice, this.header.Width, this.header.Height);

            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            if (this.SharedExerciseQueue.CurrentExercise == this.SharedExerciseQueue.Exercises[ExerciseIndex])
            {
                this.repetitionSentence = string.Format(
                    "Reps: {1}\nStarted:{3}",
                    Title,
                    SharedExerciseQueue.Exercises[ExerciseIndex].Repetitions,
                    ExerciseIndex, SharedExerciseQueue.Exercises[ExerciseIndex].RepetitionStarted,CheckpointId
                );
            }
            else
            {
                this.repetitionSentence = null;
            }

            if (this.TitleSize > this.Size.X - (2 * MARGIN))
            {
                this.drawableSection.X = this.drawableSection.X + SCROLL_RATE;

                if (Math.Abs(this.drawableSection.X) > (this.Size.X + this.TitleSize - MARGIN))
                {
                    this.drawableSection.X = this.header.Width + MARGIN;
                }
            }

            this.titleTexture = CreateScrollingHeader();

            base.Update(gameTime);
        }

        /// <summary>
        /// This method renders the current state of the ExerciseTile.
        /// </summary>
        /// <param name="gameTime">The elapsed game time.</param>
        public override void Draw(GameTime gameTime)
        {
            this.SharedSpriteBatch.Begin();

            if (titleTexture != null)
            {
                this.SharedSpriteBatch.Draw(
                    this.titleTexture,
                    this.header,
                    Color.White
                );
            }

            if (blankTexture != null)
            {
                this.SharedSpriteBatch.Draw(
                    this.blankTexture,
                    this.border,
                    Color.DarkMagenta
                );

                this.SharedSpriteBatch.Draw(
                this.blankTexture,
                this.body,
                Color.White
                );
            }



            if (!string.IsNullOrEmpty(repetitionSentence))
            {
                this.SharedSpriteBatch.DrawString(
                    this.spriteFont,
                    this.repetitionSentence,
                    // stupid quick way of centering this for the meeting
                    new Vector2(
                        this.body.X + (this.body.Width / 4),
                        this.body.Y + (this.body.Height / 2) - 40
                    ),
                    Color.Blue
                );
            }

            if (checkpointTexture != null)
            {

                this.SharedSpriteBatch.Draw(
               this.checkpointTexture,
               new Vector2(
                        this.body.X + (this.body.Width / 4),
                        this.body.Y + (this.body.Height / 2) - 12
                    ),
                    Color.White
               );
            }

            this.SharedSpriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Create a texture with the text scrolled
        /// </summary>
        /// <returns>Updated texture</returns>
        private Texture2D CreateScrollingHeader()
        {
            Game.GraphicsDevice.SetRenderTarget(this.renderTarget2d);
            Game.GraphicsDevice.Clear(ClearOptions.Target, Color.DarkMagenta, 0, 0);
            this.SharedSpriteBatch.Begin();

            this.SharedSpriteBatch.Draw(
                this.blankTexture,
                new Rectangle(
                    MARGIN,
                    MARGIN,
                    this.header.Width - (2 * MARGIN),
                    this.header.Height - (2 * MARGIN)
                ),
                Color.DarkMagenta
            );

            this.SharedSpriteBatch.DrawString(
                this.spriteFont,
                this.Title,
                this.drawableSection,
                Color.White
            );

            this.SharedSpriteBatch.End();
            Game.GraphicsDevice.SetRenderTarget(null);

            return (Texture2D)renderTarget2d;
        }

        public void HandleCheckpointChange(object sender, CheckpointChangedEventArgs args)
        {
            // display the file. 
            this.checkpointTexture = contentManager.Load<Texture2D>("Exercises/"+args.FileId);
        }
    }
}
