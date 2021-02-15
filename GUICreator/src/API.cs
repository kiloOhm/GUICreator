namespace Oxide.Plugins
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
}