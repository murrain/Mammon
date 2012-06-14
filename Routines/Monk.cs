using System;
using System.Linq;
using Mammon.Dynamics;
using Mammon.Helpers;
using Zeta;
using Zeta.Common;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
using Zeta.TreeSharp;


/*
 *  Credits to Nuok whom are the author of this routine.
 */

namespace Mammon.Routines
{
    public class Monk
    {
        [Class(ActorClass.Monk)]
        [Behavior(BehaviorType.Combat)]
        public static Composite MonkCombat()
        {
            return
                new PrioritySelector(ctx => CombatTargeting.Instance.FirstNpc,

                    Common.CreateWaitWhileIncapacitated(),
                    Common.CreateWaitForAttack(),
                    Common.CreateUsePotion(),

                    new Decorator(ctx => ctx != null && ((DiaUnit)ctx).Distance > 20f,
                        CommonBehaviors.MoveTo(ctx => ((DiaUnit)ctx).Position, "Moving towards unit")),

                        //Heals
                        Spell.Buff(SNOPower.Monk_Serenity, extra => ZetaDia.Me.HitpointsCurrentPct <= 0.6),
                        Spell.CastAOESpell(SNOPower.Monk_BreathOfHeaven,
                            extra => ZetaDia.Me.HitpointsCurrentPct <= 0.4 || !ZetaDia.Me.HasBuff(SNOPower.Monk_BreathOfHeaven)),


                        //Buffs
                        Mantra(),
                        Spell.Buff(SNOPower.Monk_MysticAlly, extra => !HasMysticAlly),
                        Spell.Buff(SNOPower.Monk_SweepingWind, extra => !ZetaDia.Me.HasBuff(SNOPower.Monk_SweepingWind)),

                        //Spells to move closer 
                        Spell.CastOnUnit(SNOPower.Monk_FistsofThunder, 1,
                            ctx => ((DiaUnit)ctx).ACDGuid,
                            extra => CombatTargeting.Instance.FirstNpc.Distance > 20f),
                        Spell.CastOnUnit(SNOPower.Monk_DashingStrike,
                            1, ctx => ((DiaUnit)ctx).ACDGuid,
                            extra => CombatTargeting.Instance.FirstNpc.Distance > 15f),


                        //Focus Skills
                        Spell.CastAOESpell(SNOPower.Monk_CycloneStrike,
                            extra => Clusters.GetClusterCount(ZetaDia.Me, CombatTargeting.Instance.LastObjects, ClusterType.Radius, 20f) >= 3),
                        Spell.CastAOESpell(SNOPower.Monk_BlindingFlash,
                            extra => CombatTargeting.Instance.FirstNpc.Distance <= 18f),
                        Spell.CastAOESpell(SNOPower.Monk_SevenSidedStrike,
                            extra => Clusters.GetClusterCount(ZetaDia.Me, CombatTargeting.Instance.LastObjects, ClusterType.Radius, 20f) >= 3),

                        //Secondary
                        Spell.CastOnUnit(SNOPower.Monk_LashingTailKick, 1,
                            ctx => ((DiaUnit)ctx).ACDGuid,
                            extra => Clusters.GetClusterCount(ZetaDia.Me, CombatTargeting.Instance.LastObjects, ClusterType.Radius, 20f) >= 4),
                        Spell.CastOnUnit(SNOPower.Monk_TempestRush, 1,
                            ctx => ((DiaUnit)ctx).ACDGuid,
                            extra => CombatTargeting.Instance.FirstNpc.Distance > 20f),
                        Spell.CastOnUnit(SNOPower.Monk_WaveOfLight, 1,
                            ctx => ((DiaUnit)ctx).ACDGuid),

                        // Primary Skills. 
                        Spell.CastOnUnit(SNOPower.Monk_DeadlyReach, 1, ctx => ((DiaUnit)ctx).ACDGuid),
                        Spell.CastOnUnit(SNOPower.Monk_CripplingWave, 1, ctx => ((DiaUnit)ctx).ACDGuid),
                        Spell.CastOnUnit(SNOPower.Monk_WayOfTheHundredFists, 1, ctx => ((DiaUnit)ctx).ACDGuid),
                        Spell.CastOnUnit(SNOPower.Monk_FistsofThunder, 1, ctx => ((DiaUnit)ctx).ACDGuid)
                        )
                    ;
        }

        private static Composite Mantra()
        {
            return new PrioritySelector(
                Spell.Buff(SNOPower.Monk_MantraOfEvasion, extra => !ZetaDia.Me.HasBuff(SNOPower.Monk_MantraOfEvasion)),
                Spell.Buff(SNOPower.Monk_MantraOfConviction, extra => !ZetaDia.Me.HasBuff(SNOPower.Monk_MantraOfConviction)),
                Spell.Buff(SNOPower.Monk_MantraOfHealing, extra => !ZetaDia.Me.HasBuff(SNOPower.Monk_MantraOfHealing)),
                Spell.Buff(SNOPower.Monk_MantraOfRetribution, extra => !ZetaDia.Me.HasBuff(SNOPower.Monk_MantraOfRetribution))
                );
        }


        public static bool HasMysticAlly
        {
            get
            {
                int dynId = ZetaDia.Me.CommonData.DynamicId;
                var summoned = ZetaDia.Actors.GetActorsOfType<DiaUnit>().FirstOrDefault(u => u.SummonedByACDId == dynId && u.Name.Contains("mysticAlly"));
                return summoned != null;
            }
        }

        public static void MonkOnLevelUp(object sender, EventArgs e)
        {
            if (ZetaDia.Me.ActorClass != ActorClass.Monk)
                return;

            int myLevel = ZetaDia.Me.Level;

            Logger.Write("Player leveled up, congrats! Your level is now: {0}",
                myLevel
                );

            // Set Lashing tail kick once we reach level 2
            if (myLevel == 2)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Monk_LashingTailKick, -1, 1);
                Logger.Write("Setting Lash Tail Kick as Secondary");
            }

            // Set Dead reach it's better then Fists of thunder imo.
            if (myLevel == 3)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Monk_DeadlyReach, -1, 0);
                Logger.Write("Setting Deadly Reach as Primary");
            }

            // Make sure we set binding flash, useful spell in crowded situations!
            if (myLevel == 4)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Monk_BlindingFlash, -1, 2);
                Logger.Write("Setting Binding Flash as Defensive");
            }

            // Binding flash is nice but being alive is even better!
            if (myLevel == 8)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Monk_BreathOfHeaven, -1, 2);
                Logger.Write("Setting Breath of Heaven as Defensive");
            }

            // Make sure we set Dashing strike, very cool and useful spell great opener.
            if (myLevel == 9)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Monk_DashingStrike, -1, 3);
                Logger.Write("Setting Dashing Strike as Techniques");
            }
        }
    }
}