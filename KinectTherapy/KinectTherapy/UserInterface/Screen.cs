using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Samples.Kinect.XnaBasics;

namespace SWENG.UserInterface
{
    public enum ScreenState
    {
        Active,
        Hidden,
    }

    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public abstract class Screen : DrawableGameComponent
    {
        /// <summary>
        /// Gets the KinectChooser from the services.
        /// </summary>
        public KinectChooser Chooser
        {
            get
            {
                return (KinectChooser)this.Game.Services.GetService(typeof(KinectChooser));
            }
        }

        /// <summary>
        /// Gets the SpriteBatch from the services.
        /// </summary>
        public SpriteBatch SharedSpriteBatch
        {
            get
            {
                return (SpriteBatch)this.Game.Services.GetService(typeof(SpriteBatch));
            }
        }

        /// <summary>
        /// Protected set so only the screen itself can change its state
        /// </summary>
        public ScreenState ScreenState { get; protected set; }

        /// <summary>
        /// Internal 
        /// </summary>
        public Manager Manager { get; internal set; }

        public string Title { get; internal set; }

        public Screen(Game game)
            : base(game)
        {
            this.ScreenState = UserInterface.ScreenState.Hidden;
        }


        /// <summary>
        /// By default, this will just switch between active and hidden
        /// Override to add logic such as fading in/out, star swipe, bounce, etc...
        /// </summary>
        public virtual void Transition()
        {
            if (this.ScreenState == UserInterface.ScreenState.Active)
            {
                this.ScreenState = UserInterface.ScreenState.Hidden;
            }
            else
            {
                this.ScreenState = UserInterface.ScreenState.Active;
            }
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public virtual void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public virtual void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public virtual void LoadContent() { }

        public virtual void UnloadContent() { }
    }
}
