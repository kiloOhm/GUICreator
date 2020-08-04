namespace Oxide.Plugins
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
                    if (element.Parent == name)
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
}