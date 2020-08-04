namespace Oxide.Plugins
{
    using Newtonsoft.Json;
    using Oxide.Core.Plugins;
    using Oxide.Game.Rust.Cui;
    using System;
    using static Oxide.Plugins.GUICreator.GuiContainer;

    public partial class GUICreator
    {
        [JsonObject(MemberSerialization.OptIn)]
        public class GuiImage : GuiElement
        {
            public GuiColor Color { get; set; }

            public GuiImage() { }

            public static GuiImage GetNewGuiImage(Plugin plugin, string name, Rectangle rectangle, string imgNameOrData, bool raw = false, GuiElement parent = null, Layer layer = Layer.hud, GuiColor panelColor = null, float fadeIn = 0, float fadeOut = 0)
            {
                Layer higherLayer = layer;
                if (parent != null) higherLayer = (Layer)Math.Min((int)layer, (int)parent.Layer);

                return new GuiImage
                {
                    Name = name,
                    Rectangle = rectangle.WithParent(parent?.Rectangle),
                    Layer = higherLayer,
                    Parent = layers[(int)higherLayer],
                    Color = panelColor,
                    FadeOut = fadeOut,
                    Components =
                    {
                        new CuiRawImageComponent { 
                            Color = panelColor?.getColorString() ?? "1 1 1 1", 
                            FadeIn = fadeIn, 
                            Png = raw?imgNameOrData:PluginInstance.getImageData(plugin, imgNameOrData)
                        },
                        rectangle.WithParent(parent?.Rectangle)
                    }
                };
            }
        }
    }
}