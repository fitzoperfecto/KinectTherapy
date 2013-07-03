using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SWENG.Service;
using System;
using SWENG.Criteria;
using System.Diagnostics;

namespace SWENG.UserInterface
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class CatalogTileEditScreen : Screen
    {
        private readonly Rectangle _viewableArea;
        private readonly GuiButtonCollection _buttonList;

        private const float MARGIN = 10f;

        private bool _isInitialized;
        private MouseState _oldMouseState;
        private Texture2D _blankTexture;
        private Texture2D _inputBoxTexture;
        private Rectangle _inputBoxDestination;

        private Exercise _exercise;

        private string _repetitions;
        private Vector2 _repLocation;

        private string _variance;
        private Vector2 _devLocation;

        private KeyboardState _oldKeyState;

        private SpriteFont _spriteFont;

        private CatalogManager _catalogManager
        {
            get
            {
                return (CatalogManager)Game.Services.GetService(typeof(CatalogManager));
            }
        }

        private Keys[] _numerical = new Keys[] { 
            Keys.NumPad0,
            Keys.NumPad1,
            Keys.NumPad2,
            Keys.NumPad3,
            Keys.NumPad4,
            Keys.NumPad5,
            Keys.NumPad6,
            Keys.NumPad7,
            Keys.NumPad8,
            Keys.NumPad9,
            Keys.D0,
            Keys.D1,
            Keys.D2,
            Keys.D3,
            Keys.D4,
            Keys.D5,
            Keys.D6,
            Keys.D7,
            Keys.D8,
            Keys.D9,
        };
        private string _itemId;

        public CatalogTileEditScreen(Game game, Rectangle viewableArea, ScreenState startingState)
            : base(game)
        {
            ScreenState = startingState;
            _viewableArea = viewableArea;

            Title = "CatalogTileEdit";

            Vector2 modalSize = new Vector2(512, 384);

            _inputBoxDestination = new Rectangle(
                (_viewableArea.Width / 2) - ((int)modalSize.X / 2),
                (_viewableArea.Height / 2) - ((int)modalSize.Y / 2),
                (int)modalSize.X,
                (int)modalSize.Y
            );

            Vector2 buttonSize = new Vector2(121f, 60f);
            Vector2 buttonBottom = new Vector2(
                _inputBoxDestination.Right - buttonSize.X - MARGIN,
                _inputBoxDestination.Bottom - buttonSize.Y);

            Vector2 optionSize = new Vector2(311f, 61f);
            Vector2 optionBottom = new Vector2(
                    _inputBoxDestination.Right - optionSize.X - MARGIN,
                    _inputBoxDestination.Top + optionSize.Y + MARGIN 
                );
            #region Laying out the positions
            _buttonList = new GuiButtonCollection();
            _buttonList.Collection.Add(
                new GuiButton("Save", 
                    buttonSize,
                    buttonBottom
                    - (new Vector2(2 * MARGIN, 0f))
                    - (new Vector2(2 * buttonSize.Y, 0f))
                ));
            _buttonList.Collection.Add(new GuiButton("Cancel",
                    buttonSize,
                    buttonBottom
                ));

            _buttonList.Collection.Add(
                new GuiButton("Repetitions",
                    optionSize,
                    optionBottom
                ));
            _buttonList.Collection.Add(
                new GuiButton("Deviation",
                    optionSize,
                    optionBottom
                    + (new Vector2(0f, MARGIN))
                    + (new Vector2(0f, optionSize.Y))
                ));

            _repLocation = new Vector2(
                _buttonList.Collection[2].Rectangle.Right - 146,
                _buttonList.Collection[2].Rectangle.Bottom - 40
            );

            _devLocation = new Vector2(
                _buttonList.Collection[3].Rectangle.Right - 146,
                _buttonList.Collection[3].Rectangle.Bottom - 40
            );

            #endregion

            _isInitialized = false;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            _isInitialized = true;

            _buttonList.ClickEventForAll(GuiButtonWasClicked);

            base.Initialize();
        }

        private void GuiButtonWasClicked(object sender, GuiButtonClickedArgs e)
        {
            switch (e.ClickedOn)
            {
                case "Save":
                    ScreenState = UserInterface.ScreenState.Hidden;
                    saveChanges();
                    OnTransition(new TransitionEventArgs(Title, "Return"));
                    break;
                case "Cancel":
                    ScreenState = UserInterface.ScreenState.Hidden;
                    OnTransition(new TransitionEventArgs(Title, "Return"));
                    break;
            }
        }

        private void saveChanges()
        {
            CatalogManager catalogManager = _catalogManager;
            int temp = 0;
            float tempFloat = 0;
            _exercise = new Exercise();
            _exercise.Id = _itemId;
            if (int.TryParse(_repetitions, out temp))
            {
                _exercise.Repetitions = temp;
            }

            if (float.TryParse(_variance, out tempFloat))
            {
                _exercise.Variance = tempFloat;
            }
            catalogManager.SetExerciseOptions(_exercise);
        }

        private void changeOptions(string buttonId)
        {
            foreach (GuiButton button in _buttonList.Collection)
            {
                if (button.Text == buttonId)
                {
                    button.Hovered = true;
                }
            }
        }

        public override void LoadContent()
        {
            if (null == contentManager)
            {
                contentManager = new ContentManager(Game.Services, "Content");
            }

            _spriteFont = contentManager.Load<SpriteFont>(@"Arial16");

            _blankTexture = contentManager.Load<Texture2D>(@"blank");
            _inputBoxTexture = contentManager.Load<Texture2D>(@"UI\CatalogTileEdit");

            _buttonList.Collection[0].Texture2D = contentManager.Load<Texture2D>(@"UI\Submit");
            _buttonList.Collection[1].Texture2D = contentManager.Load<Texture2D>(@"UI\Cancel");

            _buttonList.Collection[2].Texture2D = contentManager.Load<Texture2D>(@"UI\CatalogItemRepetitions");
            _buttonList.Collection[3].Texture2D = contentManager.Load<Texture2D>(@"UI\CatalogItemDeviation");

            base.LoadContent();
        }

        public override void UnloadContent()
        {
            contentManager.Unload();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            if (_isInitialized)
            {
                KeyboardState keyState = Keyboard.GetState();
                MouseState currentState = Mouse.GetState();
                _buttonList.Update(currentState, _oldMouseState);

                Rectangle mouseBoundingBox = new Rectangle(currentState.X, currentState.Y, 1, 1);
                foreach (GuiButton button in _buttonList.Collection)
                {
                    if (button.Hovered)
                    {
                        Debug.WriteLine(button.Text + button.Hovered);
                        if (button.Text != "Repetitions"
                            && button.Text != "Deviation")
                        {
                            button.Hovered = false;
                        }
                        else
                        {
                            UpdateOptionString(button.Text, keyState);
                        }
                    }
                    else
                    {
                        button.Hovered = false;
                    }

                    if (mouseBoundingBox.Intersects(button.Rectangle))
                    {
                        button.Hovered = true;
                        if (currentState.LeftButton == ButtonState.Released
                            && currentState.LeftButton == ButtonState.Pressed)
                        {
                            button.Click();
                        }
                    }
                }

                _oldMouseState = currentState;
                _oldKeyState = keyState;
            }
            base.Update(gameTime);
        }

        private void UpdateOptionString(string buttonId, KeyboardState keyState)
        {
            int temp = 0;

            if (buttonId == "Repetitions")
            {
                if (_oldKeyState.IsKeyDown(Keys.Back)
                    && keyState.IsKeyUp(Keys.Back)
                    && _repetitions.Length > 0)
                {
                    _repetitions = _repetitions.Remove(_repetitions.Length - 1, 1);
                }

                _repetitions += CheckKeys(keyState);

                if (int.TryParse(_repetitions, out temp))
                {
                    _repetitions = temp.ToString();
                }

                if (_oldKeyState.IsKeyDown(Keys.Enter)
                    && keyState.IsKeyUp(Keys.Enter))
                {
                    if (string.IsNullOrEmpty(_repetitions))
                    {
                        _repetitions = "10";
                    }

                    foreach (GuiButton button in _buttonList.Collection)
                    {
                        if (button.Text == buttonId)
                        {
                            button.Hovered = false;
                        }
                    }
                }
            }
            else if (buttonId == "Deviation")
            {
                if (_oldKeyState.IsKeyDown(Keys.Back)
                    && keyState.IsKeyUp(Keys.Back)
                    && _variance.Length > 0)
                {
                    _variance = _variance.Remove(_variance.Length - 1, 1);
                }

                _variance += CheckKeys(keyState);

                if (int.TryParse(_variance, out temp))
                {
                    _variance = temp.ToString();
                }

                if (_oldKeyState.IsKeyDown(Keys.Enter)
                    && keyState.IsKeyUp(Keys.Enter))
                {
                    if (string.IsNullOrEmpty(_variance))
                    {
                        _variance = "10";
                    }

                    foreach (GuiButton button in _buttonList.Collection)
                    {
                        if (button.Text == buttonId)
                        {
                            button.Hovered = false;
                        }
                    }
                }
            }
        }

        private string CheckKeys(KeyboardState keyState)
        {
            string r = string.Empty;

            Keys[] oldKeys = _oldKeyState.GetPressedKeys();


            foreach (Keys key in oldKeys)
            {
                if (Array.IndexOf(_numerical, key) != -1)
                {
                    if (keyState.IsKeyUp(key))
                    {
                        string numb = key.ToString();
                        r = numb.Substring(numb.Length - 1);
                    }
                }
            }

            return r;
        }

        public override void Draw(GameTime gameTime)
        {
            if (_isInitialized)
            {
                var spriteBatch = SharedSpriteBatch;
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

                spriteBatch.Draw(
                    _blankTexture,
                    _viewableArea,
                    Color.WhiteSmoke * 0.5f
                );

                spriteBatch.End();

                spriteBatch.Begin();

                spriteBatch.Draw(
                    _inputBoxTexture,
                    _inputBoxDestination,
                    Color.White
                );

                _buttonList.Draw(spriteBatch);

                spriteBatch.DrawString(
                    _spriteFont,
                    _repetitions,
                    _repLocation,
                    Color.Black
                );

                spriteBatch.DrawString(
                    _spriteFont,
                    _variance,
                    _devLocation,
                    Color.Black
                );

                spriteBatch.End();
            }
            base.Draw(gameTime);
        }

        public void OpenScreen(string id)
        {
            CatalogManager catalogManager = _catalogManager;

            _exercise = catalogManager.GetExercise(id);

            _itemId = _exercise.Id;
            _repetitions = _exercise.Repetitions.ToString();
            _variance = "10";
        }
    }
}
