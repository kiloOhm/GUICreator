namespace Oxide.Plugins
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
}