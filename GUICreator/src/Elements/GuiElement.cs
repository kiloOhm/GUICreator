namespace Oxide.Plugins
{
    using Newtonsoft.Json;
    using Oxide.Game.Rust.Cui;

    public partial class GUICreator
    {
        public class GuiElement : CuiElement
        {
            [JsonIgnore]
            public Rectangle Rectangle { get; set; }

            [JsonIgnore]
            public GuiElement ParentElement { get; set; }

            [JsonIgnore]
            public GuiContainer.Layer Layer { get; set; }

            public GuiElement() { }

            public GuiElement(Rectangle rectangle, GuiElement parent = null, GuiContainer.Layer layer = GuiContainer.Layer.hud)
            {
                Rectangle = rectangle.WithParent(parent?.Rectangle);
                ParentElement = parent;
                Layer = layer;
                Parent = GuiContainer.layers[(int)layer];
            }
        }
    }
}