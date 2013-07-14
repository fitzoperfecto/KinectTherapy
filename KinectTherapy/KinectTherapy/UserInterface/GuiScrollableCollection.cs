using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SWENG.UserInterface
{
    public delegate void CollectionItemClicked(object sender, CollectionItemClickedArgs e);

    public class CollectionItemClickedArgs : EventArgs
    {
        public string ID;

        public CollectionItemClickedArgs(string id)
        {
            ID = id;
        }
    }

    class GuiScrollableCollection : GuiDrawable
    {
        #region event stuff
        public event CollectionItemClicked CollectionItemClickEvent;

        // Invoke the Changed event; called whenever repetitions changes
        protected virtual void OnClick(CollectionItemClickedArgs e)
        {
            if (CollectionItemClickEvent != null)
                CollectionItemClickEvent(this, e);
        }
        #endregion

        private const float MARGIN = 10f;
        private const float SCROLL_WIDTH = 20f;
        private int _viewableItems = 5;

        private List<GuiDrawable> _collection;
        private GuiScrollable _scrollable;
        public List<GuiDrawable> Collection { get { return _collection; } }

        private int _pages;
        private int _currentPage;
        private int _pageBeginning;
        private int _pageEnding;
        private bool _doScroll;

        public Vector2 ItemSize { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewableArea"></param>
        public GuiScrollableCollection(string text, Vector2 size, Vector2 position, int viewableItems, float itemHeight, float itemWidth)
            :base(text, size, position)
        {
            _collection = new List<GuiDrawable>();
            _viewableItems = viewableItems;

            ItemSize = new Vector2(
                itemWidth,
                itemHeight
            );

            _scrollable = new GuiScrollable(
                new Vector2(
                    SCROLL_WIDTH,
                    _viewableItems * (itemHeight + MARGIN)
                ),
                new Vector2(
                    Rectangle.Right,
                    Rectangle.Top
                ),
                1
            );

            _currentPage = 0;
            _pageBeginning = _viewableItems * _currentPage;
            _pageEnding = _pageBeginning + _viewableItems;
        }

        public override void LoadContent(Game game, ContentManager contentManager, SpriteBatch spriteBatch)
        {
            foreach (GuiDrawable tile in _collection)
            {
                tile.LoadContent(game, contentManager, spriteBatch);
            }

            _scrollable.LoadContent(game, contentManager, spriteBatch);
        }

        /// <summary>
        /// This method renders the current state of the element to the screen.
        /// </summary>
        /// <param name="spriteBatch">A SpriteBatch that has begun.</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_collection.Count != 0)
            {
                for (int i = _pageBeginning; i < _pageEnding; i = i + 1)
                {
                    _collection[i].Draw(spriteBatch);
                }
            }

            _scrollable.Draw(spriteBatch);
        }

        public override void Update(MouseState mouseState, MouseState oldMouseState, Rectangle mouseBoundingBox, GameTime gameTime)
        {
            /** The client may have accidently come off the scroll icon */
            if (_doScroll)
            {

            }

            if (mouseBoundingBox.Intersects(_scrollable.GraceArea))
            {
                _scrollable.Update(mouseState, oldMouseState, mouseBoundingBox, gameTime);
                float scrollPercent = _scrollable.GetScrollTop();

                float paginationSegment = 100.0f / (_pages);

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
                _pageBeginning = _viewableItems * _currentPage;
                _pageEnding = _pageBeginning + _viewableItems;

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
                _collection[i].Update(mouseState, oldMouseState, mouseBoundingBox, gameTime);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void AddCatalogItem(GuiDrawable item)
        {
            _collection.Add(item);

            UpdatePagination();
        }

        /// <summary>
        /// Get the next position according to the number of items within the catalog.
        /// </summary>
        /// <returns>Vector2</returns>
        public Vector2 GetNextPosition()
        {
            /** signifies the location of the item on the page */
            int instance = _collection.Count % _viewableItems;

            Vector2 itemPosition = new Vector2(
                Rectangle.Left + ((Rectangle.Width - ItemSize.X) / 2),
                Rectangle.Top + (instance * (ItemSize.Y + MARGIN))
            );

            return itemPosition;
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdatePagination()
        {
            int count = _collection.Count;

            /** Zero based paging system */
            _pages = (int)Math.Ceiling((1.0 * count) / _viewableItems);
            _scrollable.Count = _pages;
            _pageBeginning = 0;
            _pageEnding = count < _viewableItems ? count : _viewableItems - 1;
        }

        public void ClearCollection()
        {
            _collection.Clear();
        }
    }
}
