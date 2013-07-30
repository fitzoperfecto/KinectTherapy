using System;
using System.Globalization;
using System.IO;
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
        private readonly float _scale;
        private readonly float _timeSpan;
        private readonly float _repDuration;

        private Texture2D _xAxisTitleTexture;
        private Texture2D _yAxisTitleTexture;
        private Texture2D _chartMarkerTexture;
        private Texture2D _yAxisIntervalTexture;
        private Texture2D _dataPointTexture;

        private Rectangle _yIntervalDestination;
        private Rectangle _yIntervalSource;

        private Rectangle _xAxisDestination;
        private Rectangle _xSource;
        private Rectangle _yAxisDestination;
        private Rectangle _ySource;
        private Rectangle _dataPointDestination;
        private Rectangle _dataPointTextureDestination;
        private Rectangle _dataPointTextureSource;

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
        private readonly Vector2 _size;

        private const int MARGIN = 10;
        private const int MarkerSize = 2;
        #endregion

        public GuiChart(string text, Vector2 size, Vector2 position)
            : base(text, size, position)
        { }

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
            _scale = chartOptions.Scale;
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

            if (Texture2D != null)
            {
                spriteBatch.Draw(
                    Texture2D,
                    new Vector2(_yAxisDestination.X * _scale, _scale),
                    null,
                    Color.White,
                    0,
                    new Vector2(_position.X, _position.Y),
                    _scale,
                    SpriteEffects.None,
                    0
                  );

                spriteBatch.Draw(
                    _xAxisTitleTexture,
                    new Vector2(_xAxisDestination.X * _scale, (_xAxisDestination.Y * _scale) + MarkerSize * 2),
                    null,
                    Color.White,
                    0,
                    new Vector2(_position.X, _position.Y),
                    _scale,
                    SpriteEffects.None,
                    0
                  );

                if (_chartType.Equals("Time"))
                {
                    spriteBatch.DrawString(
                        _spriteFont,
                        (_repDuration * .001f).ToString(CultureInfo.InvariantCulture),
                        new Vector2((_yAxisTitleTexture.Width) + _texture2DRectangle.Width, _texture2DRectangle.Height + MARGIN),
                        Color.Blue,
                        0,
                        new Vector2(_position.X, _position.Y),
                        _scale,
                        SpriteEffects.None,
                        0
                      );
                }

                spriteBatch.Draw(
                    _yAxisTitleTexture,
                    new Vector2(_yAxisDestination.X * _scale, _yAxisDestination.Y * _scale),
                    null,
                    Color.White,
                    0,
                    new Vector2(_position.X, _position.Y),
                    _scale,
                    SpriteEffects.None,
                    0
                  );

                spriteBatch.Draw(
                    _yAxisIntervalTexture,
                    new Vector2(_yIntervalDestination.X * _scale, _yIntervalDestination.Y * _scale),
                    null,
                    Color.White,
                    0,
                    new Vector2(_position.X, _position.Y),
                    _scale,
                    SpriteEffects.None,
                    0
                  );

                spriteBatch.Draw(
                    _dataPointTexture,
                    new Vector2((_yAxisTitleTexture.Width * _scale) + MARGIN, MARGIN),
                    null,
                    Color.White,
                    0,
                    new Vector2(_position.X, _position.Y),
                    _scale,
                    SpriteEffects.None,
                    0
                    );

            }

        }

        /// <summary>
        /// This method creates a new content manager and loads all textures and fonts
        /// </summary>
        /// <param name="game"></param>
        /// <param name="contentManager"></param>
        /// <param name="spriteBatch"></param>
        public override void LoadContent(Game game, ContentManager contentManager, SpriteBatch spriteBatch)
        {
            if (null == contentManager) return;

            //Texture2D = contentManager.Load<Texture2D>(@"UI\ChartTexture");
            //_texture2DRectangle = new Rectangle((int)_position.X, (int)_position.Y, Texture2D.Width, Texture2D.Height);

            Texture2D = CreateChartTexture(game, contentManager, spriteBatch);

            _chartMarkerTexture = contentManager.Load<Texture2D>(@"blank");

            _xAxisTitleTexture = CreateXAxisTitleTexture(game, contentManager, spriteBatch);
            _yAxisTitleTexture = CreateYAxisTitleTexture(game, contentManager, spriteBatch);

            _xAxisTitleTexture = CreateXAxisTitleTexture(game, contentManager, spriteBatch);
            _xAxisDestination = new Rectangle(
                _texture2DRectangle.Width / 2,
                _texture2DRectangle.Height + (MARGIN * 3),
                _xAxisTitleTexture.Width,
                _xAxisTitleTexture.Height
            );

            _xSource = new Rectangle(
                0,
                0,
                _xAxisTitleTexture.Width,
                _xAxisTitleTexture.Height
            );

            _yAxisTitleTexture = CreateYAxisTitleTexture(game, contentManager, spriteBatch);
            _yAxisDestination = new Rectangle(
                MARGIN,
                _texture2DRectangle.Height / 2,
                _yAxisTitleTexture.Width,
                _yAxisTitleTexture.Height
            );

            _ySource = new Rectangle(
                0,
                0,
                _yAxisTitleTexture.Width,
                _yAxisTitleTexture.Height
            );

            _yAxisIntervalTexture = CreateYAxisIntervalTexture(game, contentManager, spriteBatch);
            _yIntervalDestination = new Rectangle(
                _yAxisTitleTexture.Width,
                Texture2D.Bounds.Top,
                _yAxisIntervalTexture.Width,
                _yAxisIntervalTexture.Height
            );

            _yIntervalSource = new Rectangle(
                0,
                0,
                _yAxisIntervalTexture.Width,
                _yAxisIntervalTexture.Height
            );

            _dataPointTexture = DrawDataPointTexture(game, spriteBatch, _dataPoints, _timeSpan);
            _dataPointTextureDestination = new Rectangle(_texture2DRectangle.X, _texture2DRectangle.Y, _dataPointTexture.Width, _dataPointTexture.Height);
            _dataPointTextureSource = new Rectangle(0, 0, _dataPointTexture.Width, _dataPointTexture.Height);

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
            if (!mouseBoundingBox.Intersects(_texture2DRectangle)) return;

            if (mouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed)
            {
                MouseXCoord = mouseState.X;
                MouseXPercent = 100 - ((MouseXCoord / Rectangle.Width) * 100);

                MouseYCoord = mouseState.Y;
                MouseYPercent = 100 - ((MouseYCoord / Rectangle.Height) * 100);
            }
        }

        /// <summary>
        /// Creates the game texture used to create the chart background
        /// </summary>
        /// <param name="game"></param>
        /// <param name="contentManager"></param>
        /// <param name="spriteBatch"></param>
        public Texture2D CreateChartTexture(Game game, ContentManager contentManager, SpriteBatch spriteBatch)
        {
            Texture2D = contentManager.Load<Texture2D>(@"UI\ChartTexture");
            _texture2DRectangle = new Rectangle((int)_position.X, (int)_position.Y, Texture2D.Width, Texture2D.Height);

            RenderTarget2D renderTarget2D = new RenderTarget2D(game.GraphicsDevice, Texture2D.Width + 50, Texture2D.Height + 50);
            game.GraphicsDevice.SetRenderTarget(renderTarget2D);
            game.GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 0, 0);

            spriteBatch.Begin();

            spriteBatch.Draw(Texture2D, _texture2DRectangle, Color.White);

            spriteBatch.End();
            game.GraphicsDevice.SetRenderTarget(null);

            return renderTarget2D;
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
            _spriteFont = contentManager.Load<SpriteFont>("Arial12");
            Vector2 xAxisMeasure = _spriteFont.MeasureString(_xAxisLabel);
            RenderTarget2D renderTarget2d = new RenderTarget2D(game.GraphicsDevice, (int)xAxisMeasure.X, (int)xAxisMeasure.Y);

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
            SpriteFont spriteFont = contentManager.Load<SpriteFont>("Arial12");
            Vector2 yAxisMeasure = spriteFont.MeasureString(_yAxisLabel);
            RenderTarget2D renderTarget2d = new RenderTarget2D(game.GraphicsDevice, (int)yAxisMeasure.X, (int)yAxisMeasure.Y);

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
        /// Creates the game texture used to create the render target for the y-axis values 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="contentManager"></param>
        /// <param name="spriteBatch"></param>
        /// <returns></returns>
        private Texture2D CreateYAxisIntervalTexture(Game game, ContentManager contentManager, SpriteBatch spriteBatch)
        {
            SpriteFont spriteFont = contentManager.Load<SpriteFont>("Arial12");
            Vector2 yAxisMeasure = spriteFont.MeasureString(_yAxisLabel);
            RenderTarget2D renderTarget2d = new RenderTarget2D(game.GraphicsDevice, (int)yAxisMeasure.X, (int)yAxisMeasure.Y);

            game.GraphicsDevice.SetRenderTarget(renderTarget2d);
            game.GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 0, 0);

            spriteBatch.Begin();

            spriteBatch.DrawString(
                spriteFont,
                "1",
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
        /// <param name="dataPoints"></param>
        /// <param name="timeSpan"></param>
        private Texture2D DrawDataPointTexture(Game game, SpriteBatch spriteBatch, float[] dataPoints, float timeSpan)
        {
            int i;
            float x;
            float y;

            RenderTarget2D renderTarget2D = new RenderTarget2D(game.GraphicsDevice, _texture2DRectangle.Width, _texture2DRectangle.Bottom + MARGIN * 2);
            game.GraphicsDevice.SetRenderTarget(renderTarget2D);
            game.GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 0, 0);

            spriteBatch.Begin();

            for (i = 0; i < dataPoints.Length - 1; i++)
            {
                x = (i * _texture2DRectangle.Width) / timeSpan;
                y = ((dataPoints[i] * -(_texture2DRectangle.Height)) + (_texture2DRectangle.Height - _chartMarkerTexture.Height * MarkerSize));

                /* Normalize y-point to go no higher than 1 */
                if (y.Equals(-(_chartMarkerTexture.Height * MarkerSize)))
                    y = -(y + (_chartMarkerTexture.Height * MarkerSize));

                _dataPointDestination = new Rectangle((int)x, (int)y, _chartMarkerTexture.Width * MarkerSize, _chartMarkerTexture.Height * MarkerSize);

                spriteBatch.Draw(_chartMarkerTexture, _dataPointDestination, Color.Red);

                if (_chartLines)
                {
                    Vector2 vectorStart = new Vector2(x, y + (MarkerSize * 2));
                    Vector2 vectorStop = Vector2.Zero;

                    //Normalize for boundaries
                    if (dataPoints[i + 1].Equals(1))
                    {
                        vectorStop = new Vector2(x + (_texture2DRectangle.Width / timeSpan),
                                                         ((dataPoints[i + 1] * -(_texture2DRectangle.Height)) +
                                                          _texture2DRectangle.Height) + (MarkerSize * 2));
                    }
                    else if (dataPoints[i + 1].Equals(0))
                    {
                        vectorStop = new Vector2(x + (_texture2DRectangle.Width / timeSpan),
                                    ((dataPoints[i + 1] * -(_texture2DRectangle.Height)) +
                                     _texture2DRectangle.Height) - (MarkerSize * 2));
                    }
                    else
                    {
                        vectorStop = new Vector2(x + (_texture2DRectangle.Width / timeSpan),
                                                         ((dataPoints[i + 1] * -(_texture2DRectangle.Height)) +
                                                          _texture2DRectangle.Height));
                    }

                    float length = (vectorStop - vectorStart).Length();
                    Rectangle dataLineRectangle = new Rectangle((int)x + (_chartMarkerTexture.Width * MarkerSize / 2), (int)vectorStart.Y, (int)length, 1);

                    float rotation = (float)Math.Atan2(vectorStop.Y - vectorStart.Y, vectorStop.X - vectorStart.X);
                    spriteBatch.Draw(_chartMarkerTexture, dataLineRectangle, null, Color.Red, rotation, Vector2.Zero,
                                     SpriteEffects.None, 0);
                }

                if (_tickMarks)
                    DrawXInterval(spriteBatch, i.ToString(CultureInfo.InvariantCulture), new Vector2((int)x, _texture2DRectangle.Height + MARGIN));
            }

            /* Plot last point */
            x = i * _texture2DRectangle.Width / timeSpan;
            y = ((dataPoints[i] * -(_texture2DRectangle.Height)) + (_texture2DRectangle.Height - _chartMarkerTexture.Height * MarkerSize));

            _dataPointDestination = new Rectangle((int)x, (int)y, _chartMarkerTexture.Width * MarkerSize, _chartMarkerTexture.Height * MarkerSize);

            spriteBatch.Draw(_chartMarkerTexture, _dataPointDestination, Color.Red);

            if (_tickMarks)
                DrawXInterval(spriteBatch, i.ToString(CultureInfo.InvariantCulture), new Vector2((int)x, _texture2DRectangle.Height + MARGIN));

            spriteBatch.End();

            game.GraphicsDevice.SetRenderTarget(null);

            return renderTarget2D;

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