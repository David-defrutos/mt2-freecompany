using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace mt2_freecompany.Plugin;

// The animated quad, not the large reference SpriteRenderer, is the visible art.
// Scope every patch to our two champions and resolve the optional renderer by name.
internal static class ChampionSpriteVisuals
{
    private static readonly Type? Animated = AccessTools.TypeByName("TrainworksReloaded.Base.Prefab.CharacterUIMeshAnimatedSprite");
    private static readonly Type? Finalizer = AccessTools.TypeByName("TrainworksReloaded.Base.Prefab.GameObjectCharacterArtFinalizer");
    internal static bool ArtName(string name) => name.IndexOf("mt2_freecompany", StringComparison.OrdinalIgnoreCase) >= 0
        && (name.IndexOf("VesperCharacterArt", StringComparison.Ordinal) >= 0
            || name.IndexOf("RodericCharacterArt", StringComparison.Ordinal) >= 0);
    private static bool Ours(Component component)
    {
        for (var t = component.transform; t != null; t = t.parent)
            if (ArtName(t.name)) return true;
        var ui = component.GetComponentInParent<CharacterUI>();
        var sprite = ui == null ? null : ui.GetComponent<SpriteRenderer>()?.sprite;
        return sprite != null && ArtName(sprite.name);
    }
    private static Component? Mesh(CharacterUI ui)
    {
        if (Animated == null) return null;
        var mesh = ui.GetComponentInChildren(Animated, true) as Component;
        return mesh != null && Ours(mesh) ? mesh : null;
    }
    internal static Bounds InParentSpace(Bounds b, Transform mesh, Transform parent)
    {
        var bounds = new Bounds(parent.InverseTransformPoint(mesh.TransformPoint(b.center)), Vector3.zero);
        var min = b.min; var max = b.max;
        for (int x = 0; x < 2; ++x)
            for (int y = 0; y < 2; ++y)
                for (int z = 0; z < 2; ++z)
                    bounds.Encapsulate(parent.InverseTransformPoint(mesh.TransformPoint(new Vector3(
                        x == 0 ? min.x : max.x, y == 0 ? min.y : max.y, z == 0 ? min.z : max.z))));
        return bounds;
    }
    [HarmonyPatch]
    private static class PrefabPatch
    {
        private static bool Prepare() => Finalizer != null && AccessTools.Method(Finalizer, "CreateCharacterWithFrameAnimations") != null;
        private static MethodBase TargetMethod() => AccessTools.Method(Finalizer!, "CreateCharacterWithFrameAnimations");
        private static void Postfix(GameObject original, string name)
        {
            if (!ArtName(name) && !ArtName(original.name)) return;
            var ui = original.transform.Find("CharacterScale/CharacterUI");
            if (ui != null && ui.TryGetComponent<SpriteRenderer>(out var reference)) reference.enabled = false;
        }
    }
    [HarmonyPatch]
    private static class BoundsPatch
    {
        private static bool Prepare() => Animated != null && AccessTools.Method(Animated, "Setup") != null;
        private static MethodBase TargetMethod() => AccessTools.Method(Animated!, "Setup");
        private static void Postfix(Component __instance, ref Bounds meshBounds)
        {
            if (!Ours(__instance)) return;
            var filter = __instance.GetComponent<MeshFilter>();
            var ui = __instance.GetComponentInParent<CharacterUI>();
            if (filter?.sharedMesh != null && ui != null)
            {
                meshBounds = InParentSpace(filter.sharedMesh.bounds, filter.transform, ui.transform);
                var reference = ui.GetComponent<SpriteRenderer>();
                if (reference != null) reference.enabled = false;
            }
        }
    }
    [HarmonyPatch(typeof(CharacterUI), "SetupAnchors")]
    private static class AnchorsPatch
    {
        private static void Postfix(CharacterUI __instance)
        {
            var mesh = Mesh(__instance);
            var filter = mesh?.GetComponent<MeshFilter>();
            if (mesh == null || filter?.sharedMesh == null) return;
            var container = __instance.transform.Find("VfxAnchors");
            if (container == null) return;
            var b = InParentSpace(filter.sharedMesh.bounds, filter.transform, __instance.transform);
            var min = b.min; var max = b.max; var c = b.center;
            void Place(string name, float x, float y)
            {
                var anchor = container.Find(name);
                if (anchor != null) anchor.position = __instance.transform.TransformPoint(new Vector3(x, y, c.z));
            }
            Place("VfxCenterAnchor", c.x, c.y);
            Place("VfxTopAnchor", c.x, max.y + 0.15f);
            Place("VfxBottomAnchor", c.x, min.y);
            var facing = (float)AccessTools.Field(typeof(CharacterUI), "_facingDir").GetValue(__instance);
            float front = facing < 0f ? min.x : max.x;
            float back = facing < 0f ? max.x : min.x;
            Place("VfxSideFwdAnchor", front, c.y);
            Place("VfxSideBackAnchor", back, c.y);
            Place("VfxSideFwdBottomAnchor", front, min.y);
            Place("VfxSideBackBottomAnchor", back, min.y);
        }
    }
}

// 2026-10-26-2052||codex-freecompany-fx||C:\Users\david\AppData\Roaming\Thunderstore Mod Manager\DataFolder\MonsterTrain2\profiles\Default\BepInEx\plugins\David-FreeCompany\src\code\ChampionSpriteVisuals.cs||oculta SpriteRenderer de referencia y calcula bounds/anclajes sobre quad animado real de Roderic y Vesper

// 2026-10-26-2053||codex-freecompany-fx||C:\Users\david\AppData\Roaming\Thunderstore Mod Manager\DataFolder\MonsterTrain2\profiles\Default\BepInEx\plugins\David-FreeCompany\src\code\ChampionSpriteVisuals.cs||conserva orientación frontal/trasera nativa en anclajes de campeones
