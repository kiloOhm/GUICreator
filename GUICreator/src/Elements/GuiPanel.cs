namespace Oxide.Plugins
{
    using Newtonsoft.Json;
    using Oxide.Core.Plugins;
    using System;
    using System.Collections.Generic;
    using static Oxide.Plugins.GUICreator.GuiContainer;

    public partial class GUICreator
    {
        [JsonObject(MemberSerialization.OptIn)]
        public class GuiPanel
        {
            public GuiPanel() { }

            public static List<GuiElement> GetNewGuiPanel(
                Plugin plugin,
                string name, 
                Rectangle rectangle, 
                GuiElement parent, 
                Layer layer, 
                GuiColor panelColor = null, 
                float FadeIn = 0, 
                float FadeOut = 0, 
                GuiText text = null, 
                string imgName = null, 
                Blur blur = Blur.none)
            {
                List<GuiElement> elements = new List<GuiElement>();

                Layer higherLayer = layer;
                if (parent != null) higherLayer = (Layer)Math.Min((int)layer, (int)parent.Layer);

                if(string.IsNullOrEmpty(imgName))
                {
                    GuiPlainPanel plainPanel = GuiPlainPanel.GetNewGuiPlainPanel(name, rectangle, parent, layer, panelColor, FadeIn, FadeOut, blur);
                    elements.Add(plainPanel);
                }
                else
                {
                    GuiImage image = GuiImage.GetNewGuiImage(plugin, name, rectangle, imgName, false, parent, layer, panelColor, FadeIn, FadeOut);
                    elements.Add(image);
                }
                if (text != null)
                {
                    text.FadeIn = FadeIn;
                    GuiLabel label = new GuiLabel
                    {
                        Name = name + "_txt",
                        Rectangle = new Rectangle(),
                        Layer = higherLayer,
                        Parent = name,
                        Text = text,
                        FadeOut = FadeOut,
                        Components =
                        {
                            text,
                            new Rectangle()
                        }
                    };
                    elements.Add(label);
                }

                return elements;
            }
        }
    }
}