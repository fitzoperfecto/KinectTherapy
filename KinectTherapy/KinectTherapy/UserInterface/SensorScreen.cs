using System.Threading;
using Microsoft.Samples.Kinect.XnaBasics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


using SWENG.Sensor;

namespace SWENG.UserInterface
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class SensorScreen : Screen
    {
        private ContentManager _contentManager;
        private Texture2D _blankTexture;
        private SpriteFont _spriteFont;
        private bool _isInitialized;
        private MouseState _oldMouseState;
        private const int MARGIN = 10;

        private readonly Rectangle _viewableArea;

        private readonly GuiButton[] _buttonList;
        private readonly GuiLabel _banner;
        private readonly GuiLabel _separator;

        #region ColorStreamRenderer Variables
        private readonly ColorStreamRenderer _colorStream;
        private readonly Vector2 _colorStreamPosition;
        private readonly Vector2 _colorStreamSize;
        #endregion

        #region Sensor Control Variables
        private readonly SensorTile[] _sensorControls;

        private KinectSensorController _kinectSensorController;

        private const int StepAngleUp = 2;
        private const int StepAngleDown = -2;
        private const float Brighter = 0.1f;
        private const float Darker = -0.1f;
        #endregion


        public SensorScreen(Game game, Rectangle viewableArea, ScreenState startingState) : base(game)
        {
            ScreenState = startingState;

            _viewableArea = viewableArea;
            _colorStream = new ColorStreamRenderer(game);

            Title = "Sensor";

            _sensorControls = new[] {
                new SensorTile(game, "Sensor"),
                new SensorTile(game, "Brightness")
            };

            #region Laying out the positions
            var bannerSize = new Vector2(_viewableArea.Width, 110f);
            var bannerStartingPosition = new Vector2(0, 50);
            _banner = new GuiLabel("Session Setup", bannerSize, bannerStartingPosition);

            _colorStreamPosition = new Vector2(
                    viewableArea.X, viewableArea.Y + bannerSize.Y
                );

            _colorStreamSize = new Vector2(
                    (float)(0.7 * viewableArea.Width),
                    (float)(0.7 * viewableArea.Height)
                );

            var sensorTileStartingPosition = new Vector2(
                    _colorStreamPosition.X + _colorStreamSize.X + (MARGIN * 2),
                   _colorStreamPosition.Y
                );

            var sensorTileSize = new Vector2(
                    (float)(0.25 * viewableArea.Width),
                    (float)(0.16 * viewableArea.Height)
                );

                // Construct region for sensor control buttons
            foreach (var sensorTile in _sensorControls)
            {
                sensorTile.Position = sensorTileStartingPosition;
                sensorTile.Size = sensorTileSize;

                // bump the next tile down by the size of the tile and a y margin
                sensorTileStartingPosition = new Vector2(
                    sensorTileStartingPosition.X,
                    sensorTileStartingPosition.Y + sensorTileSize.Y + MARGIN
                );
            }

            var separatorSize = new Vector2(195, 10f);
            var separatorStartingPosition = new Vector2(sensorTileStartingPosition.X, sensorTileStartingPosition.Y + sensorTileSize.Y - (MARGIN * 6));
            _separator = new GuiLabel("", separatorSize, separatorStartingPosition);

            // Construct Buttons
            var sensorButtonSize = new Vector2(189, 30f);

            var sensorAngleUpButtonPosition = new Vector2(
                    _colorStreamPosition.X + _colorStreamSize.X + 23,
                    _colorStreamPosition.Y + 21
                );

            var sensorAngleDownButtonPosition = new Vector2(
                    _colorStreamPosition.X + _colorStreamSize.X + 23,
                    sensorAngleUpButtonPosition.Y + 41
                );

            var sensorBrightnessUpButtonPosition = new Vector2(
                    _colorStreamPosition.X + _colorStreamSize.X + 23,
                    sensorAngleDownButtonPosition.Y + 65
                );

            var sensorBrightnessDownButtonPosition = new Vector2(
                    _colorStreamPosition.X + _colorStreamSize.X + 23,
                    sensorBrightnessUpButtonPosition.Y + 40
                );

            var catalogButtonSize = new Vector2(195, 30f);

            var catalogButtonStartingPosition = new Vector2(
                _colorStreamPosition.X + _colorStreamSize.X + (MARGIN * 2),
                sensorTileStartingPosition.Y + sensorTileSize.Y
            );


            var exitButtonSize = new Vector2(100, 30f);
            var exitButtonPosition = new Vector2(
                (
                    (_colorStreamPosition.X + (_colorStreamSize.X / 3)) - (exitButtonSize.X / 2) 
                ),
                (
                    _viewableArea.Height + 50
                )
            );

            _buttonList = new[]{
                new GuiButton("Up", sensorButtonSize, sensorAngleUpButtonPosition),
                new GuiButton("Down", sensorButtonSize, sensorAngleDownButtonPosition),
                new GuiButton("Brighter", sensorButtonSize, sensorBrightnessUpButtonPosition),
                new GuiButton("Darker", sensorButtonSize, sensorBrightnessDownButtonPosition),
                new GuiButton("Catalog", catalogButtonSize, catalogButtonStartingPosition), 

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
            _kinectSensorController = new KinectSensorController();

            _colorStream.Position = _colorStreamPosition;
            _colorStream.Size = _colorStreamSize;
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
                        switch (button.Text)
                        {
                            case "Exercise":
                                Transition();
                                Manager.CallOpen("Exercise");
                                break;

                            case "Catalog":
                                Transition();
                                Manager.CallOpen("Catalog");
                                break;

                            case "Exit":
                                _kinectSensorController.KinectSensorTerminate();
                                Game.Exit();
                                break;

                            case "Up":
                                _kinectSensorController.KinectSensorElevationControl(StepAngleUp);
                                button.Hovered = false;
                                Thread.Sleep(1350);
                                break;

                            case "Down":
                                _kinectSensorController.KinectSensorElevationControl(StepAngleDown);
                                Thread.Sleep(1350);
                                button.Hovered = false;
                                break;

                            case "Brighter":
                                _kinectSensorController.KinectSensorBrightnessLevel(Brighter);
                                button.Hovered = false;
                                break;

                            case "Darker":
                                _kinectSensorController.KinectSensorBrightnessLevel(Darker);
                                button.Hovered = false;
                                break;
                        }
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

                foreach (var sensorTile in _sensorControls)
                {
                    sensorTile.Draw(gameTime);
                }

                SharedSpriteBatch.Begin();

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
                        Color.White
                    );
                }

                SharedSpriteBatch.Draw(
                        _blankTexture,
                        _banner.Rectangle,
                        Color.DarkBlue
                    );

                SharedSpriteBatch.DrawString(
                    _spriteFont,
                    _banner.Text,
                    new Vector2(x: (_viewableArea.Width - _colorStreamSize.Length()) * 3, y: 100),
                    Color.White
                );

                SharedSpriteBatch.Draw(
                        _blankTexture,
                        _separator.Rectangle,
                        Color.DarkBlue
                    );

                SharedSpriteBatch.End();
                _colorStream.Draw(gameTime);
            }
            base.Draw(gameTime);
        }
    }
}
