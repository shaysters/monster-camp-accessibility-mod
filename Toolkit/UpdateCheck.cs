using System;
using System.Text.RegularExpressions;
using GameMod.Toolkit.Localization;
using UnityEngine;
using UnityEngine.Networking;

namespace GameMod.Toolkit
{
    // Once per game start, asks GitHub for the latest release and says so if it's newer than the
    // installed mod. Reads the release manifest the installer itself uses
    // (releases/latest/download/loadstone-release.json: a plain redirect, no API token or rate limit).
    // Nothing is sent except the request itself, and a failed check is only logged.
    internal static class UpdateCheck
    {
        private const string ManifestUrl = "https://github.com/shaysters/monster-camp-accessibility-mod/releases/latest/download/loadstone-release.json";
        private static readonly Regex VersionField = new Regex("\"version\"\\s*:\\s*\"v?([0-9.]+)\"");


        private static UnityWebRequest _request;
        private static string _newerVersion;
        private static bool _started;
        private static bool _done;

        // Called every frame from ModMain.OnUpdate.
        public static void Tick()
        {
            if (_done || HookContext.Speech == null)
            {
                return;
            }

            if (_newerVersion != null)
            {
                if (!UpdateNotice.Ready(HookContext.Speech.SpeakCount, Time.unscaledTime - HookContext.Speech.LastSpeakTime))
                {
                    return;
                }

                _done = true;
                // Spoken through the router, so R repeats it. Stats, not System, so it doesn't
                // hold back the next focus announcement.
                HookContext.Speech.Speak(ModLocalization.Format("update_available_fmt", _newerVersion), SpeechCategory.Stats, "update-check", false);
                return;
            }

            if (!_started)
            {
                _started = true;
                _request = UnityWebRequest.Get(ManifestUrl);
                _request.timeout = 15;
                _request.SendWebRequest();
                return;
            }

            if (_request == null || !_request.isDone)
            {
                return;
            }

            try
            {
                if (_request.isNetworkError || _request.isHttpError)
                {
                    HookContext.Log?.Info("UPDATE", "CHECK", "Update check skipped.", ("error", _request.error));
                    _done = true;
                    return;
                }

                var m = VersionField.Match(_request.downloadHandler.text);
                var latest = m.Success ? Parse(m.Groups[1].Value) : null;
                var installed = Parse(ModMain.Version);
                HookContext.Log?.Info("UPDATE", "CHECK", "Update check.", ("installed", ModMain.Version), ("latest", m.Success ? m.Groups[1].Value : "unknown"));
                if (latest != null && installed != null && latest > installed)
                {
                    _newerVersion = m.Groups[1].Value;
                }
                else
                {
                    _done = true;
                }
            }
            finally
            {
                _request.Dispose();
                _request = null;
            }
        }

        private static Version Parse(string s)
        {
            try
            {
                return new Version(s);
            }
            catch
            {
                return null;
            }
        }
    }

    // When the update notice may be spoken: after the title menu's first line has been read
    // and the mod has then been quiet for a moment, so it neither cuts off the start-up speech
    // nor gets cut off by it. If the menu never speaks by itself, a longer silence will do.
    internal static class UpdateNotice
    {
        // Lines the mod speaks by itself before the title menu: the "Accessibility loaded" message.
        private const int StartupLines = 1;
        private const float QuietSeconds = 2f;
        private const float FallbackQuietSeconds = 30f;

        internal static bool Ready(int spokenLines, float secondsQuiet)
        {
            if (secondsQuiet < QuietSeconds)
            {
                return false;
            }

            return spokenLines > StartupLines
                || (secondsQuiet >= FallbackQuietSeconds && Time.realtimeSinceStartup >= 30f);
        }
    }
}
