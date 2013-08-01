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
        private readonly float _timeSpan;
        private readonly float _repDuration;

        private Texture2D _xAxisTitleTexture;
        private Texture2D _yAxisTitleTexture;
        private Texture2D _chartMarkerTexture;
        private Texture2D _yAxisIntervalTexture;
        private Texture2D _dataPointTexture;

        private Vector2 _xAxisMeasure;
        private Vector2 _yAxisMeasure;

        private Rectangle _dataPointDestination;
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

        private const int MARGIN = 10;
        private const int Scale = 20000;
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
            _timeSpan = chartOptions.TimeInterval;
            _repDuration = (float) Math.Round(chartOptions.RepDuration * .001f, 3);
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
                    new Vector2(_texture2DRectangle.Right - (MARGIN * 3), _texture2DRectangle.Bottom),
                    Color.Blue
                    );
            }

            spriteBatch.Draw(
                _yAxisTitleTexture,
                new Vector2((_texture2DRectangle.X - _yAxisMeasure.X), (_texture2DRectangle.Bottom - (_texture2DRectangle.Height / 2))),
                Color.White
                );

            spriteBatch.Draw(
                _yAxisIntervalTexture,
                new Vector2(_texture2DRectangle.X - MARGIN, _texture2DRectangle.Top + 1),
                Color.White
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
                (int)((int)_position.X + _yAxisMeasure.Length()), 
                (int)_position.Y, 
                (int) ((int)Size.X - (_yAxisMeasure.X + MARGIN)), 
                (int) ((int)Size.Y - (_xAxisMeasure.Y - 3))
            );

            _yAxisIntervalTexture = CreateYAxisIntervalTexture(game, contentManager, spriteBatch);

            _dataPointTexture = DrawDataPointTexture(game, spriteBatch, _dataPoints, _timeSpan);
            _dataPointTextureDestination = new Rectangle(_texture2DRectangle.X + 5, _texture2DRectangle.Y, _texture2DRectangle.Width + MarkerSize, _texture2DRectangle.Height);
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
        /// Creates the game texture used to create the chart background
        /// </summary>
        /// <param name="game"></param>
        /// <param name="contentManager"></param>
        /// <param name="spriteBatch"></param>
        public Texture2D CreateChartTexture(Game game, ContentManager contentManager, SpriteBatch spriteBatch)
        {
            Texture2D = contentManager.Load<Texture2D>(@"UI\ChartTexture");
            _texture2DRectangle = new Rectangle((int)_position.X, (int)_position.Y, Texture2D.Width, Texture2D.Height);

            RenderTarget2D renderTarget2D = new RenderTarget2D(game.GraphicsDevice, Texture2D.Width + 50, Texture2D.Height + 150);
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
        /// Creates the game texture used to create the render target for the y-axis values 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="contentManager"></param>
        /// <param name="spriteBatch"></param>
        /// <returns></returns>
        private Texture2D CreateYAxisIntervalTexture(Game game, ContentManager contentManager, SpriteBatch spriteBatch)
        {
            SpriteFont spriteFont = contentManager.Load<SpriteFont>("Arial10");
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
                //y = _texture2DRectangle.Height - ((dataPoints[i]*Scale) * (_texture2DRectangle.Bottom));// + (_texture2DRectangle.Height - _chartMarkerTexture.Height * MarkerSize));
                y = _texture2DRectangle.Bottom - (dataPoints[i] * Scale);

                /* Normalize y-point to go no higher than 1 */
                if (y.Equals(-(_chartMarkerTexture.Height * MarkerSize)))
                    y = -(y + (_chartMarkerTexture.Height * MarkerSize));

                _dataPointDestination = new Rectangle((int)x, (int)y, _chartMarkerTexture.Width, _chartMarkerTexture.Height * MarkerSize);

                spriteBatch.Draw(_chartMarkerTexture, _dataPointDestination, Color.Red);

                if (_chartLines)
                {
                    Vector2 vectorStart = new Vector2(x, y);
                    Vector2 vectorStop;

                    //Normalize for boundaries
                    if (dataPoints[i + 1].Equals(1))
                    {
                        vectorStop = new Vector2(x + (_texture2DRectangle.Width / timeSpan),
                                                         (((dataPoints[i + 1] * Scale) * -(_texture2DRectangle.Height)) +
                                                          _texture2DRectangle.Height) + (MarkerSize * 2));
                    }
                    else if (dataPoints[i + 1].Equals(0))
                    {
                        vectorStop = new Vector2(x + (_texture2DRectangle.Width / timeSpan),
                                    (((dataPoints[i + 1] * Scale) * -(_texture2DRectangle.Height)) +
                                     _texture2DRectangle.Height) - (MarkerSize * 2));
                    }
                    else
                    {
                        vectorStop = new Vector2(x + (_texture2DRectangle.Width / timeSpan), (_texture2DRectangle.Bottom - (dataPoints[i + 1] * Scale)));
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
            y = _texture2DRectangle.Bottom - (dataPoints[i] * Scale);

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