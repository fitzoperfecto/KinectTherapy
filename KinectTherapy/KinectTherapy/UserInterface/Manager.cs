using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace SWENG.UserInterface
{
    public class Manager : DrawableGameComponent
    {
        /// <summary>
        /// A list of the screens added to the Manager
        /// </summary>
        private List<Screen> _screenList;

        /// <summary>
        /// There is a chance that the class has not been properly
        /// initialized first.  If it is not initialized before calling
        /// Draw() the LoadContent() method will not have been called
        /// either.  Thus, any content being drawn will throw an error.
        /// </summary>
        private bool _isInitialized = false;

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
            _screenList = new List<Screen>();
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            _isInitialized = true;

            foreach (Screen screen in _screenList)
            {
                screen.TransitionEvent -= TransitionEventManager;

                screen.TransitionEvent += TransitionEventManager;
            }

            base.Initialize();
        }

        /// <summary>
        /// This will go through and call the LoadContent method
        /// on all Screens in its control
        /// </summary>
        protected override void LoadContent()
        {
            foreach (Screen screen in _screenList)
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
            foreach (Screen screen in _screenList)
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
            foreach (Screen screen in _screenList)
            {
                if ((screen.ScreenState & ScreenState.Active) != 0)
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
            foreach (Screen screen in _screenList)
            {
                if ((screen.ScreenState & ScreenState.Active) != 0)
                {
                    screen.Draw(gameTime);
                }
            }

            base.Draw(gameTime);
        }

        /// <summary>
        /// Acts as the router for transition events of the screens
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TransitionEventManager(object sender, TransitionEventArgs e)
        {
            switch (e.ClickedOn)
            {
                case "ExitProgram":
                    UnloadContent();
                    Game.Exit();
                    break;
                case "Finished":
                    if (e.ScreenName == "Summary")
                    {
                        CallOpen("Catalog");
                    }
                    break;
                case "Menu":
                    if (e.ScreenName == "Exercise")
                    {
                        CallOpen("Home");
                    }
                    break;
                case "Catalog":
                    if (e.ScreenName == "Home")
                    {
                        CallOpen("Catalog");
                    }
                    break;
                case "Start":
                    if (e.ScreenName == "Catalog")
                    {
                        CallOpen("Loading");
                    }
                    break;
                case "CatalogTileEdit":
                    CallOpenEdit(e.ScreenName);
                    break;
                case "LoadingIsDone":
                    if (e.ScreenName == "Loading")
                    {
                        CallOpen("Exercise");
                    }
                    break;
                case "SensorSetup":
                    CallOpenModal("Sensor Setup");
                    break;
                case "Return":
                    foreach (Screen screen in _screenList)
                    {
                        if ((screen.ScreenState & ScreenState.Active) == ScreenState.Active
                            && (screen.ScreenState & ScreenState.NonInteractive) == ScreenState.NonInteractive)
                        {
                            screen.ScreenState = ScreenState.Active;
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Add a screen to the manager.
        /// </summary>
        /// <param name="screen">A class that inheritted the screen class</param>
        public void AddScreen(Screen screen)
        {
            screen.Manager = this;

            /** Make sure that the content needed is loaded since initialization is only called once. */
            screen.Initialize();
            screen.TransitionEvent -= TransitionEventManager;
            screen.TransitionEvent += TransitionEventManager;

            _screenList.Add(screen);
        }

        /// <summary>
        /// Remove a screen from the manager.
        /// </summary>
        /// <param name="screen">A class that inheritted the screen class</param>
        public void RemoveScreen(Screen screen)
        {
            if (_isInitialized)
            {
                screen.UnloadContent();
                screen.TransitionEvent -= TransitionEventManager;
            }

            _screenList.Remove(screen);
        }

        /// <summary>
        /// This will fire the Transition method on
        /// the screen.  It is case sensitive.
        /// </summary>
        /// <param name="title">The title of the screen to call the transition on.</param>
        public void CallOpen(string title)
        {
            foreach (Screen screen in _screenList)
            {
                if (screen.Title == title)
                {
                    screen.ScreenState = ScreenState.Active;
                    screen.OpenScreen();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">The Id of the catalog item.</param>
        private void CallOpenEdit(string id)
        {
            foreach (Screen screen in _screenList)
            {
                if (screen.Title == "CatalogTileEdit")
                {
                    screen.ScreenState = ScreenState.Active;
                    ((CatalogTileEditScreen)screen).OpenScreen(id);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modalId"></param>
        private void CallOpenModal(string modalId)
        {
            foreach (Screen screen in _screenList)
            {
                if (screen.Title == modalId)
                {
                    screen.ScreenState = ScreenState.Active;
                    screen.OpenScreen();
                }
            }
        }
    }
}
