using System;
using System.Collections.Generic;
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

        private const float MARGIN = 10f;
        private const int SCROLL_RATE = 5;
        private const int INITIAL_WAIT = 60;

        private string _description;
        private int _frameCount = 0;
        private int _updateButtonIndex;

        private Game _game;
        private Texture2D _overlayTexture;
        private SpriteFont _spriteFont;
        private Rectangle _innerTextDestination;
        private Rectangle _innerTextSource;
        private GuiDrawable[] _guiDrawables;

        /// <summary>
        /// Meant for debugging purposes only.
        /// </summary>
        public GuiDrawable[] GuiDrawables { get { return _guiDrawables; } }
        public string ItemID { get; private set; }

        private CatalogManager _catalogManager
        {
            get
            {
                return (CatalogManager)_game.Services.GetService(typeof(CatalogManager));
            }
        }

        private SpriteBatch _sharedSpriteBatch
        {
            get
            {
                return (SpriteBatch)_game.Services.GetService(typeof(SpriteBatch));
            }
        }

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

        public override void LoadContent(Game game, ContentManager contentManager, SpriteBatch spriteBatch) 
        {
            Texture2D = contentManager.Load<Texture2D>(@"UI\CatalogTile");

            foreach (GuiDrawable guiDrawable in _guiDrawables)
            {
                guiDrawable.LoadContent(_game, contentManager, _sharedSpriteBatch);
            }

            _spriteFont = contentManager.Load<SpriteFont>("Arial10");

            _overlayTexture = CreateNewTexture();
        }


        /// <summary>
        /// This method renders the current state of the element to the screen.
        /// </summary>
        /// <param name="spriteBatch">A SpriteBatch that has begun.</param>
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
        }

        /// <summary>
        /// Central button click management.
        /// </summary>
        private void GuiButtonWasClicked(object sender, GuiButtonClickedArgs e)
        {
            switch (e.ClickedOn)
            {
                case "UpdateQueue":
                    changeTileQueueStatus();
                    break;
                case "EditSettings":
                    /** Add to the queue if it is not already */
                    ChangeTileQueueStatus(true);
                    OnEditSettings(new EditCatalogSettingsArgs(ItemID));
                    break;
            }
        }

        private void changeTileQueueStatus()
        {
            bool isEnqueued = !Hovered;
            ChangeTileQueueStatus(isEnqueued);
        }

        /// <summary>
        /// Notify those that care to add or remove the catalog item from the queue
        /// </summary>
        private void ChangeTileQueueStatus(bool isEnqueued)
        {
            Hovered = isEnqueued;
            ((GuiCheckbox)_guiDrawables[_updateButtonIndex]).Checked = Hovered;

            if (_catalogManager != null)
            {
                if (isEnqueued)
                {
                    _catalogManager.AddExerciseToSelected(ItemID, Text);
                }
                else
                {
                    _catalogManager.RemoveExerciseFromSelected(ItemID);
                }
            }
        }

        public void SilentSetChecked()
        {
            Hovered = true;
            ((GuiCheckbox)_guiDrawables[_updateButtonIndex]).Checked = true;
        }

        /// <summary>
        /// Creates a texture to show in the tile instead of constantly re-drawing the text.
        /// </summary>
        public Texture2D CreateNewTexture()
        {
            /** The title width will need to be subtracted for the string width measurement */
            Vector2 titleWidth = _spriteFont.MeasureString(Text + ": ");

            string toWrite = string.Empty;

            /** Descriptions may be of an indeterminate length */
            for (int i = 0; i < _description.Length; ++i)
            {
                /** If one more letter is added will it go beyond the width of the acceptable area?  Push it to the next line if so. */
                if (_spriteFont.MeasureString(toWrite + _description.Substring(i, 1)).X >= _innerTextDestination.Width - titleWidth.X)
                {
                    toWrite += "\r\n";
                }

                toWrite += _description.Substring(i, 1);
            }

            RenderTarget2D renderTarget2d = new RenderTarget2D(_game.GraphicsDevice, _innerTextDestination.Width, (int)_spriteFont.MeasureString(toWrite).Y);
            _game.GraphicsDevice.SetRenderTarget(renderTarget2d);
            _game.GraphicsDevice.Clear(ClearOptions.Target, Color.White, 0, 0);

            _sharedSpriteBatch.Begin();

            /** Red title */
            _sharedSpriteBatch.DrawString(
                _spriteFont,
                Text + ": ",
                Vector2.Zero,
                Color.DarkRed
            );

            /** Black text */
            _sharedSpriteBatch.DrawString(
                    _spriteFont,
                    toWrite,
                    new Vector2(titleWidth.X, 0f),
                    Color.Black
                );

            _sharedSpriteBatch.End();
            _game.GraphicsDevice.SetRenderTarget(null);

            return (Texture2D)renderTarget2d;
        }
    }
}
