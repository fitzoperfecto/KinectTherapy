using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SWENG.UserInterface
{
    class GuiChart : GuiDrawable
    {
        #region Game related objects
        private ContentManager _contentManager;
        private SpriteBatch _spriteBatch;
        private SpriteFont _spriteFont;
        private Game _game;
        #endregion

        #region Chart objects and variables
        private readonly string _xAxisLabel;
        private readonly string _yAxisLabel;
        private readonly string _chartType;

        private readonly bool _tickMarks;
        private readonly bool _chartLines;
        private readonly float[] _dataPoints;
        private readonly int _markerSize;
        private readonly float _timeSpan;
        private readonly float _repDuration;

        private Texture2D _xAxisTitleTexture;
        private Texture2D _yAxisTitleTexture;
        private Texture2D _chartMarkerTexture;
        private Texture2D _yAxisIntervalTexture;
        private Rectangle _yIntervalDestination;
        private Rectangle _yIntervalSource;

        private Rectangle _xAxisDestination;
        private Rectangle _xSource;
        private Rectangle _yAxisDestination;
        private Rectangle _ySource;
        private Rectangle _dataPointDestination;
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

        private const int MARGIN = 10;
        #endregion

        /// <summary>
        /// Empty constructor for use in the StatsScreen class
        /// </summary>
        /// <param name="text"></param>
        /// <param name="size"></param>
        /// <param name="position"></param>
        public GuiChart(string text, Vector2 size, Vector2 position) : base(text, size, position)
        {}

        /// <summary>
        /// GuiChart is a class that handles taking datapoints from the Summary Screen and creates a chart object to be displayed and returns a percentage
        /// value of where in the chart between x-axis begin and x-axis end the mouse was clicked.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="size"></param>
        /// <param name="position"></param>
        /// <param name="chartOptions"></param>
        public GuiChart(string text, Vector2 size, Vector2 position, GuiChartOptions chartOptions) : base(text, size, position)
        {
            _xAxisLabel = chartOptions.AxesName[0];
            _yAxisLabel = chartOptions.AxesName[1];
            _chartType = chartOptions.ChartType;
            _tickMarks = chartOptions.TickMarks;
            _dataPoints = chartOptions.DataPoints;
            _markerSize = chartOptions.MarkerSize;
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
            spriteBatch.Begin();

            if (Texture2D != null)
            {
                spriteBatch.Draw(
                    Texture2D,
                    _position,
                    Color.White
                );

                spriteBatch.Draw(
                    _xAxisTitleTexture,
                    _xAxisDestination,
                    _xSource,
                    Color.White
                );

                if (_chartType.Equals("Time"))
                {
                    spriteBatch.DrawString(
                        _spriteFont,
                        (_repDuration * .001f).ToString(CultureInfo.InvariantCulture),
                        new Vector2((_yAxisTitleTexture.Width + MARGIN) + Texture2D.Width, Texture2D.Height + (MARGIN * 3)),
                        Color.Blue
                    );
                } 
                
                spriteBatch.Draw(
                    _yAxisTitleTexture,
                    _yAxisDestination,
                    _ySource,
                    Color.White
                );

                spriteBatch.Draw(
                    _yAxisIntervalTexture,
                    _yIntervalDestination,
                    _yIntervalSource,
                    Color.White
                );

                DrawDataPointTexture(_game, spriteBatch, _dataPoints, _timeSpan);

            }
            spriteBatch.End();

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

            _game = game;
            _contentManager = contentManager;
            _spriteBatch = spriteBatch;

            Texture2D = _contentManager.Load<Texture2D>(@"UI\ChartTexture");
            _chartMarkerTexture = _contentManager.Load<Texture2D>(@"UI\ChartMarker");

            _xAxisTitleTexture = CreateXAxisTitleTexture(game, _contentManager, spriteBatch);
            _yAxisTitleTexture = CreateYAxisTitleTexture(game, _contentManager, spriteBatch);

            _xAxisTitleTexture = CreateXAxisTitleTexture(game, _contentManager, spriteBatch);
            _xAxisDestination = new Rectangle(
                Texture2D.Width / 2,
                Texture2D.Height + (MARGIN * 3),
                _xAxisTitleTexture.Width,
                _xAxisTitleTexture.Height
            );

            _xSource = new Rectangle(
                0,
                0,
                _xAxisTitleTexture.Width,
                _xAxisTitleTexture.Height
            );

            _yAxisTitleTexture = CreateYAxisTitleTexture(game, _contentManager, spriteBatch);
            _yAxisDestination = new Rectangle(
                MARGIN,
                Texture2D.Height / 2,
                _yAxisTitleTexture.Width,
                _yAxisTitleTexture.Height
            );

            _ySource = new Rectangle(
                0,
                0,
                _yAxisTitleTexture.Width,
                _yAxisTitleTexture.Height
            );

            _yAxisIntervalTexture = CreateYAxisIntervalTexture(game, _contentManager, spriteBatch);
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

            /* Create bounding rectangle for mouse functionality */
            _texture2DRectangle = new Rectangle(Texture2D.Bounds.X + MARGIN, Texture2D.Bounds.Y + 3, Texture2D.Width + 100, Texture2D.Height + 3);

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
                MouseXPercent = 100 - ((MouseXCoord/_texture2DRectangle.Width) * 100);

                MouseYCoord = mouseState.Y;
                MouseYPercent = 100 - ((MouseYCoord/_texture2DRectangle.Height) * 100);

                /*============= Test of percentage conversion of mouse coordinates (not working -yet) ==============*/
                var yMouseCoordTexture = CreateMouseCoordTexture(_game, _contentManager, _spriteBatch);
                var yMouseSource = new Rectangle(0, 0, yMouseCoordTexture.Width, yMouseCoordTexture.Height);
                var yMouseDestination = new Rectangle(0, Texture2D.Height + 50, yMouseCoordTexture.Width,
                                                        yMouseCoordTexture.Height);

                _spriteBatch.Begin();
                _spriteBatch.Draw(yMouseCoordTexture, yMouseDestination, yMouseSource, Color.Blue);
                _spriteBatch.End();
                /*==================================================================================================*/
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
        private void DrawDataPointTexture(Game game, SpriteBatch spriteBatch, float[] dataPoints, float timeSpan)
        {
            int i;
            float x;
            float y;

            int j;

            RenderTarget2D renderTarget2D = new RenderTarget2D(game.GraphicsDevice, _chartMarkerTexture.Width * _markerSize, _chartMarkerTexture.Height * _markerSize);
            game.GraphicsDevice.SetRenderTarget(renderTarget2D);
            game.GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 0, 0);

            for (i = 0; i < dataPoints.Length - 1; i++)
            {
                x = i * Texture2D.Width / timeSpan;
                y = ((Math.Abs(dataPoints[i]) * -(Texture2D.Height)) + Texture2D.Bounds.Bottom) + 5;

                _dataPointDestination = new Rectangle((int) ((_yAxisTitleTexture.Width + MARGIN) + x), (int) y, _chartMarkerTexture.Width * _markerSize, _chartMarkerTexture.Height * _markerSize);

                spriteBatch.Draw(_chartMarkerTexture, _dataPointDestination, Color.Red);

                Vector2 vectorStart = new Vector2(x,y);
                Vector2 vectorStop = new Vector2((i + 1) * Texture2D.Width / timeSpan, ((Math.Abs(dataPoints[i + 1]) * -(Texture2D.Height)) + Texture2D.Bounds.Bottom) + 5);

                float length = (vectorStop - vectorStart).Length();
                Rectangle dataLineRectangle = new Rectangle((int) ((_yAxisTitleTexture.Width + MARGIN) + x) + (_markerSize / 2), (int) vectorStart.Y, (int) length, 1);

                if (_chartLines)
                {
                    float rotation = (float) Math.Atan2(vectorStop.Y - vectorStart.Y, vectorStop.X - vectorStart.X);
                    spriteBatch.Draw(_chartMarkerTexture, dataLineRectangle, null, Color.Red, rotation, Vector2.Zero,
                                     SpriteEffects.None, 0);
                }

                if (_tickMarks)
                    DrawXInterval(spriteBatch, i.ToString(CultureInfo.InvariantCulture), new Vector2((int)((_yAxisTitleTexture.Width + MARGIN) + x), Texture2D.Height + MARGIN));
            }

            /* Plot last point */
            x = i * Texture2D.Width / timeSpan;
            y = ((Math.Abs(dataPoints[i]) * -(Texture2D.Height)) + Texture2D.Bounds.Bottom) + 5;

            _dataPointDestination = new Rectangle((int)((_yAxisTitleTexture.Width + MARGIN) + x), (int) y, _chartMarkerTexture.Width * _markerSize, _chartMarkerTexture.Height * _markerSize);

            spriteBatch.Draw(_chartMarkerTexture, _dataPointDestination, Color.Red);

            if (_tickMarks)
                DrawXInterval(spriteBatch, i.ToString(CultureInfo.InvariantCulture), new Vector2((int)((_yAxisTitleTexture.Width + MARGIN) + x), Texture2D.Height + MARGIN));

            game.GraphicsDevice.SetRenderTarget(null);
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

        /* ======================  Mouse coordinate test functionality ======================================== */
        private Texture2D CreateMouseCoordTexture(Game game, ContentManager contentManager, SpriteBatch spriteBatch)
        {
            SpriteFont spriteFont = contentManager.Load<SpriteFont>("Arial12");
            Vector2 yAxisMeasure = spriteFont.MeasureString(MouseYPercent.ToString(CultureInfo.InvariantCulture));
            RenderTarget2D renderTarget2d = new RenderTarget2D(game.GraphicsDevice, (int)yAxisMeasure.X, (int)yAxisMeasure.Y);

            game.GraphicsDevice.SetRenderTarget(renderTarget2d);
            game.GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 0, 0);

            spriteBatch.Begin();

            spriteBatch.DrawString(
                spriteFont,
                String.Format("X Coord: {0} | Y Coord:{1}", MouseXCoord, MouseYCoord),
                Vector2.Zero, 
                Color.Blue
            );

            spriteBatch.End();

            game.GraphicsDevice.SetRenderTarget(null);

            return renderTarget2d;
        }
        /* ==================================================================================================== */
    }
}
