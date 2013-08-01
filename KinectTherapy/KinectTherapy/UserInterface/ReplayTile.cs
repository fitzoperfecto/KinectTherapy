using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SWENG.Service;

namespace SWENG.UserInterface
{
    public delegate void ReplaySelected(object sender, ReplaySelectedEventArgs e);

    public class ReplaySelectedEventArgs : EventArgs
    {
        public string ID;

        public ReplaySelectedEventArgs(string id)
        {
            ID = id;
        }
    }

    /// <summary>
    /// GUI box that displays information regarding a workout.
    /// </summary>
    public class ReplayTile : GuiDrawable
    {
        #region event stuff
        public event ReplaySelected OnSelected;
        private Game _game;
        /** Invoke a selected event in order to tell the subscribers */
        protected virtual void ReplaySelected(ReplaySelectedEventArgs e)
        {
            if (OnSelected != null)
                OnSelected(this, e);
        }
        #endregion

        private const int HEADER = 40;
        private const int SCROLLRATE = 5;
        private const int MARGIN = 10;
        private const double MILLISECONDS = 100;

        private string _repetitionSentence;
        private int _repetitionNumber;
        private double _oldGameTime;

        private SkeletonStamp[] _skeletons;
        private Texture2D _blank;
        private Texture2D _titleTexture;
        private Rectangle _titleDestination;
        private Rectangle _titleSource;
		private GuiChart _chartTexture;

        public string Title { get; private set; }
        public int ExerciseIndex { get; private set; }
        public string FileId { get; private set; }

        /// <summary>
        /// Construct a 
        /// </summary>
        /// <param name="size"></param>
        /// <param name="position"></param>
        /// <param name="fileId">File ID to send to any click event.</param>
        /// <param name="exerciseName">This becomes the drawable's text.</param>
        /// <param name="repetitionNumber">The repetition the replay refers to.</param>
        public ReplayTile(Vector2 size, Vector2 position, string fileId, string exerciseName, int repetitionNumber)
            : base(exerciseName, size, position)
        {
            Title = string.Format("{0}: {1}",
                repetitionNumber,
                exerciseName
            );
            FileId = fileId;
            _repetitionNumber = repetitionNumber;
            _repetitionSentence = string.Format(
                "Rep: {0}",
                _repetitionNumber
            );
        }

        /// <summary>
        /// Load relevant resources
        /// </summary>
        /// <param name="game">Provide access to the game.</param>
        /// <param name="contentManager">Provide access to the screen's content manager.</param>
        /// <param name="spriteBatch">Provide access to the screen's sprite batch.</param>
        public override void LoadContent(Game game, ContentManager contentManager, SpriteBatch spriteBatch)
        {
            if (contentManager == null)
            {
                return;
            }
			
            Texture2D = contentManager.Load<Texture2D>(@"UI\ReplayTile");
            _blank = contentManager.Load<Texture2D>(@"blank");
            _titleTexture = CreateTitleTexture(game, contentManager, spriteBatch);
            _titleDestination = new Rectangle(
                (int)Position.X + MARGIN,
                (int)Position.Y + MARGIN,
                _titleTexture.Width,
                _titleTexture.Height
            );

            _game = game;

            /** Grab the data off of the recording manager now because we don't have a reference to the game forever */
            RecordingManager recorder = (RecordingManager)game.Services.GetService(typeof(RecordingManager));
            _skeletons = recorder.ReadProcessedData(FileId);

            string[] axesNames = { "100 Millisecond Intervals", "Average\n\rTotal\n\rDeviation" };
            string chartType = "Repetitions";
            bool chartLines = true;
            bool tickMarks = false;
            float scale = 1f;
            float repetitionDuration = 0f;
            float[] dataPoints = CalculateSkeletonChartLine(_skeletons, out repetitionDuration);
            float timeSpan;
            timeSpan = dataPoints.Length;

            GuiChartOptions chartOptions = new GuiChartOptions(axesNames, chartType, chartLines, tickMarks, scale, dataPoints, timeSpan, repetitionDuration);

            _chartTexture = new GuiChart(
                "Text",
                Size - new Vector2(MARGIN * 2, MARGIN + 30f),
                Position + new Vector2(MARGIN, _titleTexture.Height),
                chartOptions
            );

            _chartTexture.LoadContent(game, contentManager, spriteBatch);

            _titleSource = new Rectangle(
                0,
                0,
                _titleTexture.Width,
                _titleTexture.Height
            );
        }

        private float[] CalculateSkeletonChartLine(SkeletonStamp[] skeletons, out float repetitionDuration)
        {
            List<float> consolidatedTimeSpans = new List<float>();
            int listLength = 0;
            float sumOfAbsoluteAmounts = 0;
            float numberOfInstances = 0;
            int dataPoints = 0;
            repetitionDuration = 0f;

            foreach (SkeletonStamp stamp in skeletons)
            {
                repetitionDuration += stamp.TimeStamp;

                if (repetitionDuration - (100 * dataPoints) >= 100)
                {
                    ++dataPoints;

                    consolidatedTimeSpans.Add(0f);
                    listLength = consolidatedTimeSpans.Count - 1;

                    if (consolidatedTimeSpans[listLength] != null)
                    {
                        consolidatedTimeSpans[listLength] = sumOfAbsoluteAmounts / numberOfInstances;
                    }

                    sumOfAbsoluteAmounts = 0;
                    numberOfInstances = 0;
                }

                foreach (double d in stamp.PercentBad)
                {
                    sumOfAbsoluteAmounts += (float)Math.Abs(d);
                }

                ++numberOfInstances;
            }

            return consolidatedTimeSpans.ToArray();
        }

        /// <summary>
        /// Use RenderTarget2D to create a custom title texture.
        /// </summary>
        /// <param name="game">Provide access to the game.</param>
        /// <param name="contentManager">Provide access to the screen's content manager.</param>
        /// <param name="spriteBatch">Provide access to the screen's sprite batch.</param>
        /// <returns>Custom title texture</returns>
        private Texture2D CreateTitleTexture(Game game, ContentManager contentManager, SpriteBatch spriteBatch)
        {
            SpriteFont spriteFont = contentManager.Load<SpriteFont>("Arial16");
            Vector2 textMeasure = spriteFont.MeasureString(Title);
            RenderTarget2D renderTarget2d = new RenderTarget2D(game.GraphicsDevice, (int)textMeasure.X, (int)textMeasure.Y);

            game.GraphicsDevice.SetRenderTarget(renderTarget2d);
            game.GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 0, 0);

            spriteBatch.Begin();

            spriteBatch.DrawString(
                spriteFont,
                Title,
                Vector2.Zero,
                Color.White
            );

            spriteBatch.End();

            game.GraphicsDevice.SetRenderTarget(null);

            return (Texture2D)renderTarget2d;
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(MouseState currentState, MouseState oldMouseState, Rectangle mouseBoundingBox, GameTime gameTime)
        {
            double currentGameTime = gameTime.TotalGameTime.TotalMilliseconds;

            if (currentGameTime - _oldGameTime >= MILLISECONDS)
            {
                if (_titleSource.Width >= Texture2D.Width - (2 * MARGIN))
                {
                    _titleSource.X += SCROLLRATE;

                    if (_titleSource.X > _titleTexture.Width)
                    {
                        _titleSource.X = 0;
                    }
                }
            }

            if (mouseBoundingBox.Intersects(Rectangle))
            {
                if (currentState.LeftButton == ButtonState.Released
                    && oldMouseState.LeftButton == ButtonState.Pressed)
                {
                    ReplaySelected(new ReplaySelectedEventArgs(FileId));
                }
            }

            _chartTexture.Update(currentState, oldMouseState, mouseBoundingBox, gameTime);
			
            _oldGameTime = currentGameTime;
        }

        /// <summary>
        /// This method renders the current state of the element to the screen.
        /// </summary>
        /// <param name="spriteBatch">A SpriteBatch that has begun.</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_titleTexture != null)
            {
                spriteBatch.Draw(
                    _blank,
                    new Rectangle((int)Position.X - 5, (int)Position.Y - 5, (int)Size.X, (int)Size.Y),
                    Color.Red
                );
                spriteBatch.Draw(
                    Texture2D,
                    Position,
                    Color.White
                );

                spriteBatch.Draw(
                    _titleTexture,
                    _titleDestination,
                    _titleSource,
                    Color.White
                );

                _chartTexture.Draw(spriteBatch);
            }
        }
    }
}
