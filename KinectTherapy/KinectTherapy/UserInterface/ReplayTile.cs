using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SWENG.Service;

namespace SWENG.UserInterface
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class ReplayTile : DrawableGameComponent
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

        private Vector2 _position;
        public Vector2 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                if (null != _size)
                {
                    UpdateSizes();
                    Rectangle = new Rectangle(
                        (int)_position.X,
                        (int)_position.Y,
                        (int)_size.X,
                        (int)_size.Y
                    );
                }
            }
        }

        public Vector2 ToPosition { get; set; }

        private Vector2 _size;
        public Vector2 Size
        {
            get { return _size; }
            set
            {
                _size = value;
                if (null != _position)
                {
                    UpdateSizes();
                    Rectangle = new Rectangle(
                        (int)_position.X,
                        (int)_position.Y,
                        (int)_size.X,
                        (int)_size.Y
                    );
                }
            }
        }

        private Rectangle _rectangle;
        public Rectangle Rectangle
        {
            get { return _rectangle; }
            set
            {
                _rectangle = value;
                _position = new Vector2(value.X, value.Y);
                _size = new Vector2(value.Width, value.Height);
                UpdateSizes();
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

        public string Title { get; internal set; }
        public int ExerciseIndex { get; internal set; }

        private string repetitionSentence;
        private int _repetitionNumber;

        public string FileId { get; private set; }

        public ReplayTile(Game game, string title)
            : base(game)
        {
            this.Title = title;
        }

        // TODO: With a reference to the exerciseIndex and the ExerciseQueue passing in 
        // the ExerciseGameComponent may be redundant.  Need to check performance measures
        public ReplayTile(Game game, string fileId, string exerciseName, int repetitionNumber)
            : base(game)
        {
            Title = string.Format("{0}: {1}",
                repetitionNumber,
                exerciseName
            );
            FileId = fileId;
            _repetitionNumber = repetitionNumber;
            this.repetitionSentence = string.Format(
                "Rep: {0}",
                _repetitionNumber
            );
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

            UpdateSizes();

            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
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

            this.SharedSpriteBatch.Draw(
                this.titleTexture,
                this.header,
                Color.White
            );

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

            this.SharedSpriteBatch.DrawString(
                this.spriteFont, 
                this.repetitionSentence,
                // stupid quick way of centering this for the meeting
                new Vector2(
                    this.body.X + (this.body.Width / 4), 
                    this.body.Y + (this.body.Height / 2) - 12 
                ), 
                Color.Blue
            );

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

        private void UpdateSizes()
        {
            header = new Rectangle(
                (int)Position.X,
                (int)Position.Y,
                (int)Size.X,
                (int)(0.2 * Size.Y)
            );

            border = new Rectangle(
                (int)Position.X,
                (int)Position.Y + header.Height,
                (int)Size.X,
                (int)(0.8 * Size.Y)
            );

            body = new Rectangle(
                border.X + MARGIN,
                border.Y + MARGIN,
                border.Width - (2 * MARGIN),
                border.Height - (2 * MARGIN)
            );

            if (header.Width != 0 && header.Height != 0)
                renderTarget2d = new RenderTarget2D(Game.GraphicsDevice, header.Width, header.Height);
        }
    }
}
