using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SWENG.Service;
using Microsoft.Xna.Framework.Input;

namespace SWENG.UserInterface
{
    public delegate void ReplaySelected(object sender, ReplaySelectedEventArgs e);

    public class ReplaySelectedEventArgs : EventArgs
    {
        public string ID;

        public ReplaySelectedEventArgs(string id)
        {
            ID = id;
        }
    }

    public class ReplayTile : GuiDrawable
    {
        #region event stuff
        public event ReplaySelected OnSelected;

        // Invoke the Changed event; called whenever repetitions changes
        protected virtual void ReplaySelected(ReplaySelectedEventArgs e)
        {
            if (OnSelected != null)
                OnSelected(this, e);
        }
        #endregion

        private ContentManager _contentManager;

        private Texture2D _titleTexture;
        private Rectangle _titleDestination;
        private Rectangle _titleSource;

        private const int HEADER = 40;

        private const int SCROLLRATE = 5;
        private const double MILLISECONDS = 100;
        private const int MARGIN = 10;
        private double _oldGameTime;

        public string Title { get; internal set; }
        public int ExerciseIndex { get; internal set; }

        private string _repetitionSentence;
        private int _repetitionNumber;

        public string FileId { get; private set; }


        // TODO: With a reference to the exerciseIndex and the ExerciseQueue passing in 
        // the ExerciseGameComponent may be redundant.  Need to check performance measures
        public ReplayTile(Vector2 size, Vector2 position, string fileId, string exerciseName, int repetitionNumber)
            : base(exerciseName, size, position)
        {
            Title = string.Format("{0}: {1}",
                repetitionNumber,
                exerciseName
            );
            FileId = fileId;
            _repetitionNumber = repetitionNumber;
            _repetitionSentence = string.Format(
                "Rep: {0}",
                _repetitionNumber
            );
        }

        public override void LoadContent(ContentManager contentManager) { }

        public override void LoadContent(Game game, ContentManager contentManager, SpriteBatch spriteBatch)
        {
            if (null == contentManager)
            {
                return;
            }

            _contentManager = contentManager;
            Texture2D = contentManager.Load<Texture2D>(@"UI\ReplayTile");

            _titleTexture = CreateTitleTexture(game, _contentManager, spriteBatch);
            _titleDestination = new Rectangle(
                (int)Position.X + MARGIN,
                (int)Position.Y + MARGIN,
                _titleTexture.Width,
                _titleTexture.Height
            );

            _titleSource = new Rectangle(
                0,
                0,
                _titleTexture.Width,
                _titleTexture.Height
            );
        }

        private Texture2D CreateTitleTexture(Game game, ContentManager contentManager, SpriteBatch spriteBatch)
        {
            SpriteFont spriteFont = contentManager.Load<SpriteFont>("Arial16");
            Vector2 textMeasure = spriteFont.MeasureString(Title);
            RenderTarget2D renderTarget2d = new RenderTarget2D(game.GraphicsDevice, (int)textMeasure.X, (int)textMeasure.Y);

            game.GraphicsDevice.SetRenderTarget(renderTarget2d);
            game.GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 0, 0);

            spriteBatch.Begin();

            spriteBatch.DrawString(
                spriteFont,
                Title,
                Vector2.Zero,
                Color.White
            );

            spriteBatch.End();

            game.GraphicsDevice.SetRenderTarget(null);

            return (Texture2D)renderTarget2d;
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(MouseState currentState, MouseState oldMouseState, Rectangle mouseBoundingBox, GameTime gameTime)
        {
            double currentGameTime = gameTime.TotalGameTime.TotalMilliseconds;

            if (currentGameTime - _oldGameTime >= MILLISECONDS)
            {
                if (_titleSource.Width >= Texture2D.Width - (2 * MARGIN))
                {
                    _titleSource.X += SCROLLRATE;

                    if (_titleSource.X > _titleTexture.Width)
                    {
                        _titleSource.X = 0;
                    }
                }
            }

            if (mouseBoundingBox.Intersects(Rectangle))
            {
                if (currentState.LeftButton == ButtonState.Released
                    && oldMouseState.LeftButton == ButtonState.Pressed)
                {
                    ReplaySelected(new ReplaySelectedEventArgs(FileId));
                }
            }

            _oldGameTime = currentGameTime;
        }

        /// <summary>
        /// This method renders the current state of the ExerciseTile.
        /// </summary>
        /// <param name="gameTime">The elapsed game time.</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_titleTexture != null)
            {
                spriteBatch.Draw(
                    Texture2D,
                    Position,
                    Color.White
                );

                spriteBatch.Draw(
                    _titleTexture,
                    _titleDestination,
                    _titleSource,
                    Color.White
                );
            }
        }
    }
}
