namespace Oxide.Plugins
{
    using Newtonsoft.Json;
    using Oxide.Core.Libraries;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    partial class GUICreator : RustPlugin
    {
        public class WebResponse
        {
            public SteamUser response;
        }
        
        public class SteamUser
        {
            public PlayerSummary[] players;
        }

        public class PlayerSummary
        {
            public string steamid;
            public string personaname;
            public string profileurl;
            public string avatarfull;
        }

        public void GetSteamUserData(List<ulong> steamIDs, Action<PlayerSummary[]> callback)
        {
            if(string.IsNullOrEmpty(config.steamAPIKey))
            {
                Puts(lang.GetMessage("apiKey", this));
                return;
            }
            string url = "https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key=" + config.steamAPIKey.ToString() + "&steamids=" + String.Join(",", steamIDs.Select(s => s.ToString()));
            webrequest.Enqueue(url, null, (code, response) =>
            {
                if (code != 200 || response == null)
                {
                    Puts($"Couldn't get an answer from Steam!");
                    return;
                }
                WebResponse webResponse = JsonConvert.DeserializeObject<WebResponse>(response);
                if (webResponse?.response?.players == null)
                {
                    Puts("response is null");
                    callback(null);
                    return;
                }
                if(webResponse.response.players.Length == 0)
                {
                    Puts("response has no playerSummaries");
                    callback(null);
                    return;
                }

                callback(webResponse.response.players);
            }, this, RequestMethod.GET);
        }
    }
}