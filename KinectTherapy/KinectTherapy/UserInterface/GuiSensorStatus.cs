using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Samples.Kinect.XnaBasics;
using Microsoft.Kinect;

namespace SWENG.UserInterface
{
    class GuiSensorStatus : GuiDrawable
    {
        private readonly Game _game;

        private int _oldStatus;

        private KinectChooser _chooser
        {
            get
            {
                return (KinectChooser)_game.Services.GetService(typeof(KinectChooser));
            }
        }

        public GuiSensorStatus(string text, Vector2 size, Vector2 position, Game game)
            : base(text, size, position)
        {
            _game = game;
        }

        public override void LoadContent(Game game, Microsoft.Xna.Framework.Content.ContentManager contentManager, SpriteBatch spriteBatch) { }

        public override void Update(MouseState mouseState, MouseState oldMouseState, Rectangle mouseBoundingBox, GameTime gameTime)
        {
            int currentStatus = 0;

            switch (_chooser.Sensor.Status)
            {
                case KinectStatus.Connected:
                    currentStatus = 1;
                    break;
                case KinectStatus.Initializing:
                    currentStatus = 0;
                    break;
                default:
                    currentStatus = 2;
                    break;
            }

            if (_oldStatus != Frame)
            {
                Frame = currentStatus;
            }

            _oldStatus = currentStatus;
        }
    }
}
