namespace Oxide.Plugins
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
}