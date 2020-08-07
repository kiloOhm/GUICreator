//#define DEBUG

using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("GUICreator", "kOhm", "1.3.1")]
    [Description("API Plugin for centralized GUI creation and management")]
    public partial class GUICreator : RustPlugin
    {
        [PluginReference]
        private Plugin ImageLibrary;

        private static GUICreator PluginInstance = null;

        public GUICreator()
        {
            PluginInstance = this;
        }

        public const int offsetResX = 1280;
        public const int offsetResY = 720;
    }
}