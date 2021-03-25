//#define DEBUG
//#define CoroutineDEBUG

using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("GUICreator", "kOhm", "1.5.0")]
    [Description("API Plugin for centralized GUI creation and management")]
    public partial class GUICreator : RustPlugin
    {
        [PluginReference]
        private Plugin ImageLibrary;

        private static GUICreator PluginInstance = null;

        private static DownloadManager _DownloadManager;

        public GUICreator()
        {
            PluginInstance = this;
        }

        public const int offsetResX = 1280;
        public const int offsetResY = 720;
    }
}﻿namespace Oxide.Plugins
{
    using Oxide.Core.Plugins;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public partial class GUICreator
    {
        public enum gametipType { gametip, warning, error }

        public void customGameTip(BasePlayer player, string text, float duration = 0, gametipType type = gametipType.gametip)
        {
            GuiTracker.getGuiTracker(player).destroyGui(this, "gameTip");

            GuiContainer container = new GuiContainer(this, "gameTip");
            switch (type)
            {
                case gametipType.gametip:
                    //container.addImage("gametip", new Rectangle(375, 844, 1170, 58, 1920, 1080, true), "bgTex", GuiContainer.Layer.overall, new GuiColor("#25639BF0"), 0.5f, 1);
                    container.addPlainPanel("gametip", new Rectangle(375, 844, 1170, 58, 1920, 1080, true), GuiContainer.Layer.overall, new GuiColor("#25639BF4"), 0.5f, 1, GuiContainer.Blur.greyout);
                    container.addText("gametip_txt", new Rectangle(433, 844, 1112, 58, 1920, 1080, true), GuiContainer.Layer.overall, new GuiText(text, 20, new GuiColor("#FFFFFFD9")), 0.5f, 1);
                    container.addImage("gametip_icon", new Rectangle(375, 844, 58, 58, 1920, 1080, true), "gameTipIcon", GuiContainer.Layer.overall, new GuiColor("#FFFFFFD9"), 0.5f, 1);
                    break;

                case gametipType.warning:
                    container.addPlainPanel("gametip", new Rectangle(375, 844, 1170, 58, 1920, 1080, true), GuiContainer.Layer.overall, new GuiColor("#DED502F0"), 0.5f, 1);
                    container.addText("gametip_txt", new Rectangle(433, 844, 1112, 58, 1920, 1080, true), GuiContainer.Layer.overall, new GuiText(text, 20, new GuiColor("#000000D9")), 0.5f, 1);
                    container.addImage("gametip_icon", new Rectangle(375, 844, 58, 58, 1920, 1080, true), "warning_alpha", GuiContainer.Layer.overall, new GuiColor("#FFFFFFD9"), 0.5f, 1);
                    break;

                case gametipType.error:
                    //container.addImage("gametip", new Rectangle(375, 844, 1170, 58, 1920, 1080, true), "bgTex", GuiContainer.Layer.overall, new GuiColor("#BB0000F0"), 0.5f, 1);
                    container.addPlainPanel("gametip", new Rectangle(375, 844, 1170, 58, 1920, 1080, true), GuiContainer.Layer.overall, new GuiColor("#BB0000F0"), 0.5f, 1, GuiContainer.Blur.greyout);
                    container.addText("gametip_txt", new Rectangle(433, 844, 1112, 58, 1920, 1080, true), GuiContainer.Layer.overall, new GuiText(text, 20, new GuiColor("#FFFFFFD9")), 0.5f, 1);
                    container.addImage("gametip_icon", new Rectangle(375, 844, 58, 58, 1920, 1080, true), "white_cross", GuiContainer.Layer.overall, new GuiColor("#FFFFFFD9"), 0.5f, 1);
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

        public void Tickbox(BasePlayer player, Plugin plugin, Rectangle parentRectangle, GuiContainer.Layer layer, string name, string parentContainer, Action<BasePlayer, string[]> onTick, Action<BasePlayer, string[]> onUntick, bool initialState = false, bool disabled = false, float fadeIn = 0, float fadeOut = 0)
        {
            GuiContainer c = new GuiContainer(plugin, name + "_APITickbox", parentContainer);
            GuiColor lightGrey = new GuiColor(0.5f, 0.5f, 0.5f, 1);
            GuiColor darkGrey = new GuiColor(0.8f, 0.8f, 0.8f, 1);

            if(initialState)
            {
                Rectangle fgPos = new Rectangle(0.15f, 0.15f, 0.72f, 0.72f, 1f, 1f, true).WithParent(parentRectangle);
                c.addPlainButton("bg", parentRectangle, layer, disabled ? lightGrey : GuiColor.White, fadeIn, fadeOut, callback: disabled ? null : onUntick);
                c.addPlainButton("fg", fgPos, layer, disabled ? darkGrey : GuiColor.Black, fadeIn, fadeOut, callback: disabled ? null : onUntick);
            }
            else
            {
                c.addPlainButton("bg", parentRectangle, layer, disabled ? lightGrey : GuiColor.White, fadeIn, fadeOut, callback: disabled ? null : onTick);
            }

            c.display(player);
        }

        public void PlayerSearch(BasePlayer player, string name, Action<BasePlayer> callback)
        {
            if (string.IsNullOrEmpty(name)) return;
            ulong id;
            ulong.TryParse(name, out id);
            List<BasePlayer> results = BasePlayer.allPlayerList.Where((p) => p.displayName.ToUpper().Contains(name.ToUpper()) || p.userID == id).ToList();

            if(results == null || results.Count == 0)
            {
                prompt(player, "Player search", "No players found!");
                return;
            }

            if(results.Count == 1)
            {
                callback(results[0]);
                return;
            }

            Action<PlayerSummary[]> cb = (pss) =>
            {
                var help = new List<KeyValuePair<BasePlayer, PlayerSummary>>();
                foreach(PlayerSummary ps in pss)
                {
                    help.Add( new KeyValuePair<BasePlayer, PlayerSummary>(results.Find(p => p.UserIDString == ps.steamid), ps));
                }
                SendPlayerSearchUI(player, help.ToArray(), callback);
            };
            GetSteamUserData(results.Select(p => p.userID).ToList(), cb);
        }

        private void SendPlayerSearchUI(BasePlayer player, KeyValuePair<BasePlayer, PlayerSummary>[] results, Action<BasePlayer> callback, int page = 0)
        {
            List<List<KeyValuePair<BasePlayer, PlayerSummary>>> listOfLists = SplitIntoChunks(results.ToList(), 5);

            GuiContainer c = new GuiContainer(PluginInstance, "PlayerSearch");
            //clickout
            c.addPlainButton("close", new Rectangle(), GuiContainer.Layer.overall, new GuiColor(0, 0, 0, 0.3f), 0.1f, 0.1f, blur: GuiContainer.Blur.medium);

            GuiColor black60 = new GuiColor(0, 0, 0, 0.6f);
            GuiColor black40 = new GuiColor(0, 0, 0, 0.4f);
            GuiColor white70 = new GuiColor(1, 1, 1, 0.7f);

            //background
            Rectangle bgPos = new Rectangle(710, 260, 500, 560, 1920, 1080, true);
            c.addPlainPanel("background", bgPos, GuiContainer.Layer.overall, black60, 0.2f, 0, GuiContainer.Blur.medium);
            c.addPlainPanel("background2", bgPos, GuiContainer.Layer.overall, black60, 0.2f, 0, GuiContainer.Blur.greyout);

            //header
            Rectangle headerPos = new Rectangle(710, 260, 500, 60, 1920, 1080, true);
            GuiText headerText = new GuiText("Players found:", 20, white70);
            c.addText("header", headerPos, GuiContainer.Layer.overall, headerText, 0.2f, 0);

            //navigators
            if (page != 0)
            {
                //up
                Rectangle upPos = new Rectangle(945, 325, 30, 30, 1920, 1080, true);
                Action<BasePlayer, string[]> upCb = (p, a) =>
                {
                    SendPlayerSearchUI(player, results, callback, page - 1);
                };
                c.addButton("upbtn", upPos, GuiContainer.Layer.overall, white70, 0.2f, 0, callback: upCb, imgName: "triangle_up");
            }
            if (page != listOfLists.Count - 1)
            {
                //down
                Rectangle downPos = new Rectangle(945, 785, 30, 30, 1920, 1080, true);
                Action<BasePlayer, string[]> downCb = (p, a) =>
                {
                    SendPlayerSearchUI(player, results, callback, page + 1);
                };
                c.addButton("downbtn", downPos, GuiContainer.Layer.overall, white70, 0.2f, 0, callback: downCb, imgName: "triangle_down");
            }

            c.display(player);

            //entries
            int count = 0;
            int sizeEach = 80;
            int gap = 5;
            foreach (KeyValuePair<BasePlayer, PlayerSummary> kvp in listOfLists[page])
            {
                int ccount = count;
                int csizeEach = sizeEach;
                int cgap = gap;
                Action imageCb = () =>
                {
                    SendEntry(player, kvp, ccount, csizeEach, cgap, callback);
                };
                registerImage(PluginInstance, kvp.Key.UserIDString, kvp.Value.avatarfull, imageCb, true);
                count++;
            }
        }

        private void SendEntry(BasePlayer player, KeyValuePair<BasePlayer, PlayerSummary> kvp, int count, int sizeEach, int gap, Action<BasePlayer> callback)
        {
            if (GuiTracker.getGuiTracker(player).getContainer(PluginInstance, "PlayerSearch") == null) return;

            GuiColor black60 = new GuiColor(0, 0, 0, 0.6f);
            GuiColor black40 = new GuiColor(0, 0, 0, 0.4f);
            GuiColor white70 = new GuiColor(1, 1, 1, 0.7f);

            GuiContainer c = new GuiContainer(PluginInstance, $"{count}ImageContainer", "PlayerSearch");

            //background
            Rectangle entryBgPos = new Rectangle(715, 360 + count * (sizeEach + gap), 490, 80, 1920, 1080, true);
            c.addPlainPanel($"{count}EntryBG", entryBgPos, GuiContainer.Layer.overall, black60, 0.2f, 0);

            //ID
            Rectangle idPos = new Rectangle(795, 365 + count * (sizeEach + gap), 405, 35, 1920, 1080, true);
            GuiText idText = new GuiText($"[{kvp.Key.userID}]", 14, white70);
            c.addText($"{count}id", idPos, GuiContainer.Layer.overall, idText, 0.2f, 0);

            //Name
            Rectangle namePos = new Rectangle(795, 400 + count * (sizeEach + gap), 405, 35, 1920, 1080, true);
            GuiText nameText = new GuiText($"{kvp.Key.displayName}", getFontsizeByFramesize(kvp.Key.displayName.Length, namePos), white70);
            c.addText($"{count}name", namePos, GuiContainer.Layer.overall, nameText, 0.2f, 0);

            //button
            Action<BasePlayer, string[]> buttonCb = (p2, a) =>
            {
                GuiTracker.getGuiTracker(player).destroyGui(PluginInstance, "PlayerSearch"); 
                callback(kvp.Key);
            };
            c.addPlainButton($"{count}btnOverlay", entryBgPos, GuiContainer.Layer.overall, new GuiColor(0, 0, 0, 0), 0.2f, 0, callback: buttonCb, CursorEnabled: true);

            Rectangle imgPos = new Rectangle(720, 365 + count * (sizeEach + gap), 70, 70, 1920, 1080, true);
            c.addButton($"{count}Image", imgPos, GuiContainer.Layer.overall, null, 0.2f, 0, callback: buttonCb, close: "PlayerSearch", imgName: kvp.Key.UserIDString);

            c.display(player);
        }

        public void prompt(BasePlayer player, string header, string msg, Action<BasePlayer, string[]> Callback = null)
        {
            GuiContainer containerGUI = new GuiContainer(this, "prompt");
            containerGUI.addPlainButton("close", new Rectangle(), GuiContainer.Layer.overall, new GuiColor(0, 0, 0, 0.3f), 0.1f, 0.1f, blur: GuiContainer.Blur.medium);
            containerGUI.addPlainPanel("background", new Rectangle(710, 390, 500, 300, 1920, 1080, true), GuiContainer.Layer.overall, new GuiColor(0, 0, 0, 0.6f), 0.1f, 0.1f, GuiContainer.Blur.medium);
            containerGUI.addPanel("header", new Rectangle(710, 390, 500, 60, 1920, 1080, true), GuiContainer.Layer.overall, new GuiColor(0, 0, 0, 0), 0.1f, 0.1f, new GuiText(header, 25, new GuiColor(1, 1, 1, 0.7f)));
            containerGUI.addPanel("msg", new Rectangle(735, 400, 450, 150, 1920, 1080, true), GuiContainer.Layer.overall, new GuiColor(0, 0, 0, 0), 0.1f, 0.1f, new GuiText(msg, 14, new GuiColor(1, 1, 1, 0.7f)));
            containerGUI.addPlainButton("ok", new Rectangle(860, 520, 200, 50, 1920, 1080, true), GuiContainer.Layer.overall, new GuiColor(0, 0, 1, 0.6f), 0.1f, 0.1f, new GuiText("OK", 20, new GuiColor(1, 1, 1, 0.7f)), Callback, "prompt");
            containerGUI.display(player);
        }

        public void BigConfirmPrompt(BasePlayer player, string header, string msg, Action<BasePlayer, string[]> yesCallback, Action<BasePlayer, string[]> noCallback)
        {
            GuiContainer containerGUI = new GuiContainer(this, "prompt");
            containerGUI.addPlainButton("close", new Rectangle(), GuiContainer.Layer.overall, new GuiColor(0, 0, 0, 0.3f), 0.1f, 0.1f, callback: noCallback, blur: GuiContainer.Blur.medium);
            containerGUI.addPlainPanel("background", new Rectangle(710, 390, 500, 300, 1920, 1080, true), GuiContainer.Layer.overall, new GuiColor(0, 0, 0, 0.6f), 0.1f, 0.1f, GuiContainer.Blur.medium);
            containerGUI.addPanel("header", new Rectangle(710, 390, 500, 60, 1920, 1080, true), GuiContainer.Layer.overall, new GuiColor(0, 0, 0, 0), 0.1f, 0.1f, new GuiText(header, 25, new GuiColor(1, 1, 1, 0.7f)));
            containerGUI.addPanel("msg", new Rectangle(735, 450, 450, 150, 1920, 1080, true), GuiContainer.Layer.overall, new GuiColor(0, 0, 0, 0), 0.1f, 0.1f, new GuiText(msg, 10, new GuiColor(1, 1, 1, 0.7f)));
            containerGUI.addPlainButton("yes", new Rectangle(740, 620, 200, 50, 1920, 1080, true), GuiContainer.Layer.overall, new GuiColor(0, 1, 0, 0.6f), 0.1f, 0.1f, new GuiText("YES", 20, new GuiColor(1, 1, 1, 0.7f)), yesCallback, "prompt");
            containerGUI.addPlainButton("no", new Rectangle(980, 620, 200, 50, 1920, 1080, true), GuiContainer.Layer.overall, new GuiColor(1, 0, 0, 0.6f), 0.1f, 0.1f, new GuiText("NO", 20, new GuiColor(1, 1, 1, 0.7f)), noCallback, "prompt");
            containerGUI.display(player);
        }

        public void SmallConfirmPrompt(BasePlayer player, string header, Action<BasePlayer, string[]> yesCallback, Action<BasePlayer, string[]> noCallback)
        {
            GuiContainer containerGUI = new GuiContainer(this, "prompt");
            containerGUI.addPlainButton("close", new Rectangle(), GuiContainer.Layer.overall, new GuiColor(0, 0, 0, 0.3f), 0.1f, 0.1f, callback: noCallback, blur: GuiContainer.Blur.medium);
            containerGUI.addPlainPanel("background", new Rectangle(710, 465, 500, 150, 1920, 1080, true), GuiContainer.Layer.overall, new GuiColor(0, 0, 0, 0.6f), 0.1f, 0.1f, GuiContainer.Blur.medium);
            containerGUI.addPanel("header", new Rectangle(710, 465, 500, 60, 1920, 1080, true), GuiContainer.Layer.overall, new GuiColor(0, 0, 0, 0), 0.1f, 0.1f, new GuiText(header, 25, new GuiColor(1, 1, 1, 0.7f)));
            containerGUI.addPlainButton("yes", new Rectangle(740, 545, 200, 50, 1920, 1080, true), GuiContainer.Layer.overall, new GuiColor(0, 1, 0, 0.6f), 0.1f, 0.1f, new GuiText("YES", 20, new GuiColor(1, 1, 1, 0.7f)), yesCallback, "prompt");
            containerGUI.addPlainButton("no", new Rectangle(980, 545, 200, 50, 1920, 1080, true), GuiContainer.Layer.overall, new GuiColor(1, 0, 0, 0.6f), 0.1f, 0.1f, new GuiText("NO", 20, new GuiColor(1, 1, 1, 0.7f)), noCallback, "prompt");
            containerGUI.display(player);
        }

        private class Submission
        {
            public string Input { get; set; } = null;
        }

        public void SubmitPrompt(BasePlayer player, string header, Action<BasePlayer, string> inputCallback)
        {
            GuiContainer containerGUI = new GuiContainer(this, "prompt");
            containerGUI.addPlainButton("close", new Rectangle(), GuiContainer.Layer.overall, new GuiColor(0, 0, 0, 0.3f), 0.1f, 0.1f, blur: GuiContainer.Blur.medium);
            containerGUI.addPlainPanel("background", new Rectangle(710, 425, 500, 230, 1920, 1080, true), GuiContainer.Layer.overall, new GuiColor(0, 0, 0, 0.6f), 0.1f, 0.1f, GuiContainer.Blur.medium);
            containerGUI.addPanel("header", new Rectangle(710, 435, 500, 60, 1920, 1080, true), GuiContainer.Layer.overall, new GuiColor(0, 0, 0, 0), 0.1f, 0.1f, new GuiText(header, 25, new GuiColor(1, 1, 1, 0.7f)));
            Submission submission = new Submission();
            Action< BasePlayer, string[]> inputFieldCallback = (p, a) =>
            {
                submission.Input = string.Join(" ", a);
            };
            containerGUI.addInput("input", new Rectangle(735, 505, 450, 60, 1920, 1080, true), inputFieldCallback, GuiContainer.Layer.overall, null, new GuiColor(1, 1, 1, 1), 100, new GuiText("", 14, new GuiColor(0, 0, 0, 1)), 0.1f, 0.1f);
            Action<BasePlayer, string[]> cb = (p, a) =>
            {
                inputCallback(p, submission.Input);
            };
            containerGUI.addPlainButton("submit", new Rectangle(860, 583, 200, 50, 1920, 1080, true), GuiContainer.Layer.overall, new GuiColor(1, 0, 0, 0.6f), 0.1f, 0.1f, new GuiText("SUBMIT", 20, new GuiColor(1, 1, 1, 0.7f)), cb, "prompt");
            containerGUI.display(player);
        }

        public void dropdown(Plugin plugin, BasePlayer player, List<string> options, Rectangle rectangle, GuiContainer.Layer layer, string parent, GuiColor panelColor, GuiColor textColor, Action<string> callback, bool allowNew = false, int page = 0, Predicate<string> predicate = null)
        {
            if (allowNew) options.Add("(add new)");
            int maxItems = 5;
            rectangle.H *= ((float)options.Count / (float)maxItems);
            List<List<string>> ListOfLists = SplitIntoChunks<string>(options, maxItems);
            GuiContainer container = new GuiContainer(plugin, "dropdown_API", parent);

            double cfX = rectangle.W / 300;
            double cfY = rectangle.H / 570;

            Action<BasePlayer, string[]> up = (bPlayer, input) =>
            {
                dropdown(plugin, player, options, rectangle, layer, parent, panelColor, textColor, callback, allowNew, page - 1, predicate);
            };
            Action<BasePlayer, string[]> down = (bPlayer, input) =>
            {
                dropdown(plugin, player, options, rectangle, layer, parent, panelColor, textColor, callback, allowNew, page + 1, predicate);
            };
            if (page > 0) container.addPlainButton("dropdown_up", new Rectangle(0, 1, 298, 36, 300, 570, true), new GuiColor(1, 1, 1, 0.4f), 0, 0, new GuiText("<b>∧</b>", (int)Math.Floor(22 * cfY), new GuiColor("black")), up, parent: "dropdown_background");
            if (page < ListOfLists.Count - 1) container.addPlainButton("dropdown_up", new Rectangle(0, 533, 298, 37, 300, 570, true), new GuiColor(1, 1, 1, 0.4f), 0, 0, new GuiText("<b>∨</b>", (int)Math.Floor(22 * cfY), new GuiColor("black")), down, parent: "dropdown_background");

            int count = 0;
            foreach (string option in ListOfLists[page])
            {
                int hEach = (int)(rectangle.H / ListOfLists[page].Count);
                Rectangle pos = new Rectangle(0, 0 + (count*hEach), rectangle.W, hEach, rectangle.W, rectangle.H, true).WithParent(rectangle);

                Action<BasePlayer, string[]> btnCallback = null;
                if (option == "(add new)")
                {
                    btnCallback = (bPlayer, input) => dropdownAddNew(plugin, player, pos, callback, predicate);
                }
                else
                {
                    string selected = option;
                    btnCallback = (bPlayer, input) =>
                    {
                        callback(selected);
                    };
                }
                container.addPlainButton($"dropdown_option_{option}", pos, layer, panelColor, 0, 0, new GuiText(option, color: textColor), btnCallback, blur: GuiContainer.Blur.strong);
                count++;
            }

            container.display(player);
        }

        private void dropdownAddNew(Plugin plugin, BasePlayer player, Rectangle rectangle, Action<string> callback, Predicate<string> predicate)
        {
            GuiContainer container = new GuiContainer(this, "dropdown_addNew", "dropdown_API");
            Action<BasePlayer, string[]> inputCallback = (bPlayer, input) =>
            {
                if (input.Length == 0) return;
                GuiTracker.getGuiTracker(player).destroyGui(PluginInstance, "dropdown_addNew");
                StringBuilder newName = new StringBuilder();
                int i = 1;
                foreach (string s in input)
                {
                    newName.Append(s);
                    if (i != input.Length) newName.Append(" ");
                    i++;
                }

                if (predicate != null)
                {
                    if (!predicate(newName.ToString()))
                    {
                        prompt(player, "Your input is invalid!", "INVALID INPUT");
                        dropdownAddNew(plugin, player, rectangle, callback, predicate);
                        return;
                    }
                }
                callback(newName.ToString());
                GuiTracker.getGuiTracker(player).destroyGui(PluginInstance, container);
            };

            container.addInput("dropdown_addNew_input", rectangle, inputCallback, GuiContainer.Layer.menu, null, new GuiColor("white"), 15, new GuiText("", color: new GuiColor("black")), 0, 0);
            container.display(player);
        }

        public void SendColorPicker(BasePlayer player, Action<GuiColor> callback, string header = "Color Picker")
        {
            new ColorPicker(player, callback, header);
        }

        public void registerImage(Plugin plugin, string name, string url, Action callback = null, bool force = false, int? imgSizeX = null, int? imgSizeY = null)
        {
            string safeName = $"{plugin.Name}_{name}";

            if (!force && ImageLibrary.Call<bool>("HasImage", safeName, (ulong)0))
            {
                callback?.Invoke();
            }
            else
            {
                if (imgSizeX != null && imgSizeY != null)
                {
                    _DownloadManager.Enqueue(new DownloadManager.Request {SafeName = safeName, Url = url, ImgSizeX = imgSizeX.Value, ImgSizeY = imgSizeY.Value, Callback = callback });
                }
                else
                {
                    ImageLibrary.Call("AddImage", url, safeName, (ulong)0, callback);
                }
            }
        }

        public bool HasImage(Plugin plugin, string name)
        {
            string safeName = $"{plugin.Name}_{name}";

            if (ImageLibrary.Call<bool>("HasImage", safeName, (ulong)0))
            {
                return true;
            }

            return false;
        }
    }
}﻿namespace Oxide.Plugins
{
    using System;

    public partial class GUICreator
    {
        private class ColorPicker
        {
            private const float fadeIn = 0.1f;
            private const float fadeOut = 0.1f;
            private const int resX = 1920;
            private const int resY = 1080;
            private const GuiContainer.Layer layer = GuiContainer.Layer.hud;
            private const int hueRes = 25;
            private const int satRes = 10;
            private const int valueRes = 7;

            public BasePlayer Player { get; set; }

            public Action<GuiColor> Callback { get; set; }

            private readonly string header;

            public int Hue { get; set; }

            public double Value { get; set; }

            public double Saturation { get; set; }

            public ColorPicker(BasePlayer player, Action<GuiColor> callback, string header = "Color Picker")
            {
                Player = player;
                Callback = callback;
                this.header = header;

                Hue = 0;
                Value = 1;
                Saturation = 1;

                Display();
            }

            public void Display()
            {
                SendBackgound();
                SendButton();
                SendHuePicker();
            }

            private void SendBackgound()
            {
                GuiContainer c = new GuiContainer(PluginInstance, nameof(ColorPicker));

                //clickout
                c.addPlainButton("close", new Rectangle(), layer, GuiColor.Transparent);

                //Panel
                Rectangle panelPos = new Rectangle(560, 265, 800, 550, resX, resY, true);
                GuiColor panelColor = new GuiColor(0, 0, 0, 0.5f);
                c.addPlainPanel("bgPanel", panelPos, layer, panelColor, fadeIn, fadeOut, GuiContainer.Blur.medium);

                //Label
                Rectangle labelPos = new Rectangle(560, 265, 800, 60, resX, resY, true);
                GuiText labelText = new GuiText(header, 30, GuiColor.White.withAlpha(0.7f));
                c.addText("bgLabel", labelPos, layer, labelText, fadeIn, fadeOut);

                c.display(Player);
            }

            private void SendHuePicker()
            {
                GuiContainer c = new GuiContainer(PluginInstance, "HuePicker", nameof(ColorPicker));

                int baseX = 610;
                int y = 732;
                double width = 700d / hueRes;
                int height = 30;
                double hueIncrement = 360d / hueRes;

                for(int i = 0; i < hueRes; i++)
                {
                    Rectangle pos = new Rectangle(baseX + i * width, y, width, height, resX, resY, true);
                    GuiColor color = new GuiColor(i * hueIncrement, 1, 1, 1);
                    int hue = (int)(i * hueIncrement);

                    int index = i;

                    Action<BasePlayer, string[]> callback = (p, a) =>
                    {
                        Hue = hue;
                        SendHueSelector(index);
                        SendVSPicker();
                    };

                    c.addPlainButton($"hue_{i}", pos, layer, color, fadeIn, fadeOut, callback: callback);
                }

                c.display(Player);

                SendHueSelector(0);
                SendVSPicker();
            }

            private void SendHueSelector(int i)
            {
                GuiContainer c = new GuiContainer(PluginInstance, "HueSelector", "HuePicker");

                int baseX = 603;
                int y = 722;
                double width = 700d / hueRes;
                double hueIncrement = 360d / hueRes;

                Rectangle pos = new Rectangle(baseX + i * width, y, 50, 50, resX, resY, true);
                GuiColor color = new GuiColor(i * hueIncrement, 1, 1, 1);
                c.addImage("selected", pos, "circle", layer, color, fadeIn, fadeOut);

                c.display(Player);
            }

            private void SendVSPicker()
            {
                GuiContainer c = new GuiContainer(PluginInstance, "SVPicker", nameof(ColorPicker));

                int baseX = 610;
                int baseY = 330;
                double width = 525d / (satRes + 1);
                double height = 375d / (valueRes + 1);
                double satIncrement = 1d / satRes;
                double valIncrement = 1d / valueRes;

                for (int v = 0; v <= valueRes; v++)
                {
                    for(int s = 0; s <= satRes; s++)
                    {
                        double saturation = s * satIncrement;
                        double value = (valueRes - v) * valIncrement;
                        Rectangle pos = new Rectangle(baseX + s * width, baseY + v * height, width, height, resX, resY, true);
                        GuiColor color = new GuiColor(Hue, saturation, value, 1);

                        int si = s;
                        int vi = v;

                        Action<BasePlayer, string[]> callback = (p, a) =>
                        {
                            Saturation = saturation;
                            Value = value;
                            SendSVSelector(si, vi);
                            SendPreview();
                        };

                        c.addPlainButton($"sv_{s}_{v}", pos, layer, color, fadeIn, fadeOut, callback: callback);
                    }
                }

                c.display(Player);

                SendPreview();
            }

            private void SendSVSelector(int s, int v)
            {
                GuiContainer c = new GuiContainer(PluginInstance, "SVSelector", "SVPicker");

                int baseX = 595;
                int baseY = 315;
                double width = 525d / (satRes + 1);
                double height = 375d / (valueRes + 1);

                Rectangle pos = new Rectangle(baseX + s * width, baseY + v * height, 75, 75, resX, resY, true);
                GuiColor color = new GuiColor(Hue, Saturation, Value, 1);
                c.addImage("selected", pos, "circle", layer, color, fadeIn, fadeOut);

                c.display(Player);
            }

            private void SendPreview()
            {
                GuiContainer c = new GuiContainer(PluginInstance, "Preview", nameof(ColorPicker));

                Rectangle pos = new Rectangle(1160, 368, 150, 150, resX, resY, true);
                GuiColor color = new GuiColor(Hue, Saturation, Value, 1);

                c.addPlainPanel("panel", pos, layer, color, fadeIn, fadeOut);

                c.display(Player);
            }

            private void SendButton()
            {
                GuiContainer c = new GuiContainer(PluginInstance, "Ok", nameof(ColorPicker));

                Rectangle pos = new Rectangle(1160, 540, 150, 60, resX, resY, true);
                GuiColor color = new GuiColor(0, 1, 0, 0.5f);
                GuiText text = new GuiText("OK", 30, GuiColor.White.withAlpha(0.7f));

                Action<BasePlayer, string[]> callback = (p, a) =>
                {
                    Callback?.Invoke(new GuiColor(Hue, Value, Saturation, 1));
                    GuiTracker.getGuiTracker(Player).destroyGui(PluginInstance, nameof(ColorPicker));
                };

                c.addPlainButton("button", pos, layer, color, fadeIn, fadeOut, text, callback);

                c.display(Player);
            }
        }
    }
}﻿namespace Oxide.Plugins
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    public partial class GUICreator
    {
        public class Command
        {
            const string flagSymbols = "/-";
            public string fullString;
            public string name;
            public List<string> args = new List<string>();
            public Dictionary<string, List<string>> flags = new Dictionary<string, List<string>>();

            public Command(string fullString)
            {
                this.fullString = fullString;
                string[] split = Regex.Split(fullString, " ");
                if (split.Length < 1) return;
                name = split.First();
                if (split.Length < 2) return;
                split = split.Skip(1).ToArray();
                List<string> flag = null;
                foreach (string arg in split)
                {
                    if (flagSymbols.Contains(arg.First()))
                    {
                        string sanitizedFlag = arg.Substring(1);
                        if (!flags.ContainsKey(sanitizedFlag))
                        {
                            flags.Add(sanitizedFlag, new List<string>());
                        }
                        flag = flags[sanitizedFlag];
                    }
                    else if (flag != null) flag.Add(arg);
                    else args.Add(arg);
                }
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder($"{name} ");
                foreach (string arg in args)
                {
                    sb.Append($"{arg} ");
                }
                foreach (string flag in flags.Keys)
                {
                    sb.Append($"\n{flag}: ");
                    foreach (string arg in flags[flag])
                    {
                        sb.Append($"{arg} ");
                    }
                }
                return sb.ToString();
            }
        }
    }
}﻿namespace Oxide.Plugins
{
    using Facepunch.Extend;
    using Oxide.Core.Plugins;
    using Oxide.Game.Rust.Cui;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public partial class GUICreator
    {
        public void InitCommands()
        {
            cmd.AddConsoleCommand("gui.close", this, nameof(closeUi));
            cmd.AddConsoleCommand("gui.input", this, nameof(OnGuiInput));
            cmd.AddConsoleCommand("gui.list", this, nameof(listContainers));

            cmd.AddChatCommand("player", this, nameof(PlayerSearchCommand));
            cmd.AddChatCommand("guidemo", this, nameof(demoCommand));
            cmd.AddChatCommand("img", this, nameof(imgPreviewCommand));
            cmd.AddChatCommand("imgraw", this, nameof(imgrawPreviewCommand));
            cmd.AddChatCommand("colorpicker", this, nameof(colorpickerCommand));
        }

        private void PlayerSearchCommand(BasePlayer player, string command, string[] args)
        {
            if (args.Length == 0) return;
            PlayerSearch(player, args[0], (p) => player.ChatMessage($"player selected: {p.displayName}"));
        }

        private void colorpickerCommand(BasePlayer player, string command, string[] args)
        {
            SendColorPicker(player, (c) => player.ChatMessage($"R: {c.color.r} G: {c.color.g} B: {c.color.b}"));
        }

        private void closeUi(ConsoleSystem.Arg arg)
        {
            GuiTracker.getGuiTracker(arg.Player()).destroyAllGui();
        }

        private void listContainers(ConsoleSystem.Arg arg)
        {
            BasePlayer player = arg.Player();
            if (arg.Args != null) player = BasePlayer.FindByID(ulong.Parse(arg.Args[0]));
            if (player == null) return;
            GuiTracker tracker = GuiTracker.getGuiTracker(player);
            if (tracker.activeGuiContainers.Count == 0)
            {
                SendReply(arg, "you don't have any active guiContainers!");
                return;
            }
            foreach (GuiContainer container in tracker.activeGuiContainers)
            {
                SendReply(arg, $"Plugin: {container.plugin.Name}, Container: {container.name}, Parent: {container.parent}:");
                foreach (GuiElement element in container)
                {
                    SendReply(arg, $"- Element: {element.Name}, Parent: {element.Parent}, ParentElement: {element.ParentElement?.Name ?? "null"}");
                }
            }
        }

        private void OnGuiInput(ConsoleSystem.Arg arg)
        {
            //gui.input pluginName containerName inputName --close elementNames... --input text...
            if (arg.Args.Length < 3)
            {
                SendReply(arg, "OnGuiInput: not enough arguments given!");
                return;
            }

            BasePlayer player = arg.Player();
            if (player == null) return;

#if DEBUG2
            SendReply(arg, $"OnGuiInput: gui.input {arg.FullString}");
            player.ChatMessage($"OnGuiInput: gui.input {arg.FullString}");
#endif

            GuiTracker tracker = GuiTracker.getGuiTracker(player);

            #region parsing

            Command cmd = new Command("gui.input " + arg.FullString);
#if DEBUG
            player.ChatMessage(cmd.ToString());
#endif
            Plugin plugin = Manager.GetPlugin(cmd.args[0]);
            if (plugin == null)
            {
                SendReply(arg, "OnGuiInput: Plugin not found!");
                return;
            }

            GuiContainer container = tracker.getContainer(plugin, cmd.args[1]);
            if (container == null)
            {
                SendReply(arg, "OnGuiInput: Container not found!");
                return;
            }

            string inputName = cmd.args[2];

            bool closeContainer = false;
            if (inputName == "close" || inputName == "close_btn") closeContainer = true;
            #endregion

            #region execution
            string[] input = null;
            if (cmd.flags.ContainsKey("-input")) input = cmd.flags["-input"].ToArray();

            if (!container.runCallback(inputName, player, input))
            {
#if DEBUG
                SendReply(arg, $"OnGuiInput: Callback for {plugin.Name} {container.name} {inputName} wasn't found");
#endif
            }

            if (cmd.flags.ContainsKey("-close"))
            {
                foreach (string name in cmd.flags["-close"])
                {
                    if (name == container.name) tracker.destroyGui(plugin, container);
                    else tracker.destroyGui(plugin, container, name);
                }
            }

            if (closeContainer) tracker.destroyGui(plugin, container);

            #endregion

        }
        
        private void demoCommand(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, "gui.demo"))
            {
                PrintToChat(player, lang.GetMessage("noPermission", this, player.UserIDString));
                return;
            }
            if (args.Length == 0)
            {
                GuiContainer container = new GuiContainer(this, "demo");
                container.addPanel("demo_panel", new Rectangle(0.25f, 0.5f, 0.25f, 0.25f), GuiContainer.Layer.hud, new GuiColor(0, 1, 0, 1), 1, 1, new GuiText("This is a regular panel", 30));
                container.addPanel("demo_img", new Rectangle(0.25f, 0.25f, 0.25f, 0.25f), FadeIn: 1, FadeOut: 1, text: new GuiText("this is an image with writing on it", 30, color: new GuiColor(1, 1, 1, 1)), imgName: "flower");
                Action<BasePlayer, string[]> heal = (bPlayer, input) => { bPlayer.Heal(10); };
                container.addButton("demo_healButton", new Rectangle(0.5f, 0.5f, 0.25f, 0.25f), GuiContainer.Layer.hud, null, 1, 1, new GuiText("heal me", 40), heal, null, false, "flower");
                Action<BasePlayer, string[]> hurt = (bPlayer, input) => { bPlayer.Hurt(10); };
                container.addButton("demo_hurtButton", new Rectangle(0.5f, 0.25f, 0.25f, 0.25f), new GuiColor(1, 0, 0, 0.5f), 1, 1, new GuiText("hurt me", 40), hurt);
                container.addText("demo_inputLabel", new Rectangle(0.375f, 0.85f, 0.25f, 0.1f), new GuiText("Print to chat:", 30, null, TextAnchor.LowerCenter), 1, 1);
                Action<BasePlayer, string[]> inputCallback = (bPlayer, input) => { PrintToChat(bPlayer, string.Concat(input)); };
                container.addInput("demo_input", new Rectangle(0.375f, 0.75f, 0.25f, 0.1f), inputCallback, GuiContainer.Layer.hud, null, new GuiColor("white"), 100, new GuiText("", 50), 1, 1);
                container.addButton("close", new Rectangle(0.1f, 0.1f, 0.1f, 0.1f), new GuiColor("red"), 1, 0, new GuiText("close", 50));
                container.display(player);

                GuiContainer container2 = new GuiContainer(this, "demo_child", "demo");
                container2.addPanel("layer1", new Rectangle(1400, 800, 300, 100, 1920, 1080, true), GuiContainer.Layer.overall, new GuiColor("red"), 1, 1, new GuiText("overall", align: TextAnchor.LowerLeft));
                container2.addPanel("layers_label", new Rectangle(1400, 800, 300, 100, 1920, 1080, true), GuiContainer.Layer.overall, null, 1, 1, new GuiText("Available layers:", 20, align: TextAnchor.UpperLeft));
                container2.addPanel("layer2", new Rectangle(1425, 825, 300, 100, 1920, 1080, true), GuiContainer.Layer.overlay, new GuiColor("yellow"), 1, 1, new GuiText("overlay", align: TextAnchor.LowerLeft));
                container2.addPanel("layer3", new Rectangle(1450, 850, 300, 100, 1920, 1080, true), GuiContainer.Layer.menu, new GuiColor("green"), 1, 1, new GuiText("menu", align: TextAnchor.LowerLeft));
                container2.addPanel("layer4", new Rectangle(1475, 875, 300, 100, 1920, 1080, true), GuiContainer.Layer.hud, new GuiColor("blue"), 1, 1, new GuiText("hud", align: TextAnchor.LowerLeft));
                container2.addPanel("layer5", new Rectangle(1500, 900, 300, 100, 1920, 1080, true), GuiContainer.Layer.under, new GuiColor("purple"), 1, 1, new GuiText("under", align: TextAnchor.LowerLeft));
                container2.display(player);

                GuiContainer container3 = new GuiContainer(this, "demo_anchors", "demo");
                container3.addPlainPanel("bl", new Rectangle(20, 960, 100, 100, 1920, 1080, true, Rectangle.Anchors.BottomLeft), GuiContainer.Layer.menu, new GuiColor("white"), 1, 1);
                container3.addPlainPanel("cl", new Rectangle(20, 490, 100, 100, 1920, 1080, true, Rectangle.Anchors.CenterLeft), GuiContainer.Layer.menu, new GuiColor("white"), 1, 1);
                container3.addPlainPanel("ul", new Rectangle(20, 20, 100, 100, 1920, 1080, true, Rectangle.Anchors.UpperLeft), GuiContainer.Layer.menu, new GuiColor("white"), 1, 1);
                container3.addPlainPanel("uc", new Rectangle(910, 20, 100, 100, 1920, 1080, true, Rectangle.Anchors.UpperCenter), GuiContainer.Layer.menu, new GuiColor("white"), 1, 1);
                container3.addPlainPanel("ur", new Rectangle(1800, 20, 100, 100, 1920, 1080, true, Rectangle.Anchors.UpperRight), GuiContainer.Layer.menu, new GuiColor("white"), 1, 1);
                container3.addPlainPanel("cr", new Rectangle(1800, 490, 100, 100, 1920, 1080, true, Rectangle.Anchors.CenterRight), GuiContainer.Layer.menu, new GuiColor("white"), 1, 1);
                container3.addPlainPanel("br", new Rectangle(1800, 960, 100, 100, 1920, 1080, true, Rectangle.Anchors.BottomRight), GuiContainer.Layer.menu, new GuiColor("white"), 1, 1);
                container3.addPlainPanel("bc", new Rectangle(910, 960, 100, 100, 1920, 1080, true, Rectangle.Anchors.BottomCenter), GuiContainer.Layer.menu, new GuiColor("white"), 1, 1);
                container3.display(player);

                customGameTip(player, "This is a custom gametip!", 5);
                return;
            }
            else if (args.Length >= 3)
            {
                if (args[0] == "gametip") customGameTip(player, args[2], 3f, (gametipType)Enum.Parse(typeof(gametipType), args[1]));
            }
        }
        
        private void imgPreviewCommand(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, "gui.demo"))
            {
                PrintToChat(player, lang.GetMessage("noPermission", this, player.UserIDString));
                return;
            }
            imgPreview(player, args[0]);
        }

        public void imgPreview(BasePlayer player, string url)
        {
            Action callback = () =>
            {
                Rectangle rectangle = new Rectangle(710, 290, 500, 500, 1920, 1080, true);
                GuiContainer container = new GuiContainer(PluginInstance, $"imgPreview");
                container.addRawImage($"img", rectangle, ImageLibrary.Call<string>("GetImage", $"GUICreator_preview_{url}"), "Hud");
                container.addPlainButton("close", new Rectangle(0.15f, 0.15f, 0.1f, 0.1f), new GuiColor(1, 0, 0, 0.8f), 0, 0, new GuiText("close"));
                container.display(player);
            };
            if (ImageLibrary.Call<bool>("HasImage", $"GUICreator_preview_{url}", (ulong)0))
            {
                callback();
            }
            else ImageLibrary.Call<bool>("AddImage", url, $"GUICreator_preview_{url}", (ulong)0, callback);

        }
        
        private void imgrawPreviewCommand(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, "gui.demo"))
            {
                PrintToChat(player, lang.GetMessage("noPermission", this, player.UserIDString));
                return;
            }
            GuiContainer container = new GuiContainer(this, "imgPreview");

            container.addRawImage("img", new Rectangle(710, 290, 500, 500, 1920, 1080, true), args[0], GUICreator.GuiContainer.Layer.hud);
            container.addPlainButton("close", new Rectangle(0.15f, 0.15f, 0.1f, 0.1f), new GuiColor(1, 0, 0, 0.8f), 0, 0, new GuiText("close"));
            container.display(player);
        }
    }
}﻿namespace Oxide.Plugins
{
    public partial class GUICreator
    {
        private static ConfigData config;

        private class ConfigData
        {
            public string steamAPIKey;
            public string gameTipIcon;
        }

        private ConfigData GetDefaultConfig()
        {
            return new ConfigData
            {
                steamAPIKey = "",
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
            PrintError("Configuration file is corrupt(or doesn't exist), creating new one!");
            config = GetDefaultConfig();
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(config);
        }
    }
}﻿namespace Oxide.Plugins
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public partial class GUICreator
    {
        private class DownloadManager
        {
            private Queue<Request> Requests = new Queue<Request>();
            private bool loading = false;

            public void Enqueue(Request request)
            {
                Requests.Enqueue(request);
                if(!loading)
                {
                    ServerMgr.Instance.StartCoroutine(Process());
                }
            }

            private IEnumerator Process()
            {
                if(loading || Requests.Count == 0)
                {
                    yield break;
                }
                loading = true;

                Request request = Requests.Dequeue();
                request.ProcessDownload(() =>
                {
                    loading = false;
                    ServerMgr.Instance.StartCoroutine(Process());
                });
            }

            public class Request
            {
                public string SafeName { get; set; }
                public string Url { get; set; }
                public int ImgSizeX { get; set; }
                public int ImgSizeY { get; set; }
                public Action Callback { get; set; }

                public void ProcessDownload(Action finishCallback)
                {
                    var il = new GameObject("WebObject").AddComponent<ImageLoader>();
                    il.StartCoroutine(il.DownloadImage(Url, (b) =>
                    {
                        PluginInstance.ImageLibrary.Call("AddImageData", SafeName, b, (ulong)0, Callback);
                        finishCallback();
#if CoroutineDEBUG
                    PluginInstance.Puts($"completed processing image download: {SafeName}");
#endif

                    }, ImgSizeX, ImgSizeY, finishCallback));
                }
            }
        }            
    }
}﻿namespace Oxide.Plugins
{
    using Oxide.Game.Rust.Cui;

    public partial class GUICreator
    {
        public class ExtendedCuiElementContainer : CuiElementContainer
        {
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
        }
    }
}﻿namespace Oxide.Plugins
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
                    if (e.Name == name)
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

            public List<GuiElement> addInput(string name, Rectangle rectangle, Action<BasePlayer, string[]> callback, Layer layer, string close = null, GuiColor panelColor = null, int charLimit = 100, GuiText text = null, float FadeIn = 0, float FadeOut = 0, bool isPassword = false, bool CursorEnabled = true, string imgName = null, Blur blur = Blur.none)
            {
                return addInput(name, rectangle, callback, null, layer, close, panelColor, charLimit, text, FadeIn, FadeOut, isPassword, CursorEnabled, imgName, blur);
            }

            public List<GuiElement> addInput(string name, Rectangle rectangle, Action<BasePlayer, string[]> callback, string parent = "Hud", string close = null, GuiColor panelColor = null, int charLimit = 100, GuiText text = null, float FadeIn = 0, float FadeOut = 0, bool isPassword = false, bool CursorEnabled = true, string imgName = null, Blur blur = Blur.none)
            {
                return addInput(name, rectangle, callback, GetParent(parent), Layer.hud, close, panelColor, charLimit, text, FadeIn, FadeOut, isPassword, CursorEnabled, imgName, blur);
            }

            public List<GuiElement> addInput(string name, Rectangle rectangle, Action<BasePlayer, string[]> callback, GuiElement parent, Layer layer, string close = null, GuiColor panelColor = null, int charLimit = 100, GuiText text = null, float FadeIn = 0, float FadeOut = 0, bool isPassword = false, bool CursorEnabled = true, string imgName = null, Blur blur = Blur.none)
            {
                if (string.IsNullOrEmpty(name)) name = "input";

                purgeDuplicates(name);

                List<GuiElement> input = GuiInputField.GetNewGuiInputField(
                    plugin, 
                    this, 
                    name, 
                    rectangle, 
                    callback, 
                    parent, 
                    layer, 
                    close, 
                    panelColor, 
                    charLimit, 
                    text, 
                    FadeIn, 
                    FadeOut, 
                    isPassword, 
                    CursorEnabled, 
                    imgName, 
                    blur);

                if (callback != null) this.registerCallback(name, callback);

                AddRange(input);

                return input;
            }
        }
    }
}﻿namespace Oxide.Plugins
{
    using Oxide.Core.Plugins;
    using Oxide.Game.Rust.Cui;
    using System.Collections.Generic;
    using UnityEngine;

    public partial class GUICreator
    {
        public class GuiTracker : MonoBehaviour
        {
            private BasePlayer player;
            public List<GuiContainer> activeGuiContainers = new List<GuiContainer>();

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
                name = removeWhiteSpaces(name);
                foreach (GuiContainer container in activeGuiContainers)
                {
                    if (container.plugin != plugin) continue;
                    if (container.name == name) return container;
                }
                return null;
            }

            public void addGuiToTracker(Plugin plugin, GuiContainer container)
            {
#if DEBUG
                player.ChatMessage($"adding {container.name} to tracker");
#endif
                if (getContainer(plugin, container.name) != null) destroyGui(plugin, container.name);
                activeGuiContainers.Add(container);
            }

            public void destroyGui(Plugin plugin, string containerName, string name = null)
            {
                if (name != null) name = removeWhiteSpaces(name);
                destroyGui(plugin, getContainer(plugin, removeWhiteSpaces(containerName)), name);
            }

            public void destroyGui(Plugin plugin, GuiContainer container, string name = null)
            {
                if (container == null) return;
                if (name == null)
                {
                    List<GuiContainer> garbage = new List<GuiContainer>();
                    destroyGuiContainer(plugin, container, garbage);
                    foreach (GuiContainer cont in garbage)
                    {
                        activeGuiContainers.Remove(cont);
                    }
                }
                else
                {
                    name = removeWhiteSpaces(name);
                    name = PluginInstance.prependContainerName(container, name);
                    List<GuiElement> eGarbage = new List<GuiElement>();
                    destroyGuiElement(plugin, container, name, eGarbage);
                    foreach (GuiElement element in eGarbage)
                    {
                        container.Remove(element);
                    }
                }
            }

            private void destroyGuiContainer(Plugin plugin, GuiContainer container, List<GuiContainer> garbage)
            {
#if DEBUG
                player.ChatMessage($"destroyGuiContainer: start {plugin.Name} {container.name}");
#endif
                if (activeGuiContainers.Contains(container))
                {
                    foreach (GuiContainer cont in activeGuiContainers)
                    {
                        if (cont.plugin != container.plugin) continue;
                        if (cont.parent == container.name) destroyGuiContainer(cont.plugin, cont, garbage);
                    }
                    container.closeCallback?.Invoke(player);
                    List<GuiElement> eGarbage = new List<GuiElement>();
                    foreach (GuiElement element in container)
                    {
                        destroyGuiElement(plugin, container, element.Name, eGarbage);
                    }
                    foreach (GuiElement element in eGarbage)
                    {
                        container.Remove(element);
                    }
                    foreach (Timer timer in container.timers)
                    {
                        timer.Destroy();
                    }
                    garbage.Add(container);
                }
                else PluginInstance.Puts($"destroyGui(container.name: {container.name}): no GUI containers found");
            }

            private void destroyGuiElement(Plugin plugin, GuiContainer container, string name, List<GuiElement> garbage)
            {
                name = removeWhiteSpaces(name);
#if DEBUG
                player.ChatMessage($"destroyGui: {plugin.Name} {name}");
#endif
                if (container == null) return;
                if (container.plugin != plugin) return;
                if (string.IsNullOrEmpty(name)) return;
                GuiElement target = null;
                foreach (GuiElement element in container)
                {
                    if (element.Parent == name || element.ParentElement?.Name == name)
                    {
                        CuiHelper.DestroyUi(player, element.Name);
                        garbage.Add(element);
                    }
                    if (element.Name == name) target = element;
                }
                if (target == null) return;
                CuiHelper.DestroyUi(player, target.Name);
                garbage.Add(target);


            }

            public void destroyAllGui(Plugin plugin)
            {
                foreach (GuiContainer container in activeGuiContainers)
                {
                    if (container.plugin != plugin) continue;
                    foreach (Timer timer in container.timers)
                    {
                        timer.Destroy();
                    }
                    foreach (GuiElement element in container)
                    {
                        CuiHelper.DestroyUi(player, element.Name);
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
                    foreach (GuiElement element in container)
                    {
                        CuiHelper.DestroyUi(player, element.Name);
                    }
                }
                Destroy(this);
            }
        }
    }
}﻿namespace Oxide.Plugins
{
    using Oxide.Core.Plugins;
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    public partial class GUICreator
    {
        public int getFontsizeByFramesize(float length, Rectangle rectangle)
        {
            double W = rectangle.W / rectangle.resX;
            double H = rectangle.H / rectangle.resY;
            double refH = 100f / 1080f;

            double maxSize = 55f * Math.Pow((H / refH), 0.98f);
            double maxLengthAtMaxSize = W * (3.911f / H);
            if (length <= maxLengthAtMaxSize) return (int)maxSize;
            return (int)Math.Floor((maxSize * (maxLengthAtMaxSize / length)));
        }

        public List<List<T>> SplitIntoChunks<T>(List<T> list, int chunkSize = 30)
        {
            if (chunkSize <= 0)
            {
                return null;
            }

            List<List<T>> retVal = new List<List<T>>();
            int index = 0;
            while (index < list.Count)
            {
                int count = list.Count - index > chunkSize ? chunkSize : list.Count - index;
                retVal.Add(list.GetRange(index, count));

                index += chunkSize;
            }

            return retVal;
        }

        private string prependContainerName(GuiContainer container, string name)
        {
            if (GuiContainer.layers.Contains(name)) return name;
            return $"{container.name}_{removeWhiteSpaces(name)}";
        }

        public string getItemIcon(string shortname)
        {
            ItemDefinition itemDefinition = ItemManager.FindItemDefinition(shortname);
            if (itemDefinition != null) return ImageLibrary.Call<string>("GetImage", shortname);
            else return "";
        }

        private string getImageData(Plugin plugin, string name)
        {
            return (string)PluginInstance.ImageLibrary.Call("GetImage", $"{plugin.Name}_{name}");
        }

        private static string removeWhiteSpaces(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            return Regex.Replace(name, " ", "_");
        }
    }
}﻿namespace Oxide.Plugins
{
    public partial class GUICreator
    {
        private void Init()
        {
            permission.RegisterPermission("gui.demo", this);

            InitLocalization();
            InitCommands();
        }

        private void OnServerInitialized()
        {
            if (ImageLibrary == null)
            {
                Puts("ImageLibrary is not loaded! get it here https://umod.org/plugins/image-library");
                return;
            }

            _DownloadManager = new DownloadManager();

            registerImage(this, "flower", "https://i.imgur.com/uAhjMNd.jpg");
            registerImage(this, "gameTipIcon", config.gameTipIcon);
            registerImage(this, "warning_alpha", "https://i.imgur.com/u0bNKXx.png");
            registerImage(this, "white_cross", "https://i.imgur.com/fbwkYDj.png");
            registerImage(this, "triangle_up", "https://i.imgur.com/Boa8nZf.png");
            registerImage(this, "triangle_down", "https://i.imgur.com/CaQOAjm.png");
            registerImage(this, "circle", "https://i.imgur.com/hUeA7Lc.png");
        }

        private void Unload()
        {
            foreach (BasePlayer player in BasePlayer.activePlayerList)
            {
                GuiTracker.getGuiTracker(player).destroyAllGui();
            }

            PluginInstance = null;
            GuiContainer.blurs = null;
            GuiContainer.layers = null;
            _DownloadManager = null;
        }

        private void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            GuiTracker.getGuiTracker(player).destroyAllGui();
        }
    }
}﻿namespace Oxide.Plugins
{
    using System;
    using System.Collections;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Networking;

    public partial class GUICreator
    {
        public class ImageLoader : MonoBehaviour
        {

            public IEnumerator DownloadImage(string url, Action<byte[]> callback, int? sizeX = null, int? sizeY = null, Action ErrorCallback = null)
            {
                UnityWebRequest www = UnityWebRequest.Get(url);

                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    PluginInstance.Puts(string.Format("Image failed to download! Error: {0}, Image URL: {1}", www.error, url));
                    www.Dispose();
                    ErrorCallback?.Invoke();
                    yield break;
                }

                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(www.downloadHandler.data);

                byte[] originalBytes = null;
                byte[] resizedBytes = null;

                if (texture != null)
                {
                    originalBytes = texture.EncodeToPNG();
                }
                else
                {
                    ErrorCallback?.Invoke();
                }

                if (sizeX != null && sizeY != null)
                {
                    resizedBytes = Resize(originalBytes, sizeX.Value, sizeY.Value, sizeX.Value, sizeY.Value, true);
                }

                if(originalBytes.Length <= resizedBytes.Length)
                {
                    callback(originalBytes);
                }
                else
                {
                    callback(resizedBytes);
                }

                www.Dispose();
            }

            //public static byte[] Resize(byte[] bytes, int sizeX, int sizeY)
            //{
            //    Image img = (Bitmap)(new ImageConverter().ConvertFrom(bytes));
            //    Bitmap cutPiece = new Bitmap(sizeX, sizeY);
            //    System.Drawing.Graphics graphic = System.Drawing.Graphics.FromImage(cutPiece);
            //    graphic.DrawImage(img, new System.Drawing.Rectangle(0, 0, sizeX, sizeY), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);
            //    graphic.Dispose();
            //    MemoryStream ms = new MemoryStream();
            //    cutPiece.Save(ms, ImageFormat.Jpeg);
            //    return ms.ToArray();
            //}

            public static byte[] Resize(byte[] bytes, int width, int height, int targetWidth, int targetHeight, bool enforceJpeg, RotateFlipType rotation = RotateFlipType.RotateNoneFlipNone)
            {
                byte[] resizedImageBytes;

                using (MemoryStream originalBytesStream = new MemoryStream(), resizedBytesStream = new MemoryStream())
                {
                    // Write the downloaded image bytes array to the memorystream and create a new Bitmap from it.
                    originalBytesStream.Write(bytes, 0, bytes.Length);
                    Bitmap image = new Bitmap(originalBytesStream);

                    if (rotation != RotateFlipType.RotateNoneFlipNone)
                    {
                        image.RotateFlip(rotation);
                    }

                    // Check if the width and height match, if they don't we will have to resize this image.
                    if (image.Width != targetWidth || image.Height != targetHeight)
                    {
                        // Create a new Bitmap with the target size.
                        Bitmap resizedImage = new Bitmap(width, height);

                        // Draw the original image onto the new image and resize it accordingly.
                        using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(resizedImage))
                        {
                            graphics.DrawImage(image, new System.Drawing.Rectangle(0, 0, targetWidth, targetHeight));
                        }


                        // Save the bitmap to a MemoryStream as either Jpeg or Png.
                        if (enforceJpeg)
                        {
                            resizedImage.Save(resizedBytesStream, ImageFormat.Jpeg);
                        }
                        else
                        {
                            resizedImage.Save(resizedBytesStream, ImageFormat.Png);
                        }

                        // Grab the bytes array from the new image's MemoryStream and dispose of the resized image Bitmap.
                        resizedImageBytes = resizedBytesStream.ToArray();
                        resizedImage.Dispose();
                    }
                    else
                    {
                        // The image has the correct size so we can just return the original bytes without doing any resizing.
                        resizedImageBytes = bytes;
                    }

                    // Dispose of the original image Bitmap.
                    image.Dispose();
                }

                // Return the bytes array.
                return resizedImageBytes;
            }
        }
    }
}﻿namespace Oxide.Plugins
{
    using System.Collections.Generic;
    public partial class GUICreator
    {
        public void InitLocalization()
        {
            lang.RegisterMessages(messages, this);
        }

        private Dictionary<string, string> messages = new Dictionary<string, string>()
        {
            {"noPermission", "You don't have permission to use this command!"}
        };
    }
}﻿namespace Oxide.Plugins
{
    using Newtonsoft.Json;
    using Oxide.Core.Libraries;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    partial class GUICreator : RustPlugin
    {
        public class WebResponse
        {
            public SteamUser response;
        }
        
        public class SteamUser
        {
            public PlayerSummary[] players;
        }

        public class PlayerSummary
        {
            public string steamid;
            public string personaname;
            public string profileurl;
            public string avatarfull;
        }

        public void GetSteamUserData(List<ulong> steamIDs, Action<PlayerSummary[]> callback)
        {
            if(string.IsNullOrEmpty(config.steamAPIKey))
            {
                Puts(lang.GetMessage("apiKey", this));
                return;
            }
            string url = "https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key=" + config.steamAPIKey.ToString() + "&steamids=" + String.Join(",", steamIDs.Select(s => s.ToString()));
            webrequest.Enqueue(url, null, (code, response) =>
            {
                if (code != 200 || response == null)
                {
                    Puts($"Couldn't get an answer from Steam!");
                    return;
                }
                WebResponse webResponse = JsonConvert.DeserializeObject<WebResponse>(response);
                if (webResponse?.response?.players == null)
                {
                    Puts("response is null");
                    callback(null);
                    return;
                }
                if(webResponse.response.players.Length == 0)
                {
                    Puts("response has no playerSummaries");
                    callback(null);
                    return;
                }

                callback(webResponse.response.players);
            }, this, RequestMethod.GET);
        }
    }
}﻿namespace Oxide.Plugins
{
    using Oxide.Game.Rust.Cui;

    public partial class GUICreator
    {
        public class CuiInputField
        {
            //public CuiImageComponent Image { get; set; } = new CuiImageComponent();
            public CuiInputFieldComponent InputField { get; set; } = new CuiInputFieldComponent();

            public CuiRectTransformComponent RectTransform { get; set; } = new CuiRectTransformComponent();
            public bool CursorEnabled { get; set; }
            public float FadeOut { get; set; }
        }
    }
}﻿namespace Oxide.Plugins
{
    using System;
    using UnityEngine;

    public partial class GUICreator
    {
        public class GuiColor
        {
            public Color color;

            public GuiColor()
            {
                color = new Color(1, 1, 1, 1);
            }

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

            public GuiColor(double hue, double saturation, double value, float alpha)
            {
                while (hue < 0) { hue += 360; };
                while (hue >= 360) { hue -= 360; };
                double R, G, B;
                if (value <= 0)
                { R = G = B = 0; }
                else if (saturation <= 0)
                {
                    R = G = B = value;
                }
                else
                {
                    double hf = hue / 60.0;
                    int i = (int)Math.Floor(hf);
                    double f = hf - i;
                    double pv = value * (1 - saturation);
                    double qv = value * (1 - saturation * f);
                    double tv = value * (1 - saturation * (1 - f));
                    switch (i)
                    {

                        // Red is the dominant color

                        case 0:
                            R = value;
                            G = tv;
                            B = pv;
                            break;

                        // Green is the dominant color

                        case 1:
                            R = qv;
                            G = value;
                            B = pv;
                            break;
                        case 2:
                            R = pv;
                            G = value;
                            B = tv;
                            break;

                        // Blue is the dominant color

                        case 3:
                            R = pv;
                            G = qv;
                            B = value;
                            break;
                        case 4:
                            R = tv;
                            G = pv;
                            B = value;
                            break;

                        // Red is the dominant color

                        case 5:
                            R = value;
                            G = pv;
                            B = qv;
                            break;

                        // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

                        case 6:
                            R = value;
                            G = tv;
                            B = pv;
                            break;
                        case -1:
                            R = value;
                            G = pv;
                            B = qv;
                            break;

                        // The color is not defined, we should throw an error.

                        default:
                            R = G = B = value; // Just pretend its black/white
                            break;
                    }
                }
                color.r = (float)R;
                color.g = (float)G;
                color.b = (float)B;
                color.a = alpha;
            }

            public void setAlpha(float alpha)
            {
                this.color.a = alpha;
            }

            public GuiColor withAlpha(float alpha)
            {
                this.color.a = alpha;
                return this;
            }

            public string getColorString()
            {
                return $"{color.r} {color.g} {color.b} {color.a}";
            }

            public string ToHex()
            {
                return "#" + ColorUtility.ToHtmlStringRGBA(color);
            }

            public static GuiColor Transparent => new GuiColor(0, 0, 0, 0);
            public static GuiColor White => new GuiColor(1, 1, 1, 1);
            public static GuiColor Black => new GuiColor(0, 0, 0, 1);
            public static GuiColor Red => new GuiColor(1, 0, 0, 1);
            public static GuiColor Green => new GuiColor(0, 1, 0, 1);
            public static GuiColor Blue => new GuiColor(0, 0, 1, 1);

        }
    }
}﻿namespace Oxide.Plugins
{
    using Oxide.Game.Rust.Cui;
    using UnityEngine;

    public partial class GUICreator
    {
        public class GuiText : CuiTextComponent
        {
            public GuiText()
            {
            }

            public GuiText(string text, int fontSize = 14, GuiColor color = null, TextAnchor align = TextAnchor.MiddleCenter, float FadeIn = 0)
            {
                this.Text = text;
                this.FontSize = fontSize;
                this.Align = align;
                this.Color = color?.getColorString() ?? new GuiColor(0, 0, 0, 1).getColorString();
                this.FadeIn = FadeIn;
            }
        }
    }
}﻿namespace Oxide.Plugins
{
    using Newtonsoft.Json;
    using Oxide.Game.Rust.Cui;
    using UnityEngine;

    public partial class GUICreator
    {
        [JsonObject(MemberSerialization.OptIn)]
        public class Rectangle : CuiRectTransformComponent
        {
            public enum Anchors { BottomLeft = 0, BottomCenter, BottomRight, CenterLeft, Center, CenterRight, UpperLeft, UpperCenter, UpperRight }

            double[,] AnchorData = new double[9, 4]
            {
                {0, 0, 0, 0 },
                {0.5d, 0, 0.5d, 0 },
                {1, 0, 1, 0 },
                {0, 0.5d, 0, 0.5d },
                {0.5d, 0.5d, 0.5d, 0.5d },
                {1, 0.5d, 1, 0.5d },
                {0, 1, 0, 1 },
                {0.5d, 1, 0.5d, 1 },
                {1, 1, 1, 1 }
            };

            public double anchorMinX;
            public double anchorMinY;
            public double anchorMaxX;
            public double anchorMaxY;

            public double fractionalMinX;
            public double fractionalMinY;
            public double fractionalMaxX;
            public double fractionalMaxY;

            public double offsetMinX;
            public double offsetMinY;
            public double offsetMaxX;
            public double offsetMaxY;

            public double X;
            public double Y;
            public double W;
            public double H;

            public double resX;
            public double resY;

            public bool topLeftOrigin;

            public Anchors Anchor;

            public Rectangle()
            {
                AnchorMin = "0, 0";
                AnchorMax = "1, 1";

            }

            public Rectangle(double X, double Y, double W, double H, double resX = 1, double resY = 1, bool topLeftOrigin = false, Anchors anchor = Anchors.Center)
            {
                anchorMinX = AnchorData[(int)anchor, 0];
                anchorMinY = AnchorData[(int)anchor, 1];
                anchorMaxX = AnchorData[(int)anchor, 2];
                anchorMaxY = AnchorData[(int)anchor, 3];

                AnchorMin = $"{anchorMinX} {anchorMinY}";
                AnchorMax = $"{anchorMaxX} {anchorMaxY}";

                this.X = X;
                this.Y = Y;
                this.W = W;
                this.H = H;

                this.resX = resX;
                this.resY = resY;

                this.topLeftOrigin = topLeftOrigin;

                Anchor = anchor;

                double newY = topLeftOrigin ? resY - Y - H : Y;

                fractionalMinX = X / resX;
                fractionalMinY = newY / resY;
                fractionalMaxX = (X + W) / resX;
                fractionalMaxY = (newY + H) / resY;
                //PluginInstance.PrintToChat($"{newY} + {H} / {resY} = {fractionalMaxY}");
                //PluginInstance.PrintToChat($"{fractionalMinX} {fractionalMinY} : {fractionalMaxX} {fractionalMaxY}");
                offsetMinX = -(anchorMinX - fractionalMinX) * offsetResX;
                offsetMinY = -(anchorMinY - fractionalMinY) * offsetResY;
                offsetMaxX = -(anchorMaxX - fractionalMaxX) * offsetResX;
                offsetMaxY = -(anchorMaxY - fractionalMaxY) * offsetResY;
                //PluginInstance.PrintToChat($"-({0.5d} - {fractionalMaxY} * {offsetResY} = {offsetMaxY}");

                OffsetMin = $"{offsetMinX} {offsetMinY}";
                OffsetMax = $"{offsetMaxX} {offsetMaxY}";
            }
        
            public Rectangle WithParent(Rectangle rectangle)
            {
                if (rectangle == null) return this;
                return new Rectangle(
                    ((X/resX)*rectangle.W) + rectangle.X,
                    ((Y / resY) * rectangle.H) + rectangle.Y + ((!topLeftOrigin && rectangle.topLeftOrigin) ? ((H / resY) * rectangle.H) : 0),
                    (W/resX) * rectangle.W,
                    (H/resY) * rectangle.H,
                    rectangle.resX,
                    rectangle.resY,
                    rectangle.topLeftOrigin,
                    Anchor
                    );;
            }
        }
    }
}﻿namespace Oxide.Plugins
{
    using Newtonsoft.Json;
    using Oxide.Game.Rust.Cui;

    public partial class GUICreator
    {
        public class GuiElement : CuiElement
        {
            [JsonIgnore]
            public Rectangle Rectangle { get; set; }

            [JsonIgnore]
            public GuiElement ParentElement { get; set; }

            [JsonIgnore]
            public GuiContainer.Layer Layer { get; set; }

            public GuiElement() { }

            public GuiElement(Rectangle rectangle, GuiElement parent = null, GuiContainer.Layer layer = GuiContainer.Layer.hud)
            {
                Rectangle = rectangle.WithParent(parent?.Rectangle);
                ParentElement = parent;
                Layer = layer;
                Parent = GuiContainer.layers[(int)layer];
            }
        }
    }
}﻿namespace Oxide.Plugins
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
}﻿namespace Oxide.Plugins
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
}﻿namespace Oxide.Plugins
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
}﻿namespace Oxide.Plugins
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
}﻿namespace Oxide.Plugins
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
}﻿namespace Oxide.Plugins
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