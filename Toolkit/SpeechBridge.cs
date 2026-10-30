using System;
using GameMod.Toolkit.Speech;

namespace GameMod.Toolkit
{
    internal sealed class SpeechBridge : IDisposable
    {
        private readonly StructuredLog _log;
        private readonly ISpeechProvider[] _providers;
        private ISpeechProvider _active;

        public SpeechBridge(StructuredLog log)
        {
            _log = log;
            _providers = new ISpeechProvider[]
            {
                new Speech.PrismSpeechProvider(log)
            };
        }

        public bool IsAvailable => _active != null;
        public string ActiveProviderName => _active?.Name ?? "none";

        public void Initialize()
        {
            foreach (var provider in _providers)
            {
                try
                {
                    if (provider.Initialize())
                    {
                        _active = provider;
                        _log.Info("SPEECH", "PROVIDER", "Speech provider selected.", ("provider", provider.Name));
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _log.Warn("SPEECH", "PROVIDER", "Speech provider initialization failed.", ("provider", provider.Name), ("error", ex.Message));
                }
            }

            _log.Warn("SPEECH", "PROVIDER", "No speech provider initialized.");
        }

        public void Speak(SpeechMessage message)
        {
            if (_active == null)
            {
                return;
            }

            _active.Speak(message.Text, message.Interrupt);
        }

        public void Stop()
        {
            _active?.Stop();
        }

        public void Dispose()
        {
            foreach (var provider in _providers)
            {
                try
                {
                    provider.Dispose();
                }
                catch
                {
                }
            }

            _active = null;
        }
    }
}
