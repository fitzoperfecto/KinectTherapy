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
    public class CatalogTileEditScreen : Screen
    {
        private readonly Rectangle _viewableArea;
        private readonly GuiDrawable[] _guiDrawable;

        private const float MARGIN = 10f;

        private string _itemId;
        private int _repetitionIndex;
        private int _varianceIndex;
        private bool _isInitialized;

        private MouseState _oldMouseState;
        private Texture2D _blankTexture;
        private Texture2D _inputBoxTexture;
        private Rectangle _inputBoxDestination;
        private Exercise _exercise;

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
            Dictionary<string, GuiDrawable> _buttonDct = new Dictionary<string, GuiDrawable>();
            _buttonDct.Add(
                "Submit",
                new GuiButton(
                    "Submit",
                    buttonSize,
                    buttonBottom
                    - (new Vector2(2 * MARGIN, 0f))
                    - (new Vector2(2 * buttonSize.Y, 0f))
                )
            );

            _buttonDct.Add(
                "Cancel", 
                new GuiButton("Cancel",
                    buttonSize,
                    buttonBottom
                )
            );

            _buttonDct.Add("Repetitions",
                new GuiInputBox(
                    "Repetitions",
                    new Vector2(100f, 50f),
                    new Vector2(_inputBoxDestination.Right - 100 - MARGIN,
                        optionBottom.Y),
                    game, 10, 100, 0
                )
            );

            _repetitionIndex = _buttonDct.Count - 1;

            _buttonDct.Add("Variance",
                new GuiInputBox(
                    "Variance",
                    new Vector2(100f, 50f),
                    new Vector2(_inputBoxDestination.Right - 100 - MARGIN,
                        optionBottom.Y
                        + MARGIN
                        + optionSize.Y),
                    game, 10, 100, 0
                )
            );

            _varianceIndex = _buttonDct.Count - 1;

            _buttonDct.Add("RepetitionLabel",
                new GuiLabel(
                    "RepetitionLabel",
                    new Vector2(144f, 50f),
                    _buttonDct["Repetitions"].Position
                        - new Vector2(144 + MARGIN, 0)
                )
            );

            _buttonDct.Add("VarianceLabel",
                new GuiLabel(
                    "VarianceLabel",
                    new Vector2(110f, 50f),
                    _buttonDct["Variance"].Position
                        - new Vector2(110 + MARGIN, 0)
                )
            );

            #endregion

            _guiDrawable = new GuiDrawable[_buttonDct.Count];
            _buttonDct.Values.CopyTo(_guiDrawable, 0);

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
                else if (t == typeof(GuiInputBox))
                {
                    ((GuiInputBox)guiDrawable).OnSelected += InputBoxSelected;
                }
            }

            base.Initialize();
        }

        public override void LoadContent()
        {
            if (null == contentManager)
            {
                contentManager = new ContentManager(Game.Services, "Content");
            }

            _blankTexture = contentManager.Load<Texture2D>(@"blank");
            _inputBoxTexture = contentManager.Load<Texture2D>(@"UI\CatalogTileEdit");

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
            if (_isInitialized)
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

                foreach (GuiDrawable guiDrawable in _guiDrawable)
                {
                    guiDrawable.Draw(spriteBatch);
                }

                spriteBatch.End();
            }
            base.Draw(gameTime);
        }


        /// <summary>
        /// Reset input boxes "checked" status.
        /// </summary>
        private void InputBoxSelected(object sender, SelectedEventArgs e)
        {
            foreach (GuiDrawable guiDrawable in _guiDrawable)
            {
                if (guiDrawable.GetType() == typeof(GuiInputBox))
                {
                    if (guiDrawable.Text != e.ID)
                    {
                        ((GuiInputBox)guiDrawable).State = CheckboxState.Default;
                    }
                }
            }
        }

        /// <summary>
        /// Central button click management.
        /// </summary>
        private void GuiButtonWasClicked(object sender, GuiButtonClickedArgs e)
        {
            InputBoxSelected(sender, new SelectedEventArgs(e.ClickedOn));

            switch (e.ClickedOn)
            {
                case "Submit":
                    ScreenState = UserInterface.ScreenState.Hidden;
                    SaveChanges();
                    OnTransition(new TransitionEventArgs(Title, "Return"));
                    break;
                case "Cancel":
                    ScreenState = UserInterface.ScreenState.Hidden;
                    OnTransition(new TransitionEventArgs(Title, "Return"));
                    break;
            }
        }

        /// <summary>
        /// Pass back the data to the CatalogManager
        /// </summary>
        private void SaveChanges()
        {
            int tempInt = 0;
            float tempFloat = 0;
            _exercise = new Exercise();
            _exercise.Id = _itemId;

            /** Check the validity of the data first for the Repitition value */
            GuiInputBox guiInputBox = (GuiInputBox)_guiDrawable[_repetitionIndex];
            if (int.TryParse(guiInputBox.Value, out tempInt))
            {
                _exercise.Repetitions = tempInt;
            }

            /** Check the validity of the data first for the Variance value */
            guiInputBox = (GuiInputBox)_guiDrawable[_varianceIndex];
            if (float.TryParse(guiInputBox.Value, out tempFloat))
            {
                _exercise.Variance = tempFloat;
            }

            _catalogManager.SetExerciseOptions(_exercise);
        }

        /// <summary>
        /// Update the input boxes with the saved data from the CatalogManager.
        /// </summary>
        /// <param name="id">Exercise ID</param>
        public void OpenScreen(string id)
        {
            _exercise = _catalogManager.GetExercise(id);

            _itemId = _exercise.Id;

            GuiInputBox guiInputBox = (GuiInputBox)_guiDrawable[_repetitionIndex];
            guiInputBox.Value = _exercise.Repetitions.ToString();

            guiInputBox = (GuiInputBox)_guiDrawable[_varianceIndex];
            guiInputBox.Value = _exercise.Variance.ToString();
        }
    }
}
