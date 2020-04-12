//#define DEBUG

/*
 TODO:
 -change input over to proper callbacks
 -add parenting
 -make more prefabs
 
 */

using Facepunch.Extend;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("GUICreator", "OHM", "1.1.0")]
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

            cmd.AddConsoleCommand("gui.close", this, nameof(closeUi));
            cmd.AddConsoleCommand("gui.button", this, nameof(OnButtonClick));
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
            registerImage("warning_mask", "https://i.imgur.com/bIEQ3IV.png");
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

        public class GuiElement:CuiElement
        {
            private GuiElement ParentInternal = null;
            private List<GuiElement> Children = new List<GuiElement>();

            public void setParent(GuiElement parent)
            {
                this.ParentInternal = parent;
                parent.Children.Add(this);
            }

            public CuiElementContainer getChildren()
            {
                if (Children.Count == 0) return null;
                CuiElementContainer output = new CuiElementContainer();
                output.AddRange(Children);
                return output;
            }
        }

        public class GuiContainer : CuiElementContainer
        {
            public string name;
            public List<Timer> timers = new List<Timer>();
            private Dictionary<string, Action<BasePlayer>> callbacks = new Dictionary<string, Action<BasePlayer>>();

            public GuiContainer(string name)
            {
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
                if (callbacks.ContainsKey(name)) callbacks.Remove(name);
                callbacks.Add(name, callback);
            }

            public bool runCallback(string name, BasePlayer player)
            {
                if (!callbacks.ContainsKey(name)) return false;
                try
                {
                    callbacks[name](player);
                    return true;
                }
                catch(Exception E)
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

            public void addPanel(string name, Rectangle rectangle, Layer layer = Layer.hud, GuiColor panelColor = null, float FadeIn = 0, float FadeOut = 0, GuiText text = null, string imgName = null)
            {
                if (string.IsNullOrEmpty(name))
                {
                    name = "panel";
                }
                if (string.IsNullOrEmpty(imgName))
                {
                    this.addPlainPanel(name, rectangle, layer, panelColor, FadeIn, FadeOut);
                }
                else
                {
                    this.addImage(name, rectangle, imgName, layer, panelColor, FadeIn, FadeOut);
                }
                if (text != null) this.addText(name + "_txt", rectangle, layer, text, FadeIn, FadeOut);
            }

            public void addPlainPanel(string name, Rectangle rectangle, Layer layer = Layer.hud, GuiColor panelColor = null, float FadeIn = 0, float FadeOut = 0)
            {
                if (string.IsNullOrEmpty(name)) name = "plainPanel";

                this.Add(new CuiElement
                {
                    Parent = layers[(int)layer],
                    Name = name,
                    Components =
                {
                    new CuiImageComponent { Color = (panelColor != null)?panelColor.getColorString():"0 0 0 0", FadeIn = FadeIn},
                    new CuiRectTransformComponent { AnchorMin = rectangle.anchorMin, AnchorMax = rectangle.anchorMax }
                },
                    FadeOut = FadeOut
                });
            }

            public void addImage(string name, Rectangle rectangle, string imgName, Layer layer = Layer.hud, GuiColor panelColor = null, float FadeIn = 0, float FadeOut = 0)
            {
                if (string.IsNullOrEmpty(name)) name = "image";

                this.Add(new CuiElement
                {
                    Parent = layers[(int)layer],
                    Name = name,
                    Components =
                {
                    new CuiRawImageComponent { Color = (panelColor != null)?panelColor.getColorString():"1 1 1 1", FadeIn = FadeIn, Png = (string)PluginInstance.ImageLibrary.Call("GetImage", imgName)},
                    new CuiRectTransformComponent { AnchorMin = rectangle.anchorMin, AnchorMax = rectangle.anchorMax }
                },
                    FadeOut = FadeOut
                });
            }

            public void addText(string name, Rectangle rectangle, Layer layer = Layer.hud, GuiText text = null, float FadeIn = 0, float FadeOut = 0)
            {
                if (string.IsNullOrEmpty(name)) name = "text";

                this.Add(new CuiElement
                {
                    Parent = layers[(int)layer],
                    Name = name,
                    Components =
                {
                    new CuiTextComponent { FadeIn = FadeIn, Text = text.text, FontSize = text.fontSize, Align = text.align, Color = text.color.getColorString()},
                    new CuiRectTransformComponent { AnchorMin = rectangle.anchorMin, AnchorMax = rectangle.anchorMax }
                },
                    FadeOut = FadeOut
                });
            }

            public void addButton(string name, Rectangle rectangle, GuiColor panelColor = null, float FadeIn = 0, GuiText text = null, Action<BasePlayer> callback = null, bool close = false, bool CursorEnabled = true, string imgName = null, Layer layer = Layer.hud)
            {
                if (string.IsNullOrEmpty(name)) name = "button";
                if (layer == Layer.under) layer = Layer.hud;

                this.addPlainButton(name, rectangle, panelColor, FadeIn, text, callback, close, CursorEnabled, layer);

                if (imgName != null) this.addImage(name + "_img", rectangle, imgName, layer + 1, null, FadeIn);
            }

            public void addPlainButton(string name, Rectangle rectangle, GuiColor panelColor = null, float FadeIn = 0, GuiText text = null, Action<BasePlayer> callback = null, bool close = false, bool CursorEnabled = true, Layer layer = Layer.hud)
            {
                if (string.IsNullOrEmpty(name)) name = "plainButton";

                this.Add(new CuiButton()
                {
                    Button = { Command = $"gui.button {this.name} {name}{(close?" close":"")}", FadeIn = FadeIn, Color = (panelColor != null) ? panelColor.getColorString() : "0 0 0 0" },
                    RectTransform = { AnchorMin = rectangle.anchorMin, AnchorMax = rectangle.anchorMax },
                    Text = { Text = text.text, Align = text.align, FontSize = text.fontSize, FadeIn = FadeIn, Color = text.color.getColorString() },
                }, layers[(int)layer], name);

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

            public void addInput(string name, Rectangle rectangle, string command, Layer layer = Layer.hud, GuiColor panelColor = null, int charLimit = 100, GuiText text = null, float FadeIn = 0, float FadeOut = 0, bool isPassword = false, bool CursorEnabled = true, string imgName = null)
            {
                if (string.IsNullOrEmpty(name)) name = "input";
                if (layer == Layer.under) layer = Layer.hud;

                this.Add(new CuiInputField()
                {
                    InputField = { Align = text.align, FontSize = text.fontSize, Color = text.color.getColorString(), Command = command, CharsLimit = charLimit, IsPassword = isPassword },
                    RectTransform = { AnchorMin = rectangle.anchorMin, AnchorMax = rectangle.anchorMax },
                    CursorEnabled = CursorEnabled,
                    FadeOut = FadeOut
                }, layers[(int)layer], name);

                if (imgName != null) this.addImage(name + "_img", rectangle, imgName, layer - 1, panelColor, FadeIn, FadeOut);
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

            public GuiContainer getContainer(string name)
            {
                foreach(GuiContainer container in activeGuiContainers)
                {
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

            public void destroyGui(GuiContainer container)
            {
                if (activeGuiContainers.Contains(container))
                {
                    foreach (CuiElement element in container)
                    {
                        CuiHelper.DestroyUi(player, element.Name);
                    }
#if DEBUG
                    PluginInstance.PrintToChat($"cloing container.name: {container.name}");
#endif
                    foreach (Timer timer in container.timers)
                    {
                        timer.Destroy();
#if DEBUG
                        if (timer.Destroyed)
                        {
                            PluginInstance.PrintToChat($"{container.name} timer destroyed!");
                        }
#endif
                    }

                    activeGuiContainers.Remove(container);
                    //if (activeGuiElements.Count == 0) Destroy(this);
                }
                else PluginInstance.Puts($"destroyGui(container.name: {container.name}): no GUI containers found");
            }

            public void destroyGui(string name)
            {
                GuiContainer container = null;
                foreach(GuiContainer cont in activeGuiContainers)
                {
                    if (cont.name == name) container = cont;
                }
                if(container == null)
                {
                    PluginInstance.Puts($"destroyGui({name}): no GUI containers found");
                    return;
                }
                destroyGui(container);
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

        public enum gametipType {gametip, warning1, warning2, error}

        public void customGameTip(BasePlayer player, string text, float duration = 0, gametipType type = gametipType.gametip)
        {
            GuiTracker.getGuiTracker(player).destroyGui("gameTip");

            GuiContainer container = new GuiContainer("gameTip");
            switch(type)
            {
                case gametipType.gametip:
                    container.addImage("gametip", new Rectangle(375, 844, 1170, 58, 1920, 1080, true), "bgTex", GuiContainer.Layer.hud, new GuiColor("#25639BF0"), 0.5f, 1);
                    container.addText("gametip_txt", new Rectangle(433, 844, 1112, 58, 1920, 1080, true), GuiContainer.Layer.hud, new GuiText(text, 20, new GuiColor("#FFFFFFD9")), 0.5f, 1);
                    container.addImage("gametip_icon", new Rectangle(375, 844, 58, 58, 1920, 1080, true), "gameTipIcon", GuiContainer.Layer.hud, new GuiColor("#FFFFFFD9"), 0.5f, 1);
                    break;
                case gametipType.warning1:
                    container.addPlainPanel("gametip", new Rectangle(375, 844, 1170, 58, 1920, 1080, true), GuiContainer.Layer.hud, new GuiColor("#DED502F0"), 0.5f, 1);
                    container.addText("gametip_txt", new Rectangle(433, 844, 1112, 58, 1920, 1080, true), GuiContainer.Layer.hud, new GuiText(text, 20, new GuiColor("#000000D9")), 0.5f, 1);
                    container.addImage("gametip_icon", new Rectangle(375, 844, 58, 58, 1920, 1080, true), "warning_alpha", GuiContainer.Layer.hud, new GuiColor("#FFFFFFD9"), 0.5f, 1);
                    break;
                case gametipType.warning2:
                    container.addPlainPanel("gametip", new Rectangle(375, 844, 1170, 58, 1920, 1080, true), GuiContainer.Layer.under, new GuiColor("#DED502F0"), 0.5f, 1);
                    container.addImage("gametip_mask", new Rectangle(375, 844, 1170, 58, 1920, 1080, true), "warning_mask", GuiContainer.Layer.hud, new GuiColor("#FFFFFFFF"), 0.5f, 1);
                    container.addText("gametip_txt", new Rectangle(375, 844, 1170, 58, 1920, 1080, true), GuiContainer.Layer.hud, new GuiText(text, 20, new GuiColor("#FFFFFFD9")), 0.5f, 1);
                    break;
                case gametipType.error:
                    container.addImage("gametip", new Rectangle(375, 844, 1170, 58, 1920, 1080, true), "bgTex", GuiContainer.Layer.hud, new GuiColor("#BB0000F0"), 0.5f, 1);
                    container.addText("gametip_txt", new Rectangle(433, 844, 1112, 58, 1920, 1080, true), GuiContainer.Layer.hud, new GuiText(text, 20, new GuiColor("#FFFFFFD9")), 0.5f, 1);
                    container.addImage("gametip_icon", new Rectangle(375, 844, 58, 58, 1920, 1080, true), "white_cross", GuiContainer.Layer.hud, new GuiColor("#FFFFFFD9"), 0.5f, 1);
                    break;
            }

            if (duration != 0)
            {
                Timer closeTimer = timer.Once(duration, () =>
                {
                    GuiTracker.getGuiTracker(player).destroyGui(container);
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
            if (arg.Player() == null || arg.Args.Length < 1)
            {
                GuiTracker.getGuiTracker(arg.Player()).destroyAllGui();
            }
            GuiTracker.getGuiTracker(arg.Player()).destroyGui(arg.Args[0]);
        }

        private void OnButtonClick(ConsoleSystem.Arg arg)
        {
            BasePlayer player = arg.Player();
            if (player == null) return;
            if (arg.Args.Length < 2) return;
            GuiTracker tracker = GuiTracker.getGuiTracker(player);
            GuiContainer container = tracker.getContainer(arg.Args[0]);
            if (container == null)
            {
#if DEBUG
                PrintToChat($"OnButtonClick: getContainer({arg.Args[0]}) is null!");
#endif
                return;
            }
            if(arg.Args[1] == "close")
            {
                tracker.destroyGui(container.name);
                return;
            }
            if (!container.runCallback(arg.Args[1], player)) Puts($"OnButtonClick: {container.name} callback {arg.Args[1]} wasn't found");

            if (arg.Args.Length != 3) return;
            if (arg.Args[2] == "close") tracker.destroyGui(container.name);
        }

        private void OnGuiInput(ConsoleSystem.Arg arg)
        {
            PrintToChat("not implemented yet!");
        }

        [ChatCommand("gc")]
        private void gcCommand(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, "gui.demo"))
            {
                PrintToChat(player, lang.GetMessage("noPermission", this, player.UserIDString));
                return;
            }
            if(args.Length == 0)
            {
                GuiContainer container = new GuiContainer("demo");
                container.addPanel("demo_panel", new Rectangle(0.25f, 0.5f, 0.25f, 0.25f), GuiContainer.Layer.hud, new GuiColor(0, 1, 0, 0.5f), 1, 1, new GuiText("This is a regular panel", 30));
                container.addPanel("demo_img", new Rectangle(0.25f, 0.25f, 0.25f, 0.25f), FadeIn: 1, FadeOut: 1, text: new GuiText("this is an image with writing on it", 30, color:new GuiColor(1,1,1,1)), imgName: "flower");
                Action<BasePlayer> heal = (arg) => { ((BasePlayer)arg).Heal(10); };
                container.addButton("demo_healButton", new Rectangle(0.5f, 0.5f, 0.25f, 0.25f), null, 1, new GuiText("heal me", 40), heal, false, false, "flower");
                Action<BasePlayer> hurt = (arg) => { ((BasePlayer)arg).Hurt(10); };
                container.addButton("demo_hurtButton", new Rectangle(0.5f, 0.25f, 0.25f, 0.25f), new GuiColor(1, 0, 0, 0.5f), 1, new GuiText("hurt me", 40), hurt);
                container.addButton("close", new Rectangle(0.1f, 0.1f, 0.1f, 0.1f), new GuiColor("red"), 1, new GuiText("close", 50));

                container.addPanel("layer1", new Rectangle(1400, 800, 300, 100, 1920, 1080, true), GuiContainer.Layer.overall, new GuiColor("red"), 1, 1, new GuiText("overall", align: TextAnchor.LowerLeft));
                container.addPanel("layer2", new Rectangle(1425, 825, 300, 100, 1920, 1080, true), GuiContainer.Layer.overlay, new GuiColor("yellow"), 1, 1, new GuiText("overlay", align: TextAnchor.LowerLeft));
                container.addPanel("layer3", new Rectangle(1450, 850, 300, 100, 1920, 1080, true), GuiContainer.Layer.menu, new GuiColor("green"), 1, 1, new GuiText("menu", align: TextAnchor.LowerLeft));
                container.addPanel("layer4", new Rectangle(1475, 875, 300, 100, 1920, 1080, true), GuiContainer.Layer.hud, new GuiColor("blue"), 1, 1, new GuiText("hud", align: TextAnchor.LowerLeft));
                container.addPanel("layer5", new Rectangle(1500, 900, 300, 100, 1920, 1080, true), GuiContainer.Layer.under, new GuiColor("purple"), 1, 1, new GuiText("under", align: TextAnchor.LowerLeft));
                container.addPanel("layers_label", new Rectangle(1400, 775, 300, 100, 1920, 1080, true), GUICreator.GuiContainer.Layer.overall, null, 1, 1, new GuiText("Available layers:", 20));

                container.display(player);

                customGameTip(player, "This is a custom gametip!", 5);
                return;
            }
            try
            {
                switch (args[0])
                {
                    case "panel":
                        GuiContainer container = new GuiContainer("panelTest");
                        container.addPlainPanel("panelTest", new Rectangle(0.25f, 0.25f, 0.5f, 0.5f), GuiContainer.Layer.hud, new GuiColor(args[1]), 1f, 1f);
                        container.addPlainButton("close", new Rectangle(0.25f, 0.25f, 0.15f, 0.05f), new GuiColor("red"), 1f,new GuiText("close", 10));
                        container.display(player);
                        break;
                    case "input":
                        GuiContainer container5 = new GuiContainer("inputTest");
                        container5.addInput("inputTest", new Rectangle(0.25f, 0.25f, 0.5f, 0.5f), "say input:", GuiContainer.Layer.hud, new GuiColor(0, 255, 0, 1),100, new GuiText("", 40), CursorEnabled: true);
                        container5.addPlainButton("close", new Rectangle(0.25f, 0.25f, 0.15f, 0.05f), new GuiColor("red"), 1f, new GuiText("close", 10));
                        container5.display(player);
                        break;

                    case "raw":
                        CuiElementContainer rawContainer = new CuiElementContainer();
                        rawContainer.Add(new CuiPanel()
                        {
                            RectTransform = { AnchorMin = "0.25 0.25", AnchorMax = "0.75 0.75" },
                            Image = { Color = "0 0 0 0.5", Sprite = "assets/content/materials/highlight.png", Material = "assets/content/ui/uibackgroundblur-ingamemenu.mat" },
                            FadeOut = 2
                        }, new CuiElement().Parent, "rawTest_panel");
                        rawContainer.Add(new CuiLabel()
                        {
                            RectTransform = { AnchorMin = "0.25 0.25", AnchorMax = "0.75 0.75" },
                            Text = { Text = "test", Align = TextAnchor.MiddleCenter, FontSize = 50 },
                            FadeOut = 2
                        }, new CuiElement().Parent, "rawTest_text");
                        CuiHelper.AddUi(player, rawContainer);

                        timer.Once(2f, () =>
                        {
                            CuiHelper.DestroyUi(player, "rawTest_panel");
                            CuiHelper.DestroyUi(player, "rawTest_text");
                        });
                        break;

                    case "gametip":
                        customGameTip(player, args[2], 3f, (gametipType)Enum.Parse(typeof(gametipType), args[1]));
                        break;
                }
            }
            catch (Exception E) { }

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