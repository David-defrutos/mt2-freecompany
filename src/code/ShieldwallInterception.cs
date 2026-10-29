using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace mt2_freecompany.Plugin;

// Intercept after the original unit's native Damage Shield stage, before Armor.
// Native damage resolution still owns defenses, HP, death, Undying and preview.
internal static class ShieldwallInterception
{
    internal const string TooltipId = "mt2_freecompany.ShieldwallIntercept";

    internal static CardUpgradeData? FindUpgrade(CardState? card, CharacterState? unit, AllGameData data)
    {
        if (card == null || card.GetAssetName() != MyPluginInfo.PLUGIN_GUID + "-Card-SpawnRoderic") return null;
        for (int tier = 3; tier >= 1; --tier)
        {
            string name = MyPluginInfo.PLUGIN_GUID + "-Upgrade-upg_Wall" + tier;
            var upgrade = data.GetAllCardUpgradeData().FirstOrDefault(u => u != null && u.name == name);
            if (upgrade != null && (card.HasUpgrade(upgrade) || (unit != null && unit.HasUpgrade(upgrade)))) return upgrade;
        }
        return null;
    }

    internal static bool IsEnemyAttack(CharacterState target, CharacterState.ApplyDamageParams parameters)
        => target != null && target.GetTeamType() == Team.Type.Monsters
            && parameters.attacker != null && parameters.attacker.GetTeamType() == Team.Type.Heroes
            && Damage.CheckTypeCompatibleWithOnAttackingTrigger(parameters.damageType);

    internal static CharacterState? FindInterceptor(CharacterState target, ICoreGameManagers managers)
    {
        if (target == null || target.IsDestroyed || target.IsDead || target.GetHP() <= 0
            || target.HasStatusEffect("untouchable")) return null;
        var room = target.GetCurrentRoom();
        var point = target.GetSpawnPoint();
        if (room == null || point == null) return null;
        var allies = new List<CharacterState>();
        room.AddCharactersToList(allies, Team.Type.Monsters);
        CharacterState? first = null;
        int firstIndex = int.MaxValue;
        foreach (var unit in allies)
        {
            if (unit == null || unit == target || unit.IsDestroyed || unit.IsDead || unit.GetHP() <= 0
                || unit.PreviewMode != target.PreviewMode || unit.HasStatusEffect("silenced")
                || unit.HasStatusEffect("muted")) continue;
            var position = unit.GetSpawnPoint();
            if (position == null) continue;
            int index = position.GetIndexInRoom();
            if (index >= point.GetIndexInRoom() || index >= firstIndex) continue;
            if (FindUpgrade(unit.GetSpawnerCard(), unit, managers.GetAllGameData()) == null) continue;
            first = unit;
            firstIndex = index;
        }
        return first;
    }

    internal sealed class AttackResult
    {
        internal int Damage, Sustained, Blocked, Unmodified, LifeLink;
    }

    internal sealed class Frame
    {
        internal CharacterState Target = null!;
        internal CharacterState.ApplyDamageParams Parameters;
        internal PlayerManager Player = null!;
        internal CardStatistics Statistics = null!;
        internal ICoreGameManagers Managers = null!;
        internal bool Transferred, IsTransfer, UsedCalculation, BeforeAttackRan;
        internal int Raw;
        internal Frame? Source;
        internal AttackResult? Result;
    }

    [ThreadStatic] private static Frame? current;
    [ThreadStatic] private static Frame? pendingTransfer;

    // Scope each MoveNext, including child iterators. No global context is left
    // active across a Unity wait or while a concurrent preview is executing.
    internal sealed class ScopedRoutine : IEnumerator, IDisposable
    {
        private readonly IEnumerator inner;
        private readonly Frame frame;
        public object? Current { get; private set; }
        internal ScopedRoutine(IEnumerator inner, Frame frame) { this.inner = inner; this.frame = frame; }
        public bool MoveNext()
        {
            var previous = current;
            current = frame;
            try
            {
                bool moved = inner.MoveNext();
                object? value = moved ? inner.Current : null;
                Current = value is IEnumerator child && value is not ScopedRoutine ? new ScopedRoutine(child, frame) : value;
                return moved;
            }
            finally { current = previous; }
        }
        public void Reset() => throw new NotSupportedException();
        public void Dispose()
        {
            var previous = current;
            current = frame;
            try { (inner as IDisposable)?.Dispose(); }
            finally { current = previous; }
        }
    }

    private static ICoreGameManagers? Managers()
    {
        var all = AllGameManagers.Instance;
        return all != null && all.AreMainSceneOnlyManagersInitialized() ? all.GetCoreManagers() : null;
    }

    internal static IEnumerator AfterShield(IEnumerator original, CharacterState unit,
        StatusEffectState.InputTriggerParams input, StatusEffectState.OutputTriggerParams output)
    {
        yield return original;
        var source = current;
        if (source == null || source.IsTransfer || source.Transferred || source.Target != unit
            || source.Parameters.attacker != input.attacker || source.Parameters.damageType != input.damageType
            || !IsEnemyAttack(unit, source.Parameters) || output.damage <= 0) yield break;
        var receiver = FindInterceptor(unit, source.Managers);
        if (receiver == null) yield break;
        int raw = output.damage;
        source.Transferred = true;
        output.damage = 0;
        output.visualDamage = 0;
        output.damageBlocked += raw;
        var parameters = source.Parameters;
        // Do not allow native Attract Damage to redirect an already intercepted hit.
        parameters.fromAttractDamageTrigger = true;
        var transfer = new Frame { Target = receiver, Parameters = parameters, Player = source.Player,
            Statistics = source.Statistics, Managers = source.Managers, Raw = raw, IsTransfer = true, Source = source };
        // ApplyDamage adds this room modifier itself. The attack's bonus calculation
        // already ran once on the original target; neither must be counted twice.
        int roomBonus = receiver.GetCurrentRoom().GetRoomStateModifiedDamage(parameters.attacker!,
            parameters.damageType, false, source.Managers);
        var previous = pendingTransfer;
        IEnumerator damage;
        pendingTransfer = transfer;
        try { damage = receiver.ApplyDamage(raw - roomBonus, parameters, source.Player, source.Statistics); }
        finally { pendingTransfer = previous; }
        if (!receiver.PreviewMode)
            receiver.GetCharacterStateThatHasMesh().GetCharacterUI().OnAnimEvent(CharacterUI.AnimNote.TakeDamage);
        yield return damage;
    }

    private static readonly Func<CharacterState, CharacterState, Damage.Type, IEnumerator> NativeBeforeAttack
        = AccessTools.MethodDelegate<Func<CharacterState, CharacterState, Damage.Type, IEnumerator>>(
            AccessTools.Method(typeof(CharacterState), "AttackingBeforeDamage"));

    private static IEnumerator Empty() { yield break; }

    [HarmonyPatch(typeof(CharacterState), nameof(CharacterState.ApplyDamage))]
    private static class DamageScopePatch
    {
        private static void Postfix(CharacterState __instance, CharacterState.ApplyDamageParams damageParams,
            PlayerManager playerManager, CardStatistics cardStatistics, ref IEnumerator __result)
        {
            var transfer = pendingTransfer;
            if (transfer != null && transfer.Target == __instance)
            {
                pendingTransfer = null;
                __result = new ScopedRoutine(__result, transfer);
                return;
            }
            var managers = Managers();
            if (managers == null) return;
            if (current == null && (!IsEnemyAttack(__instance, damageParams)
                || FindInterceptor(__instance, managers) == null)) return;
            __result = new ScopedRoutine(__result, new Frame { Target = __instance, Parameters = damageParams,
                Player = playerManager, Statistics = cardStatistics, Managers = managers });
        }
    }

    [HarmonyPatch(typeof(StatusEffectManager), nameof(StatusEffectManager.TriggerStatusEffectOnUnit))]
    private static class ShieldStagePatch
    {
        private static void Postfix(StatusEffectData.TriggerStage triggerStage, CharacterState characterState,
            StatusEffectState.InputTriggerParams inputTriggerParams, StatusEffectState.OutputTriggerParams outputTriggerParams,
            ref IEnumerator __result)
        {
            if (triggerStage == StatusEffectData.TriggerStage.OnPreAttackedDamageShield)
                __result = AfterShield(__result, characterState, inputTriggerParams, outputTriggerParams);
        }
    }

    [HarmonyPatch(typeof(CharacterState), "CalcBonusDamage")]
    private static class SingleCalculationPatch
    {
        private static bool Prefix(CharacterState __instance, ref int __result)
        {
            var frame = current;
            if (frame == null || !frame.IsTransfer || frame.Target != __instance || frame.UsedCalculation) return true;
            frame.UsedCalculation = true;
            __result = frame.Raw;
            return false;
        }
    }

    [HarmonyPatch(typeof(CharacterState), "AttackingBeforeDamage")]
    private static class BeforeAttackPatch
    {
        private static bool Prefix(CharacterState __instance, CharacterState attackedCharacter,
            Damage.Type damageType, ref IEnumerator __result)
        {
            var frame = current;
            if (frame == null || frame.Target != attackedCharacter || frame.Parameters.attacker != __instance) return true;
            if (!frame.IsTransfer)
            {
                if (!frame.BeforeAttackRan) return true;
                __result = Empty();
                return false;
            }
            // Keep Before Damage effects on the original target, but run them at
            // the native time, before HP is committed on the interceptor.
            frame.Source!.BeforeAttackRan = true;
            __result = NativeBeforeAttack(__instance, frame.Source.Target, damageType);
            return false;
        }
    }

    [HarmonyPatch(typeof(CharacterState), "Attacking")]
    private static class AttackEffectsPatch
    {
        private static bool Prefix(CharacterState __instance, CharacterState attackedCharacter, ref int damage,
            ref int damageSustained, ref int damageBlocked, ref int unmodifiedDamage,
            ref int damagePreventedByLifeLink, ref IEnumerator __result)
        {
            var frame = current;
            if (frame == null || frame.Target != attackedCharacter || frame.Parameters.attacker != __instance) return true;
            if (!frame.IsTransfer)
            {
                if (frame.Result is AttackResult result)
                {
                    damage = result.Damage; damageSustained = result.Sustained; damageBlocked = result.Blocked;
                    unmodifiedDamage = result.Unmodified; damagePreventedByLifeLink = result.LifeLink;
                }
                return true;
            }
            frame.Source!.Result = new AttackResult { Damage = damage, Sustained = damageSustained,
                Blocked = damageBlocked, Unmodified = frame.Raw, LifeLink = damagePreventedByLifeLink };
            // Attack debuffs/OnAttacking run once on the original target later.
            // Slay belongs to the attacker if the interceptor actually dies.
            __result = attackedCharacter.IsDead
                ? frame.Managers.GetCombatManager().QueueAndRunTrigger(__instance, CharacterTriggerData.Trigger.OnKill, attackedCharacter)
                : Empty();
            return false;
        }
    }

    [HarmonyPatch(typeof(CharacterState), "DoAttackedStatusEffects")]
    private static class OriginalTargetReactionsPatch
    {
        private static bool Prefix(CharacterState __instance, ref IEnumerator __result)
        {
            var frame = current;
            if (frame == null || !frame.IsTransfer || frame.Target != __instance) return true;
            // Spikes and other OnAttacked effects remain on the actual attack target.
            __result = Empty();
            return false;
        }
    }

    internal static TooltipContent RulesTooltip()
        => new((MyPluginInfo.PLUGIN_GUID + "_InterceptTitle").Localize(),
            (MyPluginInfo.PLUGIN_GUID + "_InterceptRules").Localize(),
            TooltipDesigner.TooltipDesignType.Keyword, TooltipId);

    [HarmonyPatch(typeof(CardTooltipContainer), "AddTooltipsForCardState")]
    private static class CardRulesPatch
    {
        private static void Postfix(CardTooltipContainer __instance, CardState cardState, SaveManager saveManager)
        {
            if (cardState == null || saveManager == null || __instance.HasTooltip(TooltipId)) return;
            var data = saveManager.GetAllGameData();
            if (data != null && FindUpgrade(cardState, null, data) != null) __instance.ShowTooltip(RulesTooltip());
        }
    }

    [HarmonyPatch(typeof(CharacterTooltipHelper), nameof(CharacterTooltipHelper.GetAdditionalTooltipContents))]
    private static class UnitRulesPatch
    {
        private static void Postfix(CharacterState characterState, SaveManager saveManager,
            ref IEnumerable<(TooltipContent content, bool allowDuplicates)> __result)
            => __result = Append(__result, characterState, saveManager);

        internal static IEnumerable<(TooltipContent content, bool allowDuplicates)> Append(
            IEnumerable<(TooltipContent content, bool allowDuplicates)> original, CharacterState unit, SaveManager save)
        {
            bool shown = false;
            foreach (var item in original) { if (item.content.tooltipId == TooltipId) shown = true; yield return item; }
            if (shown || unit == null || save == null) yield break;
            var data = save.GetAllGameData();
            if (data != null && FindUpgrade(unit.GetSpawnerCard(), unit, data) != null) yield return (RulesTooltip(), false);
        }
    }
}

// 2026-10-29-1950||codex-freecompany-fx||src/code/ShieldwallInterception.cs||intercepta dano tras Damage Shield nativo solo detras; conserva ataques y aflicciones en objetivo, defensas/muerte/preview nativos y tooltips localizados

// 2026-10-29-1957||codex-freecompany-fx||src/code/ShieldwallInterception.cs||conserva momento y objetivo de Before Damage mediante metodo nativo; evita duplicarlo y elimina aviso nullable validado
