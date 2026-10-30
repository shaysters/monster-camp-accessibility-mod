using HarmonyLib;
using System.Reflection;

namespace GameMod.Toolkit.Hooks
{
    [HarmonyPatch(typeof(MonsterCamp.MetaShopItemsView))]
    internal static class MetaShopItemHoverHooks
    {
        private static readonly FieldInfo ItemProfileField = typeof(MonsterCamp.MetaShopItemButton).GetField("itemProfile", BindingFlags.Instance | BindingFlags.NonPublic);

        [HarmonyPostfix]
        [HarmonyPatch("HoverOverItemButton")]
        private static void AfterHoverOverItemButton(MonsterCamp.MetaShopItemsView __instance, MonsterCamp.MetaShopItemButton button)
        {
            var itemProfile = GetItemProfile(button);
            if (__instance == null || button == null || itemProfile == null)
            {
                return;
            }

            var title = __instance.shopInfoPanel != null ? SpeechText.Normalize(__instance.shopInfoPanel.title.text) : string.Empty;
            var description = __instance.shopInfoPanel != null ? SpeechText.Normalize(__instance.shopInfoPanel.description.text) : string.Empty;
            var price = __instance.shopInfoPanel != null && __instance.shopInfoPanel.priceTag != null
                ? SpeechText.Normalize(__instance.shopInfoPanel.priceTag.priceLabel.text)
                : string.Empty;
            var currency = itemProfile.useHalloweenCash ? "Halloween memories" : "memories";
            var gameProgress = GetGameProgress();
            var availableCash = itemProfile.useHalloweenCash
                ? GetCashValue(gameProgress, "GetMetaShopCashPointsHalloween")
                : GetCashValue(gameProgress, "GetMetaShopCashPoints");
            var canAfford = gameProgress != null && itemProfile.price <= availableCash;

            var spoken = MetaShopSpeech.BuildItemSpeech(title, description, price, currency, canAfford);
            HookContext.Log?.Info(
                "SHOP",
                "ITEM",
                "Meta Shop item hovered.",
                ("id", itemProfile.id),
                ("title", title),
                ("price", price),
                ("currency", currency),
                ("canAfford", canAfford.ToString()));
            HookContext.Speech?.Speak(spoken, SpeechCategory.Focus, "MetaShopItemsView.HoverOverItemButton", true);
        }

        private static BeautifulGlitch.MetashopItemProfile GetItemProfile(MonsterCamp.MetaShopItemButton button)
        {
            if (button == null || ItemProfileField == null)
            {
                return null;
            }

            return ItemProfileField.GetValue(button) as BeautifulGlitch.MetashopItemProfile;
        }

        private static object GetDataManager()
        {
            var generalManager = BeautifulGlitch.GeneralManager.Instance;
            if (generalManager == null)
            {
                return null;
            }

            var property = generalManager.GetType().GetProperty("dataManager", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return property != null ? property.GetValue(generalManager, null) : null;
        }

        private static BeautifulGlitch.GameProgress GetGameProgress()
        {
            var dataManager = GetDataManager();
            if (dataManager == null)
            {
                return null;
            }

            var field = dataManager.GetType().GetField("gameProgress", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                return field.GetValue(dataManager) as BeautifulGlitch.GameProgress;
            }

            var property = dataManager.GetType().GetProperty("gameProgress", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return property != null ? property.GetValue(dataManager, null) as BeautifulGlitch.GameProgress : null;
        }

        private static int GetCashValue(BeautifulGlitch.GameProgress gameProgress, string methodName)
        {
            if (gameProgress == null)
            {
                return 0;
            }

            var method = gameProgress.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null)
            {
                return 0;
            }

            return (int)method.Invoke(gameProgress, null);
        }
    }

    [HarmonyPatch(typeof(MonsterCamp.MetaShopBuyItemPopup))]
    internal static class MetaShopPopupHooks
    {
        [HarmonyPostfix]
        [HarmonyPatch("ShowPopup")]
        private static void AfterShowPopup(MonsterCamp.MetaShopBuyItemPopup __instance, BeautifulGlitch.MetashopItemProfile aMetaShopItemData, System.Action<BeautifulGlitch.MetashopItemProfile> aOnConfirmPurchase, System.Action aOnCancelPurchase)
        {
            if (__instance == null || aMetaShopItemData == null)
            {
                return;
            }

            var title = SpeechText.Normalize(__instance.popupTitleLabel != null ? __instance.popupTitleLabel.text : string.Empty);
            HookContext.Log?.Info("SHOP", "POPUP", "Meta Shop buy popup shown.", ("id", aMetaShopItemData.id), ("text", title));
            HookContext.Speech?.Speak(title, SpeechCategory.Event, "MetaShopBuyItemPopup.ShowPopup", true);
        }
    }

    [HarmonyPatch(typeof(MonsterCamp.MetaShopUIManager))]
    internal static class MetaShopCashHooks
    {
        [HarmonyPostfix]
        [HarmonyPatch("UpdateCurrentCashLabel")]
        private static void AfterUpdateCurrentCashLabel(MonsterCamp.MetaShopUIManager __instance)
        {
            if (__instance == null)
            {
                return;
            }

            var normalCash = SpeechText.Normalize(__instance.currentCashLabel != null ? __instance.currentCashLabel.text : string.Empty);
            var halloweenCash = SpeechText.Normalize(__instance.currentCashHalloweenLabel != null ? __instance.currentCashHalloweenLabel.text : string.Empty);
            HookContext.Log?.Info("SHOP", "CASH", "Meta Shop cash labels updated.", ("cash", normalCash), ("halloweenCash", halloweenCash));
        }
    }

    internal static class MetaShopSpeech
    {
        public static string BuildItemSpeech(string title, string description, string price, string currency, bool canAfford)
        {
            var affordability = canAfford ? "Affordable." : "Not enough currency.";
            if (string.IsNullOrWhiteSpace(price))
            {
                return SpeechText.Normalize(title + ". " + description + ". " + affordability);
            }

            return SpeechText.Normalize(title + ". Costs " + price + " " + currency + ". " + description + ". " + affordability);
        }
    }
}
