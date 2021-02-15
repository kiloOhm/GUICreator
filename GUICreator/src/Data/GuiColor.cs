namespace Oxide.Plugins
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
}