using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SWENG.UserInterface
{
    class GuiScrollable : GuiDrawable
    {
        private GuiButton _scrollButton;
        private string _textureName;
        private int _count = 0;
        public int Count 
        { 
            get 
            { 
                return _count; 
            } 
            set 
            { 
                _count = value;
                _scrollButton = CreateGuiButton();
            } 
        }

        public Rectangle GraceArea { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size"></param>
        /// <param name="position"></param>
        /// <param name="count"></param>
        public GuiScrollable(Vector2 size, Vector2 position, int count)
            : base(string.Empty, size, position) 
        {
            _count = count;

            Vector2 scrollSize = new Vector2(size.X * 0.9f, size.Y / _count);
            _scrollButton = new GuiButton("scroll", 
                scrollSize, 
                new Vector2(
                    position.X + ((size.X * 0.1f) / 2), 
                    position.Y
                )
            );

            GraceArea = new Rectangle((int)position.X - 25, (int)position.Y, (int)size.X + 50, (int)size.Y);

            Color = new Color(0, 153, 153);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size"></param>
        /// <param name="position"></param>
        /// <param name="count"></param>
        public GuiScrollable(Vector2 size, Vector2 position, string textureName)
            : base(string.Empty, size, position)
        {
            _textureName = textureName;

            GraceArea = new Rectangle((int)position.X - 25, (int)position.Y, (int)size.X + 50, (int)size.Y);

            Color = new Color(0, 153, 153);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private GuiButton CreateGuiButton()
        {
            Vector2 scrollSize = new Vector2(Size.X * 0.9f, Size.Y / _count);
            return new GuiButton("scroll", 
                scrollSize,
                new Vector2(
                    Position.X + ((Size.X * 0.1f) / 2), 
                    Position.Y
                )
            );
        }

        /// <summary>
        /// Does nothing
        /// </summary>
        public void Initialize() { }

        public override void LoadContent(Game game, ContentManager contentManager, SpriteBatch spriteBatch) 
        {
            Texture2D = contentManager.Load<Texture2D>(@"blank");
            if (string.IsNullOrEmpty(_textureName))
            {
                _scrollButton.Texture2D = contentManager.Load<Texture2D>(@"blank");
            }
            else
            {
                Texture2D scrollTexture = contentManager.Load<Texture2D>(_textureName);
                Vector2 scrollSize = new Vector2(scrollTexture.Width, scrollTexture.Height);
                _scrollButton = new GuiButton("scroll",
                    scrollSize,
                    new Vector2(
                        Position.X - (scrollSize.X / 2),
                        Position.Y
                    )
                );

                _scrollButton.Texture2D = scrollTexture;
            }
        }


        /// <summary>
        /// This method renders the current state of the element to the screen.
        /// </summary>
        /// <param name="spriteBatch">A SpriteBatch that has begun.</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                Texture2D,
                Rectangle,
                Color
            );

            _scrollButton.Draw(spriteBatch);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mouseState"></param>
        /// <param name="oldMouseState"></param>
        public override void Update(MouseState mouseState, MouseState oldMouseState, Rectangle mouseBoundingBox, GameTime gameTime)
        {
            if (mouseBoundingBox.Intersects(_scrollButton.Rectangle))
            {
                _scrollButton.Color = Color.DarkGray;

                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    _scrollButton.Position = new Vector2(_scrollButton.Position.X, mouseState.Y - (_scrollButton.Size.Y / 2));

                    if (_scrollButton.Position.Y < Position.Y)
                    {
                        _scrollButton.Position = new Vector2(_scrollButton.Position.X, Position.Y);
                    }
                    else if (_scrollButton.Position.Y + _scrollButton.Size.Y > Position.Y + Size.Y)
                    {
                        _scrollButton.Position = new Vector2(_scrollButton.Position.X, Position.Y + Size.Y - _scrollButton.Size.Y);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal float GetScrollTop()
        {
            float top = 0.0f;

            if (Size.Y != _scrollButton.Size.Y)
            {
                top = (_scrollButton.Rectangle.Top - Rectangle.Top) / (Size.Y - _scrollButton.Size.Y);
            }
            else
            {
                top = 1.0f;
            }
            if (top > 1.0f)
            {
                top = 1.0f;
            }

            top *= 100;

            return top;
        }
    }
}
