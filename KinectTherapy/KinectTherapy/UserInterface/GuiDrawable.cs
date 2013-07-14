using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SWENG.UserInterface
{
    public abstract class GuiDrawable
    {
        private int _frame;
        /// <summary>
        /// Get or set the frame of the element to view (zero-based frame indexing).
        /// This will update the SourceRectangle property as well.
        /// </summary>
        public int Frame 
        {
            get
            {
                return _frame;
            }
            set
            {
                if (null != Texture2D && _size.Y < Texture2D.Height)
                {
                    _frame = value;
                    _sourceRectangle = new Rectangle(
                        0,
                        value * (int)_size.Y,
                        (int)_size.X,
                        (int)_size.Y
                    );
                }
            }
        }

        public Color Color { get; set; }

        private Vector2 _position;
        /// <summary>
        /// Get or set the position of the element.
        /// This will update the Rectangle property as well.
        /// </summary>
        public Vector2 Position 
        {
            get { return _position; } 
            set 
            { 
                _position = value;
                if (null != _size)
                {
                    Rectangle = new Rectangle(
                        (int)_position.X,
                        (int)_position.Y,
                        (int)_size.X,
                        (int)_size.Y
                    );
                }
            } 
        }

        private Vector2 _size;
        /// <summary>
        /// Get or set the size of the element.
        /// This will update the Rectangle property as well.
        /// </summary>
        public Vector2 Size
        {
            get { return _size; }
            set
            {
                _size = value;
                if (null != _position)
                {
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
        /// <summary>
        /// Get or set the placement of the element
        /// </summary>
        public Rectangle Rectangle
        {
            get { return _rectangle; }
            set
            {
                _rectangle = value;
                _position = new Vector2(value.X, value.Y);
                _size = new Vector2(value.Width, value.Height);
            }
        }

        private Rectangle _sourceRectangle;
        /// <summary>
        /// Get the portion of the element within view
        /// </summary>
        public Rectangle SourceRectangle
        {
            get { return _sourceRectangle; }
        }

        public Texture2D Texture2D { get; set; }

        /// <summary>
        /// Used as a unique identifier in most situations
        /// </summary>
        public string Text { get; set; }

        private bool _hovered;
        /// <summary>
        /// Get or set the Hovered state.  
        /// This will move the source view of the texture if the height is greater
        /// than the height of the element
        /// </summary>
        public bool Hovered
        {
            get { return _hovered; }
            set
            {
                _hovered = value;
                if (null != Texture2D && _size.Y < Texture2D.Height)
                {
                    if (value == false)
                    {
                        Frame = 0;
                    }
                    else
                    {
                        Frame = 1;
                    }
                }
            }
        }

        public GuiDrawable(string text, Vector2 size, Vector2 position)
        {
            this.Text = text;
            this.Size = size;
            this.Position = position;
            _sourceRectangle = new Rectangle(0, 0, (int)size.X, (int)size.Y);
            this.Hovered = false;
            Color = Color.White;
        }

        /// <summary>
        /// This method renders the current state of the element to the screen.
        /// </summary>
        /// <param name="spriteBatch">A SpriteBatch that has begun.</param>
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (null != Texture2D)
            {
                spriteBatch.Draw(
                    Texture2D,
                    Rectangle,
                    SourceRectangle,
                    Color
                );
                Color = Color.White;
            }
        }

        /// <summary>
        /// Update the old state of the element.
        /// </summary>
        /// <param name="mouseState">The most recent mouse state.</param>
        /// <param name="oldMouseState">The last mouse state.</param>
        /// <param name="mouseBoundingBox">A rectangle intended for diagnosing rectangle intersections.</param>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public abstract void Update(MouseState mouseState, MouseState oldMouseState, Rectangle mouseBoundingBox, GameTime gameTime);

        public abstract void LoadContent(Game game, ContentManager contentManager, SpriteBatch spriteBatch);
    }
}
