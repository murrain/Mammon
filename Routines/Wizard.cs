using System;

using Mammon.Dynamics;
using Mammon.Helpers;
using Zeta;
using Zeta.Common;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
using Zeta.TreeSharp;
using Action = Zeta.TreeSharp.Action;

namespace Mammon.Routines
{
    public class Wizard
    {
        private static int ClusterCount { get { return Clusters.GetClusterCount(ZetaDia.Me, CombatTargeting.Instance.LastObjects, ClusterType.Radius, 20f); } }

        [Class(ActorClass.Wizard)]
        [Behavior(BehaviorType.Combat)]
        public static Composite WizardCombat()
        {
            return
                new PrioritySelector(ctx => CombatTargeting.Instance.FirstNpc,

                    Common.CreateWaitWhileIncapacitated(),
                    Common.CreateWaitForAttack(),
                    Common.CreateUsePotion(),

                    // Make sure we are within range/line of sight of the unit.
                    Movement.MoveTo(ctx => ((DiaUnit)ctx).Position, 30f),
                    //Movement.MoveToLineOfSight(ctx => ((RActorUnit)ctx)),

                    // Cast Diamon Skin if health is below 40%
                    Spell.CastAOESpell(SNOPower.Wizard_DiamondSkin, extra => ZetaDia.Me.HitpointsCurrentPct <= 0.4),
                    
                    // AoE spells.
                    Spell.CastAOESpell(SNOPower.Wizard_WaveOfForce, extra => ClusterCount >= 4),
                    Spell.CastAOESpell(SNOPower.Wizard_FrostNova, extra => ClusterCount >= 2),

                    // Arcane power spenders.
                    Spell.CastOnUnit(SNOPower.Wizard_ArcaneOrb, 1, ctx => ((DiaUnit)ctx).ACDGuid),
                    Spell.CastOnUnit(SNOPower.Wizard_RayOfFrost, 1, ctx => ((DiaUnit)ctx).ACDGuid),

                    // Signature spells.
                    Spell.CastOnUnit(SNOPower.Wizard_Electrocute, 1, ctx => ((DiaUnit)ctx).ACDGuid),
                    Spell.CastOnUnit(SNOPower.Wizard_ShockPulse, 1, ctx => ((DiaUnit)ctx).ACDGuid),
                    Spell.CastOnUnit(SNOPower.Wizard_MagicMissile, 1, ctx => ((DiaUnit)ctx).ACDGuid)

                );
        }

        public static void WizardOnLevelUp(object sender, EventArgs e)
        {
            if (ZetaDia.Me.ActorClass != ActorClass.Wizard)
                return;

            int myLevel = ZetaDia.Me.Level;

            Logger.Write("Player leveled up, congrats! Your level is now: {0}",
                myLevel
                );

            // Set Ray of Frost as secondary spell.
            if (myLevel == 2)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Wizard_RayOfFrost, -1, 1);
                Logger.Write("Setting Ray of Frost as Secondary");
            }

            // Set Shock Pulse as primary.
            if (myLevel == 3)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Wizard_ShockPulse, -1, 0);
                Logger.Write("Setting Shock Pulse as Primary");
            }

            if (myLevel == 4)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Wizard_FrostNova, -1, 2);
                Logger.Write("Setting Frost Nova as Defensive");
            }

            if (myLevel == 5)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Wizard_ArcaneOrb, -1, 1);
                Logger.Write("Setting Arcane Orb as Secondary");
            }

            if (myLevel == 8)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Wizard_DiamondSkin, -1, 2);
                Logger.Write("Setting Diamond Skin as Defensive");
            }

            if (myLevel == 9)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Wizard_WaveOfForce, -1, 3);
                Logger.Write("Setting Wave of Force as Force");

                ZetaDia.Me.SetActiveSkill(SNOPower.Wizard_ShockPulse, -1, 2);
                Logger.Write("Changing rune for Shock Pulse: \"Explosive Bolts\"");
            }

            // First passive skill!
            if (myLevel == 10)
            {
                // Blur - Decreases melee damage taken by 20%.
                ZetaDia.Me.SetTraits(SNOPower.Wizard_Passive_Blur);
            }

            if (myLevel == 11)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Wizard_ArcaneOrb, 2, 1);
                Logger.Write("Changing rune for Arcane Orb: \"Obliteration\"");
            }
        }
    }
}
