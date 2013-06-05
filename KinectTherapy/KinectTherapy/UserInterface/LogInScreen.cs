using Microsoft.Samples.Kinect.XnaBasics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SWENG.UserInterface
{
    class LogInScreen : Screen
    {
        private ContentManager _contentManager;
        private Texture2D _blankTexture;
        private SpriteFont _spriteFont;
        private bool _isInitialized;
        private MouseState _oldMouseState;

        private Vector2 _fontSize;
        private Vector2 _fontPosition;

        private const int MARGIN = 10;
        private const float BannerHeight = 110;
        private const float TextBoxWidth = 300;

        private readonly Rectangle _viewableArea;

        private readonly GuiButton[] _buttonList;
        private readonly GuiTextBox _userNameTextBox; 
        private readonly GuiTextBox _passwordTextBox;
        private readonly GuiLabel _label;
        private readonly GuiLabel _userName;
        private readonly GuiLabel _passWord;

        public LogInScreen(Game game, Rectangle viewableArea, ScreenState startingState) : base(game)
        {
            ScreenState = startingState;
            _viewableArea = viewableArea;

            Title = "Log In";

            #region Laying out the positions

            var bannerSize = new Vector2(800, BannerHeight);
            var bannerStartingPosition = new Vector2(0, 50);
            _label = new GuiLabel("System Log In", bannerSize, bannerStartingPosition);

            var buttonSize = new Vector2(100, 30f);
            var buttonPosition = new Vector2(
                (
                    (_viewableArea.X + // get the far left position
                    (_viewableArea.Width / 2)) - // add half of the width of the viewable area
                    (buttonSize.X / 4) // and then get rid of half the button width... now we are centered
                ),
                (
                    (_viewableArea.Y + _viewableArea.Y) + // get the bottom of the stream
                    (
                        (_viewableArea.Height) / 2 - // get the entire viewable area 
                        (buttonSize.Y / 2) // and then get rid of half of the button height... again, centered
                    )
                )
            );

            bannerSize = new Vector2(175, BannerHeight / 4);
            bannerStartingPosition = new Vector2(MARGIN, 250);
            _userName = new GuiLabel("User Name:", bannerSize, bannerStartingPosition);

            var textboxSize = new Vector2(TextBoxWidth, BannerHeight / 4);
            var userNamePosition = new Vector2(bannerSize.Length() + MARGIN, 250);
            _userNameTextBox = new GuiTextBox("youngb", textboxSize, userNamePosition);

            bannerSize = new Vector2(175, BannerHeight / 4);
            bannerStartingPosition = new Vector2(MARGIN, 300);
            _passWord = new GuiLabel("Password:", bannerSize, bannerStartingPosition);

            textboxSize = new Vector2(TextBoxWidth, BannerHeight / 4);
            var passWordPosition = new Vector2(bannerSize.Length() + MARGIN, 300);
            _passwordTextBox = new GuiTextBox("*********", textboxSize, passWordPosition);

            _buttonList = new[] { new GuiButton("Log In", buttonSize, buttonPosition) };
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

            base.Initialize();
        }

        public override void LoadContent()
        {
            if (null == _contentManager)
            {
                _contentManager = new ContentManager(Game.Services, "Content");
            }

            _spriteFont = _contentManager.Load<SpriteFont>("Arial16");
            _blankTexture = _contentManager.Load<Texture2D>("blank");

            base.LoadContent();
        }

        public override void UnloadContent()
        {
            _contentManager.Unload();
        }
        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            var mouseState = Mouse.GetState();
            var mouseBoundingBox = new Rectangle(mouseState.X, mouseState.Y, 1, 1);

            foreach (var button in _buttonList)
            {
                if (mouseBoundingBox.Intersects(button.Rectangle))
                {
                    button.Hovered = true;

                    if (mouseState.LeftButton == ButtonState.Pressed && mouseState.LeftButton != _oldMouseState.LeftButton)
                    {
                        Transition();
                        Manager.CallOpen("The Hub");
                    }
                }
                else
                {
                    button.Hovered = false;
                }
            }

            _oldMouseState = mouseState;

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (_isInitialized)
            {
                _fontSize = _spriteFont.MeasureString(_label.Text);
                _fontPosition = new Vector2(x: _viewableArea.Width / 2 - _fontSize.Length() / 2, y: BannerHeight - _fontSize.Y);  //Center text in banner

                GraphicsDevice.Clear(Color.White);
                SharedSpriteBatch.Begin();

                //========= User Name Text Box =============//
                SharedSpriteBatch.Draw(
                    _blankTexture,
                    _userNameTextBox.Rectangle,
                    Color.DarkGoldenrod);

                SharedSpriteBatch.DrawString(
                    _spriteFont,
                    _userNameTextBox.Text,
                    _userNameTextBox.Position,
                    Color.White
                );

                //========= Password Text Box =============//
                SharedSpriteBatch.Draw(
                    _blankTexture,
                    _passwordTextBox.Rectangle,
                    Color.DarkGoldenrod);

                SharedSpriteBatch.DrawString(
                    _spriteFont,
                    _passwordTextBox.Text,
                    _passwordTextBox.Position,
                    Color.White
                );

                //============== Log In Button ===============//
                foreach (var button in _buttonList)
                {
                    SharedSpriteBatch.Draw(
                        _blankTexture,
                        button.Rectangle,
                        !button.Hovered ? Color.DarkGray : Color.Gray
                        );

                    SharedSpriteBatch.DrawString(
                        _spriteFont,
                        button.Text,
                        button.Position,
                        Color.Black
                    );
                }

                //========= User Name Label =============//
                SharedSpriteBatch.Draw(
                        _blankTexture,
                        _userName.Rectangle,
                        Color.DarkBlue
                    );

                SharedSpriteBatch.DrawString(
                    _spriteFont,
                    _userName.Text,
                    _userName.Position,
                    Color.White
                );

                //========= Password Label =============//
                SharedSpriteBatch.Draw(
                        _blankTexture,
                        _passWord.Rectangle,
                        Color.DarkBlue
                    );

                SharedSpriteBatch.DrawString(
                    _spriteFont,
                    _passWord.Text,
                    _passWord.Position,
                    Color.White
                );

                SharedSpriteBatch.Draw(
                        _blankTexture,
                        _label.Rectangle,
                        Color.DarkBlue
                    );

                SharedSpriteBatch.DrawString(
                    _spriteFont,
                    _label.Text,
                    _fontPosition, 
                    Color.White
                );
                SharedSpriteBatch.End();
            }
            base.Draw(gameTime);
        }
    }
}
