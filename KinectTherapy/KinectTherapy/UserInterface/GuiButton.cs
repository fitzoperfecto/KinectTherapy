using System;
using Microsoft.Xna.Framework;

namespace SWENG.UserInterface
{
    public delegate void GuiButtonClicked(object sender, GuiButtonClickedArgs e);

    public class GuiButtonClickedArgs : EventArgs
    {
        public string ClickedOn;

        public GuiButtonClickedArgs(string clickedOn)
        {
            ClickedOn = clickedOn;
        }
    }

    public class GuiButton : GuiDrawable
    {
        #region event stuff
        public event GuiButtonClicked ClickEvent;

        // Invoke the Changed event; called whenever repetitions changes
        protected virtual void OnClick(GuiButtonClickedArgs e)
        {
            if (ClickEvent != null)
                ClickEvent(this, e);
        }
        #endregion

        public GuiButton(string text, Vector2 size, Vector2 position)
            : base(text, size, position) { }

        public void Click()
        {
            OnClick(new GuiButtonClickedArgs(Text));
        }
    }
}
