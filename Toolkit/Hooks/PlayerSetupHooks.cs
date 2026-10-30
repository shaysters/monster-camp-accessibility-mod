using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using GameMod.Toolkit.Localization;

namespace GameMod.Toolkit.Hooks
{
    [HarmonyPatch(typeof(MonsterCamp.PlayersSelectView))]
    internal static class PlayerSelectionHooks
    {
        [HarmonyPostfix]
        [HarmonyPatch("PrepareNextPlayerSelection")]
        private static void AfterPrepareNextPlayerSelection(MonsterCamp.PlayersSelectView __instance, int numOfConfigured, int nextPlayerNumber)
        {
            var text = __instance.choosePlayerLabel != null ? SpeechText.Normalize(__instance.choosePlayerLabel.text) : string.Empty;
            if (string.IsNullOrWhiteSpace(text))
            {
                text = ModLocalization.Get("choose_your_character");
            }

            HookContext.Log?.Info(
                "PLAYER",
                "SELECT",
                "Choose player prompt updated.",
                ("configured", numOfConfigured.ToString()),
                ("nextPlayer", nextPlayerNumber.ToString()),
                ("text", text));

            HookContext.Speech?.Speak(text, SpeechCategory.Name, "PlayersSelectView.PrepareNextPlayerSelection", true);
        }
    }

    [HarmonyPatch(typeof(MonsterCamp.PlayerCustomizeView))]
    internal static class PlayerCustomizeHooks
    {
        private static readonly MethodInfo CharacterGetDisplayNameMethod = typeof(BeautifulGlitch.CharacterProfile).GetMethod("GetDisplayName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly MethodInfo GetNameButtonMethod = typeof(MonsterCamp.PlayerCustomizeView).GetMethod("GetNameButton", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly MethodInfo GetPronounButtonMethod = typeof(MonsterCamp.PlayerCustomizeView).GetMethod("GetPronounButton", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        [HarmonyPostfix]
        [HarmonyPatch("SetupCustomizePanelPositionAndShow")]
        private static void AfterSetupCustomizePanelPositionAndShow(MonsterCamp.PlayerCustomizeView __instance, UIWidget aReferenceWidget, BeautifulGlitch.CharacterProfile aCharacterProfile)
        {
            var characterName = ReadCharacterName(aCharacterProfile);
            var currentName = ReadLabel(__instance.customizeNameButton != null ? __instance.customizeNameButton.label : null);
            var currentPronoun = ReadLabel(__instance.customizePronounButton != null ? __instance.customizePronounButton.label : null);

            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(characterName))
            {
                parts.Add(ModLocalization.Format("character_selected_fmt", characterName));
            }
            else
            {
                parts.Add(ModLocalization.Get("character_selected"));
            }

            if (!string.IsNullOrWhiteSpace(currentName))
            {
                parts.Add("Name. " + currentName);
            }

            if (!string.IsNullOrWhiteSpace(currentPronoun))
            {
                parts.Add("Pronouns. " + currentPronoun);
            }

            parts.Add(ModLocalization.Get("change_name_pronouns_confirm"));

            var spoken = SpeechText.Normalize(string.Join(". ", parts));
            HookContext.Log?.Info(
                "CUSTOMIZE",
                "SHOW",
                "Customize panel shown.",
                ("character", characterName),
                ("name", currentName),
                ("pronouns", currentPronoun));

            HookContext.Speech?.Speak(spoken, SpeechCategory.Name, "PlayerCustomizeView.SetupCustomizePanelPositionAndShow", true);
        }

        [HarmonyPostfix]
        [HarmonyPatch("OpenCustomizeName")]
        private static void AfterOpenCustomizeName(MonsterCamp.PlayerCustomizeView __instance)
        {
            var option1 = ReadNameOption(__instance, 0);
            var option2 = ReadNameOption(__instance, 1);
            var parts = new List<string> { ModLocalization.Get("choose_name") };

            if (!string.IsNullOrWhiteSpace(option1))
            {
                parts.Add(ModLocalization.Format("option_number_text_fmt", 1, option1));
            }

            if (!string.IsNullOrWhiteSpace(option2))
            {
                parts.Add(ModLocalization.Format("option_number_text_fmt", 2, option2));
            }

            parts.Add(ModLocalization.Format("option_number_text_fmt", 3, ModLocalization.Get("custom_name")));

            var spoken = SpeechText.Normalize(string.Join(". ", parts));
            HookContext.Log?.Info("CUSTOMIZE", "NAME", "Name options opened.", ("option1", option1), ("option2", option2), ("custom", "Custom name"));
            HookContext.Speech?.Speak(spoken, SpeechCategory.Name, "PlayerCustomizeView.OpenCustomizeName", true);
        }

        [HarmonyPostfix]
        [HarmonyPatch("OpenCustomizePronoun")]
        private static void AfterOpenCustomizePronoun(MonsterCamp.PlayerCustomizeView __instance)
        {
            var options = new List<string>();
            for (var i = 0; i < 4; i++)
            {
                var button = GetPronounButton(__instance, i);
                var label = button != null ? ReadLabel(button.label) : string.Empty;
                if (!string.IsNullOrWhiteSpace(label))
                {
                    options.Add(label);
                }
            }

            var spoken = options.Count == 0
                ? ModLocalization.Get("choose_pronouns")
                : SpeechText.Normalize(ModLocalization.Get("choose_pronouns") + " " + string.Join(". ", options));

            HookContext.Log?.Info("CUSTOMIZE", "PRONOUN", "Pronoun options opened.", ("options", string.Join(" | ", options)));
            HookContext.Speech?.Speak(spoken, SpeechCategory.Name, "PlayerCustomizeView.OpenCustomizePronoun", true);
        }

        private static string ReadNameOption(MonsterCamp.PlayerCustomizeView view, int index)
        {
            var button = GetNameButton(view, index);
            return button != null ? ReadLabel(button.label) : string.Empty;
        }

        private static string ReadCharacterName(BeautifulGlitch.CharacterProfile profile)
        {
            if (profile == null || CharacterGetDisplayNameMethod == null)
            {
                return string.Empty;
            }

            return SpeechText.Normalize(CharacterGetDisplayNameMethod.Invoke(profile, null) as string);
        }

        private static MonsterCamp.ProloguePlayerNameButton GetNameButton(MonsterCamp.PlayerCustomizeView view, int index)
        {
            if (view == null || GetNameButtonMethod == null)
            {
                return null;
            }

            return GetNameButtonMethod.Invoke(view, new object[] { index }) as MonsterCamp.ProloguePlayerNameButton;
        }

        private static MonsterCamp.ProloguePlayerPronounButton GetPronounButton(MonsterCamp.PlayerCustomizeView view, int index)
        {
            if (view == null || GetPronounButtonMethod == null)
            {
                return null;
            }

            return GetPronounButtonMethod.Invoke(view, new object[] { index }) as MonsterCamp.ProloguePlayerPronounButton;
        }

        private static string ReadLabel(UILabel label)
        {
            return label != null ? SpeechText.Normalize(label.text) : string.Empty;
        }
    }

    [HarmonyPatch(typeof(MonsterCamp.BackpackView))]
    internal static class BackpackHooks
    {
        [HarmonyPostfix]
        [HarmonyPatch("Show")]
        private static void AfterShow(MonsterCamp.BackpackView __instance, BeautifulGlitch.PlayerId playerId, BeautifulGlitch.CharacterGroup characterGroup, List<MonsterCamp.BackpackItemProfile> availableItems)
        {
            var spoken = ModLocalization.Get("choose_what_to_pack");
            HookContext.Log?.Info(
                "BACKPACK",
                "SHOW",
                "Backpack screen shown.",
                ("playerId", playerId.ToString()),
                ("characterGroup", characterGroup.ToString()),
                ("itemCount", availableItems != null ? availableItems.Count.ToString() : "0"));
            HookContext.Speech?.Speak(spoken, SpeechCategory.Name, "BackpackView.Show", true);
        }

        [HarmonyPostfix]
        [HarmonyPatch("SetItem")]
        private static void AfterSetItem(MonsterCamp.BackpackView __instance, int itemIdx)
        {
            var itemName = __instance.itemInfoLabel != null ? SpeechText.Normalize(__instance.itemInfoLabel.text) : string.Empty;
            HookContext.Log?.Info("BACKPACK", "ITEM", "Backpack item set.", ("index", itemIdx.ToString()), ("text", itemName));

            if (!string.IsNullOrWhiteSpace(itemName))
            {
                HookContext.Speech?.Speak(itemName, SpeechCategory.Focus, "BackpackView.SetItem", false);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("SetRandom")]
        private static void AfterSetRandom(MonsterCamp.BackpackView __instance)
        {
            var spoken = ModLocalization.Get("pack_random_items");
            HookContext.Log?.Info("BACKPACK", "RANDOM", "Random backpack option set.", ("text", spoken));
            HookContext.Speech?.Speak(spoken, SpeechCategory.Focus, "BackpackView.SetRandom", false);
        }

        [HarmonyPostfix]
        [HarmonyPatch("ConfirmItem")]
        private static void AfterConfirmItem(MonsterCamp.BackpackView __instance, int itemIdx)
        {
            var itemName = __instance.itemInfoLabel != null ? SpeechText.Normalize(__instance.itemInfoLabel.text) : string.Empty;
            if (string.IsNullOrWhiteSpace(itemName))
            {
                return;
            }

            var spoken = ModLocalization.Format("packed_fmt", itemName);
            HookContext.Log?.Info("BACKPACK", "CONFIRM", "Backpack item confirmed.", ("index", itemIdx.ToString()), ("text", itemName));
            HookContext.Speech?.Speak(spoken, SpeechCategory.Name, "BackpackView.ConfirmItem", false);
        }
    }
}
