namespace GameMod.Toolkit
{
    internal sealed class HookInstaller
    {
        private readonly StructuredLog _log;
        private HarmonyLib.Harmony _harmony;

        public HookInstaller(StructuredLog log)
        {
            _log = log;
        }

        public void Install(BuildProbeResult build)
        {
            _harmony = new HarmonyLib.Harmony("codex.monstercamp.toolkit");

            if (build.TargetName == "full")
            {
                _harmony.PatchAll(typeof(Hooks.FullTitleMenuHooks).Assembly);
                _log.Info("FOCUS", "PATCH", "Installed full-build focus hooks.", ("target", build.TargetName));
                return;
            }

            _log.Info("FOCUS", "PATCH", "No focus hooks installed for current build.", ("target", build.TargetName));
        }

        public void Uninstall()
        {
            _harmony?.UnpatchSelf();
            _harmony = null;
        }
    }
}
