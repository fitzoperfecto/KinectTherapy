using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SWENG.UserInterface
{
    class GuiHeader : GuiDrawable
    {
        public GuiHeader(string text, Vector2 size, Vector2 position)
            : base(text, size, position)
        {
            this.Hovered = false;
        }

        public override void Update(MouseState mouseState, MouseState oldMouseState, Rectangle mouseBoundingBox, GameTime gameTime) { }

        public override void LoadContent(Game game, ContentManager contentManager, SpriteBatch spriteBatch)
        {
            try
            {
                Texture2D = contentManager.Load<Texture2D>(@"UI\KinectTherapy");
            }
            catch (Exception e)
            {
                Texture2D = contentManager.Load<Texture2D>(@"blank");
            }
        }
    }
}
