using System;
using MelonLoader;
using GameMod.Toolkit.Localization;

namespace GameMod.Toolkit
{
    internal sealed class ToolkitBootstrap : IDisposable
    {
        private readonly StructuredLog _log;
        private readonly BuildTargetAdapter _buildTarget;
        private readonly SpeechRouter _speech;
        private readonly HookInstaller _hooks;
        private readonly StatsReader _stats;

        public ToolkitBootstrap(MelonLogger.Instance logger)
        {
            _log = new StructuredLog(logger);
            _buildTarget = new BuildTargetAdapter(_log);
            _speech = new SpeechRouter(_log, new SpeechBridge(_log));
            _hooks = new HookInstaller(_log);
            _stats = new StatsReader(_log);
        }

        public void Initialize()
        {
            _log.Info("BOOT", "INIT", "Toolkit bootstrap start.");

            var build = _buildTarget.Detect();
            HookContext.Initialize(_log, _speech, build);
            _log.Info(
                "BUILD",
                "DETECT",
                "Build target detected.",
                ("target", build.TargetName),
                ("assembly", build.AssemblyName),
                ("focusType", build.FocusTypeName),
                ("nameType", build.NameTypeName),
                ("eventType", build.EventTypeName),
                ("turnType", build.TurnTypeName),
                ("statsType", build.StatsTypeName));

            _speech.Initialize();
            _hooks.Install(build);

            _log.Info(
                "SPEECH",
                "INIT",
                "Speech bridge initialized.",
                ("provider", _speech.ActiveProviderName),
                ("available", _speech.IsAvailable.ToString()));

            _speech.Speak(ModLocalization.Get("accessibility_loaded"), SpeechCategory.System, "bootstrap", interrupt: true);
        }

        public void Dispose()
        {
            _hooks.Uninstall();
            _speech.Dispose();
            _log.Info("BOOT", "SHUTDOWN", "Toolkit bootstrap shutdown.");
        }

        public void RepeatLast()
        {
            _speech.RepeatLast();
        }

        public void SpeakCurrentStats()
        {
            var spoken = _stats.BuildCurrentPlayerStatsSpeech();
            _speech.Speak(spoken, SpeechCategory.Stats, "hotkey.stats", true);
        }
    }
}
