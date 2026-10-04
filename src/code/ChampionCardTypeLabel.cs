using System.Linq;
using HarmonyLib;

namespace mt2_freecompany.Plugin;

// Native GetLocalizedSubtypes produces no text with more than three translated
// subtypes. Keep role/species subtypes for mechanics; display the native champion
// label on these two cards only, including champion upgrade previews.
[HarmonyPatch(typeof(CardState), nameof(CardState.GetCardTypeCardText))]
internal static class ChampionCardTypeLabel
{
    private static void Postfix(CardState __instance, ref string outCardText)
    {
        string asset = __instance.GetAssetName();
        if (asset != MyPluginInfo.PLUGIN_GUID + "-Card-SpawnRoderic"
            && asset != MyPluginInfo.PLUGIN_GUID + "-Card-SpawnVesper") return;
        var champion = __instance.GetSpawnCharacterData()?.GetSubtypes()
            .FirstOrDefault(subtype => subtype != null && subtype.IsChampion);
        string? label = champion?.LocalizedName;
        if (!string.IsNullOrEmpty(label)) outCardText = label!;
    }
}

// 2026-10-04-0628||codex-freecompany-fx||src/code/ChampionCardTypeLabel.cs||muestra subtipo Champion nativo localizado para Roderic y Vesper; conserva subtipos de roles
