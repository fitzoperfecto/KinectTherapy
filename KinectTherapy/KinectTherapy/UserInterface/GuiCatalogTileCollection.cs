using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SWENG.UserInterface
{
    class GuiCatalogTileCollection
    {
        protected bool _IsReadOnly = false;
        private readonly Rectangle _viewableArea;
        private const float MARGIN = 10f;
        private const float CATALOG_WIDTH = 700f;
        private const float CATALOG_HEIGHT = 90f;
        private const float SCROLL_WIDTH = 20f;
        private const int VIEWABLE_TILES = 5;

        private List<GuiCatalogTile> _collection;
        private GuiScrollable _scrollable;
        public List<GuiCatalogTile> Collection { get { return _collection; } }

        private int _pages;
        private int _currentPage;
        private int _pageBeginning;
        private int _pageEnding;
        private bool _doScroll;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewableArea"></param>
        public GuiCatalogTileCollection(Rectangle viewableArea)
        {
            _viewableArea = viewableArea;

            _collection = new List<GuiCatalogTile>();

            _scrollable = new GuiScrollable(
                new Vector2(
                    SCROLL_WIDTH,
                    (int)(VIEWABLE_TILES * (MARGIN + CATALOG_HEIGHT))
                ),
                new Vector2(
                    _viewableArea.Right,
                    _viewableArea.Top
                ),
                1
            );

            _currentPage = 0;
            _pageBeginning = VIEWABLE_TILES * _currentPage;
            _pageEnding = _pageBeginning + VIEWABLE_TILES;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
            foreach (GuiCatalogTile catalogTile in _collection)
            {
                catalogTile.Initialize();
            }

            _scrollable.Initialize();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentManager"></param>
        public void LoadContent(ContentManager contentManager)
        {
            foreach (GuiCatalogTile catalogTile in _collection)
            {
                catalogTile.LoadContent(contentManager);
            }

            _scrollable.LoadContent(contentManager);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = _pageBeginning; i < _pageEnding; i = i + 1)
            {
                _collection[i].Draw(spriteBatch);
            }

            _scrollable.Draw(spriteBatch);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mouseState"></param>
        /// <param name="oldMouseState"></param>
        public void Update(MouseState mouseState, MouseState oldMouseState)
        {
            Rectangle mouseBoundingBox = new Rectangle(mouseState.X, mouseState.Y, 1, 1);
            /** The client may have accidently come off the scroll icon */
            if (_doScroll)
            {

            }

            if (mouseBoundingBox.Intersects(_scrollable.GraceArea))
            {
                _scrollable.Update(mouseState, oldMouseState);
                float scrollPercent = _scrollable.GetScrollTop();

                float paginationSegment = 100.0f / (_pages);
                Debug.WriteLine(
                    string.Format("pages = {0}", _pages)
                );

                /** 100% scrolled would always give you a page 1 if this wasn't implemented */
                if (scrollPercent != 100.0f)
                {
                    _currentPage = (int)(scrollPercent / paginationSegment);
                }
                else
                {
                    /** Zero based paging system */
                    _currentPage = _pages - 1;
                }
                _pageBeginning = VIEWABLE_TILES * _currentPage;
                _pageEnding = _pageBeginning + VIEWABLE_TILES;

                if (_pageEnding >= _collection.Count)
                    _pageEnding = _collection.Count;

                _doScroll = true;
            }
            else
            {
                if (_pageEnding >= _collection.Count)
                    _pageEnding = _collection.Count;

                _doScroll = false;
            }

            for (int i = _pageBeginning; i < _pageEnding; i = i + 1)
            {
                _collection[i].Update(mouseState, oldMouseState);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public void AddCatalogItem(Game game, string itemId, string title, string description)
        {
            /** signifies the location of the item on the page */
            int instance = _collection.Count % VIEWABLE_TILES;

            Vector2 itemSize = new Vector2(
                CATALOG_WIDTH,
                CATALOG_HEIGHT
            );

            Vector2 itemPosition = new Vector2(
                _viewableArea.Left + ((_viewableArea.Width - CATALOG_WIDTH) / 2),
                _viewableArea.Top + (instance * (CATALOG_HEIGHT + MARGIN))
            );

            _collection.Add(
                new GuiCatalogTile(
                    game,
                    itemId,
                    title,
                    description,
                    itemSize,
                    itemPosition)
            );

            UpdatePagination();
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdatePagination()
        {
            int count = _collection.Count;

            /** Zero based paging system */
            _pages = (int)Math.Ceiling((1.0 * count) / VIEWABLE_TILES);
            _scrollable.Count = _pages;
        }
    }
}
