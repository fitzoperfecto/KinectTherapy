using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SWENG.UserInterface
{
    class GuiSensorStatus
    {
        private Vector2 _position;
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
        public Rectangle SourceRectangle
        {
            get { return _sourceRectangle; }
        }

        public Texture2D Texture2D { get; set; }

        public string Text { get; set; }

        private bool _hovered;
        public bool Hovered
        {
            get { return _hovered; }
            set
            {
                _hovered = value;
            }
        }

        public GuiSensorStatus(string text, Vector2 size, Vector2 position)
        {
            this.Text = text;
            this.Size = size;
            this.Position = position;
            _sourceRectangle = new Rectangle(0, 0, (int)size.X, (int)size.Y);
            this.Hovered = false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                Texture2D,
                Rectangle,
                SourceRectangle,
                Color.White
            );
        }
    }
}
