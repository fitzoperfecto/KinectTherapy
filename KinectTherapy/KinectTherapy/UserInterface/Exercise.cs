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
using System.Diagnostics;


namespace SWENG.UserInterface
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class ExerciseSessionManager : DrawableGameComponent
    {
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

        List<Screen> ScreenList = new List<Screen>();
        private bool _isInitialized = false;

        public ExerciseSessionManager(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            _isInitialized = true;
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            foreach (Screen screen in ScreenList)
            {
                screen.LoadContent();
            }
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();

            foreach (Screen screen in ScreenList)
            {
                screen.UnloadContent();
            }
        }

        /// <summary>
        /// Component updates itself.
        /// Will only update screens that are active.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            foreach (Screen screen in ScreenList)
            {
                if (screen.ScreenState == ScreenState.Active)
                {
                    screen.Update(gameTime);
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Component draws itself.
        /// Will only draw screens that are active.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            foreach (Screen screen in ScreenList)
            {
                if (screen.ScreenState == ScreenState.Active)
                {
                    screen.Draw(gameTime);
                }
            }

            base.Draw(gameTime);
        }

        /// <summary>
        /// Add a screen to the manager.
        /// </summary>
        /// <param name="screen"></param>
        public void AddScreen(Screen screen)
        {
            screen.Manager = this;

            /* 
             * Make sure that the content needed is loaded since 
             * initialization is only called once.
             */
            screen.Initialize();
            screen.LoadContent();

            this.ScreenList.Add(screen);
        }

        /// <summary>
        /// Remove a screen from the manager.
        /// </summary>
        /// <param name="screen"></param>
        public void RemoveScreen(Screen screen)
        {
            if (this._isInitialized)
            {
                screen.UnloadContent();
            }

            this.ScreenList.Remove(screen);
        }

        public void CallOpen(string title)
        {
            foreach (Screen screen in ScreenList)
            {
                if (screen.Title == title)
                {
                    screen.Transition();
                }
            }
        }
    }
}
