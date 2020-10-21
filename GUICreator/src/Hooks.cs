namespace Oxide.Plugins
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
            registerImage(this, "flower", "https://i.imgur.com/uAhjMNd.jpg");
            registerImage(this, "gameTipIcon", config.gameTipIcon);
            registerImage(this, "warning_alpha", "https://i.imgur.com/u0bNKXx.png");
            registerImage(this, "white_cross", "https://i.imgur.com/fbwkYDj.png");
            registerImage(this, "triangle_up", "https://i.imgur.com/Boa8nZf.png");
            registerImage(this, "triangle_down", "https://i.imgur.com/CaQOAjm.png");
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
        }

        private void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            GuiTracker.getGuiTracker(player).destroyAllGui();
        }
    }
}