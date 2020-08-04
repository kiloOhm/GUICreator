namespace Oxide.Plugins
{
    using Oxide.Game.Rust.Cui;
    using UnityEngine;

    public partial class GUICreator
    {
        public class GuiText : CuiTextComponent
        {
            public GuiText()
            {
            }

            public GuiText(string text, int fontSize = 14, GuiColor color = null, TextAnchor align = TextAnchor.MiddleCenter, float FadeIn = 0)
            {
                this.Text = text;
                this.FontSize = fontSize;
                this.Align = align;
                this.Color = color?.getColorString() ?? new GuiColor(0, 0, 0, 1).getColorString();
                this.FadeIn = FadeIn;
            }
        }
    }
}