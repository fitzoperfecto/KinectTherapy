using System;
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
        private const float MARGIN = 10f;
        GuiButtonCollection _buttonList;
        public GuiButtonCollection ButtonList { get { return _buttonList; } }

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
        private string _itemId;
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
            _itemId = itemId;
            _description = description;
            _game = game;

            Vector2 buttonSize = new Vector2(45f, 45f);
            Vector2 buttonBottom = new Vector2(
                size.X + position.X - buttonSize.X,
                size.Y + position.Y - buttonSize.Y
            );

            _buttonList = new GuiButtonCollection();
            _buttonList.Collection.Add(new GuiButton(
                    "Update Queue",
                    buttonSize,
                    buttonBottom
                ));

            _buttonList.Collection.Add(new GuiButton(
                    "Edit Settings",
                    buttonSize,
                    new Vector2(buttonBottom.X, buttonBottom.Y - buttonSize.Y)
                ));

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
        }

        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
            _buttonList.ClickEventForAll(GuiButtonWasClicked);
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
                case "Update Queue":
                    changeTileQueueStatus();
                    break;
                case "Edit Settings":
                    /** Add to the queue if it is not already */
                    changeTileQueueStatus(true);
                    OnEditSettings(new EditCatalogSettingsArgs(_itemId));
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
            _buttonList.Collection[0].Hovered = Hovered;

            if (isEnqueued)
            {
                _catalogManager.AddExerciseToSelected(_itemId);
            }
            else
            {
                _catalogManager.RemoveExerciseFromSelected(_itemId);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentManager"></param>
        public void LoadContent(ContentManager contentManager)
        {
            Texture2D = contentManager.Load<Texture2D>(@"UI\CatalogTile");
            _buttonList.Collection[0].Texture2D = contentManager.Load<Texture2D>(@"UI\CatalogCheckbox");
            _buttonList.Collection[1].Texture2D = contentManager.Load<Texture2D>(@"UI\CatalogEdit");
            _spriteFont = contentManager.Load<SpriteFont>("Arial10");

            _overlayTexture = CreateNewTexture();

#if DEBUG
            FileStream fs = File.Open(@"c:\school\" + Text + ".png", FileMode.Create);
            _overlayTexture.SaveAsPng(fs, _overlayTexture.Width, _overlayTexture.Height);
            fs.Close();
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spriteBatch">A SpriteBatch that has already "begun"</param>
        public void Draw(SpriteBatch spriteBatch)
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

                _buttonList.Draw(spriteBatch);

                spriteBatch.Draw(
                    _overlayTexture,
                    _innerTextDestination,
                    _innerTextSource,
                    Color.White
                );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mouseState">Instance of the mouse state</param>
        /// <param name="oldMouseState">Instance of the previous mouse state</param>
        public void Update(MouseState mouseState, MouseState oldMouseState)
        {
            Rectangle mouseBoundingBox = new Rectangle(mouseState.X, mouseState.Y, 1, 1);
            foreach (GuiButton button in _buttonList.Collection)
            {
                /** The catalog item was added to the queue */
                if (Hovered)
                {
                    /** The "Update Queue" button should remain in the hovered state */
                    if (button.Text != "Update Queue")
                    {
                        button.Hovered = false;
                    }
                }
                /** The catalog is NOT added to the queue */
                else
                {
                    button.Hovered = false;
                }

                if (mouseBoundingBox.Intersects(button.Rectangle))
                {
                    button.Hovered = true;
                    if (mouseState.LeftButton == ButtonState.Released
                        && oldMouseState.LeftButton == ButtonState.Pressed)
                    {
                        button.Click();
                    }
                }
            }

            if (mouseBoundingBox.Intersects(_innerTextDestination))
            {
                /** Scroll the top of source of the texture, but keep the height */
                _innerTextSource.Y = TextScrollTop();
            }
        }

        /// <summary>
        /// Snapshot of where the top of the text Texture2D should be.
        /// </summary>
        /// <returns>Value between 0 and Texture2D Height</returns>
        public int TextScrollTop()
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
                if (string.IsNullOrEmpty(r[i]))
                {
                    r[i] = _description.Substring(start, 1);
                    start = start + 1;
                }

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
