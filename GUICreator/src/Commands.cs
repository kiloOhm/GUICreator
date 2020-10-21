namespace Oxide.Plugins
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
        }

        private void PlayerSearchCommand(BasePlayer player, string command, string[] args)
        {
            PlayerSearch(player, args[0], (p) => player.ChatMessage($"player selected: {p.displayName}"));
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
}