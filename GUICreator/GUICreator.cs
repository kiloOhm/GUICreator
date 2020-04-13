//#define DEBUG

using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("GUICreator", "OHM", "1.2.0")]
    [Description("GUICreator")]
    class GUICreator : RustPlugin
    {
        #region global

        [PluginReference]
        private Plugin ImageLibrary;

        private static GUICreator PluginInstance = null;

        public GUICreator()
        {
            PluginInstance = this;
        }

        #endregion

        #region oxide hooks
        void Init()
        {
            permission.RegisterPermission("gui.demo", this);

            lang.RegisterMessages(messages, this);
            cmd.AddConsoleCommand("gui.close", this, nameof(closeUi));
            cmd.AddConsoleCommand("gui.input", this, nameof(OnGuiInput));
        }

        void OnServerInitialized()
        {
            if (ImageLibrary == null)
            {
                Puts("ImageLibrary is not loaded!");
                return;
            }
            registerImage("flower", "https://i.imgur.com/uAhjMNd.jpg");
            registerImage("gameTipIcon", config.gameTipIcon);
            registerImage("bgTex", "https://i.imgur.com/OAa71Rt.png");
            registerImage("warning_alpha", "https://i.imgur.com/u0bNKXx.png");
            registerImage("white_cross", "https://i.imgur.com/fbwkYDj.png");
        }

        void Unload()
        {
            foreach (BasePlayer player in BasePlayer.activePlayerList)
            {
                GuiTracker.getGuiTracker(player).destroyAllGui();
            }
        }

        void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            GuiTracker.getGuiTracker(player).destroyAllGui();
        }

        object OnPlayerWound(BasePlayer player)
        {
            GuiTracker.getGuiTracker(player).destroyAllGui();
            return null;
        }

        object OnPlayerDeath(BasePlayer player, HitInfo info)
        {
            GuiTracker.getGuiTracker(player).destroyAllGui();
            return null;
        }

        private object OnServerCommand(ConsoleSystem.Arg arg)
        {
#if DEBUG
            PrintToChat(arg.FullString);
#endif
            return null;
        }

        #endregion

        #region classes

        public class Rectangle
        {
            public float anchorMinX;
            public float anchorMinY;
            public float anchorMaxX;
            public float anchorMaxY;
            public string anchorMin => $"{anchorMinX} {anchorMinY}";
            public string anchorMax => $"{anchorMaxX} {anchorMaxY}";

            public Rectangle() { }

            public Rectangle(float X, float Y, float W, float H, int resX = 1, int resY = 1, bool topLeftOrigin = false)
            {
                float newY = topLeftOrigin ? resY - Y - H : Y;

                anchorMinX = X / resX;
                anchorMinY = newY / resY;
                anchorMaxX = (X + W) / resX;
                anchorMaxY = (newY + H) / resY;
            }

        }

        public class GuiColor
        {
            Color color;

            public GuiColor() { }

            public GuiColor(float R, float G, float B, float alpha)
            {
                if (R > 1) R /= 255;
                if (G > 1) G /= 255;
                if (B > 1) B /= 255;
                color = new Color(R, G, B, alpha);
            }

            public GuiColor(string hex)
            {
                ColorUtility.TryParseHtmlString(hex, out color);
            }

            public string getColorString()
            {
                return $"{color.r} {color.g} {color.b} {color.a}";
            }
        }

        public class GuiText
        {
            public string text;
            public int fontSize;
            public TextAnchor align;
            public GuiColor color;

            public GuiText() { }

            public GuiText(string text, int fontSize = 14, GuiColor color = null, TextAnchor align = TextAnchor.MiddleCenter)
            {
                this.text = text;
                this.fontSize = fontSize;
                this.align = align;
                this.color = color ?? new GuiColor(0, 0, 0, 1);
            }
        }

        public class CuiInputField
        {
            //public CuiImageComponent Image { get; set; } = new CuiImageComponent();
            public CuiInputFieldComponent InputField { get; } = new CuiInputFieldComponent();
            public CuiRectTransformComponent RectTransform { get; } = new CuiRectTransformComponent();
            public bool CursorEnabled { get; set; }
            public float FadeOut { get; set; }
        }

        public class GuiContainer : CuiElementContainer
        {
            public Plugin plugin;
            public string name;
            public List<Timer> timers = new List<Timer>();
            private Dictionary<string, Action<BasePlayer>> buttonCallbacks = new Dictionary<string, Action<BasePlayer>>();
            private Dictionary<string, Action<BasePlayer, string>> inputCallbacks = new Dictionary<string, Action<BasePlayer, string>>();

            public GuiContainer(Plugin plugin, string name)
            {
                this.plugin = plugin;
                this.name = name;
            }

            public enum Layer { overall, overlay, menu, hud, under };
            //Rust UI elements (inventory, Health, etc.) are between the hud and the menu layer
            private List<string> layers = new List<string> {"Overall", "Overlay", "Hud.Menu", "Hud", "Under"};

            public void display(BasePlayer player)
            {
                if (this.Count == 0) return;
                GuiTracker.getGuiTracker(player).addGuiToTracker(this);
                CuiHelper.AddUi(player, this);
            }

            public void registerCallback(string name, Action<BasePlayer> callback)
            {
                if (buttonCallbacks.ContainsKey(name)) buttonCallbacks.Remove(name);
                buttonCallbacks.Add(name, callback);
            }

            public void registerCallback(string name, Action<BasePlayer, string> callback)
            {
                if (inputCallbacks.ContainsKey(name)) buttonCallbacks.Remove(name);
                inputCallbacks.Add(name, callback);
            }

            public bool runCallback(string name, BasePlayer player)
            {
                if (!buttonCallbacks.ContainsKey(name)) return false;
                try
                {
                    buttonCallbacks[name](player);
                    return true;
                }
                catch(Exception E)
                {
                    PluginInstance.Puts($"Failed to run callback: {name}, {E.Message}");
                    return false;
                }
            }

            public bool runCallback(string name, BasePlayer player, string input)
            {
                if (!inputCallbacks.ContainsKey(name)) return false;
                try
                {
                    inputCallbacks[name](player, input);
                    return true;
                }
                catch (Exception E)
                {
                    PluginInstance.Puts($"Failed to run callback: {name}, {E.Message}");
                    return false;
                }
            }

            public string Add(CuiInputField InputField, string Parent = "Hud", string Name = "")
            {
                if (string.IsNullOrEmpty(Name)) Name = CuiHelper.GetGuid();

                CuiElement element = new CuiElement()
                {
                    Name = Name,
                    Parent = Parent,
                    FadeOut = InputField.FadeOut,
                    Components = {
                    InputField.InputField,
                    InputField.RectTransform
                }
                };
                //if (InputField.Image != null) element.Components.Add(InputField.Image);
                if (InputField.CursorEnabled) element.Components.Add(new CuiNeedsCursorComponent());

                Add(element);
                return Name;
            }

            public void addPanel(string name, Rectangle rectangle, Layer layer, GuiColor panelColor = null, float FadeIn = 0, float FadeOut = 0, GuiText text = null, string imgName = null)
            {
                addPanel(name, rectangle, layers[(int)layer], panelColor, FadeIn, FadeOut, text, imgName);
            }

            public void addPanel(string name, Rectangle rectangle, string parent = "Hud", GuiColor panelColor = null, float FadeIn = 0, float FadeOut = 0, GuiText text = null, string imgName = null)
            {
                if (string.IsNullOrEmpty(name)) name = "panel";
                if (name == this.name) name = name + "_";

                if (string.IsNullOrEmpty(imgName))
                {
                    addPlainPanel(name, rectangle, parent, panelColor, FadeIn, FadeOut);
                }
                else
                {
                   this.addImage(name, rectangle, imgName, parent, panelColor, FadeIn, FadeOut);
                }
                if (text != null) this.addText(name + "_txt", new Rectangle(0, 0, 1, 1), text, FadeIn, FadeOut, name);
            }

            public void addPlainPanel(string name, Rectangle rectangle, Layer layer, GuiColor panelColor = null, float FadeIn = 0, float FadeOut = 0)
            {
                addPlainPanel(name, rectangle, layers[(int)layer], panelColor, FadeIn, FadeOut);
            }

            public void addPlainPanel(string name, Rectangle rectangle, string parent = "Hud", GuiColor panelColor = null, float FadeIn = 0, float FadeOut = 0)
            {
                if (string.IsNullOrEmpty(name)) name = "plainPanel";
                if (name == this.name) name = name + "_";

                this.Add(new CuiElement
                {
                    Parent = parent,
                    Name = name,
                    Components =
                {
                    new CuiImageComponent { Color = (panelColor != null)?panelColor.getColorString():"0 0 0 0", FadeIn = FadeIn},
                    new CuiRectTransformComponent { AnchorMin = rectangle.anchorMin, AnchorMax = rectangle.anchorMax }
                },
                    FadeOut = FadeOut
                });
            }

            public void addImage(string name, Rectangle rectangle, string imgName, Layer layer, GuiColor panelColor = null, float FadeIn = 0, float FadeOut = 0)
            {
                addImage(name, rectangle, imgName, layers[(int)layer], panelColor, FadeIn, FadeOut);
            }

            public void addImage(string name, Rectangle rectangle, string imgName, string parent = "Hud", GuiColor panelColor = null, float FadeIn = 0, float FadeOut = 0)
            {
                if (string.IsNullOrEmpty(name)) name = "image";
                if (name == this.name) name = name + "_";

                this.Add(new CuiElement
                {
                    Parent = parent,
                    Name = name,
                    Components =
                {
                    new CuiRawImageComponent { Color = (panelColor != null)?panelColor.getColorString():"1 1 1 1", FadeIn = FadeIn, Png = (string)PluginInstance.ImageLibrary.Call("GetImage", imgName)},
                    new CuiRectTransformComponent { AnchorMin = rectangle.anchorMin, AnchorMax = rectangle.anchorMax }
                },
                    FadeOut = FadeOut
                });
            }

            public void addText(string name, Rectangle rectangle, Layer layer, GuiText text = null, float FadeIn = 0, float FadeOut = 0)
            {
                addText(name, rectangle, text, FadeIn, FadeOut, layers[(int)layer]);
            }

            public void addText(string name, Rectangle rectangle, GuiText text = null, float FadeIn = 0, float FadeOut = 0, string parent = "Hud")
            {
                if (string.IsNullOrEmpty(name)) name = "text";
                if (name == this.name) name = name + "_";

                this.Add(new CuiElement
                {
                    Parent = parent,
                    Name = name,
                    Components =
                {
                    new CuiTextComponent { FadeIn = FadeIn, Text = text.text, FontSize = text.fontSize, Align = text.align, Color = text.color.getColorString()},
                    new CuiRectTransformComponent { AnchorMin = rectangle.anchorMin, AnchorMax = rectangle.anchorMax }
                },
                    FadeOut = FadeOut
                });
            }

            public void addButton(string name, Rectangle rectangle, Layer layer, GuiColor panelColor = null, float FadeIn = 0, GuiText text = null, Action<BasePlayer> callback = null, string close = null, bool CursorEnabled = true, string imgName = null)
            {
                addButton(name, rectangle, panelColor, FadeIn, text, callback, close, CursorEnabled, imgName, layers[(int)layer]);
            }

            public void addButton(string name, Rectangle rectangle, GuiColor panelColor = null, float FadeIn = 0, GuiText text = null, Action<BasePlayer> callback = null, string close = null, bool CursorEnabled = true, string imgName = null, string parent = "Hud")
            {
                if (string.IsNullOrEmpty(name)) name = "button";
                if (name == this.name) name = name + "_";

                if (imgName != null)
                {
                    this.addImage(name, rectangle, imgName, parent, null, FadeIn);
                    this.addPlainButton(name + "_btn", new Rectangle(0, 0, 1, 1), panelColor, FadeIn, text, callback, close, CursorEnabled, name);
                }
                else
                {
                    this.addPlainButton(name, rectangle, panelColor, FadeIn, text, callback, close, CursorEnabled, parent);
                }
            }

            public void addPlainButton(string name, Rectangle rectangle, Layer layer, GuiColor panelColor = null, float FadeIn = 0, GuiText text = null, Action<BasePlayer> callback = null, string close = null, bool CursorEnabled = true)
            {
                addPlainButton(name, rectangle, panelColor, FadeIn, text, callback, close, CursorEnabled, layers[(int)layer]);
            }

            public void addPlainButton(string name, Rectangle rectangle, GuiColor panelColor = null, float FadeIn = 0, GuiText text = null, Action<BasePlayer> callback = null, string close = null, bool CursorEnabled = true, string parent = "Hud")
            {
                if (string.IsNullOrEmpty(name)) name = "plainButton";
                if (name == this.name) name = name + "_";

                StringBuilder closeString = new StringBuilder("");
                if (close != null)
                {
                    closeString.Append(" close ");
                    closeString.Append(close);
                }

                this.Add(new CuiButton()
                {
                    Button = { Command = $"gui.input button {plugin.Name} {this.name} {name}{closeString.ToString()}", FadeIn = FadeIn, Color = (panelColor != null) ? panelColor.getColorString() : "0 0 0 0" },
                    RectTransform = { AnchorMin = rectangle.anchorMin, AnchorMax = rectangle.anchorMax },
                    Text = { Text = text.text, Align = text.align, FontSize = text.fontSize, FadeIn = FadeIn, Color = text.color.getColorString() },
                }, parent, name);

                if (CursorEnabled)
                {
                    this.Add(new CuiElement()
                    {
                        Parent = name,
                        Components =
                    {
                        new CuiNeedsCursorComponent()
                    }
                    });
                }

                if (callback != null) this.registerCallback(name, callback);
            }

            public void addInput(string name, Rectangle rectangle, Action<BasePlayer,string> callback, Layer layer, string close = null, GuiColor panelColor = null, int charLimit = 100, GuiText text = null, float FadeIn = 0, float FadeOut = 0, bool isPassword = false, bool CursorEnabled = true, string imgName = null)
            {
                addInput(name, rectangle, callback, layers[(int)layer], close, panelColor, charLimit, text, FadeIn, FadeOut, isPassword, CursorEnabled, imgName);
            }

            public void addInput(string name, Rectangle rectangle, Action<BasePlayer, string> callback, string parent = "Hud", string close = null, GuiColor panelColor = null, int charLimit = 100, GuiText text = null, float FadeIn = 0, float FadeOut = 0, bool isPassword = false, bool CursorEnabled = true, string imgName = null)
            {
                if (string.IsNullOrEmpty(name)) name = "input";
                if (name == this.name) name = name + "_";

                StringBuilder closeString = new StringBuilder("");
                if (close != null)
                {
                    closeString.Append(" close ");
                    closeString.Append(close);
                }

                if (imgName != null || panelColor != null)
                {
                    this.addPanel(name, rectangle, parent, panelColor, FadeIn, FadeOut, null, imgName);

                    this.Add(new CuiInputField()
                    {
                        InputField = { Align = text.align, FontSize = text.fontSize, Color = text.color.getColorString(), Command = $"gui.input text {plugin.Name} {this.name} {name}{closeString.ToString()}", CharsLimit = charLimit, IsPassword = isPassword },
                        RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
                        CursorEnabled = CursorEnabled,
                        FadeOut = FadeOut
                    }, name, name + "_ipt");
                }
                else
                {
                    this.Add(new CuiInputField()
                    {
                        InputField = { Align = text.align, FontSize = text.fontSize, Color = text.color.getColorString(), Command = $"gui.input text {plugin.Name} {this.name} {name}{closeString.ToString()}", CharsLimit = charLimit, IsPassword = isPassword },
                        RectTransform = { AnchorMin = rectangle.anchorMin, AnchorMax = rectangle.anchorMax },
                        CursorEnabled = CursorEnabled,
                        FadeOut = FadeOut
                    }, parent, name);
                }

                if (callback != null) this.registerCallback(name, callback);
            }
        }

        public class GuiTracker : MonoBehaviour
        {
            BasePlayer player;
            List<GuiContainer> activeGuiContainers = new List<GuiContainer>();

            public static GuiTracker getGuiTracker(BasePlayer player)
            {
                GuiTracker output = null;

                player.gameObject.TryGetComponent<GuiTracker>(out output);

                if (output == null)
                {
                    output = player.gameObject.AddComponent<GuiTracker>();
                    output.player = player;
                }

                return output;
            }

            public GuiContainer getContainer(Plugin plugin, string name)
            {
                foreach(GuiContainer container in activeGuiContainers)
                {
                    if (container.plugin != plugin) continue;
                    if (container.name == name) return container;
                }
                return null;
            }

            public void addGuiToTracker(GuiContainer container)
            {
#if DEBUG
                PluginInstance.PrintToChat($"adding {container.name} to tracker");
#endif
                activeGuiContainers.Add(container);
            }

            public void destroyGui(Plugin plugin, GuiContainer container)
            {
                if (activeGuiContainers.Contains(container))
                {
                    destroyGui(plugin, container.name);
                }
                else PluginInstance.Puts($"destroyGui(container.name: {container.name}): no GUI containers found");
            }

            public void destroyGui(Plugin plugin, string name)
            {
                GuiContainer container = getContainer(plugin, name);
                if(container != null)
                {
                    foreach(CuiElement element in container)
                    {
                        destroyGui(plugin, element.Name);
                    }
                    foreach (Timer timer in container.timers)
                    {
                        timer.Destroy();
                    }
                    activeGuiContainers.Remove(container);
                }
                else
                {
                    foreach(GuiContainer cont in activeGuiContainers)
                    {
                        if (cont.plugin != plugin) continue;
                        foreach (CuiElement element in cont)
                        {
                            if (element.Parent == name)
                            {
                                CuiHelper.DestroyUi(player, element.Name);
                            }
                        }
                        CuiHelper.DestroyUi(player, name);
                    }
                    
                }
            }

            public void destroyAllGui()
            {
                foreach (GuiContainer container in activeGuiContainers)
                {
                    foreach (Timer timer in container.timers)
                    {
                        timer.Destroy();
                    }
                    foreach (CuiElement element in container)
                    {
                        CuiHelper.DestroyUi(player, element.Name);
                    }
                }
                Destroy(this);
            }

        }

        #endregion

        #region API

        public enum gametipType {gametip, warning, error}

        public void customGameTip(BasePlayer player, string text, float duration = 0, gametipType type = gametipType.gametip)
        {
            GuiTracker.getGuiTracker(player).destroyGui(this, "gameTip");

            GuiContainer container = new GuiContainer(this, "gameTip");
            switch(type)
            {
                case gametipType.gametip:
                    container.addImage("gametip", new Rectangle(375, 844, 1170, 58, 1920, 1080, true), "bgTex", GuiContainer.Layer.menu, new GuiColor("#25639BF0"), 0.5f, 1);
                    container.addText("gametip_txt", new Rectangle(433, 844, 1112, 58, 1920, 1080, true), GuiContainer.Layer.menu, new GuiText(text, 20, new GuiColor("#FFFFFFD9")), 0.5f, 1);
                    container.addImage("gametip_icon", new Rectangle(375, 844, 58, 58, 1920, 1080, true), "gameTipIcon", GuiContainer.Layer.menu, new GuiColor("#FFFFFFD9"), 0.5f, 1);
                    break;
                case gametipType.warning:
                    container.addPlainPanel("gametip", new Rectangle(375, 844, 1170, 58, 1920, 1080, true), GuiContainer.Layer.menu, new GuiColor("#DED502F0"), 0.5f, 1);
                    container.addText("gametip_txt", new Rectangle(433, 844, 1112, 58, 1920, 1080, true), GuiContainer.Layer.menu, new GuiText(text, 20, new GuiColor("#000000D9")), 0.5f, 1);
                    container.addImage("gametip_icon", new Rectangle(375, 844, 58, 58, 1920, 1080, true), "warning_alpha", GuiContainer.Layer.menu, new GuiColor("#FFFFFFD9"), 0.5f, 1);
                    break;
                case gametipType.error:
                    container.addImage("gametip", new Rectangle(375, 844, 1170, 58, 1920, 1080, true), "bgTex", GuiContainer.Layer.menu, new GuiColor("#BB0000F0"), 0.5f, 1);
                    container.addText("gametip_txt", new Rectangle(433, 844, 1112, 58, 1920, 1080, true), GuiContainer.Layer.menu, new GuiText(text, 20, new GuiColor("#FFFFFFD9")), 0.5f, 1);
                    container.addImage("gametip_icon", new Rectangle(375, 844, 58, 58, 1920, 1080, true), "white_cross", GuiContainer.Layer.menu, new GuiColor("#FFFFFFD9"), 0.5f, 1);
                    break;
            }

            if (duration != 0)
            {
                Timer closeTimer = timer.Once(duration, () =>
                {
                    GuiTracker.getGuiTracker(player).destroyGui(this, container);
                });
                container.timers.Add(closeTimer);
            }
            container.display(player);
        }

        #endregion

        #region helpers

        public void registerImage(string name, string url)
        {
            ImageLibrary.Call("AddImage", url, name);
        }

        #endregion

        #region commands
        private void closeUi(ConsoleSystem.Arg arg)
        {
            GuiTracker.getGuiTracker(arg.Player()).destroyAllGui();
        }

        private void OnGuiInput(ConsoleSystem.Arg arg)
        {
            //           0           1      2             3          4     5           6
            //gui.button button/text plugin containerName inputName close elementName inputText

            BasePlayer player = arg.Player();
            if (player == null) return;
            if (arg.Args.Length < 3) return;
            bool input = false;
            if (arg.Args[0] == "text") input = true;
            GuiTracker tracker = GuiTracker.getGuiTracker(player);
            Plugin plugin = Manager.GetPlugin(arg.Args[1]);
            GuiContainer container = tracker.getContainer(plugin, arg.Args[2]);
            if (container == null)
            {
                Puts($"OnGuiInput: getContainer({arg.Args[2]}) is null!");
#if DEBUG
                PrintToChat($"OnGuiInput: getContainer({arg.Args[2]}) is null!");
#endif
                return;
            }
            if(arg.Args[3] == "close")
            {
                tracker.destroyGui(plugin, container.name);
                return;
            }

            //execute callback if button
            if(!input)
            {
                if (!container.runCallback(arg.Args[3], player)) Puts($"OnGuiInput: {container.name} callback {arg.Args[3]} wasn't found");
                if(arg.Args.Length == 6)
                {
                    if (arg.Args[4] == "close")
                    {
                            tracker.destroyGui(plugin, arg.Args[5]);
                    }
                }
            }
            else
            {
                if(arg.Args.Length == 5)
                {
                    //execute callback if input
                    if (!container.runCallback(arg.Args[3], player, arg.Args[4])) Puts($"OnGuiInput: {container.name} callback {arg.Args[3]} wasn't found");
                }
                else if (arg.Args.Length >= 6)
                {
                    if (arg.Args[4] == "close")
                    {
                        if (arg.Args.Length >= 6)
                        {
                            tracker.destroyGui(plugin, arg.Args[5]);
                        }
                    }
                    //execute callback if input & close
                    if (!container.runCallback(arg.Args[3], player, arg.Args[6])) Puts($"OnGuiInput: {container.name} callback {arg.Args[3]} wasn't found");
                }
            }

            
            
        }

        [ChatCommand("guidemo")]
        private void gcCommand(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, "gui.demo"))
            {
                PrintToChat(player, lang.GetMessage("noPermission", this, player.UserIDString));
                return;
            }
            if(args.Length == 0)
            {
                GuiContainer container = new GuiContainer(this, "demo");
                container.addPanel("demo_panel", new Rectangle(0.25f, 0.5f, 0.25f, 0.25f), GuiContainer.Layer.hud, new GuiColor(0, 1, 0, 0.5f), 1, 1, new GuiText("This is a regular panel", 30));
                container.addPanel("demo_img", new Rectangle(0.25f, 0.25f, 0.25f, 0.25f), FadeIn: 1, FadeOut: 1, text: new GuiText("this is an image with writing on it", 30, color:new GuiColor(1,1,1,1)), imgName: "flower");
                Action<BasePlayer> heal = (arg) => { ((BasePlayer)arg).Heal(10); };
                container.addButton("demo_healButton", new Rectangle(0.5f, 0.5f, 0.25f, 0.25f), GuiContainer.Layer.hud, null, 1, new GuiText("heal me", 40), heal, null, false, "flower");
                Action<BasePlayer> hurt = (arg) => { ((BasePlayer)arg).Hurt(10); };
                container.addButton("demo_hurtButton", new Rectangle(0.5f, 0.25f, 0.25f, 0.25f), new GuiColor(1, 0, 0, 0.5f), 1, new GuiText("hurt me", 40), hurt);
                container.addText("demo_inputLabel", new Rectangle(0.375f, 0.85f, 0.25f, 0.1f), new GuiText("Print to chat:", 50), 1, 1);
                Action<BasePlayer, string> inputCallback = (bPlayer, input) => { PrintToChat(bPlayer, input); };
                container.addInput("demo_input", new Rectangle(0.375f, 0.75f, 0.25f, 0.1f), inputCallback, GuiContainer.Layer.hud, null, new GuiColor("white"), 100, new GuiText("", 50), 1, 1);

                container.addPanel("layer1", new Rectangle(1400, 800, 300, 100, 1920, 1080, true), GuiContainer.Layer.overall, new GuiColor("red"), 1, 1, new GuiText("overall", align: TextAnchor.LowerLeft));
                container.addPanel("layers_label", new Rectangle(0,0,1,1), "layer1", null, 1, 1, new GuiText("Available layers:", 20, align:TextAnchor.UpperLeft));
                container.addPanel("layer2", new Rectangle(1425, 825, 300, 100, 1920, 1080, true), GuiContainer.Layer.overlay, new GuiColor("yellow"), 1, 1, new GuiText("overlay", align: TextAnchor.LowerLeft));
                container.addPanel("layer3", new Rectangle(1450, 850, 300, 100, 1920, 1080, true), GuiContainer.Layer.menu, new GuiColor("green"), 1, 1, new GuiText("menu", align: TextAnchor.LowerLeft));
                container.addPanel("layer4", new Rectangle(1475, 875, 300, 100, 1920, 1080, true), GuiContainer.Layer.hud, new GuiColor("blue"), 1, 1, new GuiText("hud", align: TextAnchor.LowerLeft));
                container.addPanel("layer5", new Rectangle(1500, 900, 300, 100, 1920, 1080, true), GuiContainer.Layer.under, new GuiColor("purple"), 1, 1, new GuiText("under", align: TextAnchor.LowerLeft));

                container.addButton("demo_close", new Rectangle(0.1f, 0.1f, 0.1f, 0.1f), new GuiColor("red"), 1, new GuiText("close", 50), null, "demo");

                container.display(player);

                customGameTip(player, "This is a custom gametip!", 5);
                return;
            }
            else if(args.Length >= 3)
            {
                if(args[0] == "gametip") customGameTip(player, args[2], 3f, (gametipType)Enum.Parse(typeof(gametipType), args[1]));
            }
        }
        #endregion

        #region Config
        private static ConfigData config;

        private class ConfigData
        {
            public string gameTipIcon;
        }

        private ConfigData GetDefaultConfig()
        {
            return new ConfigData
            {
                gameTipIcon = "https://i.imgur.com/VFBB8ib.png"
            };
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();

            try
            {
                config = Config.ReadObject<ConfigData>();
            }
            catch
            {
                LoadDefaultConfig();
            }

            SaveConfig();
        }

        protected override void LoadDefaultConfig()
        {
            PrintError("Configuration file is corrupt(or not exists), creating new one!");
            config = GetDefaultConfig();
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(config);
        }
        #endregion

        #region Localization
        Dictionary<string, string> messages = new Dictionary<string, string>()
    {
        {"noPermission", "You don't have permission to use this command!"}
    };
        #endregion
    }
}