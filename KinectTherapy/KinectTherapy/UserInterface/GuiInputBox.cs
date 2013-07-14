using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SWENG.UserInterface
{
    public enum CheckboxState
    {
        Default,
        Hovered,
        Blinking
    }

    public delegate void Selected(object sender, SelectedEventArgs e);

    public class SelectedEventArgs : EventArgs
    {
        public string ID;

        public SelectedEventArgs(string id)
        {
            ID = id;
        }
    }

    public class GuiInputBox : GuiDrawable
    {
        public CheckboxState State;

        #region event stuff
        public event Selected OnSelected;

        // Invoke the Changed event; called whenever repetitions changes
        protected virtual void BoxSelected(SelectedEventArgs e)
        {
            if (OnSelected != null)
                OnSelected(this, e);
        }
        #endregion

        private KeyboardState _oldKeyState;
        #region Keys
        private Keys[] _acceptable = new Keys[]{
                Keys.NumPad0,
                Keys.NumPad1,
                Keys.NumPad2,
                Keys.NumPad3,
                Keys.NumPad4,
                Keys.NumPad5,
                Keys.NumPad6,
                Keys.NumPad7,
                Keys.NumPad8,
                Keys.NumPad9,
                Keys.D0,
                Keys.D1,
                Keys.D2,
                Keys.D3,
                Keys.D4,
                Keys.D5,
                Keys.D6,
                Keys.D7,
                Keys.D8,
                Keys.D9,
                Keys.OemPeriod
            };
        #endregion
        private double _gameTimeStamp;
        private SpriteFont _spriteFont;
        private float _defaultValue;
        private float _minValue;
        private float _maxValue;

        public string Value { get; set; }

        public GuiInputBox(string id, Vector2 size, Vector2 position, Game game, float defaultValue, float maxValue, float minValue)
            : base(id, size, position)
        {
            Value = defaultValue.ToString();

            _defaultValue = defaultValue;
            _maxValue = maxValue;
            _minValue = minValue;
            _gameTimeStamp = double.MinValue;
            State = CheckboxState.Default;
        }

        public override void Update(MouseState mouseState, MouseState oldMouseState, Rectangle mouseBoundingBox, GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();

            if ((State & CheckboxState.Blinking) == 0)
            {
                if (mouseBoundingBox.Intersects(Rectangle))
                {
                    State = CheckboxState.Hovered;

                    if (mouseState.LeftButton == ButtonState.Released
                        && oldMouseState.LeftButton == ButtonState.Pressed)
                    {
                        State = CheckboxState.Blinking;
                        BoxSelected(new SelectedEventArgs(Text));
                    }
                }
                else
                {
                    State = CheckboxState.Default;
                }
            }
            else
            {
                if (keyState.IsKeyUp(Keys.Enter)
                    && _oldKeyState.IsKeyDown(Keys.Enter))
                {
                    State = CheckboxState.Default;
                }

                UpdateOptionString(keyState);
            }

            switch (State)
            {
                case CheckboxState.Default:
                    Frame = 0;
                    break;
                case CheckboxState.Hovered:
                    Frame = 1;
                    break;
                case CheckboxState.Blinking:
                    if (gameTime.TotalGameTime.TotalMilliseconds - _gameTimeStamp > 750)
                    {
                        if (Frame == 2)
                        {
                            Frame = 3;
                        }
                        else
                        {
                            Frame = 2;
                        }
                        _gameTimeStamp = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    break;
            }

            _oldKeyState = keyState;
        }

        public override void LoadContent(Game game, ContentManager contentManager, SpriteBatch spriteBatch) 
        {
            try
            {
                Texture2D = contentManager.Load<Texture2D>(@"UI\InputBox");
            }
            catch (Exception e)
            {
                Texture2D = contentManager.Load<Texture2D>(@"blank");
            }

            try
            {
                _spriteFont = contentManager.Load<SpriteFont>("Arial16");
            }
            catch (Exception e) { }
        }

        /// <summary>
        /// This method renders the current state of the element to the screen.
        /// </summary>
        /// <param name="spriteBatch">A SpriteBatch that has begun.</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (null != Texture2D)
            {
                spriteBatch.Draw(
                    Texture2D,
                    Rectangle,
                    SourceRectangle,
                    Color.White
                );

                if (null != _spriteFont)
                {
                    Vector2 measuredString = _spriteFont.MeasureString(Value);
                    spriteBatch.DrawString(
                        _spriteFont,
                        Value,
                        new Vector2(
                            Position.X + Size.X - measuredString.X - 20,
                            Position.Y + ((Size.Y - measuredString.Y) / 2)
                        ),
                        Color.Black
                    );
                }
            }
        }

        private void UpdateOptionString(KeyboardState keyState)
        {
            float temp = 0;

            if (_oldKeyState.IsKeyDown(Keys.Back)
                && keyState.IsKeyUp(Keys.Back)
                && Value.Length > 0)
            {
                Value = Value.Remove(Value.Length - 1, 1);
            }

            Value += CheckKeys(keyState);

            if (float.TryParse(Value, out temp))
            {
                if (temp > _maxValue)
                {
                    temp = _maxValue;
                }
                else if (temp < _minValue)
                {
                    temp = _minValue;
                }

                Value = temp.ToString();
            }

            if (_oldKeyState.IsKeyDown(Keys.Enter)
                && keyState.IsKeyUp(Keys.Enter))
            {
                if (string.IsNullOrEmpty(Value))
                {
                    Value = _defaultValue.ToString();
                }
            }
        }

        private string CheckKeys(KeyboardState keyState)
        {
            string r = string.Empty;

            Keys[] oldKeys = _oldKeyState.GetPressedKeys();

            foreach (Keys key in oldKeys)
            {
                if (Array.IndexOf(_acceptable, key) != -1)
                {
                    if (keyState.IsKeyUp(key))
                    {
                        string numb = key.ToString();
                        r = numb.Substring(numb.Length - 1);
                    }
                }
            }

            return r;
        }
    }
}
