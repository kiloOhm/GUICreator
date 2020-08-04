namespace Oxide.Plugins
{
    using ConVar;
    using Newtonsoft.Json;
    using Oxide.Core.Plugins;
    using Oxide.Game.Rust.Cui;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    public partial class GUICreator
    {
        public class GuiContainer : List<GuiElement>
        {
            public Plugin plugin;
            public string name;
            public string parent;
            public List<Timer> timers = new List<Timer>();
            public Action<BasePlayer> closeCallback;
            private Dictionary<string, Action<BasePlayer, string[]>> callbacks = new Dictionary<string, Action<BasePlayer, string[]>>();

            private ExtendedCuiElementContainer CuiContainer { get
                {
                    ExtendedCuiElementContainer container = new ExtendedCuiElementContainer();
                    foreach( GuiElement e in this)
                    {
                        container.Add(e);
                    }
                    return container;
                } }

            public GuiContainer(Plugin plugin, string name, string parent = null, Action<BasePlayer> closeCallback = null)
            {
                this.plugin = plugin;
                this.name = removeWhiteSpaces(name);
                this.parent = removeWhiteSpaces(parent);
                this.closeCallback = closeCallback;
            }

            public enum Layer { overall, overlay, menu, hud, under };

            //Rust UI elements (inventory, Health, etc.) are between the hud and the menu layer
            public static List<string> layers = new List<string> { "Overall", "Overlay", "Hud.Menu", "Hud", "Under" };

            public enum Blur { slight, medium, strong, greyout, none };
            public static List<string> blurs = new List<string>
            {
                "assets/content/ui/uibackgroundblur-notice.mat",
                "assets/content/ui/uibackgroundblur-ingamemenu.mat",
                "assets/content/ui/uibackgroundblur.mat",
                "assets/icons/greyout.mat"
            };

            public void display(BasePlayer player)
            {
                if (this.Count == 0) return;

                foreach (CuiElement element in this)
                {
                    if (!string.IsNullOrEmpty(element.Name)) element.Name = PluginInstance.prependContainerName(this, element.Name);
                    if (!string.IsNullOrEmpty(element.Parent) && !layers.Contains(element.Parent)) element.Parent = PluginInstance.prependContainerName(this, element.Parent);
                }

                GuiTracker.getGuiTracker(player).addGuiToTracker(plugin, this);

#if DEBUG
                PluginInstance.Puts(JsonConvert.SerializeObject(this));
                player.ConsoleMessage(JsonConvert.SerializeObject(this));
#endif

                CuiHelper.AddUi(player, CuiContainer);
            }

            public void destroy(BasePlayer player)
            {
                GuiTracker.getGuiTracker(player).destroyGui(plugin, this);
            }

            public void registerCallback(string name, Action<BasePlayer, string[]> callback)
            {
                name = removeWhiteSpaces(name);
                if (callbacks.ContainsKey(name)) callbacks.Remove(name);
                callbacks.Add(name, callback);
            }

            public bool runCallback(string name, BasePlayer player, string[] input)
            {
                name = removeWhiteSpaces(name);
                if (!callbacks.ContainsKey(name)) return false;
                try
                {
                    callbacks[name](player, input);
                    return true;
                }
                catch (Exception E)
                {
                    PluginInstance.Puts($"Failed to run callback: {name}, {E.Message}");
                    return false;
                }
            }

            private void purgeDuplicates(string name)
            {
                foreach (GuiElement element in this)
                {
                    if (element.Name == name)
                    {
                        PluginInstance.Puts($"Duplicate element: {element.Name} in container: {this.name}");
                        this.Remove(element);
                        return;
                    }
                }
            }

            private GuiElement GetParent(string name)
            {
                foreach (GuiElement e in this)
                {
                    if (e.Name == parent)
                    {
                        return e;
                    }
                }
                return null;
            }

            public List<GuiElement> addPanel(string name, Rectangle rectangle, Layer layer, GuiColor panelColor = null, float FadeIn = 0, float FadeOut = 0, GuiText text = null, string imgName = null, Blur blur = Blur.none)
            {
                return addPanel(name, rectangle, null, layer, panelColor, FadeIn, FadeOut, text, imgName, blur);
            }

            public List<GuiElement> addPanel(string name, Rectangle rectangle, string parent = "Hud", GuiColor panelColor = null, float FadeIn = 0, float FadeOut = 0, GuiText text = null, string imgName = null, Blur blur = Blur.none)
            {
                return addPanel(name, rectangle, GetParent(parent), Layer.hud, panelColor, FadeIn, FadeOut, text, imgName, blur);
            }

            public List<GuiElement> addPanel(string name, Rectangle rectangle, GuiElement parent, Layer layer, GuiColor panelColor = null, float FadeIn = 0, float FadeOut = 0, GuiText text = null, string imgName = null, Blur blur = Blur.none)
            {
                if (string.IsNullOrEmpty(name)) name = "panel";
                purgeDuplicates(name);

                List<GuiElement> panel = GuiPanel.GetNewGuiPanel(plugin, name, rectangle, parent, layer, panelColor, FadeIn, FadeOut, text, imgName, blur);

                AddRange(panel);
                return panel;
            }

            public GuiPlainPanel addPlainPanel(string name, Rectangle rectangle, Layer layer, GuiColor panelColor = null, float FadeIn = 0, float FadeOut = 0, Blur blur = Blur.none)
            {
                return addPlainPanel(name, rectangle, null, layer, panelColor, FadeIn, FadeOut, blur);
            }

            public GuiPlainPanel addPlainPanel(string name, Rectangle rectangle, string parent, GuiColor panelColor = null, float FadeIn = 0, float FadeOut = 0, Blur blur = Blur.none)
            {
                return addPlainPanel(name, rectangle, GetParent(parent), Layer.hud, panelColor, FadeIn, FadeOut, blur);
            }

            public GuiPlainPanel addPlainPanel(string name, Rectangle rectangle, GuiElement parent = null, Layer layer = Layer.hud, GuiColor panelColor = null, float FadeIn = 0, float FadeOut = 0, Blur blur = Blur.none)
            {
                if (string.IsNullOrEmpty(name)) name = "plainPanel";

                purgeDuplicates(name);

                GuiPlainPanel pp = GuiPlainPanel.GetNewGuiPlainPanel(name, rectangle, parent, layer, panelColor, FadeIn, FadeOut, blur);

                Add(pp);
                return pp;
            }

            public GuiImage addImage(string name, Rectangle rectangle, string imgName, Layer layer, GuiColor panelColor = null, float FadeIn = 0, float FadeOut = 0)
            {
                return addImage(name, rectangle, imgName, null, layer , panelColor, FadeIn, FadeOut);
            }

            public GuiImage addImage(string name, Rectangle rectangle, string imgName, string parent = "Hud", GuiColor panelColor = null, float FadeIn = 0, float FadeOut = 0)
            {
                return addImage(name, rectangle, imgName, GetParent(parent), Layer.hud, panelColor, FadeIn, FadeOut);
            }

            public GuiImage addImage(string name, Rectangle rectangle, string imgName, GuiElement Parent, Layer layer, GuiColor panelColor = null, float FadeIn = 0, float FadeOut = 0)
            {
                if (string.IsNullOrEmpty(name)) name = "image";
                purgeDuplicates(name);

                GuiImage img = GuiImage.GetNewGuiImage(plugin, name, rectangle, imgName, false, Parent, layer, panelColor, FadeIn, FadeOut);

                Add(img);

                return img;
            }

            public GuiImage addRawImage(string name, Rectangle rectangle, string imgData, Layer layer, GuiColor panelColor = null, float FadeIn = 0, float FadeOut = 0)
            {
                return addRawImage(name, rectangle, imgData, null, layer, panelColor, FadeIn, FadeOut);
            }

            public GuiImage addRawImage(string name, Rectangle rectangle, string imgData, string parent = "Hud", GuiColor panelColor = null, float FadeIn = 0, float FadeOut = 0)
            {
                return addRawImage(name, rectangle, imgData, GetParent(parent), Layer.hud, panelColor, FadeIn, FadeOut);
            }

            public GuiImage addRawImage(string name, Rectangle rectangle, string imgData, GuiElement Parent, Layer layer, GuiColor panelColor = null, float FadeIn = 0, float FadeOut = 0)
            {
                if (string.IsNullOrEmpty(name)) name = "image";
                purgeDuplicates(name);

                GuiImage img = GuiImage.GetNewGuiImage(plugin, name, rectangle, imgData, true, Parent, layer, panelColor, FadeIn, FadeOut);

                Add(img);

                return img;
            }

            public GuiLabel addText(string name, Rectangle rectangle, Layer layer, GuiText text = null, float FadeIn = 0, float FadeOut = 0)
            {
                return addText(name, rectangle, null, layer, text, FadeIn, FadeOut);
            }

            public GuiLabel addText(string name, Rectangle rectangle, GuiText text = null, float FadeIn = 0, float FadeOut = 0, string parent = "Hud")
            {
                return addText(name, rectangle, GetParent(parent), Layer.hud, text, FadeIn, FadeOut);
            }

            public GuiLabel addText(string name, Rectangle rectangle, GuiElement Parent, Layer layer, GuiText text = null, float FadeIn = 0, float FadeOut = 0)
            {
                if (string.IsNullOrEmpty(name)) name = "text";

                purgeDuplicates(name);

                GuiLabel label = GuiLabel.GetNewGuiLabel(name, rectangle, Parent, layer, text, FadeIn, FadeOut);

                Add(label);

                return label;
            }

            public List<GuiElement> addButton(string name, Rectangle rectangle, Layer layer, GuiColor panelColor = null, float FadeIn = 0, float FadeOut = 0, GuiText text = null, Action<BasePlayer, string[]> callback = null, string close = null, bool CursorEnabled = true, string imgName = null, Blur blur = Blur.none)
            {
                return addButton(name, rectangle, null, layer, panelColor, FadeIn, FadeOut, text, callback, close, CursorEnabled, imgName, blur);
            }

            public List<GuiElement> addButton(string name, Rectangle rectangle, GuiColor panelColor = null, float FadeIn = 0, float FadeOut = 0, GuiText text = null, Action<BasePlayer, string[]> callback = null, string close = null, bool CursorEnabled = true, string imgName = null, string parent = "Hud", Blur blur = Blur.none)
            {
                return addButton(name, rectangle, GetParent(parent), Layer.hud, panelColor, FadeIn, FadeOut, text, callback, close, CursorEnabled, imgName, blur);
            }

            public List<GuiElement> addButton(string name, Rectangle rectangle, GuiElement parent, Layer layer, GuiColor panelColor = null, float FadeIn = 0, float FadeOut = 0, GuiText text = null, Action<BasePlayer, string[]> callback = null, string close = null, bool CursorEnabled = true, string imgName = null, Blur blur = Blur.none)
            {
                if (string.IsNullOrEmpty(name)) name = "button";
                purgeDuplicates(name);

                List<GuiElement> elements = new List<GuiElement>();
                 
                if (imgName != null)
                {
                    PluginInstance.PrintToChat("test");
                    elements.Add(addImage(name + "_img", rectangle, imgName, parent, layer, panelColor, FadeIn, FadeOut));
                    elements.AddRange(addPlainButton(name + "_btn", rectangle, parent, layer, new GuiColor(0,0,0,0), FadeIn, FadeOut, text, callback, close, CursorEnabled));
                }
                else
                {
                    elements.AddRange(addPlainButton(name, rectangle, parent, layer, panelColor, FadeIn, FadeOut, text, callback, close, CursorEnabled, blur));
                }
                return elements;
            }

            public List<GuiElement> addPlainButton(string name, Rectangle rectangle, Layer layer, GuiColor panelColor = null, float FadeIn = 0, float FadeOut = 0, GuiText text = null, Action<BasePlayer, string[]> callback = null, string close = null, bool CursorEnabled = true, Blur blur = Blur.none)
            {
                return addPlainButton(name, rectangle, null, layer, panelColor, FadeIn, FadeOut, text, callback, close, CursorEnabled, blur);
            }

            public List<GuiElement> addPlainButton(string name, Rectangle rectangle, GuiColor panelColor = null, float FadeIn = 0, float FadeOut = 0, GuiText text = null, Action<BasePlayer, string[]> callback = null, string close = null, bool CursorEnabled = true, string parent = "Hud", Blur blur = Blur.none)
            {
                return addPlainButton(name, rectangle, GetParent(parent), Layer.hud, panelColor, FadeIn, FadeOut, text, callback, close, CursorEnabled, blur);
            }

            public List<GuiElement> addPlainButton(string name, Rectangle rectangle, GuiElement parent, Layer layer, GuiColor panelColor = null, float FadeIn = 0, float FadeOut = 0, GuiText text = null, Action<BasePlayer, string[]> callback = null, string close = null, bool CursorEnabled = true, Blur blur = Blur.none)
            {
                if (string.IsNullOrEmpty(name)) name = "plainButton";

                purgeDuplicates(name);

                List<GuiElement> elements = GuiPlainButton.GetNewGuiPlainButton(plugin, this, name, rectangle, parent, layer, panelColor, FadeIn, FadeOut, text, callback, close, CursorEnabled, blur);

                if (callback != null) this.registerCallback(name, callback);

                AddRange(elements);

                return elements;
            }

            public void addInput(string name, Rectangle rectangle, Action<BasePlayer, string[]> callback, Layer layer, string close = null, GuiColor panelColor = null, int charLimit = 100, GuiText text = null, float FadeIn = 0, float FadeOut = 0, bool isPassword = false, bool CursorEnabled = true, string imgName = null)
            {
                addInput(name, rectangle, callback, layers[(int)layer], close, panelColor, charLimit, text, FadeIn, FadeOut, isPassword, CursorEnabled, imgName);
            }

            public void addInput(string name, Rectangle rectangle, Action<BasePlayer, string[]> callback, string parent = "Hud", string close = null, GuiColor panelColor = null, int charLimit = 100, GuiText text = null, float FadeIn = 0, float FadeOut = 0, bool isPassword = false, bool CursorEnabled = true, string imgName = null)
            {
                //if (string.IsNullOrEmpty(name)) name = "input";

                //purgeDuplicates(name);

                //StringBuilder closeString = new StringBuilder("");
                //if (close != null)
                //{
                //    closeString.Append(" --close ");
                //    closeString.Append(close);
                //}

                //if (imgName != null || panelColor != null)
                //{
                //    this.addPanel(name, rectangle, parent, panelColor, FadeIn, FadeOut, null, imgName);

                //    this.Add(new CuiInputField()
                //    {
                //        InputField = { Align = text.Align, FontSize = text.FontSize, Color = text.Color, Command = $"gui.input {plugin.Name} {this.name} {removeWhiteSpaces(name)}{closeString.ToString()} --input", CharsLimit = charLimit, IsPassword = isPassword },
                //        RectTransform = new Rectangle(),
                //        CursorEnabled = CursorEnabled,
                //        FadeOut = FadeOut
                //    }, name, name + "_ipt");
                //}
                //else
                //{
                //    this.Add(new CuiInputField()
                //    {
                //        InputField = { Align = text.Align, FontSize = text.FontSize, Color = text.Color, Command = $"gui.input text {plugin.Name} {this.name} {removeWhiteSpaces(name)}{closeString.ToString()} --input", CharsLimit = charLimit, IsPassword = isPassword },
                //        RectTransform = rectangle,
                //        CursorEnabled = CursorEnabled,
                //        FadeOut = FadeOut
                //    }, parent, name);
                //}

                //if (callback != null) this.registerCallback(name, callback);
            }
        }
    }
}