using System;
using HarmonyLib;

namespace mt2_freecompany.Plugin;

// The mechanic runs in code, so native card text cannot infer its rules.
// Read the installed path's localized description for cards and upgrade previews.
[HarmonyPatch(typeof(CardState), nameof(CardState.GenerateCardBodyText))]
internal static class BeastmasterCardText
{
    private static void Postfix(CardState __instance, ref string generatedText)
    {
        if (__instance.GetAssetName() != MyPluginInfo.PLUGIN_GUID + "-Card-SpawnVesper") return;
        var upgrades = __instance.GetCardStateModifiers()?.GetCardUpgrades();
        if (upgrades == null) return;
        for (int level = 3; level >= 1; level--)
        {
            string name = MyPluginInfo.PLUGIN_GUID + "-Upgrade-upg_Beastmaster" + level;
            foreach (var upgrade in upgrades)
            {
                if (upgrade == null || upgrade.GetAssetName() != name) continue;
                var source = upgrade.GetSourceCardUpgradeData();
                if (source == null) continue;
                string body = upgrade.GetUpgradeDescriptionKey().Localize(
                    new CardEffectLocalizationContext(source, null, __instance));
                if (string.IsNullOrWhiteSpace(body)) return;
                if (generatedText == null || generatedText.IndexOf(body, StringComparison.Ordinal) < 0)
                    generatedText = string.IsNullOrWhiteSpace(generatedText) ? body : generatedText + "\n" + body;
                return;
            }
        }
    }
}

// 2026-10-08-1912||codex-freecompany-fx||C:\Users\david\AppData\Roaming\Thunderstore Mod Manager\DataFolder\MonsterTrain2\profiles\Default\BepInEx\plugins\David-FreeCompany\src\code\BeastmasterCardText.cs||muestra reglas localizadas del nivel Beastmaster instalado en cuerpo de carta Vesper y previews, sin duplicados
