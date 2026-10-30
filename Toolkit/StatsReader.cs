using System.Reflection;
using UnityEngine;
using GameMod.Toolkit.Localization;

namespace GameMod.Toolkit
{
    internal sealed class StatsReader
    {
        private readonly StructuredLog _log;
        private static readonly MethodInfo GetCurrentPlayerDataMethod = typeof(MonsterCamp.GameManager).GetMethod("GetCurrentPlayerData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly MethodInfo GetPrettyPrintNameMethod = typeof(BeautifulGlitch.PlayerData).GetMethod("GetPrettyPrintName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly MethodInfo GetStatMethod = typeof(BeautifulGlitch.PlayerData).GetMethod("GetStat", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        public StatsReader(StructuredLog log)
        {
            _log = log;
        }

        public string BuildCurrentPlayerStatsSpeech()
        {
            var gameManager = Object.FindObjectOfType<MonsterCamp.GameManager>();
            if (gameManager == null || GetCurrentPlayerDataMethod == null || GetStatMethod == null)
            {
                _log.Info("STATS", "READ", "Stats read unavailable because game manager or methods were not found.");
                return ModLocalization.Get("stats_unavailable");
            }

            var player = GetCurrentPlayerDataMethod.Invoke(gameManager, null) as BeautifulGlitch.PlayerData;
            if (player == null)
            {
                _log.Info("STATS", "READ", "Stats read unavailable because current player was null.");
                return ModLocalization.Get("stats_unavailable");
            }

            var name = SpeechText.Normalize(GetPrettyPrintNameMethod != null ? GetPrettyPrintNameMethod.Invoke(player, null) as string : string.Empty);
            var boldness = ReadStat(player, BeautifulGlitch.StatType.Boldness);
            var smarts = ReadStat(player, BeautifulGlitch.StatType.Smarts);
            var creativity = ReadStat(player, BeautifulGlitch.StatType.Creativity);
            var charm = ReadStat(player, BeautifulGlitch.StatType.Charm);
            var fun = ReadStat(player, BeautifulGlitch.StatType.Fun);

            var spoken = SpeechText.Normalize(
                (string.IsNullOrWhiteSpace(name) ? ModLocalization.Get("current_player") : name) +
                ". " + ModLocalization.Format("stat_boldness_fmt", boldness) +
                " " + ModLocalization.Format("stat_smarts_fmt", smarts) +
                " " + ModLocalization.Format("stat_creativity_fmt", creativity) +
                " " + ModLocalization.Format("stat_charm_fmt", charm) +
                " " + ModLocalization.Format("stat_fun_fmt", fun));

            _log.Info(
                "STATS",
                "READ",
                "Current player stats read.",
                ("name", name),
                ("boldness", boldness.ToString()),
                ("smarts", smarts.ToString()),
                ("creativity", creativity.ToString()),
                ("charm", charm.ToString()),
                ("fun", fun.ToString()));

            return spoken;
        }

        private int ReadStat(BeautifulGlitch.PlayerData player, BeautifulGlitch.StatType stat)
        {
            return (int)GetStatMethod.Invoke(player, new object[] { stat });
        }
    }
}
