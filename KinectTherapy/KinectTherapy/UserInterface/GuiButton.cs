using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SWENG.UserInterface
{
    class GuiButton
    {
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

        public string Text { get; set; }

        public bool Hovered { get; set; }

        public GuiButton(string text, Vector2 size, Vector2 position)
        {
            this.Text = text;
            this.Size = size;
            this.Position = position;
            this.Hovered = false;
        }
    }
}
