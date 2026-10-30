using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using GameMod.Toolkit.Localization;

namespace GameMod.Toolkit.Hooks
{
    [HarmonyPatch(typeof(MonsterCamp.TitleMenuManager))]
    internal static partial class FullTitleMenuHooks
    {
        private static readonly MethodInfo GetSmartHoverTargetMethod = typeof(BeautifulGlitch.ButtonGroup).GetMethod("GetSmartHoverTarget", BindingFlags.Instance | BindingFlags.NonPublic);

        [HarmonyPostfix]
        [HarmonyPatch("HoverMainButton")]
        private static void AfterHoverMainButton(MonsterCamp.TitleMenuManager __instance, bool fromSettings, bool fromCustomize)
        {
            var defaultButton = __instance.mainButtonGroup != null ? __instance.mainButtonGroup.defaultButtonHandler : null;
            var button = InvokeGetSmartHoverTarget(__instance.mainButtonGroup, false);
            if (button == null)
            {
                button = fromSettings ? __instance.settingsButton : (!fromCustomize ? defaultButton : __instance.customizeButton);
            }

            FocusAnnouncement.TryAnnounceButton(button, "TitleMenuManager.HoverMainButton");
        }
    }

    [HarmonyPatch(typeof(MonsterCamp.PlayersNumberView))]
    internal static class FullPlayersNumberHooks
    {
        [HarmonyPostfix]
        [HarmonyPatch("Show")]
        private static void AfterShow(MonsterCamp.PlayersNumberView __instance)
        {
            var button = FullTitleMenuHooks.InvokeGetSmartHoverTarget(__instance.playersButtonGroup, true);
            if (button == null && __instance.playersButtonGroup != null)
            {
                button = __instance.playersButtonGroup.defaultButtonHandler;
            }

            FocusAnnouncement.TryAnnounceButton(button, "PlayersNumberView.Show");
        }
    }

    [HarmonyPatch(typeof(MonsterCamp.GameModeView))]
    internal static class FullGameModeHooks
    {
        [HarmonyPostfix]
        [HarmonyPatch("Show")]
        private static void AfterShow(MonsterCamp.GameModeView __instance)
        {
            var field = typeof(MonsterCamp.GameModeView).GetField("currentButtonGroup", BindingFlags.Instance | BindingFlags.NonPublic);
            var group = field != null ? field.GetValue(__instance) as BeautifulGlitch.ButtonGroup : null;
            var button = FullTitleMenuHooks.InvokeGetSmartHoverTarget(group, true);
            if (button == null && group != null)
            {
                button = group.defaultButtonHandler;
            }

            FocusAnnouncement.TryAnnounceButton(button, "GameModeView.Show");
        }
    }

    internal static partial class FullTitleMenuHooks
    {
        internal static BeautifulGlitch.ButtonHandler InvokeGetSmartHoverTarget(BeautifulGlitch.ButtonGroup group, bool returnDefaultIfNone)
        {
            if (group == null || GetSmartHoverTargetMethod == null)
            {
                return null;
            }

            return GetSmartHoverTargetMethod.Invoke(group, new object[] { returnDefaultIfNone }) as BeautifulGlitch.ButtonHandler;
        }
    }

    [HarmonyPatch(typeof(BeautifulGlitch.ButtonHandler))]
    internal static class FullButtonHandlerHooks
    {
        [HarmonyPostfix]
        [HarmonyPatch("PlayHoverOver")]
        private static void AfterPlayHoverOver(BeautifulGlitch.ButtonHandler __instance)
        {
            FocusAnnouncement.TryAnnounceButton(__instance, "ButtonHandler.PlayHoverOver");
        }
    }

    [HarmonyPatch(typeof(BeautifulGlitch.BGButton))]
    internal static class FullBgButtonHooks
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnSelect")]
        private static void AfterOnSelect(BeautifulGlitch.BGButton __instance, UnityEngine.EventSystems.BaseEventData eventData)
        {
            if (__instance != null)
            {
                FocusAnnouncement.TryAnnounceGameObject(__instance.gameObject, "BGButton.OnSelect");
            }
        }
    }

    internal static class FocusAnnouncement
    {
        private static int _lastInstanceId;
        private static string _lastLabel = string.Empty;
        private static bool _playerSelectPromptPending = true;

        public static void TryAnnounceButton(BeautifulGlitch.ButtonHandler button, string source)
        {
            if (button == null)
            {
                HookContext.Log?.Info("FOCUS", "HOOK", "Button handler was null.", ("source", source));
                return;
            }

            TryAnnounceGameObject(button.gameObject, source);
        }

        internal static void TryAnnounceGameObject(GameObject go, string source)
        {
            if (HookContext.Speech == null || HookContext.Log == null || go == null)
            {
                return;
            }

            var path = BuildPath(go.transform);
            if (ShouldSuppressGenericChoiceFocus(go, path))
            {
                HookContext.Log.Info(
                    "FOCUS",
                    "HOOK",
                    "Suppressed generic choice focus in favor of screen-specific speech.",
                    ("source", source),
                    ("object", path));
                return;
            }

            var label = ExtractLabel(go);
            var isPlayerSelectFocus = path.IndexOf("View_PlayersSelect/", StringComparison.OrdinalIgnoreCase) >= 0
                && (string.Equals(go.name, "ButtonPlayerA", StringComparison.Ordinal)
                    || string.Equals(go.name, "ButtonPlayerB", StringComparison.Ordinal)
                    || string.Equals(go.name, "ButtonPlayerC", StringComparison.Ordinal)
                    || string.Equals(go.name, "ButtonPlayerD", StringComparison.Ordinal));

            HookContext.Log.Info(
                "FOCUS",
                "HOOK",
                "Focus announcement candidate.",
                ("source", source),
                ("object", path),
                ("label", label),
                ("active", go.activeInHierarchy.ToString()));

            if (string.IsNullOrWhiteSpace(label))
            {
                return;
            }

            if (!isPlayerSelectFocus)
            {
                _playerSelectPromptPending = true;
            }
            else if (_playerSelectPromptPending)
            {
                _playerSelectPromptPending = false;
                HookContext.Log.Info("PLAYER", "SELECTLIVE", "Live player select entry detected from focus.", ("source", source), ("object", path));
                HookContext.Speech.Speak(ModLocalization.Get("choose_your_character"), SpeechCategory.Event, "PlayerSelect.LiveFocusEntry", true);
                return;
            }

            var instanceId = go.GetInstanceID();
            if (instanceId == _lastInstanceId && string.Equals(label, _lastLabel, StringComparison.Ordinal))
            {
                return;
            }

            _lastInstanceId = instanceId;
            _lastLabel = label;
            HookContext.Speech.Speak(label, SpeechCategory.Focus, source, interrupt: true);
        }

        private static bool ShouldSuppressGenericChoiceFocus(GameObject go, string path)
        {
            if (go == null || string.IsNullOrEmpty(go.name) || !go.name.StartsWith("ChoiceBox", StringComparison.Ordinal))
            {
                return false;
            }

            return path.IndexOf("View_IceBreakers/", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string ExtractLabel(GameObject go)
        {
            var name = go.name ?? string.Empty;

            if (go.GetComponent<MonsterCamp.MetaShopItemButton>() != null)
            {
                return string.Empty;
            }

            if (go.GetComponent<MonsterCamp.BackpackItemButton>() != null)
            {
                return string.Empty;
            }

            if (go.GetComponent<MonsterCamp.ItemButton>() != null)
            {
                return string.Empty;
            }

            if (go.GetComponent<BeautifulGlitch.EndingIllustration>() != null)
            {
                return string.Empty;
            }

            if (go.GetComponent<BeautifulGlitch.BGButton>() != null)
            {
                var bgLabel = TryBuildBgButtonPrompt(go);
                if (!string.IsNullOrWhiteSpace(bgLabel))
                {
                    return bgLabel;
                }
            }

            if (name.StartsWith("PlayerIcon", StringComparison.Ordinal) && name.Length > "PlayerIcon".Length)
            {
                return ModLocalization.Format("player_customize_fmt", name.Substring("PlayerIcon".Length));
            }

            switch (name)
            {
                case "Button_1P":
                    return ModLocalization.Get("player_1");
                case "Button_2P":
                    return ModLocalization.Get("player_2");
                case "Button_3P":
                    return ModLocalization.Get("player_3");
                case "Button_4P":
                    return ModLocalization.Get("player_4");
                case "PageDownButton":
                    return ModLocalization.Get("next_page");
                case "PageUpButton":
                    return ModLocalization.Get("previous_page");
                case "PromoButton2":
                case "PromoButton3":
                    return string.Empty;
                case "NextPageButton":
                    return ModLocalization.Get("next_page");
                case "PrevPageButton":
                    return ModLocalization.Get("previous_page");
                case "SettingsBackButton":
                    return ModLocalization.Get("back");
                case "MetaShopBackButton":
                    return ModLocalization.Get("back");
                case "ButtonPlayerA":
                    return ModLocalization.Get("player_a");
                case "ButtonPlayerB":
                    return ModLocalization.Get("player_b");
                case "ButtonPlayerC":
                    return ModLocalization.Get("player_c");
                case "ButtonPlayerD":
                    return ModLocalization.Get("player_d");
                case "RandomButton":
                    return ModLocalization.Get("pack_random_items");
                case "OkButton":
                    return ModLocalization.Get("confirm_character");
                case "CustomNameButton":
                    return ModLocalization.Get("custom_name");
                case "SkillsButton":
                    return ModLocalization.Get("use_your_skills");
                case "DrinkOfTheDayButton":
                    return ModLocalization.Get("drink_of_the_day");
                case "MysteryBoxButton":
                    return ModLocalization.Get("mystery_drink_box");
            }

            if (name.StartsWith("ChoiceBox", StringComparison.Ordinal))
            {
                var indexText = name.Substring("ChoiceBox".Length);
                int index;
                if (int.TryParse(indexText, out index))
                {
                    return ModLocalization.Format("option_number_fmt", index + 1);
                }
            }

            if (name == "Button_SingleShort" || name == "Button_MultiShort")
            {
                return ModLocalization.Format("short_game_fmt", ReadGameModeDuration(go));
            }

            if (name == "Button_SingleLong" || name == "Button_MultiLong" || name == "Button_SingleRegular" || name == "Button_MultiRegular")
            {
                return ModLocalization.Format("regular_game_fmt", ReadGameModeDuration(go));
            }

            if (name == "NameButton")
            {
                var current = ReadBestChildText(go);
                return string.IsNullOrWhiteSpace(current) ? ModLocalization.Get("name_activate") : ModLocalization.Format("name_current_fmt", current);
            }

            if (name == "PronounButton")
            {
                var current = ReadBestChildText(go);
                return string.IsNullOrWhiteSpace(current) ? ModLocalization.Get("pronouns_activate") : ModLocalization.Format("pronouns_current_fmt", current);
            }

            if (name.StartsWith("NamePredefined", StringComparison.Ordinal))
            {
                var current = ReadBestChildText(go);
                return string.IsNullOrWhiteSpace(current) ? ModLocalization.Get("name_option") : ModLocalization.Format("name_option_fmt", current);
            }

            var settingsPrompt = TryBuildSettingsPrompt(go);
            if (!string.IsNullOrWhiteSpace(settingsPrompt))
            {
                return settingsPrompt;
            }

            var customizePrompt = TryBuildCustomizePrompt(go);
            if (!string.IsNullOrWhiteSpace(customizePrompt))
            {
                return customizePrompt;
            }

            foreach (var component in go.GetComponentsInChildren<Component>(true))
            {
                if (component == null)
                {
                    continue;
                }

                var text = ReadText(component);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return SpeechText.Normalize(text);
                }
            }

            return SpeechText.CleanObjectName(name);
        }

        private static string ReadBestChildText(GameObject go)
        {
            foreach (var component in go.GetComponentsInChildren<Component>(true))
            {
                if (component == null)
                {
                    continue;
                }

                var text = ReadText(component);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return SpeechText.Normalize(text);
                }
            }

            return string.Empty;
        }

        private static string ReadText(Component component)
        {
            var type = component.GetType();
            var fullName = type.FullName ?? type.Name;
            if (fullName != "UILabel" && fullName != "UnityEngine.UI.Text" && fullName != "TMPro.TextMeshProUGUI")
            {
                return null;
            }

            var property = type.GetProperty("text", BindingFlags.Instance | BindingFlags.Public);
            if (property == null)
            {
                return null;
            }

            return property.GetValue(component, null) as string;
        }

        private static string BuildPath(Transform transform)
        {
            if (transform == null)
            {
                return string.Empty;
            }

            var path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }

            return path;
        }

        private static string ReadGameModeDuration(GameObject go)
        {
            foreach (var component in go.GetComponentsInChildren<Component>(true))
            {
                if (component == null)
                {
                    continue;
                }

                var text = ReadText(component);
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                var normalized = SpeechText.Normalize(text);
                if (normalized.Contains("minute"))
                {
                    return normalized;
                }
            }

            return string.Empty;
        }

        private static string TryBuildSettingsPrompt(GameObject go)
        {
            var parent = go.transform != null ? go.transform.parent : null;
            if (parent == null)
            {
                return string.Empty;
            }

            var widget = parent.GetComponent<BeautifulGlitch.SettingsWidget>();
            if (widget == null)
            {
                return string.Empty;
            }

            var title = widget.titleLabel != null ? SpeechText.Normalize(widget.titleLabel.text) : string.Empty;
            var value = widget.valueLabel != null ? SpeechText.Normalize(widget.valueLabel.text) : string.Empty;
            if (string.IsNullOrWhiteSpace(title))
            {
                return string.Empty;
            }

            if (widget.leftArrow == go)
            {
                return string.IsNullOrWhiteSpace(value) ? title + ". " + ModLocalization.Get("previous_option") : title + ". " + value + ". " + ModLocalization.Get("previous_option");
            }

            if (widget.rightArrow == go)
            {
                return string.IsNullOrWhiteSpace(value) ? title + ". " + ModLocalization.Get("next_option") : title + ". " + value + ". " + ModLocalization.Get("next_option");
            }

            return string.IsNullOrWhiteSpace(value) ? title : title + ". " + value;
        }

        private static string TryBuildBgButtonPrompt(GameObject go)
        {
            var name = go.name ?? string.Empty;
            if (name.IndexOf("Create", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return ModLocalization.Get("create_match");
            }

            if (name.IndexOf("Join", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return ModLocalization.Get("join_match");
            }

            if (name.IndexOf("Ready", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return ModLocalization.Get("ready");
            }

            if (name.IndexOf("AddLocal", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return ModLocalization.Get("add_local_player");
            }

            return string.Empty;
        }

        private static string TryBuildCustomizePrompt(GameObject go)
        {
            var triggerWidget = FindComponentInParents<MonsterCamp.TriggerWarningWidget>(go.transform);
            if (triggerWidget != null)
            {
                var title = triggerWidget.titleLabel != null ? SpeechText.Normalize(triggerWidget.titleLabel.text) : string.Empty;
                var value = triggerWidget.valueLabel != null ? SpeechText.Normalize(triggerWidget.valueLabel.text) : string.Empty;
                var currentCollider = go.GetComponent("Collider");
                var widgetType = triggerWidget.GetType();
                var leftCollider = widgetType.GetField("leftArrowCollider", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(triggerWidget);
                var rightCollider = widgetType.GetField("rightArrowCollider", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(triggerWidget);

                if (currentCollider != null)
                {
                    if (ReferenceEquals(leftCollider, currentCollider))
                    {
                        return BuildDirectionalPrompt(title, value, "Previous option");
                    }

                    if (ReferenceEquals(rightCollider, currentCollider))
                    {
                        return BuildDirectionalPrompt(title, value, "Next option");
                    }
                }

                return BuildDirectionalPrompt(title, value, null);
            }

            var customizeWidget = FindComponentInParents<MonsterCamp.CustomizeWidget>(go.transform);
            if (customizeWidget != null)
            {
                var title = SpeechText.CleanObjectName(customizeWidget.type.ToString());
                var value = customizeWidget.valueLabel != null ? SpeechText.Normalize(customizeWidget.valueLabel.text) : string.Empty;
                var direction = (go.name ?? string.Empty).IndexOf("LeftArrow", StringComparison.OrdinalIgnoreCase) >= 0
                    ? ModLocalization.Get("previous_option")
                    : (go.name ?? string.Empty).IndexOf("RightArrow", StringComparison.OrdinalIgnoreCase) >= 0
                        ? ModLocalization.Get("next_option")
                        : null;

                return BuildDirectionalPrompt(title, value, direction);
            }

            return string.Empty;
        }

        private static string BuildDirectionalPrompt(string title, string value, string direction)
        {
            title = SpeechText.Normalize(title);
            value = SpeechText.Normalize(value);
            direction = SpeechText.Normalize(direction);

            if (string.IsNullOrWhiteSpace(title))
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(direction))
            {
                return string.IsNullOrWhiteSpace(value) ? title : title + ". " + value;
            }

            return string.IsNullOrWhiteSpace(value)
                ? title + ". " + direction
                : title + ". " + value + ". " + direction;
        }

        private static T FindComponentInParents<T>(Transform transform) where T : Component
        {
            while (transform != null)
            {
                var component = transform.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }

                transform = transform.parent;
            }

            return null;
        }
    }
}
