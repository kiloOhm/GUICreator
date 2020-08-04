namespace Oxide.Plugins
{
    using Facepunch.Extend;
    using Newtonsoft.Json;
    using System;
    using UnityEngine;
    using static Oxide.Plugins.GUICreator.GuiContainer;

    public partial class GUICreator
    {

        [JsonObject(MemberSerialization.OptIn)]
        public class GuiLabel : GuiElement
        {
            public GuiText Text { get; set; }

            public GuiLabel() { }

            public static GuiLabel GetNewGuiLabel(string name, Rectangle rectangle, GuiElement parent = null, Layer layer = Layer.hud, GuiText text = null, float fadeIn = 0, float fadeOut = 0)
            {

                Layer higherLayer = layer;
                if (parent != null) higherLayer = (Layer)Math.Min((int)layer, (int)parent.Layer);

                if (text != null) text.FadeIn = fadeIn;

                return new GuiLabel
                {
                    Name = name,
                    Rectangle = rectangle.WithParent(parent?.Rectangle),
                    Layer = higherLayer,
                    Parent = layers[(int)higherLayer],
                    Text = text,
                    FadeOut = fadeOut,
                    Components =
                    {
                        text,
                        rectangle.WithParent(parent?.Rectangle)
                    }
                };
            }
        }
    }
}