namespace Oxide.Plugins
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
}