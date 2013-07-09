using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SWENG.UserInterface
{
    public class GuiLabel : GuiDrawable
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

        public GuiLabel(string text, Vector2 size, Vector2 position)
            : base(text, size, position) { }

        public override void LoadContent(Game game, ContentManager contentManager, SpriteBatch spriteBatch)
        {
            LoadContent(contentManager);
        }

        public override void Update(MouseState mouseState, MouseState oldMouseState, Rectangle mouseBoundingBox, GameTime gameTime) { }

        public override void LoadContent(ContentManager contentManager)
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
    }
}
