using System.Collections;

namespace mt2_freecompany.Plugin
{
    /// <summary>
    /// Delays Double Shift's Dazed until the start of the next player turn.
    /// One status instance holds all uses, so repeated plays do not add unit triggers.
    /// </summary>
    public sealed class StatusEffectFreeCompanyDizzyState : StatusEffectState
    {
        public override bool TestTrigger(InputTriggerParams input, OutputTriggerParams output, ICoreGameManagers core)
        {
            var unit = GetAssociatedCharacter();
            return unit != null && unit.GetStatusEffectStacks(GetStatusId()) > 0;
        }

        protected override IEnumerator OnTriggered(InputTriggerParams input, OutputTriggerParams output, ICoreGameManagers core)
        {
            var unit = GetAssociatedCharacter();
            if (unit == null)
            {
                yield break;
            }

            var stacks = unit.GetStatusEffectStacks(GetStatusId());
            if (stacks <= 0)
            {
                yield break;
            }

            unit.RemoveStatusEffect(GetStatusId(), stacks, allowModification: false);
            unit.AddStatusEffect("dazed", stacks, allowModification: false);
        }
    }
}
