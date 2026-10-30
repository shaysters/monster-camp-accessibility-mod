using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using GameMod.Toolkit.Localization;

namespace GameMod.Toolkit.Hooks
{
    [HarmonyPatch(typeof(BeautifulGlitch.PlayerTurnOverlay))]
    internal static class PlayerTurnNameHooks
    {
        [HarmonyPostfix]
        [HarmonyPatch("SetPlayer")]
        private static void AfterSetPlayer(BeautifulGlitch.PlayerTurnOverlay __instance, BeautifulGlitch.PlayerData playerData)
        {
            var playerName = AccessSpeech.ResolvePlayerName(playerData);
            var spoken = string.IsNullOrWhiteSpace(playerName) ? string.Empty : ModLocalization.Format("player_turn_fmt", playerName);
            HookContext.Log?.Info("TURN", "PLAYER", "Player turn set.", ("name", playerName));
            HookContext.Speech?.Speak(spoken, SpeechCategory.Turn, "PlayerTurnOverlay.SetPlayer", true);
        }
    }

    [HarmonyPatch(typeof(MonsterCamp.IceBreakersView))]
    internal static class IceBreakerPromptHooks
    {
        private static readonly FieldInfo ActiveAnswersField = typeof(MonsterCamp.IceBreakersView).GetField("activeAnswers", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo IceBreakerAnswerTextField = typeof(MonsterCamp.IceBreakerAnswer).GetField("text", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        [HarmonyPostfix]
        [HarmonyPatch("SetIceBreaker")]
        private static void AfterSetIceBreaker(MonsterCamp.IceBreakersView __instance, MonsterCamp.IceBreakerEvent evt, List<int> randomIndexes)
        {
            var asker = __instance.questionPlayerLabel != null ? SpeechText.Normalize(__instance.questionPlayerLabel.text) : string.Empty;
            var question = __instance.questionLabel != null ? SpeechText.Normalize(__instance.questionLabel.text) : string.Empty;
            var spoken = AccessSpeech.BuildPromptSpeech(asker, question, ModLocalization.Get("question_prefix"));
            HookContext.Log?.Info("QUIZ", "ICEBREAKER", "Icebreaker question shown.", ("speaker", asker), ("question", question));
            HookContext.Speech?.Speak(spoken, SpeechCategory.Event, "IceBreakersView.SetIceBreaker", true);
        }

        [HarmonyPostfix]
        [HarmonyPatch("SetAnswer")]
        private static void AfterSetAnswer(MonsterCamp.IceBreakersView __instance, int answerIdx)
        {
            var answerText = string.Empty;
            var activeAnswers = ActiveAnswersField != null ? ActiveAnswersField.GetValue(__instance) as IList<MonsterCamp.IceBreakerAnswer> : null;
            if (activeAnswers != null && answerIdx >= 0 && answerIdx < activeAnswers.Count)
            {
                answerText = SpeechText.Normalize(IceBreakerAnswerTextField != null ? IceBreakerAnswerTextField.GetValue(activeAnswers[answerIdx]) as string : string.Empty);
            }

            var spoken = string.IsNullOrWhiteSpace(answerText)
                ? ModLocalization.Format("option_number_fmt", answerIdx + 1)
                : ModLocalization.Format("option_number_text_fmt", answerIdx + 1, answerText);
            HookContext.Log?.Info("QUIZ", "ICEBREAKERFOCUS", "Icebreaker option focused.", ("option", spoken));
            HookContext.Speech?.Speak(spoken, SpeechCategory.Focus, "IceBreakersView.SetAnswer", true);
        }
    }

    [HarmonyPatch(typeof(MonsterCamp.GossipView))]
    internal static class GossipPromptHooks
    {
        [HarmonyPostfix]
        [HarmonyPatch("Show")]
        private static void AfterShow(MonsterCamp.GossipView __instance)
        {
            HookContext.Log?.Info("QUIZ", "GOSSIP", "Gossip view shown.");
        }

        [HarmonyPostfix]
        [HarmonyPatch("ShowStatChoice")]
        private static void AfterShowStatChoice(MonsterCamp.GossipView __instance)
        {
            SpeakPrompt("STAT", "GossipView.ShowStatChoice", __instance.statPromptLabel);
        }

        [HarmonyPostfix]
        [HarmonyPatch("ShowQualityChoice")]
        private static void AfterShowQualityChoice(MonsterCamp.GossipView __instance)
        {
            SpeakPrompt("QUALITY", "GossipView.ShowQualityChoice", __instance.qualityPromptLabel);
        }

        [HarmonyPostfix]
        [HarmonyPatch("ShowCustomChoice")]
        private static void AfterShowCustomChoice(MonsterCamp.GossipView __instance)
        {
            SpeakPrompt("CUSTOMCHOICE", "GossipView.ShowCustomChoice", __instance.customPromptLabel);
        }

        [HarmonyPostfix]
        [HarmonyPatch("ShowCustomInput")]
        private static void AfterShowCustomInput(MonsterCamp.GossipView __instance)
        {
            SpeakPrompt("CUSTOMINPUT", "GossipView.ShowCustomInput", __instance.inputChoicePromptLabel);
        }

        private static void SpeakPrompt(string area, string source, UILabel label)
        {
            var prompt = label != null ? SpeechText.Normalize(label.text) : string.Empty;
            HookContext.Log?.Info("QUIZ", area, "Quiz prompt shown.", ("text", prompt));
            HookContext.Speech?.Speak(prompt, SpeechCategory.Event, source, true);
        }
    }

    [HarmonyPatch(typeof(MonsterCamp.ShopView))]
    internal static class ShopHooks
    {
        private static readonly FieldInfo ActiveItemsField = typeof(MonsterCamp.ShopView).GetField("activeItems", BindingFlags.Instance | BindingFlags.NonPublic);

        [HarmonyPostfix]
        [HarmonyPatch("Show")]
        private static void AfterShow(MonsterCamp.ShopView __instance)
        {
            var cash = __instance.cashLabel != null ? SpeechText.Normalize(__instance.cashLabel.text) : string.Empty;
            var spoken = string.IsNullOrWhiteSpace(cash) ? ModLocalization.Get("shop") : ModLocalization.Format("shop_cash_fmt", cash);
            HookContext.Log?.Info("SHOP", "SHOW", "Shop shown.", ("cash", cash));
            HookContext.Speech?.Speak(spoken, SpeechCategory.Event, "ShopView.Show", true);
        }

        [HarmonyPostfix]
        [HarmonyPatch("SetShopkeeperDialogue_Greeting")]
        private static void AfterSetShopkeeperGreeting(MonsterCamp.ShopView __instance, bool isFirstVisit)
        {
            var spoken = ReadShopDialogue(__instance);
            HookContext.Log?.Info("SHOP", "GREETING", "Shop greeting set.", ("text", spoken), ("isFirstVisit", isFirstVisit.ToString()));
            HookContext.Speech?.Speak(spoken, SpeechCategory.Event, "ShopView.SetShopkeeperDialogue_Greeting", true);
        }

        [HarmonyPostfix]
        [HarmonyPatch("SetShopkeeperDialogue_ItemDescription")]
        private static void AfterSetShopkeeperDialogueItemDescription(MonsterCamp.ShopView __instance, BeautifulGlitch.ItemProfile itemProfile)
        {
            var spoken = BuildShopItemSpeech(__instance, itemProfile);
            HookContext.Log?.Info(
                "SHOP",
                "ITEM",
                "Shop item described.",
                ("id", itemProfile != null ? itemProfile.id : string.Empty),
                ("text", spoken));
            HookContext.Speech?.Speak(spoken, SpeechCategory.Focus, "ShopView.SetShopkeeperDialogue_ItemDescription", true);
        }

        [HarmonyPostfix]
        [HarmonyPatch("SetShopkeeperDialogue_Purchased")]
        private static void AfterSetShopkeeperDialoguePurchased(MonsterCamp.ShopView __instance)
        {
            var spoken = ReadShopDialogue(__instance);
            HookContext.Log?.Info("SHOP", "PURCHASED", "Shop purchased dialogue set.", ("text", spoken));
            HookContext.Speech?.Speak(spoken, SpeechCategory.Event, "ShopView.SetShopkeeperDialogue_Purchased", true);
        }

        [HarmonyPostfix]
        [HarmonyPatch("SetShopkeeperDialogue_ExitWithoutBuying")]
        private static void AfterSetShopkeeperDialogueExitWithoutBuying(MonsterCamp.ShopView __instance)
        {
            var spoken = ReadShopDialogue(__instance);
            HookContext.Log?.Info("SHOP", "EXIT", "Shop exit dialogue set.", ("text", spoken));
            HookContext.Speech?.Speak(spoken, SpeechCategory.Event, "ShopView.SetShopkeeperDialogue_ExitWithoutBuying", true);
        }

        [HarmonyPostfix]
        [HarmonyPatch("SetSelectedItem")]
        private static void AfterSetSelectedItem(MonsterCamp.ShopView __instance, int itemButtonIdx)
        {
            var itemProfile = GetActiveItem(__instance, itemButtonIdx);
            var spoken = BuildShopItemSpeech(__instance, itemProfile);
            HookContext.Log?.Info(
                "SHOP",
                "SELECT",
                "Shop item selected.",
                ("index", itemButtonIdx.ToString()),
                ("id", itemProfile != null ? itemProfile.id : string.Empty),
                ("text", spoken));
            HookContext.Speech?.Speak(spoken, SpeechCategory.Focus, "ShopView.SetSelectedItem", true);
        }

        internal static string BuildShopItemSpeech(MonsterCamp.ShopView view, BeautifulGlitch.ItemProfile itemProfile)
        {
            var name = view != null && view.itemInfoPanel != null ? SpeechText.Normalize(view.itemInfoPanel.nameLabel.text) : string.Empty;
            var description = view != null && view.itemInfoPanel != null ? SpeechText.Normalize(view.itemInfoPanel.descriptionLabel.text) : string.Empty;
            var price = itemProfile != null ? itemProfile.price.ToString() : string.Empty;
            var affordability = CanAfford(view, itemProfile) ? ModLocalization.Get("affordable") : ModLocalization.Get("not_enough_money");

            if (string.IsNullOrWhiteSpace(name) && itemProfile != null)
            {
                name = SpeechText.Normalize(itemProfile.displayName);
            }

            return SpeechText.Normalize(name + ". " + ModLocalization.Format("costs_fmt", price) + " " + description + ". " + affordability);
        }

        private static string ReadShopDialogue(MonsterCamp.ShopView view)
        {
            if (view == null || view.itemInfoPanel == null)
            {
                return string.Empty;
            }

            var text = SpeechText.Normalize(view.itemInfoPanel.descriptionLabel.text);
            return text;
        }

        private static bool CanAfford(MonsterCamp.ShopView view, BeautifulGlitch.ItemProfile itemProfile)
        {
            if (view == null || itemProfile == null)
            {
                return false;
            }

            var cashText = view.cashLabel != null ? SpeechText.Normalize(view.cashLabel.text) : string.Empty;
            var cashValue = ParseFirstInt(cashText);
            return cashValue >= itemProfile.price;
        }

        private static int ParseFirstInt(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return 0;
            }

            var digits = string.Empty;
            foreach (var ch in text)
            {
                if (char.IsDigit(ch))
                {
                    digits += ch;
                }
                else if (digits.Length > 0)
                {
                    break;
                }
            }

            int value;
            return int.TryParse(digits, out value) ? value : 0;
        }

        private static BeautifulGlitch.ItemProfile GetActiveItem(MonsterCamp.ShopView view, int itemButtonIdx)
        {
            if (view == null || ActiveItemsField == null)
            {
                return null;
            }

            var items = ActiveItemsField.GetValue(view) as IList<BeautifulGlitch.ItemProfile>;
            if (items == null || itemButtonIdx < 0 || itemButtonIdx >= items.Count)
            {
                return null;
            }

            return items[itemButtonIdx];
        }
    }

    [HarmonyPatch(typeof(MonsterCamp.ItemButton))]
    internal static class ShopButtonHooks
    {
        [HarmonyPostfix]
        [HarmonyPatch("UiSelectItem")]
        private static void AfterUiSelectItem(MonsterCamp.ItemButton __instance)
        {
            if (__instance == null || __instance.shopView == null)
            {
                return;
            }

            var index = AccessSpeech.ReadIntField(__instance, "itemButtonIdx");
            if (index >= 0)
            {
                var itemProfile = AccessSpeech.GetShopActiveItem(__instance.shopView, index);
                var spoken = ShopHooks.BuildShopItemSpeech(__instance.shopView, itemProfile);
                HookContext.Log?.Info("SHOP", "HOVER", "Shop button hovered.", ("index", index.ToString()), ("text", spoken));
                HookContext.Speech?.Speak(spoken, SpeechCategory.Focus, "ItemButton.UiSelectItem", true);
            }
        }
    }

    [HarmonyPatch(typeof(MonsterCamp.CreditsUIManager))]
    internal static class CreditsHooks
    {
        [HarmonyPostfix]
        [HarmonyPatch("ShowWidget_Skip")]
        private static void AfterShowWidgetSkip(MonsterCamp.CreditsUIManager __instance, float waitTime)
        {
            var spoken = ModLocalization.Get("credits_exit");

            HookContext.Log?.Info("CREDITS", "SHOW", "Credits skip shown.", ("waitTime", waitTime.ToString()), ("text", spoken));
            HookContext.Speech?.Speak(spoken, SpeechCategory.Event, "CreditsUIManager.ShowWidget_Skip", true);
        }
    }

    [HarmonyPatch(typeof(MonsterCamp.CreditsEventView))]
    internal static class CreditsEventHooks
    {
        [HarmonyPostfix]
        [HarmonyPatch("ShowNormalScene")]
        private static void AfterShowNormalScene(MonsterCamp.CreditsEventView __instance, BeautifulGlitch.EventFlow evt, BeautifulGlitch.EventScene scene, bool isFirstScene, string text)
        {
            var spoken = StorySpeech.BuildSceneSpeech(scene != null ? scene.whoSpeaksNPC : string.Empty, text);
            HookContext.Log?.Info("CREDITS", "SCENE", "Credits scene shown.", ("event", evt != null ? evt.eventName : string.Empty), ("text", SpeechText.Normalize(text)));
            HookContext.Speech?.Speak(spoken, SpeechCategory.Event, "CreditsEventView.ShowNormalScene", true);
        }
    }

    [HarmonyPatch(typeof(BeautifulGlitch.CampLocationView))]
    internal static class CampLocationHooks
    {
        [HarmonyPostfix]
        [HarmonyPatch("UiSetLocation")]
        private static void AfterUiSetLocation(BeautifulGlitch.CampLocationView __instance)
        {
            var spoken = AccessSpeech.BuildLocationSpeech(__instance != null ? __instance.locationType.ToString() : string.Empty);
            HookContext.Log?.Info("LOCATION", "CAMP", "Camp location focused.", ("location", __instance != null ? __instance.locationType.ToString() : string.Empty));
            HookContext.Speech?.Speak(spoken, SpeechCategory.Focus, "CampLocationView.UiSetLocation", true);
        }
    }

    [HarmonyPatch(typeof(MonsterCamp.CampfireLocationView))]
    internal static class CampfireLocationHooks
    {
        [HarmonyPostfix]
        [HarmonyPatch("UiSetLocation")]
        private static void AfterUiSetLocation(MonsterCamp.CampfireLocationView __instance)
        {
            var spoken = CampfireSeatDescriber.Describe(__instance);
            HookContext.Log?.Info("LOCATION", "CAMPFIRE", "Campfire location focused.", ("locationId", __instance != null ? __instance.locationId.ToString() : string.Empty));
            HookContext.Speech?.Speak(spoken, SpeechCategory.Focus, "CampfireLocationView.UiSetLocation", true);
        }
    }

    [HarmonyPatch(typeof(BeautifulGlitch.EndingChooseNpcView))]
    internal static class EndingChooseHooks
    {
        [HarmonyPostfix]
        [HarmonyPatch("ShowChooseScene")]
        private static void AfterShowChooseScene(BeautifulGlitch.EndingChooseNpcView __instance, BeautifulGlitch.PlayerData playerData)
        {
            var prompt = __instance.LabelEndingChooseNpc != null ? SpeechText.Normalize(__instance.LabelEndingChooseNpc.text) : string.Empty;
            var player = AccessSpeech.ResolvePlayerName(playerData);
            var spoken = string.IsNullOrWhiteSpace(prompt) ? ModLocalization.Get("choose_who_to_ask") : prompt;
            if (!string.IsNullOrWhiteSpace(player))
            {
                spoken = player + ". " + spoken;
            }

            HookContext.Log?.Info("ENDING", "CHOOSE", "Ending choose screen shown.", ("player", player), ("prompt", prompt));
            HookContext.Speech?.Speak(spoken, SpeechCategory.Event, "EndingChooseNpcView.ShowChooseScene", true);
        }

        [HarmonyPostfix]
        [HarmonyPatch("SetChooseNpc")]
        private static void AfterSetChooseNpc(BeautifulGlitch.EndingChooseNpcView __instance, string npcId)
        {
            var name = __instance.npcNamePanel != null && __instance.npcNamePanel.label != null
                ? SpeechText.Normalize(__instance.npcNamePanel.label.text)
                : SpeechText.TitleCaseId(npcId);
            var spoken = string.IsNullOrWhiteSpace(name) ? string.Empty : name;
            HookContext.Log?.Info("ENDING", "FOCUS", "Ending NPC focused.", ("npcId", npcId), ("name", name));
            HookContext.Speech?.Speak(spoken, SpeechCategory.Focus, "EndingChooseNpcView.SetChooseNpc", true);
        }
    }

    [HarmonyPatch(typeof(BeautifulGlitch.EndingChooseNpcConfirm))]
    internal static class EndingConfirmHooks
    {
        [HarmonyPostfix]
        [HarmonyPatch("Show")]
        private static void AfterShow(BeautifulGlitch.EndingChooseNpcConfirm __instance, string npcName, Sprite playerSprite, Sprite npcSprite, float playerAspectRatio, float npcAspectRatio)
        {
            var prompt = __instance.titleLabel != null ? SpeechText.Normalize(__instance.titleLabel.text) : string.Empty;
            HookContext.Log?.Info("ENDING", "CONFIRM", "Ending confirmation shown.", ("npcName", npcName), ("prompt", prompt));
            HookContext.Speech?.Speak(prompt, SpeechCategory.Event, "EndingChooseNpcConfirm.Show", true);
        }
    }

    [HarmonyPatch(typeof(MonsterCamp.ResultsView))]
    internal static class ResultsHooks
    {
        [HarmonyPostfix]
        [HarmonyPatch("StartResultsPage")]
        private static void AfterStartResultsPage(MonsterCamp.ResultsView __instance, int totalSecretEndings, int secretEndingsSeen, int newSecretEndings, int totalEvents, int eventsSeen, int newEvents, int totalOutcomes, int outcomesSeen, int newOutcomes, int totalSpecial, int specialSeen, int newSpecial, int totalMemories, int newMemories)
        {
            var spoken = SpeechText.Normalize(
                ModLocalization.Get("results_prefix") + " " +
                ModLocalization.Get("results_this_run") + " " +
                ModLocalization.Format("results_new_secret_endings_fmt", newSecretEndings) + " " +
                ModLocalization.Format("results_new_events_fmt", newEvents) + " " +
                ModLocalization.Format("results_new_outcomes_fmt", newOutcomes) + " " +
                ModLocalization.Format("results_new_special_fmt", newSpecial) + " " +
                ModLocalization.Format("results_new_memories_fmt", newMemories) + " " +
                ModLocalization.Format("results_total_memories_fmt", totalMemories));

            HookContext.Log?.Info("RESULTS", "SHOW", "Results page started.",
                ("newSecretEndings", newSecretEndings.ToString()),
                ("newEvents", newEvents.ToString()),
                ("newOutcomes", newOutcomes.ToString()),
                ("newSpecial", newSpecial.ToString()),
                ("newMemories", newMemories.ToString()),
                ("totalMemories", totalMemories.ToString()));
            HookContext.Speech?.Speak(spoken, SpeechCategory.Event, "ResultsView.StartResultsPage", true);
        }
    }

    [HarmonyPatch(typeof(BeautifulGlitch.UnlockScreenManager))]
    internal static class UnlockHooks
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnEnable")]
        private static void AfterOnEnable(BeautifulGlitch.UnlockScreenManager __instance)
        {
            var title = __instance.unlockScreenTitle != null ? SpeechText.Normalize(__instance.unlockScreenTitle.text) : string.Empty;
            var body = __instance.labelToChange != null ? SpeechText.Normalize(__instance.labelToChange.text) : string.Empty;
            var fallbackDetails = AccessSpeech.ReadAllTextFromGameObject(__instance != null && __instance.container != null ? __instance.container.gameObject : null);
            var details = string.IsNullOrWhiteSpace(body) ? fallbackDetails : body;
            if (!string.IsNullOrWhiteSpace(title) && string.Equals(details, title, StringComparison.OrdinalIgnoreCase))
            {
                details = string.Empty;
            }
            var spoken = SpeechText.Normalize(string.Join(". ", new[] { title, details }));
            HookContext.Log?.Info("UNLOCK", "SHOW", "Unlock screen shown.", ("title", title), ("body", body));
            HookContext.Speech?.Speak(spoken, SpeechCategory.Event, "UnlockScreenManager.OnEnable", true);
        }
    }

    [HarmonyPatch(typeof(BeautifulGlitch.PauseMenu))]
    internal static class PauseConfirmHooks
    {
        [HarmonyPostfix]
        [HarmonyPatch("ShowConfirmPanel")]
        private static void AfterShowConfirmPanel(BeautifulGlitch.PauseMenu __instance)
        {
            var prompt = __instance.confirmPanelOnlineLabel != null ? SpeechText.Normalize(__instance.confirmPanelOnlineLabel.text) : string.Empty;
            if (string.IsNullOrWhiteSpace(prompt))
            {
                prompt = ModLocalization.Get("quit_confirm");
            }

            HookContext.Log?.Info("POPUP", "QUITCONFIRM", "Pause quit confirmation shown.", ("prompt", prompt));
            HookContext.Speech?.Speak(prompt, SpeechCategory.Event, "PauseMenu.ShowConfirmPanel", true);
        }
    }

    [HarmonyPatch(typeof(MonsterCamp.OnlineInfoPopup))]
    internal static class OnlinePopupHooks
    {
        [HarmonyPostfix]
        [HarmonyPatch("Show")]
        private static void AfterShow(MonsterCamp.OnlineInfoPopup __instance, string textId)
        {
            var text = __instance.messageLabel != null ? SpeechText.Normalize(__instance.messageLabel.text) : string.Empty;
            HookContext.Log?.Info("ONLINE", "POPUP", "Online popup shown.", ("textId", textId), ("text", text));
            HookContext.Speech?.Speak(text, SpeechCategory.Event, "OnlineInfoPopup.Show", true);
        }
    }

    [HarmonyPatch(typeof(BeautifulGlitch.GenericPopup))]
    internal static class GenericPopupHooks
    {
        [HarmonyPostfix]
        [HarmonyPatch("ShowOk")]
        private static void AfterShowOk(BeautifulGlitch.GenericPopup __instance, string text, Action okCallback)
        {
            SpeakPopup(__instance != null && __instance.PopupMessageLabel != null ? __instance.PopupMessageLabel.text : text, "GenericPopup.ShowOk");
        }

        [HarmonyPostfix]
        [HarmonyPatch("ShowPopupString")]
        private static void AfterShowPopupString(BeautifulGlitch.GenericPopup __instance, string text)
        {
            SpeakPopup(__instance != null && __instance.PopupMessageLabel != null ? __instance.PopupMessageLabel.text : text, "GenericPopup.ShowPopupString");
        }

        private static void SpeakPopup(string rawText, string source)
        {
            var text = SpeechText.Normalize(rawText);
            HookContext.Log?.Info("ONLINE", "GENERICPOPUP", "Generic popup shown.", ("source", source), ("text", text));
            HookContext.Speech?.Speak(text, SpeechCategory.Event, source, true);
        }
    }

    [HarmonyPatch(typeof(BeautifulGlitch.GenericPopupYesNo))]
    internal static class GenericPopupYesNoHooks
    {
        [HarmonyPostfix]
        [HarmonyPatch("ShowYesNo")]
        private static void AfterShowYesNo(BeautifulGlitch.GenericPopupYesNo __instance, string textId, Action yesCallback, Action noCallback, string extraInfo)
        {
            var text = __instance != null && __instance.PopupMessageLabel != null ? __instance.PopupMessageLabel.text : textId;
            SpeakPopup(text, "GenericPopupYesNo.ShowYesNo", extraInfo);
        }

        [HarmonyPostfix]
        [HarmonyPatch("ShowPopupString")]
        private static void AfterShowPopupString(BeautifulGlitch.GenericPopupYesNo __instance, string text)
        {
            SpeakPopup(__instance != null && __instance.PopupMessageLabel != null ? __instance.PopupMessageLabel.text : text, "GenericPopupYesNo.ShowPopupString", string.Empty);
        }

        private static void SpeakPopup(string rawText, string source, string extraInfo)
        {
            var text = SpeechText.Normalize(rawText);
            var extra = SpeechText.Normalize(extraInfo);
            var spoken = string.IsNullOrWhiteSpace(extra) ? text : SpeechText.Normalize(text + ". " + extra);
            HookContext.Log?.Info("ONLINE", "GENERICYESNO", "Generic yes or no popup shown.", ("source", source), ("text", text), ("extraInfo", extra));
            HookContext.Speech?.Speak(spoken, SpeechCategory.Event, source, true);
        }
    }

    [HarmonyPatch(typeof(BeautifulGlitch.LoadingOnlinePopup))]
    internal static class LoadingOnlinePopupHooks
    {
        [HarmonyPostfix]
        [HarmonyPatch("ShowPopup")]
        private static void AfterShowPopup()
        {
            HookContext.Log?.Info("ONLINE", "LOADING", "Loading online popup shown.");
            HookContext.Speech?.Speak(ModLocalization.Get("connecting_online"), SpeechCategory.Event, "LoadingOnlinePopup.ShowPopup", true);
        }
    }

    [HarmonyPatch(typeof(BeautifulGlitch.OnlineLobbyMainView))]
    internal static class OnlineLobbyMainHooks
    {
        [HarmonyPostfix]
        [HarmonyPatch("Show")]
        private static void AfterShow(BeautifulGlitch.OnlineLobbyMainView __instance)
        {
            HookContext.Log?.Info("ONLINE", "MAIN", "Online main view shown.");
            HookContext.Speech?.Speak(ModLocalization.Get("online_main"), SpeechCategory.Event, "OnlineLobbyMainView.Show", true);
        }
    }

    [HarmonyPatch(typeof(BeautifulGlitch.OnlineLobbyJoinMatchView))]
    internal static class OnlineLobbyJoinHooks
    {
        [HarmonyPostfix]
        [HarmonyPatch("Show")]
        private static void AfterShow(BeautifulGlitch.OnlineLobbyJoinMatchView __instance)
        {
            HookContext.Log?.Info("ONLINE", "JOIN", "Online join view shown.");
            HookContext.Speech?.Speak(ModLocalization.Get("online_join"), SpeechCategory.Event, "OnlineLobbyJoinMatchView.Show", true);
        }
    }

    [HarmonyPatch(typeof(BeautifulGlitch.OnlineLobbyWaitingView))]
    internal static class OnlineLobbyWaitingHooks
    {
        [HarmonyPostfix]
        [HarmonyPatch("Show")]
        private static void AfterShow(BeautifulGlitch.OnlineLobbyWaitingView __instance)
        {
            var text = __instance.waitingLabel != null ? SpeechText.Normalize(__instance.waitingLabel.text) : string.Empty;
            HookContext.Log?.Info("ONLINE", "WAITING", "Online waiting view shown.", ("text", text));
            HookContext.Speech?.Speak(string.IsNullOrWhiteSpace(text) ? ModLocalization.Get("online_waiting") : text, SpeechCategory.Event, "OnlineLobbyWaitingView.Show", true);
        }
    }

    [HarmonyPatch(typeof(BeautifulGlitch.OnlineLobbyOnMatchView))]
    internal static class OnlineLobbyMatchHooks
    {
        [HarmonyPostfix]
        [HarmonyPatch("Show")]
        private static void AfterShow(BeautifulGlitch.OnlineLobbyOnMatchView __instance)
        {
            // The match code is announced separately by OnlineCodeHooks.
            var title = __instance.matchTitleLabel != null ? SpeechText.Normalize(__instance.matchTitleLabel.text) : string.Empty;
            var spoken = SpeechText.Normalize(string.Join(". ", new[] { title, ModLocalization.Get("online_lobby") }));
            HookContext.Log?.Info("ONLINE", "MATCH", "Online on-match view shown.", ("title", title));
            HookContext.Speech?.Speak(spoken, SpeechCategory.Event, "OnlineLobbyOnMatchView.Show", true);
        }
    }

    internal static class AccessSpeech
    {
        private static readonly MethodInfo GetPrettyPrintNameMethod = typeof(BeautifulGlitch.PlayerData).GetMethod("GetPrettyPrintName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly FieldInfo PlayerNameField = typeof(BeautifulGlitch.PlayerData).GetField("name", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        private static readonly FieldInfo ShopActiveItemsField = typeof(MonsterCamp.ShopView).GetField("activeItems", BindingFlags.Instance | BindingFlags.NonPublic);

        public static string ResolvePlayerName(BeautifulGlitch.PlayerData playerData)
        {
            if (playerData == null)
            {
                return string.Empty;
            }

            var pretty = GetPrettyPrintNameMethod != null ? SpeechText.Normalize(GetPrettyPrintNameMethod.Invoke(playerData, null) as string) : string.Empty;
            if (!string.IsNullOrWhiteSpace(pretty))
            {
                return pretty;
            }

            return SpeechText.Normalize(PlayerNameField != null ? PlayerNameField.GetValue(playerData) as string : string.Empty);
        }

        public static string BuildPromptSpeech(string speaker, string prompt, string fallbackPrefix)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(speaker))
            {
                return SpeechText.Normalize(fallbackPrefix + ". " + prompt);
            }

            return SpeechText.Normalize(speaker + ". " + prompt);
        }

        public static string ReadTextFromGameObject(GameObject go)
        {
            if (go == null)
            {
                return string.Empty;
            }

            foreach (var component in go.GetComponentsInChildren<Component>(true))
            {
                if (component == null)
                {
                    continue;
                }

                var type = component.GetType();
                var property = type.GetProperty("text", BindingFlags.Instance | BindingFlags.Public);
                if (property == null)
                {
                    continue;
                }

                var value = property.GetValue(component, null) as string;
                var normalized = SpeechText.Normalize(value);
                if (!string.IsNullOrWhiteSpace(normalized))
                {
                    return normalized;
                }
            }

            return string.Empty;
        }

        public static string ReadAllTextFromGameObject(GameObject go)
        {
            if (go == null)
            {
                return string.Empty;
            }

            var parts = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var component in go.GetComponentsInChildren<Component>(true))
            {
                if (component == null)
                {
                    continue;
                }

                var type = component.GetType();
                var property = type.GetProperty("text", BindingFlags.Instance | BindingFlags.Public);
                if (property == null)
                {
                    continue;
                }

                var value = property.GetValue(component, null) as string;
                var normalized = SpeechText.Normalize(value);
                if (string.IsNullOrWhiteSpace(normalized))
                {
                    continue;
                }

                if (string.Equals(normalized, "OK", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (seen.Add(normalized))
                {
                    parts.Add(normalized);
                }
            }

            return parts.Count == 0 ? string.Empty : string.Join(". ", parts);
        }

        public static int ReadIntField(object instance, string fieldName)
        {
            if (instance == null)
            {
                return -1;
            }

            var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null || field.FieldType != typeof(int))
            {
                return -1;
            }

            return (int)field.GetValue(instance);
        }

        public static BeautifulGlitch.ItemProfile GetShopActiveItem(MonsterCamp.ShopView shopView, int index)
        {
            if (shopView == null || ShopActiveItemsField == null)
            {
                return null;
            }

            var items = ShopActiveItemsField.GetValue(shopView) as IList<BeautifulGlitch.ItemProfile>;
            if (items == null || index < 0 || index >= items.Count)
            {
                return null;
            }

            return items[index];
        }

        public static string BuildLocationSpeech(string locationId)
        {
            var cleaned = SpeechText.CleanObjectName(locationId);
            if (string.IsNullOrWhiteSpace(cleaned))
            {
                return string.Empty;
            }

            return ModLocalization.Format("choose_this_location_fmt", cleaned);
        }
    }
}
