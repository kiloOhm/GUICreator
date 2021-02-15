//#define DEBUG
//#define CoroutineDEBUG

using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("GUICreator", "kOhm", "1.5.0")]
    [Description("API Plugin for centralized GUI creation and management")]
    public partial class GUICreator : RustPlugin
    {
        [PluginReference]
        private Plugin ImageLibrary;

        private static GUICreator PluginInstance = null;

        private static DownloadManager _DownloadManager;

        public GUICreator()
        {
            PluginInstance = this;
        }

        public const int offsetResX = 1280;
        public const int offsetResY = 720;
    }
}