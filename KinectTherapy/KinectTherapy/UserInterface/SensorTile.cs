using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SWENG.UserInterface
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class SensorTile : DrawableGameComponent
    {
        /// <summary>
        /// Gets the SpriteBatch from the services.
        /// </summary>
        private SpriteBatch SharedSpriteBatch
        {
            get
            {
                return (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));
            }
        }

        private ContentManager _contentManager;
        private Texture2D _blankTexture;
        private SpriteFont _spriteFont;
        private float _titleSize;
        private const int MARGIN = 3;
        private Vector2 _drawableTitle;
        private Vector2 _drawableSection;
        private const float ScrollRate = -1f;

        private Rectangle _header;
        private Rectangle _body;
        private Rectangle _border;

        private Texture2D _titleTexture;
        private RenderTarget2D _renderTarget2D;

        public string Title { get; internal set; }

        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }

        public SensorTile(Game game, string title) : base(game)
        {
            Title = title;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            if (null == _contentManager)
            {
                _contentManager = new ContentManager(Game.Services, "Content");
            }

            _spriteFont = _contentManager.Load<SpriteFont>("Arial16");
            _blankTexture = _contentManager.Load<Texture2D>("blank");

            _drawableTitle = _spriteFont.MeasureString(Title);
            _titleSize = _drawableTitle.Length();
            _drawableSection = Vector2.Zero;

            _header = new Rectangle(
                (int)Position.X, 
                (int)Position.Y, 
                (int)Size.X, 
                (int)(0.2 * Size.Y)
            );

            _border = new Rectangle(
                (int)Position.X,
                (int)Position.Y + _header.Height,
                (int)Size.X,
                (int)(0.8 * Size.Y)
            );

            _body = new Rectangle(
                _border.X + MARGIN,
                _border.Y + MARGIN,
                _border.Width - (2 * MARGIN),
                _border.Height - (2 * MARGIN)
            );

            _renderTarget2D = new RenderTarget2D(Game.GraphicsDevice, _header.Width, _header.Height);

            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            if (_titleSize > Size.X - (2 * MARGIN))
            {
                _drawableSection.X = _drawableSection.X + ScrollRate;

                if (Math.Abs(_drawableSection.X) > (Size.X + _titleSize - MARGIN))
                {
                    _drawableSection.X = _header.Width + MARGIN;
                }
            }

            Game.GraphicsDevice.SetRenderTarget(_renderTarget2D);
            Game.GraphicsDevice.Clear(ClearOptions.Target, Color.DarkBlue, 0, 0);
            SharedSpriteBatch.Begin();

            SharedSpriteBatch.Draw(
                _blankTexture,
                new Rectangle(
                    MARGIN,
                    MARGIN,
                    _header.Width - (2 * MARGIN),
                    _header.Height - (2 * MARGIN)
                ),
                Color.DarkBlue
            );

            SharedSpriteBatch.DrawString(
                _spriteFont,
                Title,
                _drawableSection,
                Color.White
            );

            SharedSpriteBatch.End();
            Game.GraphicsDevice.SetRenderTarget(null);

            _titleTexture = _renderTarget2D;

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (null != _titleTexture)
            {
                SharedSpriteBatch.Begin();

                SharedSpriteBatch.Draw(
                    _titleTexture,
                    _header,
                    Color.White
                );

                SharedSpriteBatch.Draw(
                    _blankTexture,
                    _border,
                    Color.DarkBlue
                );

                SharedSpriteBatch.Draw(
                    _blankTexture,
                    _body,
                    Color.PowderBlue
                );

                SharedSpriteBatch.End();
            }
            base.Draw(gameTime);
        }
    }
}
