namespace Oxide.Plugins
{
    using Newtonsoft.Json;
    using Oxide.Game.Rust.Cui;
    using System;
    using UnityEngine;
    using static Oxide.Plugins.GUICreator.GuiContainer;

    public partial class GUICreator
    {
        [JsonObject(MemberSerialization.OptIn)]
        public class GuiPlainPanel : GuiElement
        {
            public GuiColor Color { get; set; }

            public GuiPlainPanel() { }

            public static GuiPlainPanel GetNewGuiPlainPanel(string name, Rectangle rectangle, GuiElement parent = null, Layer layer = Layer.hud, GuiColor panelColor = null, float fadeIn = 0, float fadeOut = 0, Blur blur = Blur.none)
            {

                //PluginInstance.Puts($"name: {name ?? "null"}, rect: {rectangle?.ToString() ?? "null"}, parent: {parent?.ToString() ?? "null"}, layer: {layer}, panelColor: {panelColor?.ToString() ?? "null"}, fadein: {fadeIn}, fadeOut: {fadeOut}, blur: {blur}");

                Layer higherLayer = layer;
                if (parent != null) higherLayer = (Layer)Math.Min((int)layer, (int)parent.Layer);

                string materialString = "Assets/Icons/IconMaterial.mat";
                if (blur != Blur.none) materialString = blurs[(int)blur];

                return new GuiPlainPanel
                {
                    Name = name,
                    Rectangle = rectangle.WithParent(parent?.Rectangle),
                    Layer = higherLayer,
                    Parent = layers[(int)higherLayer],
                    Color = panelColor,
                    FadeOut = fadeOut,
                    Components =
                    {
                        new CuiImageComponent
                        {
                            Color = panelColor?.getColorString() ?? "0 0 0 0",
                            FadeIn = fadeIn,
                            Material = materialString
                        },
                        rectangle.WithParent(parent?.Rectangle)
                    }
                };
            }
        }
    }
}