using System;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(GameMod.ModMain), "Monster Camp Accessibility Mod", GameMod.ModMain.Version, "Codex and Shaysters")]
[assembly: MelonGame(null, null)]

namespace GameMod
{
    public sealed class ModMain : MelonMod
    {
        internal const string Version = "1.0.1";

        private Toolkit.ToolkitBootstrap _toolkit;

        public override void OnInitializeMelon()
        {
            _toolkit = new Toolkit.ToolkitBootstrap(LoggerInstance);
            _toolkit.Initialize();
        }

        public override void OnDeinitializeMelon()
        {
            _toolkit?.Dispose();
            _toolkit = null;
        }

        public override void OnUpdate()
        {
            if (_toolkit == null || !Application.isFocused)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                _toolkit.RepeatLast();
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                _toolkit.SpeakCurrentStats();
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                Toolkit.Hooks.OnlineCodeHooks.SpeakCurrentCode();
            }

            Toolkit.Hooks.OnlineCodeHooks.Tick();
            Toolkit.UpdateCheck.Tick();
        }
    }
}
