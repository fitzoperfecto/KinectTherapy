using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace SWENG.UserInterface
{
    public class Manager : DrawableGameComponent
    {
        /// <summary>
        /// A list of the screens added to the Manager
        /// </summary>
        private List<Screen> ScreenList;
        
        /// <summary>
        /// There is a chance that the class has not been properly
        /// initialized first.  If it is not initialized before calling
        /// Draw() the LoadContent() method will not have been called
        /// either.  Thus, any content being drawn will throw an error.
        /// </summary>
        private bool isInitialized = false;

        /// <summary>
        /// The UserInterface Manager is a switch board for 
        /// all the Screens in the application.
        /// It is also meant to be added to the game
        /// as a Component.
        /// </summary>
        /// <param name="game"></param>
        public Manager(Game game)
            : base(game)
        {
            this.ScreenList = new List<Screen>();
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            isInitialized = true;

            base.Initialize();
        }

        /// <summary>
        /// This will go through and call the LoadContent method
        /// on all Screens in its control
        /// </summary>
        protected override void LoadContent()
        {
            foreach (Screen screen in ScreenList)
            {
                screen.LoadContent();
            }

            base.LoadContent();
        }

        /// <summary>
        /// This will go through and call the UnloadContent method
        /// on all Screens in its control
        /// </summary>
        protected override void UnloadContent()
        {
            foreach (Screen screen in ScreenList)
            {
                screen.UnloadContent();
            }

            base.UnloadContent();
        }

        /// <summary>
        /// This will only update screens with an active screen state.
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
        /// This will only draw screens with an active screen state.
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
        /// <param name="screen">A class that inheritted the screen class</param>
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
        /// <param name="screen">A class that inheritted the screen class</param>
        public void RemoveScreen(Screen screen)
        {
            if (this.isInitialized)
            {
                screen.UnloadContent();
            }

            this.ScreenList.Remove(screen);
        }


        /// <summary>
        /// This will fire the Transition method on
        /// the screen.  It is case sensitive.
        /// </summary>
        /// <param name="title">The title of the screen to call the transition on.</param>
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
