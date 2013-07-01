using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SWENG.UserInterface
{
    public class GuiButtonCollection
    {
        public TimeSpan MouseClickCoolDown { get; set; }
        protected bool _IsReadOnly = false;

        public List<GuiButton> Collection;

        public GuiButtonCollection()
        {
            Collection = new List<GuiButton>();
            MouseClickCoolDown = new TimeSpan(0, 0, 0, 0, 500);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (GuiButton button in Collection)
            {
                button.Draw(spriteBatch);
            }
        }

        public void Update(MouseState mouseState, MouseState oldMouseState)
        {
            Rectangle mouseBoundingBox = new Rectangle(mouseState.X, mouseState.Y, 1, 1);
            foreach (GuiButton button in Collection)
            {
                if (mouseBoundingBox.Intersects(button.Rectangle))
                {
                    button.Hovered = true;

                    if (mouseState.LeftButton == ButtonState.Released
                        && oldMouseState.LeftButton == ButtonState.Pressed)
                    {
                        button.Click();
                    }
                }
                else
                {
                    button.Hovered = false;
                }
            }
        }

        public void ClickEventForAll(GuiButtonClicked d)
        {
            foreach (GuiButton button in Collection)
            {
                button.ClickEvent += d;
            }
        }
    }
}
