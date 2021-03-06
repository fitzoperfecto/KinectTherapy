using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SWENG.Criteria;

namespace SWENG.UserInterface
{
    public class ExerciseTile : GuiDrawable
    {
        private const int HEADER = 40;
        private const int MARGIN = 10;
        private const int SCROLLRATE = 5;
        private const double MILLISECONDS = 100;

        private double _oldGameTime;

        private ContentManager _contentManager;
        private Texture2D _titleTexture;
        private Texture2D _checkpointTexture;
        private Rectangle _titleDestination;
        private Rectangle _titleSource;
        private Rectangle _checkpointDestination;

        public string Title { get; private set; }
        public string CheckpointId { get; private set; }
        public int ExerciseIndex { get; private set; }
        public bool IsCurrentTile { get; set; }

        public ExerciseTile(Game game, ExerciseGameComponent exercise, int exerciseIndex, Vector2 size, Vector2 position)
            : base(exercise.Name, size, position)
        {
            this.Title = exercise.Name;
            exercise.repetition.CheckpointChanged += HandleCheckpointChange;

            _oldGameTime = double.MaxValue;
        }

        public override void LoadContent(Game game, ContentManager contentManager, SpriteBatch spriteBatch)
        {
            if (contentManager == null)
            {
                return;
            }

            _contentManager = contentManager;
            Texture2D = _contentManager.Load<Texture2D>(@"UI\ExerciseTile");

            _checkpointDestination = new Rectangle(
                (int)Position.X + MARGIN,
                (int)Position.Y + HEADER,
                (int)Size.X - (2 * MARGIN),
                (int)Size.Y - HEADER - MARGIN
            );

            _titleTexture = CreateTitleTexture(game, _contentManager, spriteBatch);
            _titleDestination = new Rectangle(
                (int)Position.X + MARGIN,
                (int)Position.Y + MARGIN,
                (int)Size.X - (2 * MARGIN),
                HEADER
            );

            _titleSource = new Rectangle(
                0,
                0,
                _titleDestination.Width,
                _titleDestination.Height
            );
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

            _oldGameTime = currentGameTime;
        }

        /// <summary>
        /// This method renders the current state of the element to the screen.
        /// </summary>
        /// <param name="spriteBatch">A SpriteBatch that has begun.</param>
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

                if (_checkpointTexture != null)
                {
                    spriteBatch.Draw(
                        _checkpointTexture,
                        _checkpointDestination,
                        Color.White
                    );
                }
            }
        }

        private Texture2D CreateTitleTexture(Game game, ContentManager contentManager, SpriteBatch spriteBatch)
        {
            SpriteFont spriteFont = contentManager.Load<SpriteFont>("Arial16");
            Vector2 textMeasure = spriteFont.MeasureString(Text);
            RenderTarget2D renderTarget2d = new RenderTarget2D(game.GraphicsDevice, (int)textMeasure.X, (int)textMeasure.Y);

            game.GraphicsDevice.SetRenderTarget(renderTarget2d);
            game.GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 0, 0);

            spriteBatch.Begin();

            spriteBatch.DrawString(
                spriteFont,
                Text,
                Vector2.Zero,
                Color.White
            );

            spriteBatch.End();

            game.GraphicsDevice.SetRenderTarget(null);

            return (Texture2D)renderTarget2d;
        }

        /// <summary>
        /// Update the checkpoint texture and set it to be the current tile.
        /// </summary>
        public void HandleCheckpointChange(object sender, CheckpointChangedEventArgs args)
        {
            _checkpointTexture = _contentManager.Load<Texture2D>(@"Exercises\" + args.FileId);
            IsCurrentTile = true;
        }
    }
}
