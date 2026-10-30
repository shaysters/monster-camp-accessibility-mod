using HarmonyLib;
using GameMod.Toolkit.Localization;

namespace GameMod.Toolkit.Hooks
{
    internal static class EventOptionSpeechState
    {
        private static bool _promptPending;
        private static bool _skipNextOptionFocus;
        private static string _option1 = string.Empty;
        private static string _option2 = string.Empty;

        public static void ArmPrompt(string option1, string option2)
        {
            _promptPending = true;
            _skipNextOptionFocus = false;
            _option1 = SpeechText.Normalize(option1);
            _option2 = SpeechText.Normalize(option2);
        }

        public static bool ConsumePendingPrompt()
        {
            if (!_promptPending)
                return false;

            _promptPending = false;
            _skipNextOptionFocus = true;
            return true;
        }

        public static bool ConsumeSkipNextOptionFocus()
        {
            if (!_skipNextOptionFocus)
                return false;

            _skipNextOptionFocus = false;
            return true;
        }

        public static void Clear()
        {
            _promptPending = false;
            _skipNextOptionFocus = false;
            _option1 = string.Empty;
            _option2 = string.Empty;
        }

        public static bool PromptPending => _promptPending;

        public static string GetOptionText(BeautifulGlitch.OptionNumber option)
        {
            return option == BeautifulGlitch.OptionNumber.First ? _option1 : _option2;
        }
    }

    [HarmonyPatch(typeof(MonsterCamp.PrologueEventView))]
    internal static class PrologueStoryHooks
    {
        [HarmonyPostfix]
        [HarmonyPatch("ShowNormalScene")]
        private static void AfterShowNormalScene(MonsterCamp.PrologueEventView __instance, BeautifulGlitch.EventFlow evt, BeautifulGlitch.EventScene scene, bool isFirstScene, string text)
        {
            var spoken = StorySpeech.BuildSceneSpeech(scene != null ? scene.whoSpeaksNPC : string.Empty, text);
            HookContext.Log?.Info("EVENT", "PROLOGUE", "Prologue scene shown.", ("event", evt != null ? evt.eventName : string.Empty), ("speaker", scene != null ? scene.whoSpeaksNPC : string.Empty), ("text", SpeechText.Normalize(text)));
            HookContext.Speech?.Speak(spoken, SpeechCategory.Event, "PrologueEventView.ShowNormalScene", true);
        }
    }

    [HarmonyPatch(typeof(MonsterCamp.MetaShopEventView))]
    internal static class MetaShopStoryHooks
    {
        [HarmonyPostfix]
        [HarmonyPatch("ShowNormalScene")]
        private static void AfterShowNormalScene(MonsterCamp.MetaShopEventView __instance, BeautifulGlitch.EventFlow evt, BeautifulGlitch.EventScene scene, bool isFirstScene, string text)
        {
            var spoken = StorySpeech.BuildSceneSpeech(scene != null ? scene.whoSpeaksNPC : string.Empty, text);
            HookContext.Log?.Info("EVENT", "METASHOP", "Meta Shop scene shown.", ("event", evt != null ? evt.eventName : string.Empty), ("speaker", scene != null ? scene.whoSpeaksNPC : string.Empty), ("text", SpeechText.Normalize(text)));
            HookContext.Speech?.Speak(spoken, SpeechCategory.Event, "MetaShopEventView.ShowNormalScene", true);
        }
    }

    [HarmonyPatch(typeof(BeautifulGlitch.GameEventView))]
    internal static class GameEventStoryHooks
    {
        [HarmonyPrefix]
        [HarmonyPatch("ShowNormalScene")]
        private static void BeforeShowNormalScene()
        {
            EventOptionSpeechState.Clear();
        }

        [HarmonyPostfix]
        [HarmonyPatch("ShowNormalScene")]
        private static void AfterShowNormalScene(BeautifulGlitch.GameEventView __instance, BeautifulGlitch.EventFlow evt, BeautifulGlitch.EventScene scene, bool isFirstScene, string text)
        {
            var spoken = StorySpeech.BuildSceneSpeech(scene != null ? scene.whoSpeaksNPC : string.Empty, text);
            HookContext.Log?.Info("EVENT", "SCENE", "Game event scene shown.", ("event", evt != null ? evt.eventName : string.Empty), ("speaker", scene != null ? scene.whoSpeaksNPC : string.Empty), ("text", SpeechText.Normalize(text)));
            HookContext.Speech?.Speak(spoken, SpeechCategory.Event, "GameEventView.ShowNormalScene", true);
        }

        [HarmonyPrefix]
        [HarmonyPatch("ShowOptionsScene")]
        private static void BeforeShowOptionsScene(BeautifulGlitch.GameEventView __instance, BeautifulGlitch.EventFlow evt, BeautifulGlitch.EventScene scene, BeautifulGlitch.PlayerData playerData, bool isFirstScene, string text1, string text2)
        {
            EventOptionSpeechState.ArmPrompt(text1, text2);
        }

        [HarmonyPostfix]
        [HarmonyPatch("ShowOptionsScene")]
        private static void AfterShowOptionsScene(BeautifulGlitch.GameEventView __instance, BeautifulGlitch.EventFlow evt, BeautifulGlitch.EventScene scene, BeautifulGlitch.PlayerData playerData, bool isFirstScene, string text1, string text2)
        {
            HookContext.Log?.Info("EVENT", "OPTIONS", "Game event options shown.", ("event", evt != null ? evt.eventName : string.Empty), ("option1", SpeechText.Normalize(text1)), ("option2", SpeechText.Normalize(text2)));
        }
    }

    [HarmonyPatch(typeof(BeautifulGlitch.OptionTextBoxes))]
    internal static class OptionFocusHooks
    {
        [HarmonyPrefix]
        [HarmonyPatch("SetOption")]
        private static void BeforeSetOption(BeautifulGlitch.OptionTextBoxes __instance, BeautifulGlitch.OptionNumber newOption, bool hoverOption)
        {
            if (!EventOptionSpeechState.ConsumePendingPrompt())
            {
                return;
            }

            HookContext.Log?.Info("EVENT", "OPTIONPROMPT", "Prompt announced from first option focus.", ("option", newOption.ToString()));
            HookContext.Speech?.Speak(ModLocalization.Get("make_a_choice"), SpeechCategory.Event, "OptionTextBoxes.SetOption.Prompt", true);
        }

        [HarmonyPostfix]
        [HarmonyPatch("SetOption")]
        private static void AfterSetOption(BeautifulGlitch.OptionTextBoxes __instance, BeautifulGlitch.OptionNumber newOption, bool hoverOption)
        {
            if (EventOptionSpeechState.ConsumeSkipNextOptionFocus())
            {
                HookContext.Log?.Info("EVENT", "OPTIONFOCUS", "Skipped first option focus after prompt.", ("option", newOption.ToString()));
                return;
            }

            var optionNumber = ModLocalization.Format("option_number_fmt", newOption == BeautifulGlitch.OptionNumber.First ? 1 : 2);
            var optionText = EventOptionSpeechState.GetOptionText(newOption);
            var spoken = string.IsNullOrWhiteSpace(optionText) ? optionNumber : ModLocalization.Format("option_number_text_fmt", newOption == BeautifulGlitch.OptionNumber.First ? 1 : 2, optionText);
            HookContext.Log?.Info("EVENT", "OPTIONFOCUS", "Option focus changed.", ("option", newOption.ToString()), ("text", spoken));
            HookContext.Speech?.Speak(spoken, SpeechCategory.Focus, "OptionTextBoxes.SetOption", true);
        }
    }

    [HarmonyPatch(typeof(MonsterCamp.GameAnnouncementsOverlay))]
    internal static class TurnAnnouncementHooks
    {
        [HarmonyPostfix]
        [HarmonyPatch("SetData")]
        private static void AfterSetData(MonsterCamp.GameAnnouncementsOverlay __instance, System.Collections.Generic.List<BeautifulGlitch.TurnData> turnRoadmap, int currentTurn)
        {
            var week = __instance.globalTimeWidget != null ? SpeechText.Normalize(__instance.globalTimeWidget.weekLabel.text) : string.Empty;
            var timeOfDay = __instance.globalTimeWidget != null ? SpeechText.Normalize(__instance.globalTimeWidget.timeOfDayLabel.text) : string.Empty;
            var spoken = SpeechText.Normalize(string.Join(". ", new[] { week, timeOfDay }));
            HookContext.Log?.Info("TURN", "GLOBAL", "Turn data set.", ("currentTurn", currentTurn.ToString()), ("week", week), ("time", timeOfDay));
            HookContext.Speech?.Speak(spoken, SpeechCategory.Turn, "GameAnnouncementsOverlay.SetData", true);
        }
    }

    internal static class StorySpeech
    {
        public static string BuildSceneSpeech(string speakerId, string text)
        {
            var speaker = SpeechText.TitleCaseId(speakerId);
            var line = SpeechText.Normalize(text);
            if (string.IsNullOrWhiteSpace(line))
            {
                return string.Empty;
            }

            return string.IsNullOrWhiteSpace(speaker) ? line : speaker + ". " + line;
        }

        public static string BuildOptionsSpeech()
        {
            return ModLocalization.Get("make_a_choice");
        }
    }
}
