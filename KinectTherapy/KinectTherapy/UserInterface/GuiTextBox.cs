using Microsoft.Xna.Framework;

namespace SWENG.UserInterface
{
    class GuiTextBox
    {
        private Vector2 _position;
        public Vector2 Position 
        {
            get { return _position; } 
            set 
            { 
                _position = value;
                Rectangle = new Rectangle(
                    (int)_position.X,
                    (int)_position.Y,
                    (int)_size.X,
                    (int)_size.Y
                    );
            } 
        }

        private Vector2 _size;
        public Vector2 Size
        {
            get { return _size; }
            set
            {
                _size = value;
                Rectangle = new Rectangle(
                    (int)_position.X,
                    (int)_position.Y,
                    (int)_size.X,
                    (int)_size.Y
                    );
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

        public string Text { get; set; }

        public GuiTextBox(string text, Vector2 size, Vector2 position)
        {
            Text = text;
            Size = size;
            Position = position;
        }
    }
}
