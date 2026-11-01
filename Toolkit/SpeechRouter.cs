using System;
using UnityEngine;

namespace GameMod.Toolkit
{
    internal sealed class SpeechRouter : IDisposable
    {
        private readonly StructuredLog _log;
        private readonly SpeechBridge _bridge;
        private readonly RepeatSystem _repeat;
        private string _lastText = string.Empty;
        private SpeechCategory _lastCategory;
        private float _lastSpeakTime = -100f;

        // Time.unscaledTime of the last thing spoken, so the update notice can wait for a pause.
        public float LastSpeakTime => _lastSpeakTime;

        // How many lines have been spoken since the game started.
        public int SpeakCount { get; private set; }
        private float _suppressFocusUntil = -100f;

        public SpeechRouter(StructuredLog log, SpeechBridge bridge)
        {
            _log = log;
            _bridge = bridge;
            _repeat = new RepeatSystem();
            Enabled = true;
        }

        public bool Enabled { get; set; }
        public bool IsAvailable => _bridge.IsAvailable;
        public string ActiveProviderName => _bridge.ActiveProviderName;

        public void Initialize()
        {
            _bridge.Initialize();
        }

        public void Speak(string text, SpeechCategory category, string sourceTag, bool interrupt)
        {
            if (!Enabled || string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            if (!Application.isFocused)
            {
                _log.Info("SPEECH", "SKIP", "Suppressed because game window is not focused.", ("source", sourceTag), ("category", category.ToString()));
                return;
            }

            var normalized = Normalize(text);
            if (category == SpeechCategory.Focus && Time.unscaledTime < _suppressFocusUntil)
            {
                _log.Info("SPEECH", "SKIP", "Suppressed focus during dialog grace window.", ("source", sourceTag), ("text", normalized));
                return;
            }

            if (IsDuplicate(normalized, category))
            {
                _log.Info("SPEECH", "SKIP", "Suppressed duplicate speech.", ("source", sourceTag), ("category", category.ToString()), ("text", normalized));
                return;
            }

            var message = new SpeechMessage(normalized, category, sourceTag, interrupt);

            _repeat.Remember(message);
            _lastText = normalized;
            _lastCategory = category;
            _lastSpeakTime = Time.unscaledTime;
            SpeakCount++;
            if (category == SpeechCategory.Event || category == SpeechCategory.Turn || category == SpeechCategory.System)
            {
                var graceSeconds =
                    string.Equals(sourceTag, "GameEventView.ShowOptionsScene", StringComparison.Ordinal) ||
                    string.Equals(sourceTag, "OptionTextBoxes.SetOption.Prompt", StringComparison.Ordinal) ||
                    string.Equals(sourceTag, "PlayerSelect.LiveFocusEntry", StringComparison.Ordinal)
                        ? (string.Equals(sourceTag, "PlayerSelect.LiveFocusEntry", StringComparison.Ordinal) ? 0.7f : 1.0f)
                        : 1.25f;
                _suppressFocusUntil = Time.unscaledTime + graceSeconds;
            }

            _log.Info("SPEECH", "ROUTE", "Speech routed.", ("source", sourceTag), ("category", category.ToString()), ("interrupt", interrupt.ToString()), ("text", normalized));
            _bridge.Speak(message);
        }

        public void RepeatLast()
        {
            if (!_repeat.TryGetLast(out var message))
            {
                _log.Info("SPEECH", "REPEAT", "No repeatable message cached.");
                return;
            }

            _log.Info("SPEECH", "REPEAT", "Repeating cached speech.", ("source", message.SourceTag), ("category", message.Category.ToString()), ("text", message.Text));
            _bridge.Speak(new SpeechMessage(message.Text, message.Category, "repeat", true));
        }

        public void Stop()
        {
            _log.Info("SPEECH", "STOP", "Stop requested.");
            _bridge.Stop();
        }

        public void Dispose()
        {
            _bridge.Dispose();
        }

        private static string Normalize(string text)
        {
            return text.Replace('\r', ' ').Replace('\n', ' ').Trim();
        }

        private bool IsDuplicate(string text, SpeechCategory category)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return true;
            }

            return string.Equals(text, _lastText, StringComparison.Ordinal)
                && category == _lastCategory
                && Time.unscaledTime - _lastSpeakTime <= 0.35f;
        }
    }
}
