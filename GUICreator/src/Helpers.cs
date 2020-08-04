namespace Oxide.Plugins
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
}