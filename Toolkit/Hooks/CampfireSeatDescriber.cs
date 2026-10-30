using System;
using System.Collections.Generic;
using HarmonyLib;
using GameMod.Toolkit.Localization;

namespace GameMod.Toolkit.Hooks
{
    // "Campfire option 2: Vera, Liam. 2 seats free." Each CampfireLocation has four seats
    // ("positions"), each holding an NPC id, a PlayerId name ("PlayerA"), or "".
    internal static class CampfireSeatDescriber
    {
        public static string Describe(MonsterCamp.CampfireLocationView view)
        {
            if (view == null)
            {
                return ModLocalization.Format("campfire_option_fmt", string.Empty);
            }

            var title = view.isLoveLog ? ModLocalization.Get("campfire_love_log") : ModLocalization.Format("campfire_option_fmt", (view.locationId + 1).ToString());
            try
            {
                var manager = Traverse.Create(view).Field("gameManager").Field("campfireManager");
                var location = view.isLoveLog
                    ? manager.Method("GetLoveLog").GetValue()
                    : manager.Method("GetNormalLog", view.locationId).GetValue();
                var positions = Traverse.Create(location).Field("positions").GetValue<List<string>>();
                if (positions == null)
                {
                    return title;
                }

                var names = new List<string>();
                var free = 0;
                foreach (var occupant in positions)
                {
                    if (string.IsNullOrWhiteSpace(occupant))
                    {
                        free++;
                        continue;
                    }

                    names.Add(GetOccupantName(occupant));
                }

                var spoken = title + ": ";
                if (names.Count > 0)
                {
                    spoken += string.Join(", ", names.ToArray()) + ". ";
                }

                return spoken + (free == 0 ? ModLocalization.Get("seats_free_none") : free == 1 ? ModLocalization.Get("seats_free_one") : ModLocalization.Format("seats_free_fmt", free));
            }
            catch (Exception ex)
            {
                HookContext.Log?.Warn("LOCATION", "CAMPFIRE", "Campfire seat lookup failed.", ("error", ex.Message));
                return title;
            }
        }

        private static string GetOccupantName(string occupant)
        {
            var generalManager = BeautifulGlitch.GeneralManager.Instance;
            if (Enum.IsDefined(typeof(BeautifulGlitch.PlayerId), occupant))
            {
                var playerId = (BeautifulGlitch.PlayerId)Enum.Parse(typeof(BeautifulGlitch.PlayerId), occupant);
                var player = Traverse.Create(generalManager).Field("dataManager").Field("gameSession").Method("GetPlayer", playerId).GetValue() as BeautifulGlitch.PlayerData;
                var playerName = AccessSpeech.ResolvePlayerName(player);
                return string.IsNullOrWhiteSpace(playerName) ? occupant : playerName;
            }

            var name = generalManager != null && generalManager.textManager != null
                ? SpeechText.Normalize(Traverse.Create(generalManager.textManager).Method("GetNpcName", occupant, true).GetValue() as string)
                : string.Empty;
            return string.IsNullOrWhiteSpace(name) || name.StartsWith("CHR_", StringComparison.Ordinal) ? occupant : name;
        }
    }
}
