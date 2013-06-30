using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.XPath;
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

        private ContentManager _contentManager;
        private Texture2D _blankTexture;
        private SpriteFont _spriteFont;

        private bool _isInitialized;

        private MouseState _oldMouseState;
        private const int MARGIN = 10;

        private readonly Rectangle _viewableArea;

        private readonly GuiButton[] _buttonList;
        private readonly GuiButton[] _workoutButtonList;

        private readonly GuiLabel _banner;

        #region Catalog Control Variables

        private readonly Vector2 _catalogViewPosition;
        private readonly Vector2 _catalogViewSize;
        private readonly SensorTile _catalogViewArea;

        private XPathDocument  _catalogXml;
        private List<string> _catalogList;

        private readonly string _catalogFile;

        #endregion

        #region Workout Exercise Control Variables

        private bool _isExerciseClicked;
        private bool _isWorkoutClicked;

        private GuiButton[] _exerciseButtonList;
        private List<string> _workoutList;
        private string[] _exercisesSelected;

        private ExerciseQueue _exerciseQueue;
        #endregion
    
        
        public CatalogScreen(Game game, Rectangle viewableArea, ScreenState startingState) : base(game)
        {
            ScreenState = startingState;

            _viewableArea = viewableArea;
            var buttonSize = new Vector2(100, 30f);

            Title = "Catalog";

            _catalogViewArea = new SensorTile(game, "Exercise Catalog");

            // Load exercise catalog first
            //var applicationDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            //_catalogFile = applicationDirectory + @"\Catalog\ExerciseCatalog.xml";

            //if (!File.Exists(_catalogFile)) return;

            //_catalogList = ReadXPathCatalogWorkouts(_catalogManager.CatalogFile);

            #region Laying out the positions
            var bannerSize = new Vector2(_viewableArea.Width, 110f);
            var bannerStartingPosition = new Vector2(0, 50);
            _banner = new GuiLabel("Session Setup", bannerSize, bannerStartingPosition);

            _catalogViewPosition = new Vector2(
                    viewableArea.X, viewableArea.Y + bannerSize.Y
                );

            _catalogViewSize = new Vector2(
                    (viewableArea.Width),
                    400
                );

            _catalogViewArea.Position = _catalogViewPosition;
            _catalogViewArea.Size = _catalogViewSize;

            // Construct Workout Selection Buttons
            var workoutButtonSize = new Vector2(170, 30f);

            var workoutButtonX = _catalogViewPosition.X + (MARGIN * 2);
            var workoutButtonY = _catalogViewPosition.Y + (MARGIN * 3);
            var workoutButtonPos = new Vector2(workoutButtonX, workoutButtonY);

            var i = 0;
            _workoutButtonList = new GuiButton[_catalogManager.GetExercisesByType("Arms").Count];
            WorkoutTitle = new string[_catalogManager.GetExercisesByType("Arms").Count];

            foreach (var listitem in _catalogManager.GetExercisesByType("Arms"))
            {
                WorkoutTitle[i] = listitem;
                _workoutButtonList[i] = new GuiButton(listitem, workoutButtonSize, workoutButtonPos);
                workoutButtonPos.X = workoutButtonPos.X + workoutButtonSize.Length() + (MARGIN * 10);
                i++;
            }

            // Construct Page Navigation Buttons
            var exerciseButtonPosition = new Vector2(
                (
                    (_catalogViewPosition.X + (_catalogViewSize.X * .5f)) - (buttonSize.X / 2)
                ),
                (
                    _viewableArea.Height + 50
                )
            );

            var homeButtonPosition = new Vector2(
                    exerciseButtonPosition.Length() / 4,
                    exerciseButtonPosition.Y
                );

            _buttonList = new[]{
                new GuiButton("Start", buttonSize, exerciseButtonPosition),
                new GuiButton("Home", buttonSize, homeButtonPosition)
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
            var i = 0;

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
                                // Set Catalog Selection status to start
                                _catalogManager.Status = CatalogManagerStatus.Complete;
                                Transition();
                               // Manager.CallOpen("Exercise");
                                break;

                            case "Home":
                                Transition();
                                Manager.CallOpen("The Hub");
                                break;
                        }
                    }
                }
                else
                {
                    button.Hovered = false;
                }
            }

            foreach (var workoutButton in _workoutButtonList)
            {
                if (mouseBoundingBox.Intersects(workoutButton.Rectangle))
                {
                    workoutButton.Hovered = true;

                    if (mouseState.LeftButton == ButtonState.Pressed && mouseState.LeftButton != _oldMouseState.LeftButton)
                    {
                        _isWorkoutClicked = true;

                        LoadCatalog(_workoutButtonList[i].Text);
                        Draw(gameTime);
                    }
                }
                else
                {
                    workoutButton.Hovered = false;
                }
            }

            if (_isWorkoutClicked)
            {
                foreach (var exerciseButton in _exerciseButtonList)
                {
                    if (mouseBoundingBox.Intersects(exerciseButton.Rectangle))
                    {
                        exerciseButton.Hovered = true;

                        if (mouseState.LeftButton == ButtonState.Pressed && mouseState.LeftButton != _oldMouseState.LeftButton)
                        {
                            _isExerciseClicked = true;
                            _exercisesSelected = new string[_exerciseButtonList.Length];
                            WorkoutExercises = new string[_exerciseButtonList.Length];

                            var idxPosStart = exerciseButton.Text.IndexOf("(", StringComparison.Ordinal);
                            var idxPosStop = exerciseButton.Text.IndexOf(")", StringComparison.Ordinal);
                            var length = (idxPosStop - idxPosStart) - 1;

                            _exercisesSelected[i] = exerciseButton.Text.Substring(idxPosStart + 1, length);
                        }
                    }
                    else
                    {
                        exerciseButton.Hovered = false;
                    }
                }
            }
            
            _oldMouseState = mouseState;

            _catalogViewArea.Update(gameTime);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (_isInitialized)
            {
                GraphicsDevice.Clear(Color.White);

                _catalogViewArea.Draw(gameTime);

                SharedSpriteBatch.Begin();

                foreach (var button in _workoutButtonList)
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
                    new Vector2(300, 100),
                    Color.White
                );

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


                if (_isWorkoutClicked)
                {
                    foreach (var button in _exerciseButtonList)
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

                }

                SharedSpriteBatch.End();
            }


            base.Draw(gameTime);
        }

        private void LoadCatalog(string workoutTitle)
        {
            // Set Catalog Selection status to Started
            _catalogManager.Status = CatalogManagerStatus.Start;

            var i = 0;

            var workoutViewPosY = 300;
            //var exercises = ReadXPathWorkoutExercises(workoutTitle);
            //_exerciseButtonList = new GuiButton[exercises.Count];

            //foreach (var exercise in exercises)
            //{
            //    var stringSize = _spriteFont.MeasureString(exercise);

            //    var workoutViewSize = stringSize;
            //    var workoutViewPos = new Vector2(20, workoutViewPosY + MARGIN);

            //    _exerciseButtonList[i] = new GuiButton(exercise, workoutViewSize, workoutViewPos);
            //    workoutViewPosY += (MARGIN * 5);
            //    i++;
            //}
        }


        public string WorkoutName { get; set; }

        public string[] WorkoutTitle { get; set; }

        public string[] WorkoutExercises { get; set; }

    }
}
