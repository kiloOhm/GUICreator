using ConVar;
using Facepunch.Extend;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WebSocketSharp;

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
            permission.RegisterPermission("GC.use", this);

            cmd.AddConsoleCommand("gc.closeui", this, nameof(closeGuiConsole));
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
            public enum Layer { overall = 0, overlay, menu, hud, under };
            //Rust UI elements (inventory, Health, etc.) are between the hud and the menu layer
            private List<string> layers = new List<string> {"Overall", "Overlay", "Hud.Menu", "Hud", "Under"};

            public void display(BasePlayer player)
            {
                if (this.Count == 0) return;
                GuiTracker.getGuiTracker(player).addGuiToTracker(this.First().Name, this);
                CuiHelper.AddUi(player, this);
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

            public void addPanel(string name, Rectangle rectangle, Layer layer = Layer.hud, GuiColor panelColor = null, float FadeIn = 0, float FadeOut = 0, string text = null, int fontSize = 14, TextAnchor align = TextAnchor.MiddleCenter, GuiColor textColor = null, string imgName = null)
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
                if (!text.IsNullOrEmpty()) this.addText(name + "_txt", rectangle, text, layer, textColor, fontSize, align, FadeIn, FadeOut);
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

            public void addText(string name, Rectangle rectangle, string text, Layer layer = Layer.hud, GuiColor textColor = null, int fontSize = 14, TextAnchor align = TextAnchor.MiddleCenter, float FadeIn = 0, float FadeOut = 0)
            {
                if (string.IsNullOrEmpty(name)) name = "text";

                this.Add(new CuiElement
                {
                    Parent = layers[(int)layer],
                    Name = name,
                    Components =
                {
                    new CuiTextComponent { FadeIn = FadeIn, Text = text, FontSize = fontSize, Align = align, Color = (textColor != null)?textColor.getColorString():"0 0 0 1" },
                    new CuiRectTransformComponent { AnchorMin = rectangle.anchorMin, AnchorMax = rectangle.anchorMax }
                },
                    FadeOut = FadeOut
                });
            }

            public void addButton(string name, Rectangle rectangle, string command = null, Layer layer = Layer.hud, GuiColor panelColor = null, float FadeIn = 0, string text = null, int fontSize = 14, TextAnchor align = TextAnchor.MiddleCenter, GuiColor textColor = null, bool CursorEnabled = false, string imgName = null)
            {
                if (string.IsNullOrEmpty(name)) name = "button";
                if (layer == Layer.under) layer = Layer.hud;

                this.addPlainButton(name, rectangle, layer, command, panelColor, FadeIn, text, fontSize, align, textColor, CursorEnabled);

                if (imgName != null) this.addImage(name + "_img", rectangle, imgName, layer + 1, null, FadeIn);
            }

            public void addPlainButton(string name, Rectangle rectangle, Layer layer = Layer.hud, string command = null, GuiColor panelColor = null, float FadeIn = 0, string text = null, int fontSize = 14, TextAnchor align = TextAnchor.MiddleCenter, GuiColor textColor = null, bool CursorEnabled = false)
            {
                if (string.IsNullOrEmpty(name)) name = "plainButton";

                this.Add(new CuiButton()
                {
                    Button = { Command = command, FadeIn = FadeIn, Color = (panelColor != null) ? panelColor.getColorString() : "0 0 0 0" },
                    RectTransform = { AnchorMin = rectangle.anchorMin, AnchorMax = rectangle.anchorMax },
                    Text = { Text = text, Align = align, FontSize = fontSize, FadeIn = FadeIn, Color = (textColor != null) ? textColor.getColorString() : "0 0 0 1" },
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
            }

            public void addInput(string name, Rectangle rectangle, string command, Layer layer = Layer.hud, GuiColor panelColor = null, int charLimit = 100, int fontSize = 14, TextAnchor align = TextAnchor.MiddleCenter, GuiColor textColor = null, float FadeIn = 0, float FadeOut = 0, bool isPassword = false, bool CursorEnabled = false, string imgName = null)
            {
                if (string.IsNullOrEmpty(name)) name = "input";
                if (layer == Layer.under) layer = Layer.hud;

                this.Add(new CuiInputField()
                {
                    InputField = { Align = align, FontSize = fontSize, Color = (textColor != null) ? textColor.getColorString() : "1 1 1 1", Command = command, CharsLimit = charLimit, IsPassword = isPassword },
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
            Dictionary<string, List<string>> activeGuiElements = new Dictionary<string, List<string>>();

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

            public void addGuiToTracker(string name, GuiContainer container)
            {
                if (activeGuiElements.ContainsKey(name)) destroyGui(name);

                List<string> names = new List<string>();
                foreach (CuiElement element in container)
                {
                    names.Add(element.Name);
                }

                activeGuiElements.Add(name, names);
            }

            public void destroyGui(string name)
            {
                if (activeGuiElements.ContainsKey(name))
                {
                    foreach (string element in activeGuiElements[name])
                    {
                        CuiHelper.DestroyUi(player, element);
                    }
                    activeGuiElements.Remove(name);
                    if (activeGuiElements.Count == 0) Destroy(this);
                }
                else PluginInstance.Puts($"destroyGui({name}): no GUI containers found");
            }

            public void destroyAllGui()
            {
                foreach (List<string> names in activeGuiElements.Values)
                {
                    foreach (string name in names)
                    {
                        CuiHelper.DestroyUi(player, name);
                    }
                }
                Destroy(this);
            }

        }

        #endregion

        #region API

        public void customGameTip(BasePlayer player, string text, float duration = 0)
        {
            GuiTracker.getGuiTracker(player).destroyGui("gameTip");

            GuiContainer container = new GuiContainer();
            container.addImage("gametip", new Rectangle(375, 844, 1170, 58, 1920, 1080, true), "bgTex", GuiContainer.Layer.hud, new GuiColor("#25639BF0"), 0.5f, 1);
            container.addText("gametip_txt", new Rectangle(433, 844, 1112, 58, 1920, 1080, true), text, GuiContainer.Layer.hud, new GuiColor("#FFFFFFD9"), 20, FadeIn: 0.5f, FadeOut: 1);
            container.addImage("gametip_icon", new Rectangle(375, 844, 58, 58, 1920, 1080, true), "gameTipIcon", GuiContainer.Layer.hud, new GuiColor("#FFFFFFD9"), 0.5f, 1);
            GuiTracker.getGuiTracker(player).addGuiToTracker("gameTip", container);
            CuiHelper.AddUi(player, container);

            if (duration != 0)
            {
                timer.Once(duration, () =>
                {
                    GuiTracker.getGuiTracker(player).destroyGui("gameTip");
                });
            }
        }

        public void warningGameTip1(BasePlayer player, string text, float duration = 0)
        {
            GuiTracker.getGuiTracker(player).destroyGui("gameTip");

            GuiContainer container = new GuiContainer();
            container.addPlainPanel("gametip", new Rectangle(375, 844, 1170, 58, 1920, 1080, true), GuiContainer.Layer.hud, new GuiColor("#DED502F0"), 0.5f, 1);
            container.addText("gametip_txt", new Rectangle(433, 844, 1112, 58, 1920, 1080, true), text, GuiContainer.Layer.hud, new GuiColor("#000000D9"), 20, FadeIn: 0.5f, FadeOut: 1);
            container.addImage("gametip_icon", new Rectangle(375, 844, 58, 58, 1920, 1080, true), "warning_alpha", GuiContainer.Layer.hud, new GuiColor("#FFFFFFD9"), 0.5f, 1);
            GuiTracker.getGuiTracker(player).addGuiToTracker("gameTip", container);
            CuiHelper.AddUi(player, container);

            if (duration != 0)
            {
                timer.Once(duration, () =>
                {
                    GuiTracker.getGuiTracker(player).destroyGui("gameTip");
                });
            }
        }

        public void warningGameTip2(BasePlayer player, string text, float duration = 0)
        {
            GuiTracker.getGuiTracker(player).destroyGui("gameTip");

            GuiContainer container = new GuiContainer();
            container.addPlainPanel("gametip", new Rectangle(375, 844, 1170, 58, 1920, 1080, true), GuiContainer.Layer.hud, new GuiColor("#DED502F0"), 0.5f, 1);
            container.addImage("gametip_mask", new Rectangle(375, 844, 1170, 58, 1920, 1080, true), "warning_mask", GuiContainer.Layer.hud, new GuiColor("#FFFFFFFF"), 0.5f, 1);
            container.addText("gametip_txt", new Rectangle(375, 844, 1170, 58, 1920, 1080, true), text, GuiContainer.Layer.hud, new GuiColor("#FFFFFFD9"), 20, FadeIn: 0.5f, FadeOut: 1);
            GuiTracker.getGuiTracker(player).addGuiToTracker("gameTip", container);
            CuiHelper.AddUi(player, container);

            if (duration != 0)
            {
                timer.Once(duration, () =>
                {
                    GuiTracker.getGuiTracker(player).destroyGui("gameTip");
                });
            }
        }

        public void errorGameTip(BasePlayer player, string text, float duration = 0)
        {
            GuiTracker.getGuiTracker(player).destroyGui("gameTip");

            GuiContainer container = new GuiContainer();
            container.addImage("gametip", new Rectangle(375, 844, 1170, 58, 1920, 1080, true), "bgTex", GuiContainer.Layer.hud, new GuiColor("#BB0000F0"), 0.5f, 1);
            container.addText("gametip_txt", new Rectangle(433, 844, 1112, 58, 1920, 1080, true), text, GuiContainer.Layer.hud, new GuiColor("#FFFFFFD9"), 20, FadeIn: 0.5f, FadeOut: 1);
            container.addImage("gametip_icon", new Rectangle(375, 844, 58, 58, 1920, 1080, true), "white_cross", GuiContainer.Layer.hud, new GuiColor("#FFFFFFD9"), 0.5f, 1);
            GuiTracker.getGuiTracker(player).addGuiToTracker("gameTip", container);
            CuiHelper.AddUi(player, container);

            if (duration != 0)
            {
                timer.Once(duration, () =>
                {
                    GuiTracker.getGuiTracker(player).destroyGui("gameTip");
                });
            }
        }

        #endregion

        #region helpers

        public void registerImage(string name, string url)
        {
            ImageLibrary.Call("AddImage", url, name);
        }

        #endregion

        #region commands
        private void closeGuiConsole(ConsoleSystem.Arg arg)
        {
            if (arg.Player() == null || arg.Args.Length < 1)
            {
                GuiTracker.getGuiTracker(arg.Player()).destroyAllGui();
            }
            GuiTracker.getGuiTracker(arg.Player()).destroyGui(arg.Args[0]);
        }

        [ChatCommand("gc")]
        private void gcCommand(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, "GUICreator.use"))
            {
                PrintToChat(player, lang.GetMessage("noPermission", this, player.UserIDString));
                return;
            }
            try
            {
                switch (args[0])
                {
                    case "panel":
                        GuiContainer container = new GuiContainer();
                        container.addPlainPanel("panelTest", new Rectangle(0.25f, 0.25f, 0.5f, 0.5f), GuiContainer.Layer.hud, new GuiColor(args[1]), 1f, 1f);
                        container.addPlainButton("panelTest_button", new Rectangle(0.25f, 0.25f, 0.15f, 0.05f), GuiContainer.Layer.hud, "gc.closeui panelTest", panelColor: new GuiColor("red"), FadeIn: 1f, text: "close", fontSize: 10, CursorEnabled: true);
                        container.display(player);
                        break;

                    case "text":
                        GuiContainer container1 = new GuiContainer();
                        container1.addPanel("textTest", new Rectangle(0.25f, 0.25f, 0.5f, 0.5f), FadeIn: 1f, FadeOut: 2f, text: "test", fontSize: 40, imgName: null);
                        container1.addPlainButton("textTest_button", new Rectangle(0.25f, 0.25f, 0.15f, 0.05f), GuiContainer.Layer.hud, "gc.closeui textTest", panelColor: new GuiColor(255, 0, 0, 1), FadeIn: 1f, text: "close", fontSize: 10, CursorEnabled: true);
                        container1.display(player);
                        break;

                    case "img":
                        GuiContainer container2 = new GuiContainer();
                        container2.addPanel("imgTest", new Rectangle(0.25f, 0.25f, 0.5f, 0.5f), FadeIn: 1f, FadeOut: 1f, imgName: "flower");
                        container2.addPlainButton("imgTest_button", new Rectangle(0.25f, 0.25f, 0.15f, 0.05f), GuiContainer.Layer.hud, "gc.closeui imgTest", panelColor: new GuiColor(255, 0, 0, 1), FadeIn: 1f, text: "close", fontSize: 10, CursorEnabled: true);
                        container2.display(player);
                        break;

                    case "button":
                        GuiContainer container3 = new GuiContainer();
                        container3.addPlainButton("buttonTest", new Rectangle(0.25f, 0.25f, 0.5f, 0.5f), GuiContainer.Layer.hud, "gc.closeui buttonTest", new GuiColor(255, 0, 0, 0.8f), 1f, text: "close", fontSize: 40, CursorEnabled: true);
                        container3.display(player);
                        break;

                    case "buttonImg":
                        GuiContainer container4 = new GuiContainer();
                        container4.addButton("buttonImgTest", new Rectangle(0.25f, 0.25f, 0.5f, 0.5f), "gc.closeui buttonImgTest",GuiContainer.Layer.menu, FadeIn: 1f, text: "close", fontSize: 40, CursorEnabled: true, imgName: "flower");
                        GuiTracker.getGuiTracker(player).addGuiToTracker("buttonImgTest", container4);
                        CuiHelper.AddUi(player, container4);
                        break;

                    case "input":
                        GuiContainer container5 = new GuiContainer();
                        container5.addInput("inputTest", new Rectangle(0.25f, 0.25f, 0.5f, 0.5f), "say input:", GuiContainer.Layer.hud, new GuiColor(0, 255, 0, 1), fontSize: 30, CursorEnabled: true);
                        container5.addPlainButton("inputTest_button", new Rectangle(0.25f, 0.25f, 0.15f, 0.05f), GuiContainer.Layer.hud, "gc.closeui inputTest", panelColor: new GuiColor(255, 0, 0, 1), FadeIn: 1f, text: "close", fontSize: 10);
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
                        customGameTip(player, args[1], 3f);
                        break;
                    case "warning1":
                        warningGameTip1(player, args[1], 3f);
                        break;
                    case "warning2":
                        warningGameTip2(player, args[1], 3f);
                        break;
                    case "error":
                        errorGameTip(player, args[1], 3f);
                        break;
                    case "layers":
                        GuiContainer container6 = new GuiContainer();
                        container6.addPanel("layer1", new Rectangle(1400, 800, 300, 100, 1920, 1080, true), GuiContainer.Layer.overall, new GuiColor("red"), text:"overall", align:TextAnchor.LowerLeft);
                        container6.addPanel("layer2", new Rectangle(1425, 825, 300, 100, 1920, 1080, true), GuiContainer.Layer.overlay, new GuiColor("yellow"), text: "overlay", align: TextAnchor.LowerLeft);
                        container6.addPanel("layer3", new Rectangle(1450, 850, 300, 100, 1920, 1080, true), GuiContainer.Layer.menu, new GuiColor("green"), text: "menu", align: TextAnchor.LowerLeft);
                        container6.addPanel("layer4", new Rectangle(1475, 875, 300, 100, 1920, 1080, true), GuiContainer.Layer.hud, new GuiColor("blue"), text: "hud", align: TextAnchor.LowerLeft);
                        container6.addPanel("layer5", new Rectangle(1500, 900, 300, 100, 1920, 1080, true), GuiContainer.Layer.under, new GuiColor("purple"), text: "under", align: TextAnchor.LowerLeft);
                        container6.addPlainButton("layerTest_button", new Rectangle(0.25f, 0.25f, 0.15f, 0.05f), GuiContainer.Layer.hud, "gc.closeui layer1", panelColor: new GuiColor(255, 0, 0, 1), FadeIn: 1f, text: "close", fontSize: 10, CursorEnabled: true);
                        container6.display(player);
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
        {"posOutput", "Player Coordinates: X:{0}, Y:{1}, Z:{2}"},
        {"noPermission", "You don't have permission to use this command!"}
    };
        #endregion
    }
}