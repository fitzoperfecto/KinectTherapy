using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SWENG.Service;

namespace SWENG.UserInterface
{
    public delegate void EditCatalogSettings(object sender, EditCatalogSettingsArgs e);

    public class EditCatalogSettingsArgs : EventArgs
    {
        public string ID;

        public EditCatalogSettingsArgs(string id)
        {
            ID = id;
        }
    }

    class GuiCatalogTile : GuiDrawable
    {
        #region event stuff
        public event EditCatalogSettings ClickEditSettingsEvent;

        // Invoke the Changed event; called whenever repetitions changes
        protected virtual void OnEditSettings(EditCatalogSettingsArgs e)
        {
            if (ClickEditSettingsEvent != null)
                ClickEditSettingsEvent(this, e);
        }
        #endregion

        #region Description texture creation
        private Game _game;
        Texture2D _overlayTexture;
        SpriteFont _spriteFont;
        
        /// <summary>
        /// Gets the SpriteBatch from the services.
        /// </summary>
        private SpriteBatch SharedSpriteBatch
        {
            get
            {
                return (SpriteBatch)_game.Services.GetService(typeof(SpriteBatch));
            }
        }
        #endregion

        #region Catalog data
        public string ItemID { get; private set; }
        private string _description;

        private CatalogManager _catalogManager
        {
            get
            {
                return (CatalogManager)_game.Services.GetService(typeof(CatalogManager));
            }
        }
        #endregion

        #region Scrolling Text
        /** Rates as a number of frames */
        private const int _scrollRate = 5;
        private const int _initialWait = 60;

        /** Lacking a game timer, just going to use a frame counter */
        private int _frameCount = 0;

        /** Where to place the texture */
        private Rectangle _innerTextDestination;

        /** What portion of the texture to display */
        private Rectangle _innerTextSource;
        #endregion

        private const float MARGIN = 10f;
        private GuiDrawable[] _guiDrawables;
        private int _updateButtonIndex;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="itemId"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="size"></param>
        /// <param name="position"></param>
        public GuiCatalogTile(Game game, string itemId, string title, string description, Vector2 size, Vector2 position)
            : base(title, size, position)
        {
            ItemID = itemId;
            _description = description;
            _game = game;

            Vector2 buttonSize = new Vector2(45f, 45f);
            Vector2 buttonBottom = new Vector2(
                size.X + position.X - buttonSize.X,
                size.Y + position.Y - buttonSize.Y
            );

            Dictionary<string, GuiDrawable> guiDrawableDct = new Dictionary<string, GuiDrawable>();
            guiDrawableDct.Add(
                "UpdateQueue",
                new GuiCheckbox(
                    "UpdateQueue",
                    buttonSize,
                    buttonBottom
                )
            );

            _updateButtonIndex = guiDrawableDct.Count - 1;

            guiDrawableDct.Add(
                "EditSettings",
                new GuiButton(
                    "EditSettings",
                    buttonSize,
                    new Vector2(buttonBottom.X, buttonBottom.Y - buttonSize.Y)
                )
            );

            _innerTextDestination = new Rectangle(
                (int)(position.X + MARGIN),
                (int)(position.Y + MARGIN),
                (int)(size.X - buttonSize.X - (2 * MARGIN)),
                (int)(size.Y - (2 * MARGIN))
            );

            _innerTextSource = new Rectangle(
                0, 0,
                _innerTextDestination.Width,
                _innerTextDestination.Height
            );

            _guiDrawables = new GuiDrawable[guiDrawableDct.Count];
            guiDrawableDct.Values.CopyTo(_guiDrawables, 0);

            Initialize();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
            foreach (GuiDrawable guiDrawable in _guiDrawables)
            {
                Type t = guiDrawable.GetType();
                if (t == typeof(GuiButton))
                {
                    ((GuiButton)guiDrawable).ClickEvent += GuiButtonWasClicked;
                }
                else if (t == typeof(GuiCheckbox))
                {
                    ((GuiCheckbox)guiDrawable).ClickEvent += GuiButtonWasClicked;
                }
            }
        }

        /// <summary>
        /// Snapshot of the button clicked.  Will manage communications within the screen and with the manager.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GuiButtonWasClicked(object sender, GuiButtonClickedArgs e)
        {
            switch (e.ClickedOn)
            {
                case "UpdateQueue":
                    changeTileQueueStatus();
                    break;
                case "EditSettings":
                    /** Add to the queue if it is not already */
                    changeTileQueueStatus(true);
                    OnEditSettings(new EditCatalogSettingsArgs(ItemID));
                    break;
            }
        }

        private void changeTileQueueStatus()
        {
            bool isEnqueued = !Hovered;
            changeTileQueueStatus(isEnqueued);
        }

        /// <summary>
        /// Notify those that care to add or remove the catalog item from the queue
        /// </summary>
        private void changeTileQueueStatus(bool isEnqueued)
        {
            Hovered = isEnqueued;
            ((GuiCheckbox)_guiDrawables[_updateButtonIndex]).Checked = Hovered;

            if (isEnqueued)
            {
                _catalogManager.AddExerciseToSelected(ItemID, Text);
            }
            else
            {
                _catalogManager.RemoveExerciseFromSelected(ItemID);
            }
        }

        public void SilentSetChecked()
        {
            Hovered = true;
            ((GuiCheckbox)_guiDrawables[_updateButtonIndex]).Checked = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentManager"></param>
        public override void LoadContent(ContentManager contentManager)
        {
            Texture2D = contentManager.Load<Texture2D>(@"UI\CatalogTile");

            foreach (GuiDrawable guiDrawable in _guiDrawables)
            {
                guiDrawable.LoadContent(contentManager);
            }

            _spriteFont = contentManager.Load<SpriteFont>("Arial10");

            _overlayTexture = CreateNewTexture();

#if DEBUG
            FileStream fs = File.Open(@"c:\school\" + Text + ".png", FileMode.Create);
            _overlayTexture.SaveAsPng(fs, _overlayTexture.Width, _overlayTexture.Height);
            fs.Close();
#endif
        }

        public override void LoadContent(Game game, ContentManager contentManager, SpriteBatch spriteBatch) 
        {
            LoadContent(contentManager);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spriteBatch">A SpriteBatch that has already "begun"</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            /** If any texture has not loaded just give up */
            if (null != _overlayTexture && null != Texture2D)
            {
                spriteBatch.Draw(
                    Texture2D,
                    Rectangle,
                    SourceRectangle,
                    Color
                );

                foreach (GuiDrawable guiDrawable in _guiDrawables)
                {
                    guiDrawable.Draw(spriteBatch);
                }

                spriteBatch.Draw(
                    _overlayTexture,
                    _innerTextDestination,
                    _innerTextSource,
                    Color.White
                );
            }
        }

        public override void Update(MouseState mouseState, MouseState oldMouseState, Rectangle mouseBoundingBox, GameTime gameTime)
        {
            foreach (GuiDrawable guiDrawable in _guiDrawables)
            {
                guiDrawable.Update(mouseState, oldMouseState, mouseBoundingBox, gameTime);
            }

            if (mouseBoundingBox.Intersects(_innerTextDestination))
            {
                /** Scroll the top of source of the texture, but keep the height */
                //_innerTextSource.Y = TextScrollTop(gameTime);
            }
        }

        /// <summary>
        /// Snapshot of where the top of the text Texture2D should be.
        /// </summary>
        /// <returns>Value between 0 and Texture2D Height</returns>
        public int TextScrollTop(GameTime gameTime)
        {
            int r = _innerTextSource.Y;
            _frameCount = _frameCount + 1;

            if (r != 0)
            {
                if (_frameCount % _scrollRate == 0)
                {
                    r = r + 1;
                }
            }
            else
            {
                if (_frameCount % _initialWait == 0)
                {
                    r = r + 1;
                }
            }

            if (r > _innerTextSource.Height)
            {
                _frameCount = 0;
                r = 0;
            }

            return r;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Texture2D CreateNewTexture()
        {
            float lineSpace = 0.0f;
            string[] descriptionLines = PartitionString(ref lineSpace);

            RenderTarget2D renderTarget2d = new RenderTarget2D(_game.GraphicsDevice, _innerTextDestination.Width, (int)(descriptionLines.Length * lineSpace));
            _game.GraphicsDevice.SetRenderTarget(renderTarget2d);
            _game.GraphicsDevice.Clear(ClearOptions.Target, Color.White, 0, 0);

            SharedSpriteBatch.Begin();

            SharedSpriteBatch.DrawString(
                _spriteFont,
                Text + ": ",
                Vector2.Zero,
                Color.DarkRed
            );

            Vector2 titleWidth = _spriteFont.MeasureString(Text + ": ");

            for (int i = 0; i < descriptionLines.Length; i = i + 1)
            {
                SharedSpriteBatch.DrawString(
                    _spriteFont,
                    descriptionLines[i],
                    new Vector2(
                        titleWidth.Length(),
                        i * lineSpace
                    ),
                    Color.Black
                );
            }
            SharedSpriteBatch.End();
            _game.GraphicsDevice.SetRenderTarget(null);

            return (Texture2D)renderTarget2d;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string[] PartitionString(ref float lineSpace)
        {
            Vector2 _titleSize = _spriteFont.MeasureString(Text + ": ");
            Vector2 _descriptionSize = _spriteFont.MeasureString(_description);

            Vector2 totalSize = _titleSize + _descriptionSize;
            Vector2 lineHeight = new Vector2((Size.X - 100f - (2 * MARGIN)), _descriptionSize.Y);
            lineSpace = _descriptionSize.Y;

            /** The number of lines needed should be roughly equal to the length of the text divided by the width of the texture2D */
            string[] r = new string[(int)Math.Ceiling(totalSize.Length() / lineHeight.Length())];

            int start = 0;
            for (int i = 0; i < r.Length; i = i + 1)
            {
                /** This is the very beginning and will not have anything in the array yet */
                if (string.IsNullOrEmpty(r[i]))
                {
                    r[i] = _description.Substring(start, 1);
                    start = start + 1;
                }

                /** 
                 * Continuously add to the current array index until the width is met or there 
                 * aren't anymore letters in the description
                 */
                while (_spriteFont.MeasureString(r[i]).X < lineHeight.X && start < _description.Length)
                {
                    r[i] = r[i] + _description.Substring(start, 1);
                    start = start + 1;
                }
            }

            return r;
        }
    }
}
