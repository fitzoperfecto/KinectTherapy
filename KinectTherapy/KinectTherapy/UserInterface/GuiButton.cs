using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SWENG.UserInterface
{
    public delegate void GuiButtonClicked(object sender, GuiButtonClickedArgs e);

    public class GuiButtonClickedArgs : EventArgs
    {
        public string ClickedOn;

        public GuiButtonClickedArgs(string clickedOn)
        {
            ClickedOn = clickedOn;
        }
    }

    public class GuiButton : GuiDrawable
    {
        #region event stuff
        public event GuiButtonClicked ClickEvent;

        // Invoke the Changed event; called whenever repetitions changes
        protected virtual void OnClick(GuiButtonClickedArgs e)
        {
            if (ClickEvent != null)
                ClickEvent(this, e);
        }
        #endregion

        public GuiButton(string text, Vector2 size, Vector2 position)
            : base(text, size, position) { }

        public override void Update(MouseState mouseState, MouseState oldMouseState, Rectangle mouseBoundingBox, GameTime gameTime)
        {
            if (mouseBoundingBox.Intersects(Rectangle))
            {
                Hovered = true;

                if (mouseState.LeftButton == ButtonState.Released
                    && oldMouseState.LeftButton == ButtonState.Pressed)
                {
                    Click();
                }
            }
            else
            {
                Hovered = false;
            }
        }

        public override void LoadContent(Game game, ContentManager contentManager, SpriteBatch spriteBatch)
        {
            try
            {
                Texture2D = contentManager.Load<Texture2D>(@"UI\" + Text);
            }
            catch (Exception e)
            {
                Texture2D = contentManager.Load<Texture2D>("blank");
            }
        }

        public void Click()
        {
            OnClick(new GuiButtonClickedArgs(Text));
        }
    }
}
