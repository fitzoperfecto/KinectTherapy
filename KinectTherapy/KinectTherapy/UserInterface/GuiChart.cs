using System;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SWENG.UserInterface
{
    class GuiChart : GuiDrawable
    {
        #region Game related objects

        private SpriteFont _spriteFont;

        #endregion

        #region Chart objects and variables
        private readonly string _xAxisLabel;
        private readonly string _yAxisLabel;
        private readonly string _chartType;

        private readonly bool _tickMarks;
        private readonly bool _chartLines;
        private readonly float[] _dataPoints;
        private readonly float _timeSpan;
        private readonly float _repDuration;

        private float _min;
        private float _max;

        private Texture2D _xAxisTitleTexture;
        private Texture2D _yAxisTitleTexture;
        private Texture2D _chartMarkerTexture;
        private Texture2D _dataPointTexture;

        private Vector2 _xAxisMeasure;
        private Vector2 _yAxisMeasure;

        private Rectangle _dataPointTextureDestination;
        private Rectangle _texture2DRectangle;
        #endregion

        #region Mouse handler variables
        public float MouseXCoord { get; set; }
        public float MouseYCoord { get; set; }
        public float MouseXPercent { get; set; }
        public float MouseYPercent { get; set; }
        #endregion

        #region Miscellaneous variables
        private readonly Vector2 _position;

        private const int MARGIN = 1;
        private const int Scale = 10000;
        private const int MarkerSize = 2;

        #endregion

        /// <summary>
        /// GuiChart is a class that handles taking datapoints from the Summary Screen and creates a chart object to be displayed and returns a percentage
        /// value of where in the chart between x-axis begin and x-axis end the mouse was clicked.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="size"></param>
        /// <param name="position"></param>
        /// <param name="chartOptions"></param>
        public GuiChart(string text, Vector2 size, Vector2 position, GuiChartOptions chartOptions)
            : base(text, size, position)
        {
            _xAxisLabel = chartOptions.AxesName[0];
            _yAxisLabel = chartOptions.AxesName[1];
            _chartType = chartOptions.ChartType;
            _tickMarks = chartOptions.TickMarks;
            _dataPoints = chartOptions.DataPoints;
            _timeSpan = chartOptions.TimeInterval;
            _repDuration = chartOptions.RepDuration;
            _chartLines = chartOptions.ChartLines;

            _position = position;
        }

        /// <summary>
        /// This method renders the current state of the screen
        /// </summary>
        /// <param name="spriteBatch"></param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Texture2D == null) return;

            spriteBatch.Draw(
                Texture2D,
                _texture2DRectangle,
                null,
                Color.White
              );

            spriteBatch.Draw(
                _xAxisTitleTexture,
                new Vector2(_texture2DRectangle.Right * 0.5f, _texture2DRectangle.Bottom),
                Color.White
                );

            if (_chartType.Equals("Time"))
            {
                spriteBatch.DrawString(
                    _spriteFont,
                    _repDuration.ToString(CultureInfo.InvariantCulture),
                    new Vector2(_texture2DRectangle.Right - (MARGIN * 30), _texture2DRectangle.Bottom),
                    Color.Blue
                    );
            }

            spriteBatch.Draw(
                _yAxisTitleTexture,
                new Vector2((_texture2DRectangle.X - _yAxisMeasure.X), (_texture2DRectangle.Bottom - (_texture2DRectangle.Height / 2)) - MARGIN),
                Color.White
                );

            spriteBatch.DrawString(
                _spriteFont,
                _min.ToString(CultureInfo.InvariantCulture),
                new Vector2(_texture2DRectangle.Left - (MARGIN * 55), _texture2DRectangle.Bottom - MARGIN),
                Color.Blue
                );

            spriteBatch.DrawString(
                _spriteFont,
                _max.ToString(CultureInfo.InvariantCulture),
                new Vector2(_texture2DRectangle.Left - (MARGIN * 55), _texture2DRectangle.Top),
                Color.Blue
            );

            spriteBatch.Draw(
                _dataPointTexture,
                _dataPointTextureDestination,
                Color.White
            );
        }

        /// <summary>
        /// This method creates a new content manager and loads all textures and fonts
        /// </summary>
        /// <param name="game"></param>
        /// <param name="contentManager"></param>
        /// <param name="spriteBatch"></param>
        public override void LoadContent(Game game, ContentManager contentManager, SpriteBatch spriteBatch)
        {
            if (contentManager == null) return;

            _chartMarkerTexture = contentManager.Load<Texture2D>(@"blank");

            _xAxisTitleTexture = CreateXAxisTitleTexture(game, contentManager, spriteBatch);
            _yAxisTitleTexture = CreateYAxisTitleTexture(game, contentManager, spriteBatch);

            Texture2D = contentManager.Load<Texture2D>(@"UI\ChartTexture");
            _texture2DRectangle = new Rectangle(
                (int)((int)_position.X + (_yAxisMeasure.Length() + 15)), 
                (int)_position.Y,
                (int)((int)Size.X - (_yAxisMeasure.X + (MARGIN * 9))),
                (int)((int)Size.Y - _yAxisMeasure.Y - MARGIN)
            );

            _dataPointTextureDestination = new Rectangle(_texture2DRectangle.X + 5, _texture2DRectangle.Y, _texture2DRectangle.Width - 5, _texture2DRectangle.Height);
            _dataPointTexture = DrawDataPointTexture(game, spriteBatch, _timeSpan);
        }

        /// <summary>
        /// This method provides mouse handling functionality for the chart and returns the x-coord. and y-coord.
        /// as percentages
        /// </summary>
        /// <param name="mouseState"></param>
        /// <param name="oldMouseState"></param>
        /// <param name="mouseBoundingBox"></param>
        /// <param name="gameTime"></param>
        public override void Update(MouseState mouseState, MouseState oldMouseState, Rectangle mouseBoundingBox, GameTime gameTime)
        {
            if (!mouseBoundingBox.Intersects(Rectangle)) return;

            if (mouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed)
            {
                MouseXCoord = mouseState.X;
                MouseXPercent = 100 - ((MouseXCoord / Rectangle.Width) * 100);

                MouseYCoord = mouseState.Y;
                MouseYPercent = 100 - ((MouseYCoord / Rectangle.Height) * 100);
            }
        }

        /// <summary>
        /// Creates the game texture used to create the render target for the x-axis legend 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="contentManager"></param>
        /// <param name="spriteBatch"></param>
        /// <returns></returns>
        private Texture2D CreateXAxisTitleTexture(Game game, ContentManager contentManager, SpriteBatch spriteBatch)
        {
            _spriteFont = contentManager.Load<SpriteFont>("Arial10");
            _xAxisMeasure = _spriteFont.MeasureString(_xAxisLabel);
            RenderTarget2D renderTarget2d = new RenderTarget2D(game.GraphicsDevice, (int)_xAxisMeasure.X, (int)_xAxisMeasure.Y);

            game.GraphicsDevice.SetRenderTarget(renderTarget2d);
            game.GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 0, 0);

            spriteBatch.Begin();

            spriteBatch.DrawString(
                _spriteFont,
                _xAxisLabel,
                Vector2.Zero,
                Color.Blue
            );

            spriteBatch.End();

            game.GraphicsDevice.SetRenderTarget(null);

            return renderTarget2d;
        }

        /// <summary>
        /// Creates the game texture used to create the render target for the y-axis legend 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="contentManager"></param>
        /// <param name="spriteBatch"></param>
        /// <returns></returns>
        private Texture2D CreateYAxisTitleTexture(Game game, ContentManager contentManager, SpriteBatch spriteBatch)
        {
            SpriteFont spriteFont = contentManager.Load<SpriteFont>("Arial10");
            _yAxisMeasure = spriteFont.MeasureString(_yAxisLabel);
            RenderTarget2D renderTarget2d = new RenderTarget2D(game.GraphicsDevice, (int)_yAxisMeasure.X, (int)_yAxisMeasure.Y);

            game.GraphicsDevice.SetRenderTarget(renderTarget2d);
            game.GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 0, 0);

            spriteBatch.Begin();

            spriteBatch.DrawString(
                spriteFont,
                _yAxisLabel,
                Vector2.Zero,
                Color.Blue
            );

            spriteBatch.End();

            game.GraphicsDevice.SetRenderTarget(null);

            return renderTarget2d;
        }

        /// <summary>
        /// Creates the game texture used to create the render target for the chart data points 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="timeSpan"></param>
        private Texture2D DrawDataPointTexture(Game game, SpriteBatch spriteBatch, float timeSpan)
        {
            int height = _texture2DRectangle.Bottom + MARGIN * 2;
            Rectangle dataPointDestination = Rectangle.Empty;

            float[] dataPoints = new float[_dataPoints.Length];
            _dataPoints.CopyTo(dataPoints, 0);

            /** Min and Max values for indexing */
            float min = (float)Math.Round(dataPoints.Min(), 6);
            float max = (float)Math.Round(dataPoints.Max(), 6);

            /** Min and Max values for labeling */
            _min = (float)Math.Round(min, 2);
            _max = (float)Math.Round(max, 2);

            /** Get difference for scaling */
            float difference = max - min;
            difference = (float)Math.Round(difference, 6);

            /** Get the newly indexed values and round because of sig figs */
            for (int i = 0; i < dataPoints.Length; ++i)
            {
                float indexedPoint = (float)Math.Round(dataPoints[i], 6) - min;
                dataPoints[i] = GetYScaledLocation(indexedPoint, difference, height);
            }

            RenderTarget2D renderTarget2D = new RenderTarget2D(game.GraphicsDevice, _texture2DRectangle.Width, height);
            game.GraphicsDevice.SetRenderTarget(renderTarget2D);
            game.GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 0, 0);

            spriteBatch.Begin();

            Vector2 oldPoint = Vector2.Zero;
            for (int i = 0; i < dataPoints.Length; ++i)
            {
                Vector2 point = new Vector2((i * _texture2DRectangle.Width) / timeSpan, dataPoints[i]);

                dataPointDestination = new Rectangle((int)point.X, (int)point.Y, _chartMarkerTexture.Width * MarkerSize, _chartMarkerTexture.Height * MarkerSize);

                spriteBatch.Draw(_chartMarkerTexture, dataPointDestination, Color.Red);

                if (_chartLines && i != 0)
                {
                    Vector2 diff = point - oldPoint;
                    Vector2 scale = new Vector2(1.0f, diff.Length() / _chartMarkerTexture.Width);
                    float angle = (float)Math.Atan2(diff.Y, diff.X) - MathHelper.PiOver2;

                    spriteBatch.Draw(_chartMarkerTexture, oldPoint, null, Color.DarkRed, angle, new Vector2(0.5f, 0f), scale, SpriteEffects.None, 1.0f);
                }

                if (_tickMarks)
                    DrawXInterval(spriteBatch, i.ToString(CultureInfo.InvariantCulture), new Vector2((int)point.X, _texture2DRectangle.Height + MARGIN));

                oldPoint = point;
            }

            spriteBatch.End();

            game.GraphicsDevice.SetRenderTarget(null);

            return renderTarget2D;
        }

        /// <summary>
        /// Return the scaled y location
        /// </summary>
        /// <param name="y">Indexed Y location</param>
        /// <param name="difference">The top and bottom value on the chart</param>
        /// <param name="height">The height of the chart plot</param>
        private float GetYScaledLocation(float y, float difference, float height)
        {
            float percentOfDifference = y / difference;

            return percentOfDifference * height;
        }

        /// <summary>
        /// Creates the game texture used to create the render target for the x-axis tick marks 
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="text"></param>
        /// <param name="location"></param>
        private void DrawXInterval(SpriteBatch sb, string text, Vector2 location)
        {
            sb.DrawString(_spriteFont, text, location, Color.Blue);
        }
    }
}