namespace Oxide.Plugins
{
    using Oxide.Core.Plugins;
    using System;
    using System.Collections.Generic;
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

        public void prompt(BasePlayer player, string message, string header)
        {
            Action<BasePlayer, string[]> closeCallback = (bPlayer, input) =>
            {
                GuiTracker.getGuiTracker(player).destroyGui(PluginInstance, "gametip");
            };
            GuiContainer containerGUI = new GuiContainer(this, "prompt");
            containerGUI.addPlainPanel("background", new Rectangle(700, 377, 520, 243, 1920, 1080, true), GuiContainer.Layer.overall, new GuiColor(0, 0, 0, 0.6f), 0.1f, 0.1f, GuiContainer.Blur.medium);
            containerGUI.addPanel("msg", new Rectangle(755, 469, 394, 56, 1920, 1080, true), GuiContainer.Layer.overall, new GuiColor(0, 0, 0, 0), 0.1f, 0.1f, new GuiText(message, 10, new GuiColor(255, 255, 255, 0.8f)));
            containerGUI.addPanel("header", new Rectangle(800, 404, 318, 56, 1920, 1080, true), GuiContainer.Layer.overall, new GuiColor(0, 0, 0, 0), 0.1f, 0.1f, new GuiText(header, 25, new GuiColor(255, 255, 255, 0.8f)));
            containerGUI.addPlainButton("close", new Rectangle(802, 536, 318, 56, 1920, 1080, true), GuiContainer.Layer.overall, new GuiColor(65, 33, 32, 0.8f), 0.1f, 0.1f, new GuiText("CLOSE", 20, new GuiColor(162, 51, 46, 0.8f)), closeCallback);
            containerGUI.display(player);
        }

        public void dropdown(BasePlayer player, List<string> options, Rectangle rectangle, string parent, Action<string> callback, bool allowNew = false, int page = 0, Predicate<string> predicate = null)
        {
            if (allowNew) options.Add("(add new)");
            int maxItems = 5;
            List<List<string>> ListOfLists = SplitIntoChunks<string>(options, maxItems);
            GuiContainer container = new GuiContainer(this, "dropdown", parent);
            container.addPlainPanel("dropdown_background", rectangle, GuiContainer.Layer.menu, new GuiColor(0, 0, 0, 0.6f), 0, 0, GuiContainer.Blur.medium);

            double cfX = rectangle.W / 300;
            double cfY = rectangle.H / 570;

            Action<BasePlayer, string[]> up = (bPlayer, input) =>
            {
                dropdown(player, options, rectangle, parent, callback, allowNew, page - 1, predicate);
            };
            Action<BasePlayer, string[]> down = (bPlayer, input) =>
            {
                dropdown(player, options, rectangle, parent, callback, allowNew, page + 1, predicate);
            };
            if (page > 0) container.addPlainButton("dropdown_up", new Rectangle(0, 1, 298, 36, 300, 570, true), new GuiColor(1, 1, 1, 0.4f), 0, 0, new GuiText("<b>∧</b>", (int)Math.Floor(22 * cfY), new GuiColor("black")), up, parent: "dropdown_background");
            if (page < ListOfLists.Count - 1) container.addPlainButton("dropdown_up", new Rectangle(0, 533, 298, 37, 300, 570, true), new GuiColor(1, 1, 1, 0.4f), 0, 0, new GuiText("<b>∨</b>", (int)Math.Floor(22 * cfY), new GuiColor("black")), down, parent: "dropdown_background");

            int count = 0;
            foreach (string option in ListOfLists[page])
            {
                Rectangle pos = new Rectangle(10, 40 + count * 100, 280, 90, 300, 570, true); ;
                Rectangle absPos = new Rectangle(rectangle.X + (pos.X * cfX), rectangle.Y + (pos.Y * cfY), pos.W * cfX, pos.H * cfY, 1920, 1080, true);

                Action<BasePlayer, string[]> btnCallback = null;
                if (option == "(add new)")
                {
                    btnCallback = (bPlayer, input) => dropdownAddNew(player, absPos, callback, predicate);
                }
                else
                {
                    string selected = option;
                    btnCallback = (bPlayer, input) =>
                    {
                        callback(selected);
                    };
                }
                container.addPlainButton($"dropdown_option_{option}", pos, new GuiColor(0, 0, 0, 0.7f), 0, 0, new GuiText(option, color: new GuiColor(1, 1, 1, 0.5f)), btnCallback, parent: "dropdown_background");
                count++;
            }

            container.display(player);
        }

        public void dropdownAddNew(BasePlayer player, Rectangle rectangle, Action<string> callback, Predicate<string> predicate)
        {
            GuiContainer container = new GuiContainer(this, "dropdown_addNew", "dropdown");
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
                        dropdownAddNew(player, rectangle, callback, predicate);
                        return;
                    }
                }
                callback(newName.ToString());
                GuiTracker.getGuiTracker(player).destroyGui(PluginInstance, container);
            };

            container.addInput("dropdown_addNew_input", rectangle, inputCallback, GuiContainer.Layer.menu, null, new GuiColor("white"), 15, new GuiText("", color: new GuiColor("black")), 0, 0);
            container.display(player);
        }

        public void registerImage(Plugin plugin, string name, string url, Action callback = null)
        {
            string safeName = $"{plugin.Name}_{name}";

            if (ImageLibrary.Call<bool>("HasImage", safeName))
            {
                callback?.Invoke();
            }
            else ImageLibrary.Call("AddImage", url, safeName, (ulong)0, callback);
#if DEBUG
            PrintToChat($"{plugin.Name} registered {name} image");
#endif
        }
    }
}