using System;
using Microsoft.Samples.Kinect.XnaBasics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SWENG.UserInterface
{
    public delegate void TransitionEvent(object sender, TransitionEventArgs e);

    public class TransitionEventArgs : EventArgs
    {
        public string ScreenName;
        public string ClickedOn;

        public TransitionEventArgs(string name, string clickedOn)
        {
            ScreenName = name;
            ClickedOn = clickedOn;
        }
    }

    public enum ScreenState
    {
        Active = 1,
        Hidden = 2,
        NonInteractive = 4,
    }

    /// <summary>
    /// Required abstract class if the UserInterface screen wants to be managed.
    /// </summary>
    public abstract class Screen : DrawableGameComponent
    {
        #region event stuff
        public event TransitionEvent TransitionEvent;

        // Invoke the Changed event; called whenever repetitions changes
        protected virtual void OnTransition(TransitionEventArgs e)
        {
            if (TransitionEvent != null)
                TransitionEvent(this, e);
        }
        #endregion

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
        /// Get the ScreenState
        /// </summary>
        public ScreenState ScreenState { get; set; }

        /// <summary>
        /// Get the UserInterface.Manager
        /// </summary>
        public Manager Manager { get; internal set; }

        /// <summary>
        /// Get the Title
        /// </summary>
        public string Title { get; internal set; }

        public ContentManager contentManager { get; internal set; }

        public Screen(Game game)
            : base(game)
        {
            this.ScreenState = UserInterface.ScreenState.Hidden;
        }

        /// <summary>
        /// This will just switch between active and hidden
        /// </summary>
        public virtual void Transition() { }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public virtual void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// This will call the transition.
        /// Meant for moments when the screen has special
        /// opening logic
        /// </summary>
        public virtual void OpenScreen() { }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public virtual void Update(GameTime gameTime)//, MouseState currentMouse, MouseState oldMouse)
        {
            base.Update(gameTime);
        }

        public virtual void LoadContent() { }

        /// <summary>
        /// This method ensures that the ContentManager 
        /// releases its loaded Content.
        /// </summary>
        public virtual void UnloadContent()
        {
            if (null != this.contentManager)
            {
                this.contentManager.Unload();
            }
        }
    }
}
