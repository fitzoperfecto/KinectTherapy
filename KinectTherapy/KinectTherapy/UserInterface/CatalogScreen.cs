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
    public class CatalogScreen : Screen
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
        //private readonly ColorStreamRenderer _colorStream;
        private readonly Vector2 _catalogViewPosition;
        private readonly Vector2 _catalogViewSize;
        #endregion

        #region Catalog Control Variables
        private readonly SensorTile[] _catalogControls;
        private SensorTile _catalogViewArea;
        private KinectSensorController _kinectSensorController;
        #endregion


        public CatalogScreen(Game game, Rectangle viewableArea, ScreenState startingState) : base(game)
        {
            ScreenState = startingState;

            _viewableArea = viewableArea;
            //_colorStream = new ColorStreamRenderer(game);

            Title = "Catalog";

            _catalogControls = new[] {
                new SensorTile(game, "Catalog")
            };

            _catalogViewArea = new SensorTile(game, "Exercises");

            #region Laying out the positions
            var bannerSize = new Vector2(_viewableArea.Width, 110f);
            var bannerStartingPosition = new Vector2(0, 50);
            _banner = new GuiLabel("Session Setup", bannerSize, bannerStartingPosition);

            _catalogViewPosition = new Vector2(
                    viewableArea.X, viewableArea.Y + bannerSize.Y
                );

            _catalogViewSize = new Vector2(
                    (float)(0.7 * viewableArea.Width),
                    (float)(0.7 * viewableArea.Height)
                );
            _catalogViewArea.Position = _catalogViewPosition;
            _catalogViewArea.Size = _catalogViewSize;

            var catalogTileStartingPosition = new Vector2(
                    _catalogViewPosition.X + _catalogViewSize.X + (MARGIN * 2),
                   _catalogViewPosition.Y
                );

            var catalogTileSize = new Vector2(
                    (float)(0.25 * viewableArea.Width),
                    (float)(0.16 * viewableArea.Height)
                );

            // Construct region for catalog control buttons
            foreach (var catalogTile in _catalogControls)
            {
                catalogTile.Position = catalogTileStartingPosition;
                catalogTile.Size = catalogTileSize;

            }

            // Construct Buttons
            var catalogButtonSize = new Vector2(189, 30f);

            var catalogAddButtonPosition = new Vector2(
                    _catalogViewPosition.X + _catalogViewSize.X + 23,
                    _catalogViewPosition.Y + 21
                );

            var catalogRemoveButtonPosition = new Vector2(
                    _catalogViewPosition.X + _catalogViewSize.X + 23,
                    catalogAddButtonPosition.Y + 41
                );

            var separatorSize = new Vector2(190, 10f);
            var separatorStartingPosition = new Vector2(catalogRemoveButtonPosition.X, catalogRemoveButtonPosition.Y + (MARGIN * 7));
            _separator = new GuiLabel("", separatorSize, separatorStartingPosition);


            var sensorButtonSize = new Vector2(195, 30f);

            var sensorButtonStartingPosition = new Vector2(
                _catalogViewPosition.X + _catalogViewSize.X + 23,
                catalogRemoveButtonPosition.Y + catalogTileSize.Y + 41
            ); 
            
            var exerciseButtonSize = new Vector2(100, 30f);
            var exerciseButtonPosition = new Vector2(
                (
                    (_catalogViewPosition.X + (_catalogViewSize.X * .7f)) - (exerciseButtonSize.X / 2)
                ),
                (
                    _viewableArea.Height + 50
                )
            );

            var exitButtonSize = new Vector2(100, 30f);
            var exitButtonPosition = new Vector2(
                (
                    (_catalogViewPosition.X + (_catalogViewSize.X / 3)) - (exitButtonSize.X / 2) 
                ),
                (
                    _viewableArea.Height + 50
                )
            );

            _buttonList = new[]{
                new GuiButton("Start", exerciseButtonSize, exerciseButtonPosition),
                new GuiButton("Exit", exitButtonSize, exitButtonPosition), 
                new GuiButton("Add", catalogButtonSize, catalogAddButtonPosition),
                new GuiButton("Remove", catalogButtonSize, catalogRemoveButtonPosition),
                new GuiButton("Sensor Control", sensorButtonSize, sensorButtonStartingPosition)
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

            //_colorStream.Position = _colorStreamPosition;
            //_colorStream.Size = _catalogViewSize;
            //_colorStream.Initialize();

            foreach (var catalogTile in _catalogControls)
            {
                catalogTile.Initialize();
            }

            _catalogViewArea.Initialize();

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
                            case "Start":
                                Transition();
                                Manager.CallOpen("Exercise");
                                break;

                            case "Sensor Control":
                                Transition();
                                Manager.CallOpen("The Hub");
                                break;

                            case "Exit":
                                _kinectSensorController.KinectSensorTerminate();
                                Game.Exit();
                                break;

                            case "Add":
                                button.Hovered = false;
                                break;

                            case "Remove":
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

            //_colorStream.Update(gameTime);

            foreach (var catalogTile in _catalogControls)
            {
                catalogTile.Update(gameTime);
            }

            _catalogViewArea.Update(gameTime);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (_isInitialized)
            {
                GraphicsDevice.Clear(Color.White);

                foreach (var catalogTile in _catalogControls)
                {
                    catalogTile.Draw(gameTime);
                }

                _catalogViewArea.Draw(gameTime);

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
                    new Vector2(x: (_viewableArea.Width - _catalogViewSize.Length()) * 3, y: 100),
                    Color.White
                );

                SharedSpriteBatch.Draw(
                        _blankTexture,
                        _separator.Rectangle,
                        Color.DarkBlue
                    );

                SharedSpriteBatch.End();
                //_colorStream.Draw(gameTime);
            }
            base.Draw(gameTime);
        }
    }
}
