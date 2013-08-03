using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SWENG.Criteria;
using SWENG.Service;

namespace SWENG.UserInterface
{
    public class CatalogScreen : Screen
    {
        private readonly Rectangle _viewableArea;
        private readonly GuiDrawable[] _guiDrawable;

        private const float MARGIN = 10f;

        private string _selectedCategory;
        private int _catalogLocation;
        private bool _isInitialized;

        private MouseState _oldMouseState;
        private SpriteFont _spriteFont;
        private Vector2 _listItemPosition;

        private CatalogManager _catalogManager
        {
            get
            {
                return (CatalogManager)Game.Services.GetService(typeof(CatalogManager));
            }
        }

        /// <summary>
        /// Initialize a new instance of the ExerciseScreen class.
        /// </summary>
        /// <param name="game">The related game object.</param>
        /// <param name="viewableArea">The desired canvas size to draw on.</param>
        /// <param name="startingState">The desired starting Screen State</param>
        public CatalogScreen(Game game, Rectangle viewableArea, ScreenState startingState) 
            : base(game)
        {
            ScreenState = startingState;
            _viewableArea = viewableArea;
            /** 
             * Makes it easier to just add visual components without knowing how many items we need in the beginning 
             * This will be converted into an array
             */
            List<GuiDrawable> guiDrawable = new List<GuiDrawable>();

            Title = "Catalog";

            #region Laying out the positions
            Vector2 buttonSize = new Vector2(240f, 60f);
            Vector2 buttonBottom = new Vector2(
                _viewableArea.Right - buttonSize.X + MARGIN,
                _viewableArea.Bottom - buttonSize.Y);

            Vector2 tabSize = new Vector2(120f, 60f);
            Vector2 tabPosition = new Vector2(
                _viewableArea.Left,
                _viewableArea.Top
            );

            Vector2 catalogPosition = new Vector2(
                _viewableArea.Left + MARGIN + tabSize.X,
                _viewableArea.Top + tabSize.Y
            );

            Vector2 catalogSize = new Vector2(
                _viewableArea.Width - buttonSize.X - MARGIN - tabSize.X - MARGIN,
                _viewableArea.Height - tabSize.Y
            );

            /** This is used later; hence, it is global */
            _listItemPosition = new Vector2(
                _viewableArea.Width - buttonSize.X + MARGIN,
                _viewableArea.Top + tabSize.Y
            );
            
            /** 
             * Adding the catalog and getting the location are a 
             * pair for easy reference in the future 
             */
            guiDrawable.Add(
                new GuiScrollableCollection(
                    "Catalog", 
                    catalogSize, 
                    catalogPosition, 
                    5, 
                    90f, 
                    615f
                )
            );
            _catalogLocation = guiDrawable.Count - 1;

            guiDrawable.Add(
                new GuiLabel(
                    "CatalogInstructions",
                    new Vector2(159, 60),
                    new Vector2(
                        _viewableArea.Width - buttonSize.X - 159 + (2 * MARGIN),
                        _viewableArea.Top
                    )
                ));

            guiDrawable.Add(
                new GuiLabel(
                    "Selected",
                    tabSize,
                    new Vector2(
                        _viewableArea.Width - tabSize.X,
                        _viewableArea.Top
                    )
                )
            );

            guiDrawable.Add(
                new GuiButton(
                    "Start", 
                    buttonSize,
                    buttonBottom
                    - (new Vector2(0f, 2 * MARGIN))
                    - (new Vector2(0f, 2 * buttonSize.Y))
                ));
            guiDrawable.Add(
                new GuiButton(
                    "SensorSetup", 
                    buttonSize,
                    buttonBottom
                    - new Vector2(0f, MARGIN)
                    - new Vector2(0f, buttonSize.Y)
                ));
            guiDrawable.Add(
                new GuiButton(
                    "ExitProgram", 
                    buttonSize, 
                    buttonBottom
                ));

            guiDrawable.Add(
                new GuiLabel(
                    "Categories",
                    tabSize,
                    tabPosition
                ));

            tabPosition.Y = tabPosition.Y + tabSize.Y;
            
            foreach (string tabCat in _catalogManager.Categories)
            {
                guiDrawable.Add(
                    new GuiCheckbox(
                        tabCat,
                        tabSize,
                        tabPosition
                    )
                );

                tabPosition.Y = tabPosition.Y + tabSize.Y;
            }

            guiDrawable.Add(
                new GuiSensorStatus(
                    "SensorStatus",
                    new Vector2(99f, 32f),
                    new Vector2(
                        (_viewableArea.Right / 2) - (99f / 2),
                        _viewableArea.Bottom - 32f
                    ),
                    game
                )
            );

            guiDrawable.Add(
                new GuiHeader(
                    "KinectTherapy",
                    new Vector2(326f, 52f),
                    new Vector2(
                        _viewableArea.Left,
                        _viewableArea.Top - MARGIN - 52f
                    )
                )
            );

            #endregion
            _guiDrawable = guiDrawable.ToArray();

            _isInitialized = false;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            _isInitialized = true;

            foreach (GuiDrawable guiDrawable in _guiDrawable)
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

            base.Initialize();
        }

        public override void LoadContent()
        {
            if (contentManager == null)
            {
                contentManager = new ContentManager(Game.Services, "Content");
            }

            _spriteFont = contentManager.Load<SpriteFont>("Arial16");

            foreach (GuiDrawable guiDrawable in _guiDrawable)
            {
                guiDrawable.LoadContent(Game, contentManager, SharedSpriteBatch);
            }

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
            
            if (_isInitialized
                && (ScreenState & UserInterface.ScreenState.NonInteractive) == 0)
            {
                MouseState currentState = Mouse.GetState();
                Rectangle mouseBoundingBox = new Rectangle(currentState.X, currentState.Y, 1, 1);

                foreach (GuiDrawable guiDrawable in _guiDrawable)
                {
                    guiDrawable.Update(currentState, _oldMouseState, mouseBoundingBox, gameTime);
                }

                _oldMouseState = currentState;
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            /** Without initialization then load content will not have happened; makes drawing very difficult */
            if (_isInitialized)
            {
                GraphicsDevice.Clear(Color.WhiteSmoke);
                var spriteBatch = SharedSpriteBatch;

                spriteBatch.Begin();

                foreach (GuiDrawable guiDrawable in _guiDrawable)
                {
                    guiDrawable.Draw(spriteBatch);
                }

                Vector2 itemPosition = _listItemPosition;
                Exercise[] selected = _catalogManager.GetSelectedWorkouts();
                for (int i = 0; i < selected.Length; i++)
                {
                    Vector2 measuredText = _spriteFont.MeasureString(selected[i].Name);
                    itemPosition.X = _viewableArea.Right - measuredText.X;
                    itemPosition.Y += measuredText.Y;

                    spriteBatch.DrawString(
                        _spriteFont,
                        selected[i].Name,
                        itemPosition,
                        Color.Black
                    );
                }

                spriteBatch.End();
            }
            else
            {
                Initialize();
                LoadContent();
            }
            base.Draw(gameTime);
        }

        /// <summary>
        /// Central button click management.
        /// </summary>
        private void GuiButtonWasClicked(object sender, GuiButtonClickedArgs e)
        {
            switch (e.ClickedOn)
            {
                case "Start":
                    if (_catalogManager.GetSelectedWorkouts().Length != 0)
                    {
                        ScreenState = UserInterface.ScreenState.Hidden;
                        OnTransition(new TransitionEventArgs(Title, e.ClickedOn));
                        _catalogManager.Status = CatalogManagerStatus.Complete;
                    }
                    break;
                case "SensorSetup":
                    ScreenState = UserInterface.ScreenState.Active | UserInterface.ScreenState.NonInteractive;
                    OnTransition(new TransitionEventArgs(Title, e.ClickedOn));
                    break;
                case "ExitProgram":
                    OnTransition(new TransitionEventArgs(Title, e.ClickedOn));
                    break;
                default:
                    ChangeCategory(e.ClickedOn);
                    break;
            }
        }

        /// <summary>
        /// Invoke SwitchCategories and change the checked state of the GUI elements.
        /// </summary>
        /// <param name="category">GUI text to match on.</param>
        private void ChangeCategory(string category)
        {
            foreach (GuiDrawable guiDrawable in _guiDrawable)
            {
                if (guiDrawable.Text == category)
                {
                    if (guiDrawable.GetType() == typeof(GuiCheckbox))
                    {
                        ((GuiCheckbox)guiDrawable).Checked = true;
                    }
                }
                else if (guiDrawable.GetType() == typeof(GuiCheckbox))
                {
                    ((GuiCheckbox)guiDrawable).Checked = false;
                }
            }

            SwitchCategories(category);
        }

        private void GuiCatalogTileButtonWasClicked(object sender, EditCatalogSettingsArgs e)
        {
            ScreenState = UserInterface.ScreenState.Active | UserInterface.ScreenState.NonInteractive;
            OnTransition(new TransitionEventArgs(e.ID, "CatalogTileEdit"));
        }

        public void SwitchCategories(string category)
        {
            if (category != _selectedCategory)
            {
                GuiScrollableCollection scrollableCollection = (GuiScrollableCollection)_guiDrawable[_catalogLocation];
                foreach (GuiCatalogTile catalogItem in scrollableCollection.Collection)
                {
                    catalogItem.ClickEditSettingsEvent -= GuiCatalogTileButtonWasClicked;
                }
                
                scrollableCollection.ClearCollection();

                foreach (var catalogItem in _catalogManager.GetExercisesByType(category))
                {
                    GuiCatalogTile guiCatalogTile = new GuiCatalogTile(
                        Game,
                        catalogItem.ID,
                        catalogItem.Name,
                        catalogItem.Description,
                        scrollableCollection.ItemSize,
                        scrollableCollection.GetNextPosition()
                    );

                    guiCatalogTile.ClickEditSettingsEvent += GuiCatalogTileButtonWasClicked;

                    scrollableCollection.AddCatalogItem(guiCatalogTile);
                }

                scrollableCollection.LoadContent(Game, contentManager, SharedSpriteBatch);

                Exercise[] selected = _catalogManager.GetSelectedWorkouts();

                foreach (GuiCatalogTile catalogTile in scrollableCollection.Collection)
                {
                    foreach (Exercise exercise in selected)
                    {
                        if (exercise.Id == catalogTile.ItemID)
                        {
                            catalogTile.SilentSetChecked();
                        }
                    }
                }

                _selectedCategory = category;
            }
        }

        public override void OpenScreen()
        {
            /** New workout means the old workout information should not be retained */
            _catalogManager.ClearWorkout();

            /** Reset the selected category so SwitchCategory loads the exercises */
            _selectedCategory = "";

            string defaultCategory = "Upper";
            SwitchCategories(defaultCategory);

            foreach (GuiDrawable guiDrawable in _guiDrawable)
            {
                if (guiDrawable.Text == defaultCategory)
                {
                    if (guiDrawable.GetType() == typeof(GuiCheckbox))
                    {
                        ((GuiCheckbox)guiDrawable).Checked = true;
                    }
                }
            }

            base.OpenScreen();
        }
    }
}
