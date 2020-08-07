namespace Oxide.Plugins
{
    using Newtonsoft.Json;
    using Oxide.Core.Plugins;
    using Oxide.Game.Rust.Cui;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Text;
    using static Oxide.Plugins.GUICreator.GuiContainer;

    public partial class GUICreator
    {
        [JsonObject(MemberSerialization.OptIn)]
        public class GuiPlainButton : GuiElement
        {
            public GuiText Text { get; set; }

            public Action<BasePlayer, string[]> Callback { get; set; }

            public GuiLabel Label { get; set; }

            public GuiPlainButton() { }

            public static List<GuiElement> GetNewGuiPlainButton(
                Plugin plugin, 
                GuiContainer container, 
                string name, 
                Rectangle rectangle, 
                GuiElement parent = null, 
                Layer layer = Layer.hud, 
                GuiColor panelColor = null, 
                float fadeIn = 0, 
                float fadeOut = 0, 
                GuiText text = null, 
                Action<BasePlayer, string[]> callback = null, 
                string close = null, 
                bool CursorEnabled = true, 
                Blur blur = Blur.none)
            {
                List<GuiElement> elements = new List<GuiElement>();

                Layer higherLayer = layer;
                if (parent != null) higherLayer = (Layer)Math.Min((int)layer, (int)parent.Layer);

                StringBuilder closeString = new StringBuilder("");
                if (close != null)
                {
                    closeString.Append(" --close ");
                    closeString.Append(close);
                }

                string materialString = "Assets/Icons/IconMaterial.mat";
                if (blur != Blur.none) materialString = blurs[(int)blur];

                if (text != null) text.FadeIn = fadeIn;

                GuiPlainButton button = new GuiPlainButton
                {
                    Name = name,
                    Rectangle = rectangle.WithParent(parent?.Rectangle),
                    Layer = higherLayer,
                    Parent = layers[(int)higherLayer],
                    ParentElement = parent,
                    FadeOut = fadeOut,
                    Components =
                    {
                        new CuiButtonComponent {
                            Command = $"gui.input {plugin.Name} {container.name} {removeWhiteSpaces(name)}{closeString}", 
                            FadeIn = fadeIn, 
                            Color = panelColor?.getColorString() ?? "0 0 0 0", 
                            Material = materialString},
                        rectangle.WithParent(parent?.Rectangle)
                    },
                    Label = new GuiLabel
                    {
                        Name = name + "_txt",
                        Rectangle = new Rectangle(),
                        Layer = higherLayer,
                        Parent = name,
                        Text = text,
                        FadeOut = fadeOut,
                        Components =
                        {
                            text,
                            new Rectangle()
                        }
                    }
                };

                elements.Add(button);

                if(text != null) elements.Add(button.Label);

                if (CursorEnabled)
                {
                    elements.Add(new GuiElement()
                    {
                        Name = name + "_cursor",
                        Parent = name,
                        Components =
                        {
                            new CuiNeedsCursorComponent()
                        }
                    });
                }

                return elements;
            }
        }
    }
}