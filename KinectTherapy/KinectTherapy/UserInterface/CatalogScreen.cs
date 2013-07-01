using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SWENG.Service;

namespace SWENG.UserInterface
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class CatalogScreen : Screen
    {
        private CatalogManager _catalogManager
        {
            get
            {
                return (CatalogManager)Game.Services.GetService(typeof(CatalogManager));
            }
        }

        private readonly Rectangle _viewableArea;
        private readonly GuiButtonCollection _buttonList;
        private readonly GuiHeader _header;
        private readonly GuiSensorStatus _sensorStatus;
        private readonly GuiCatalogTileCollection _catalogList;

        private const float MARGIN = 10f;
        private bool _isInitialized;
        private MouseState _oldMouseState;

        /** Needed to send to the GuiCatalogTiles */
        private Game _game;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="viewableArea"></param>
        /// <param name="startingState"></param>
        public CatalogScreen(Game game, Rectangle viewableArea, ScreenState startingState) 
            : base(game)
        {
            _game = game;
            ScreenState = startingState;
            _viewableArea = viewableArea;
            _buttonList = new GuiButtonCollection();

            Title = "Catalog";

            Vector2 buttonSize = new Vector2(240f, 60f);
            Vector2 buttonBottom = new Vector2(
                _viewableArea.Right - buttonSize.X + MARGIN,
                _viewableArea.Bottom - buttonSize.Y);
            Rectangle catalogArea = new Rectangle(
                    _viewableArea.Left - (int)MARGIN,
                    _viewableArea.Top + 100,
                    _viewableArea.Width - (int)buttonSize.X - (int)MARGIN,
                    _viewableArea.Height + 100
                );

            #region Laying out the positions
            _catalogList = new GuiCatalogTileCollection(catalogArea);

            _buttonList.Collection.Add(
                new GuiButton("Start", buttonSize,
                    buttonBottom
                    - (new Vector2(0f, 2 * MARGIN))
                    - (new Vector2(0f, 2 * buttonSize.Y))
                ));
            _buttonList.Collection.Add(new GuiButton("Sensor Setup", buttonSize,
                    buttonBottom
                    - new Vector2(0f, MARGIN)
                    - new Vector2(0f, buttonSize.Y)
                ));
            _buttonList.Collection.Add(new GuiButton("Exit Program", buttonSize, buttonBottom));

            _sensorStatus = new GuiSensorStatus("Sensor Status",
                new Vector2(99f, 32f),
                new Vector2(
                (_viewableArea.Right / 2) - (99f / 2),
                _viewableArea.Bottom - 32f
            ));

            _header = new GuiHeader("Kinect Therapy: Catalog Screen",
                new Vector2(326f, 52f),
                new Vector2(
                _viewableArea.Left,
                _viewableArea.Top - MARGIN - 52f
            ));
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

            foreach (GuiButton button in _buttonList.Collection)
            {
                button.ClickEvent += GuiButtonWasClicked;
            }

            _catalogList.Initialize();

            base.Initialize();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GuiButtonWasClicked(object sender, GuiButtonClickedArgs e)
        {
            switch (e.ClickedOn)
            {
                case "Start":
                    ScreenState = UserInterface.ScreenState.Hidden;
                    OnTransition(new TransitionEventArgs(Title, e.ClickedOn));
                    _catalogManager.Status = CatalogManagerStatus.Complete;
                    break;
                case "Sensor Setup":
                    ScreenState = UserInterface.ScreenState.Active | UserInterface.ScreenState.NonInteractive;
                    OnTransition(new TransitionEventArgs(Title, e.ClickedOn));
                    break;
                case "Exit Program":
                    OnTransition(new TransitionEventArgs(Title, e.ClickedOn));
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GuiCatalogTileButtonWasClicked(object sender, EditCatalogSettingsArgs e)
        {
            ScreenState = UserInterface.ScreenState.Active | UserInterface.ScreenState.NonInteractive;
            OnTransition(new TransitionEventArgs(e.ID, "CatalogTileEdit"));
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void LoadContent()
        {
            if (null == contentManager)
            {
                contentManager = new ContentManager(Game.Services, "Content");
            }

            _buttonList.Collection[0].Texture2D = contentManager.Load<Texture2D>(@"UI\Start");
            _buttonList.Collection[1].Texture2D = contentManager.Load<Texture2D>(@"UI\SensorSetup");
            _buttonList.Collection[2].Texture2D = contentManager.Load<Texture2D>(@"UI\ExitProgram");

            _sensorStatus.Texture2D = contentManager.Load<Texture2D>(@"UI\KinectSensorGood");

            _header.Texture2D = contentManager.Load<Texture2D>(@"UI\KinectTherapy");

            _catalogList.LoadContent(contentManager);

            base.LoadContent();
        }

        /// <summary>
        /// 
        /// </summary>
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
            if (_isInitialized
                && (ScreenState & UserInterface.ScreenState.NonInteractive) == 0)
            {
                MouseState currentState = Mouse.GetState();
                _buttonList.Update(currentState, _oldMouseState);
                _catalogList.Update(currentState, _oldMouseState);

                _oldMouseState = currentState;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            if (_isInitialized)
            {
                GraphicsDevice.Clear(Color.WhiteSmoke);
                var spriteBatch = SharedSpriteBatch;

                spriteBatch.Begin();

                _buttonList.Draw(spriteBatch);
                _catalogList.Draw(spriteBatch);
                _header.Draw(spriteBatch);
                _sensorStatus.Draw(spriteBatch);

                spriteBatch.End();
            }
            else
            {
                Initialize();
                LoadContent();
            }
            base.Draw(gameTime);
        }

        public void SwitchCategories(string category)
        {
            foreach (GuiCatalogTile catalogTile in _catalogList.Collection)
            {
                catalogTile.ClickEditSettingsEvent -= GuiCatalogTileButtonWasClicked;
            }
            _catalogList.Collection.Clear();

            foreach (var catalogItem in _catalogManager.GetExercisesByType(category))
            {
                _catalogList.AddCatalogItem(_game, catalogItem.ID, catalogItem.Name, catalogItem.Description);
            }

            _catalogList.Initialize();
            _catalogList.LoadContent(contentManager);

            foreach (GuiCatalogTile catalogTile in _catalogList.Collection)
            {
                catalogTile.ClickEditSettingsEvent += GuiCatalogTileButtonWasClicked;
            }
        }

        public override void OpenScreen()
        {
            SwitchCategories("Arms");

            base.OpenScreen();
        }
    }
}
