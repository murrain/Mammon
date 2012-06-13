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
    public class Barbarian
    {
        [Class(ActorClass.Barbarian)]
        [Behavior(BehaviorType.OutOfCombat)]
        [Priority(2)]
        public static Composite BarbarianBuffs()
        {
            return
                new PrioritySelector(
                Spell.Buff(SNOPower.Barbarian_WarCry),
                // Fury spender out of combat, movement speed = King!
                Spell.Buff(SNOPower.Barbarian_Sprint,
                    require => !ZetaDia.Me.HasBuff(SNOPower.Barbarian_Sprint)
                )
                );
        }

        [Class(ActorClass.Barbarian)]
        [Behavior(BehaviorType.Combat)]
        [Priority(1)]
        public static Composite BarbarianCombat()
        {
            return
                new PrioritySelector(ctx => CombatTargeting.Instance.FirstNpc,

                    Common.CreateWaitWhileIncapacitated(),
                    Common.CreateWaitForAttack(),
                    Common.CreateUsePotion(),

                    BarbarianBuffs(),

                    new Decorator(ctx => ctx != null,
                        new PrioritySelector(

                            // Defence low hp or many attackers.
                            Spell.Buff(SNOPower.Barbarian_IgnorePain,
                                require => ZetaDia.Me.HitpointsCurrentPct <= 25 || Clusters.GetClusterCount(ZetaDia.Me, CombatTargeting.Instance.LastObjects, ClusterType.Radius, 12f) >= 6
                            ),

                            // Pull phase.
                            new Decorator(ctx => ctx != null && ((DiaUnit)ctx).Distance > 15f,
                                 new PrioritySelector(
                                    Spell.CastAtLocation(SNOPower.Barbarian_Leap, ctx => ((DiaUnit)ctx).Position),
                                    Spell.CastAtLocation(SNOPower.Barbarian_FuriousCharge, ctx => ((DiaUnit)ctx).Position),
                                    Spell.CastAtLocation(SNOPower.Barbarian_AncientSpear, ret => CombatTargeting.Instance.FirstNpc.Position),
                                    CommonBehaviors.MoveTo(ctx => ((DiaUnit)ctx).Position, "Moving towards unit")
                                )
                            ),

                            // Revenge.
                            Spell.CastAOESpell(SNOPower.Barbarian_Revenge,
                                require => Zeta.CommonBot.PowerManager.CanCast(SNOPower.Barbarian_Revenge)
                            ),

                            // Buff attack rate!
                            Spell.CastAOESpell(SNOPower.Barbarian_WrathOfTheBerserker,
                                extra => Clusters.GetClusterCount(ZetaDia.Me, CombatTargeting.Instance.LastObjects, ClusterType.Radius, 12f) >= 3
                            ),
                            Spell.Buff(SNOPower.Barbarian_BattleRage,
                                require => !ZetaDia.Me.HasBuff(SNOPower.Barbarian_BattleRage)
                            ),

                            // AOE
                            Spell.CastAOESpell(SNOPower.Barbarian_Earthquake,
                                require => ZetaDia.Me.HitpointsCurrentPct <= 25 || Clusters.GetClusterCount(ZetaDia.Me, CombatTargeting.Instance.LastObjects, ClusterType.Radius, 12f) >= 3
                            ),
                            Spell.CastAOESpell(SNOPower.Barbarian_GroundStomp,
                                extra => Clusters.GetClusterCount(ZetaDia.Me, CombatTargeting.Instance.LastObjects, ClusterType.Radius, 12f) >= 2
                            ),
                            Spell.CastAOESpell(SNOPower.Barbarian_Overpower,
                                require => Clusters.GetClusterCount(ZetaDia.Me, CombatTargeting.Instance.LastObjects, ClusterType.Radius, 12f) >= 2
                            ),


                            // Threatning shout.
                            Spell.CastAOESpell(SNOPower.Barbarian_ThreateningShout,
                                extra => Clusters.GetClusterCount(ZetaDia.Me, CombatTargeting.Instance.LastObjects, ClusterType.Radius, 25f) >= 2
                            ),

                            // Fury spenders.
                            Spell.CastAtLocation(SNOPower.Barbarian_HammerOfTheAncients, ctx => ((DiaUnit)ctx).Position),
                            Spell.CastAtLocation(SNOPower.Barbarian_Rend, ctx => ((DiaUnit)ctx).Position),
                            Spell.CastAtLocation(SNOPower.Barbarian_SeismicSlam, ctx => ((DiaUnit)ctx).Position),

                            // Fury Generators
                            Spell.CastAtLocation(SNOPower.Barbarian_Cleave, ret => CombatTargeting.Instance.FirstNpc.Position),
                            Spell.CastAtLocation(SNOPower.Barbarian_Bash, ret => CombatTargeting.Instance.FirstNpc.Position),
                            Spell.CastAtLocation(SNOPower.Barbarian_Frenzy, ret => CombatTargeting.Instance.FirstNpc.Position)
                        )
                    ),

                    new Action(ret => RunStatus.Success)
                    );
        }

        public static void BarbarianOnLevelUp(object sender, EventArgs e)
        {
            if (ZetaDia.Me.ActorClass != ActorClass.Barbarian)
                return;

            int myLevel = ZetaDia.Me.Level;

            Logger.Write("Player leveled up, congrats! Your level is now: {0}",
                myLevel
                );

            if (myLevel == 2)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Barbarian_HammerOfTheAncients, -1, 1);
                Logger.Write("Setting Hammer of the Ancients as Secondary");
            }

            if (myLevel == 3)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Barbarian_Cleave, -1, 0);
                Logger.Write("Setting Cleave as Primary");
            }

            if (myLevel == 4)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Barbarian_GroundStomp, -1, 2);
                Logger.Write("Setting Ground Stomp as Defensive");
            }

            if (myLevel == 7)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Barbarian_HammerOfTheAncients, 1, 1);
                Logger.Write("Setting Rolling Thunder rune for Hammer of the Ancients as Secondary");
            }

            if (myLevel == 8)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Barbarian_Leap, -1, 2);
                Logger.Write("Setting Leap as Defensive");
            }

            if (myLevel == 9)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Barbarian_AncientSpear, -1, 3);
                Logger.Write("Setting Ancient Spear as Might");
            }

            if (myLevel == 10)
            {
                ZetaDia.Me.SetTraits(SNOPower.Barbarian_Passive_Ruthless);
                Logger.Write("Setting Ruthless as Passive Skill");
            }

            if (myLevel == 13)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Barbarian_Revenge, -1, 3);
                Logger.Write("Setting Revenge as Might");
            }

            if (myLevel == 14)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Barbarian_Leap, 1, 2);
                Logger.Write("Setting Iron Impact rune for Leap as Defensive");
            }

            if (myLevel == 19)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Barbarian_Revenge, 1, 3);
                Logger.Write("Setting Vengance Is Mine rune for Revenge as Might");
            }

            if (myLevel == 30)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Barbarian_WrathOfTheBerserker, -1, 5);
                Logger.Write("Setting Ruthless as Passive Skill");
            }
        }
    }
}
