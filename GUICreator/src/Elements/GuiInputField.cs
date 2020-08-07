namespace Oxide.Plugins
{
    using Newtonsoft.Json;
    using Oxide.Core.Plugins;
    using Oxide.Game.Rust.Cui;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using static Oxide.Plugins.GUICreator.GuiContainer;

    public partial class GUICreator
    {
        [JsonObject(MemberSerialization.OptIn)]
        public class GuiInputField : GuiElement
        {
            public List<GuiElement> Panel { get; set; }

            public GuiText Text { get; set; }

            public Action<BasePlayer, string[]> Callback { get; set; }

            public GuiInputField() { }

            public static List<GuiElement> GetNewGuiInputField(
                Plugin plugin,
                GuiContainer container,
                string name, 
                Rectangle rectangle, 
                Action<BasePlayer, 
                string[]> callback, 
                GuiElement parent,
                Layer layer, 
                string close = null, 
                GuiColor panelColor = null, 
                int charLimit = 100, 
                GuiText text = null, 
                float FadeIn = 0, 
                float FadeOut = 0, 
                bool isPassword = false, 
                bool CursorEnabled = true, 
                string imgName = null,
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

                if (text != null) text.FadeIn = FadeIn;

                if (imgName != null || panelColor != null)
                {
                    elements.AddRange(
                        GuiPanel.GetNewGuiPanel(
                            plugin,
                            name + "_label",
                            rectangle,
                            parent,
                            layer,
                            panelColor,
                            FadeIn,
                            FadeOut,
                            null,
                            imgName,
                            blur
                            ));
                }

                elements.Add(new GuiElement 
                { 
                    Name = name,
                    Rectangle = rectangle.WithParent(parent?.Rectangle),
                    Layer = higherLayer,
                    Parent = layers[(int)higherLayer],
                    FadeOut = FadeOut,
                    ParentElement = parent,
                    Components =
                    {
                        new CuiInputFieldComponent
                        {
                            Align = text.Align, 
                            FontSize = text.FontSize, 
                            Color = text.Color, 
                            Command = $"gui.input {plugin.Name} {container.name} {removeWhiteSpaces(name)}{closeString} --input", 
                            CharsLimit = charLimit, 
                            IsPassword = isPassword
                        },
                        rectangle.WithParent(parent?.Rectangle)
                    }
                });

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