using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SWENG.UserInterface
{
    public class GuiInputBox : GuiDrawable
    {
        #region event stuff
        #endregion

        public GuiInputBox(string id, Vector2 size, Vector2 position, Game game)
            : base(id, size, position) 
        {
            Texture2D = CreateInputTexture(game, size);
        }

        private Texture2D CreateInputTexture(Game game, Vector2 size)
        {
            RenderTarget2D renderTarget2d = new RenderTarget2D(game.GraphicsDevice, (int)(size.X * 3), (int)(size.Y));

            game.GraphicsDevice.SetRenderTarget(renderTarget2d);
            game.GraphicsDevice.Clear(ClearOptions.Target, Color.White, 0, 0);

            SpriteBatch spriteBatch = (SpriteBatch)game.Services.GetService(typeof(SpriteBatch));

            spriteBatch.Begin();

            spriteBatch.End();

            game.GraphicsDevice.SetRenderTarget(null);

            return (Texture2D)renderTarget2d;
        }
    }
}
