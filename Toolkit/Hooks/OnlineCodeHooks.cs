using System.Text;
using HarmonyLib;
using UnityEngine;
using GameMod.Toolkit.Localization;

namespace GameMod.Toolkit.Hooks
{
    // Online lobby match code.
    //
    // OnlineLobbyOnMatchView.SetMatchInfo() writes the room code into matchNameLabel.
    // It runs from OnEnable (before the Show announcement) and again whenever the
    // player list refreshes, so we only announce when the code changes and reset on Hide.
    // The announcement is delayed and queued so the lobby-opened announcement doesn't cut it off.
    // C key (ModMain.OnUpdate) re-reads the code while in the lobby.
    [HarmonyPatch(typeof(BeautifulGlitch.OnlineLobbyOnMatchView))]
    internal static class OnlineCodeHooks
    {
        private const float AnnounceDelay = 0.4f;

        private static BeautifulGlitch.OnlineLobbyOnMatchView _view;
        private static string _lastCode;
        private static string _pendingCode;
        private static float _pendingAt;

        [HarmonyPostfix]
        [HarmonyPatch("SetMatchInfo")]
        private static void AfterSetMatchInfo(BeautifulGlitch.OnlineLobbyOnMatchView __instance)
        {
            _view = __instance;
            var code = ReadCode();
            if (string.IsNullOrWhiteSpace(code) || code == _lastCode)
            {
                return;
            }

            _lastCode = code;
            _pendingCode = code;
            _pendingAt = Time.unscaledTime + AnnounceDelay;
            HookContext.Log?.Info("ONLINE", "CODE", "Online match code set.", ("code", code));
        }

        [HarmonyPostfix]
        [HarmonyPatch("Hide")]
        private static void AfterHide(BeautifulGlitch.OnlineLobbyOnMatchView __instance)
        {
            if (_view != __instance)
            {
                return;
            }

            _view = null;
            _lastCode = null;
            _pendingCode = null;
        }

        public static void Tick()
        {
            if (_pendingCode == null || Time.unscaledTime < _pendingAt)
            {
                return;
            }

            var code = _pendingCode;
            _pendingCode = null;
            HookContext.Speech?.Speak(ModLocalization.Format("online_code_announce_fmt", SpellOut(code)), SpeechCategory.Event, "OnlineLobbyOnMatchView.SetMatchInfo", false);
        }

        public static void SpeakCurrentCode()
        {
            var code = ReadCode();
            if (string.IsNullOrWhiteSpace(code))
            {
                HookContext.Speech?.Speak(ModLocalization.Get("online_code_none"), SpeechCategory.System, "hotkey:onlinecode", true);
                return;
            }

            _pendingCode = null;
            HookContext.Speech?.Speak(ModLocalization.Format("online_code_fmt", SpellOut(code)), SpeechCategory.System, "hotkey:onlinecode", true);
        }

        private static string ReadCode()
        {
            if (_view == null || _view.matchNameLabel == null || !_view.matchNameLabel.gameObject.activeInHierarchy)
            {
                return null;
            }

            return SpeechText.Normalize(_view.matchNameLabel.text);
        }

        // "48271" -> "4, 8, 2, 7, 1" so screen readers read digit by digit.
        private static string SpellOut(string code)
        {
            var sb = new StringBuilder();
            foreach (var c in code)
            {
                if (char.IsWhiteSpace(c))
                {
                    continue;
                }

                if (sb.Length > 0)
                {
                    sb.Append(", ");
                }

                sb.Append(c);
            }

            return sb.ToString();
        }
    }
}
