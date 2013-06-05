using Microsoft.Samples.Kinect.XnaBasics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SWENG.UserInterface
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class HomeScreen : Screen
    {
        private ContentManager _contentManager;
        private Texture2D _blankTexture;
        private SpriteFont _spriteFont;
        private bool _isInitialized;
        private MouseState _oldMouseState;
        private const int MARGIN = 10;

        private readonly Rectangle _viewableArea;
        private readonly GuiButton[] _buttonList;
        private readonly GuiLabel _label;

        #region ColorStreamRenderer Variables
        private readonly ColorStreamRenderer _colorStream;
        private readonly Vector2 _colorStreamPosition;
        private readonly Vector2 _colorStreamSize;
        #endregion

        #region ExerciseQueue Variables
        private readonly SensorTile[] _sensorControls;
        #endregion

        //private readonly KinectSensorItemCollection sensorItems;
        //private readonly ObservableCollection<KinectStatusItem> statusItems;

        public HomeScreen(Game game, Rectangle viewableArea, ScreenState startingState)
            : base(game)
        {
            ScreenState = startingState;
            _viewableArea = viewableArea;
            _colorStream = new ColorStreamRenderer(game);

            Title = "The Hub";

            _sensorControls = new[] {
                new SensorTile(game, "Sensor"),
                new SensorTile(game, "Brightness")
            };

            //foreach (var _sensor in _sensorCollection)
            //{
            //    _sensor[]
            //}
            #region Laying out the positions
            _colorStreamPosition = new Vector2(
                    viewableArea.X,
                    viewableArea.Y
                );

            _colorStreamSize = new Vector2(
                    (float)(0.7 * _viewableArea.Width),
                    (float)(0.7 * _viewableArea.Height)
                );

            var bannerSize = new Vector2(_viewableArea.Width, 110f);
            var bannerStartingPosition = new Vector2(0, 50);
            _label = new GuiLabel("Session Setup", bannerSize, bannerStartingPosition);

            var tileStartingPosition = new Vector2(
                    _colorStreamPosition.X + _colorStreamSize.X + (MARGIN * 2),
                    bannerStartingPosition.Y + _colorStreamPosition.Y + (MARGIN * 5)
                );

            var tileSize = new Vector2(
                    (float)(0.25 * viewableArea.Width),
                    (float)(0.25 * viewableArea.Height)
                );

            foreach (var sensorTile in _sensorControls)
            {
                sensorTile.Position = tileStartingPosition;
                sensorTile.Size = tileSize;

                // bump the next tile down by the size of the tile and a y margin
                tileStartingPosition = new Vector2(
                    tileStartingPosition.X,
                    tileStartingPosition.Y + tileSize.Y + MARGIN
                );
            } 
            
            var buttonSize = new Vector2(100, 30f);
            var buttonPosition = new Vector2(
                (
                    (_colorStreamPosition.X + // get the far left position
                    (_viewableArea.Width / 2)) - // add half of the width of the stream
                    (buttonSize.X / 4) // and then get rid of half the button width... now we are centered
                ),
                (
                    (_colorStreamPosition.Y + _colorStreamSize.Y) + // get the bottom of the stream
                    ((
                        (_viewableArea.Height) - // get the entire viewable area 
                        (_colorStreamPosition.Y + _colorStreamSize.Y) // remove what the stream has taken
                    ) / 2) - // get the center of the remaining space
                    (buttonSize.Y / 2) // and then get rid of half of the button height... again, centered
                )
            );

            _buttonList = new[]{
                new GuiButton("Exercise", buttonSize, buttonPosition)
            };
            #endregion

            _isInitialized = false;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // make sure there is a margin between the top header and left side of the screen
            _colorStream.Position = _colorStreamPosition;

            // the color stream should only be half the viewable area to keep room for more information
            _colorStream.Size = _colorStreamPosition;
            _colorStream.Initialize();

            foreach (var sensorTile in _sensorControls)
            {
                sensorTile.Initialize();
            }

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
                        Manager.CallOpen("Exercise");
                    }
                }
                else
                {
                    button.Hovered = false;
                }
            }

            _oldMouseState = mouseState;

            _colorStream.Update(gameTime);
            foreach (var sensorTile in _sensorControls)
            {
                sensorTile.Update(gameTime);
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (_isInitialized)
            {
                GraphicsDevice.Clear(Color.White);
                SharedSpriteBatch.Begin();

                foreach (var button in _buttonList)
                {
                    SharedSpriteBatch.Draw(
                        _blankTexture,
                        button.Rectangle,
                        !button.Hovered ? Color.Magenta : Color.DarkMagenta
                        );

                    SharedSpriteBatch.DrawString(
                        _spriteFont,
                        button.Text,
                        button.Position,
                        Color.White
                    );
                }

                SharedSpriteBatch.Draw(
                        _blankTexture,
                        _label.Rectangle,
                        Color.DarkBlue 
                    );

                SharedSpriteBatch.DrawString(
                    _spriteFont,
                    _label.Text,
                    new Vector2(x: (_viewableArea.Width - _colorStreamSize.Length()) * 3, y: 100), 
                    Color.White
                );
                SharedSpriteBatch.End();

                foreach (var sensorTile in _sensorControls)
                {
                    sensorTile.Draw(gameTime);
                }

            }
            base.Draw(gameTime);
        }
    }
}
