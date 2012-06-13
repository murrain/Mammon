using System;
using System.Linq;
using Mammon.Dynamics;
using Mammon.Helpers;
using Zeta;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
using Zeta.TreeSharp;
using Action = Zeta.TreeSharp.Action;

namespace Mammon.Routines
{
    public class DemonHunter
    {
        [Class(ActorClass.DemonHunter)]
        [Behavior(BehaviorType.OutOfCombat)]
        [Priority(2)]
        public static Composite DemonHunterBuffs()
        {
            return
                new PrioritySelector(

                Spell.Buff(SNOPower.DemonHunter_Companion,
                    req => !HasCompanion
                )

                // Movement speed = King!
                // Vault disabled untill i can come up with method to ensure porting is "safe".
                /*
                Spell.CastAtLocation(SNOPower.DemonHunter_Vault,
                    ctx => ZetaDia.Me.Position,
                    require => !ZetaDia.Me.HasBuff(SNOPower.DemonHunter_Vault)
                )
                */

                );
        }

        [Class(ActorClass.DemonHunter)]
        [Behavior(BehaviorType.Combat)]
        [Priority(1)]
        public static Composite DemonHunterCombat()
        {
            return
               new PrioritySelector(ctx => CombatTargeting.Instance.FirstNpc,

                    Common.CreateWaitWhileIncapacitated(),
                    Common.CreateWaitForAttack(),
                    Common.CreateUsePotion(),

                    DemonHunterBuffs(),

                    new Decorator(ctx => ctx != null,
                       new PrioritySelector(
                            new Decorator(ctx => ctx != null && ((DiaUnit)ctx).Distance > 30f,
                                Movement.MoveTo(ctx => ((DiaUnit)ctx).Position, 15f)
                            ),

                            // AOE
                            Spell.CastAOESpell(SNOPower.DemonHunter_RainOfVengeance,
                                extra => Clusters.GetClusterCount(ZetaDia.Me, CombatTargeting.Instance.LastObjects, ClusterType.Radius, 45f) >= 3
                            ),
                            Spell.CastAtLocation(SNOPower.DemonHunter_Strafe,
                                ctx => ((DiaUnit)ctx).Position,
                                req => Clusters.GetClusterCount(ZetaDia.Me, CombatTargeting.Instance.LastObjects, ClusterType.Radius, 45f) >= 2
                            ),
                            Spell.CastAtLocation(SNOPower.DemonHunter_Multishot,
                                ctx => ((DiaUnit)ctx).Position,
                                extra => Clusters.GetClusterCount(ZetaDia.Me, CombatTargeting.Instance.LastObjects, ClusterType.Radius, 45f) >= 2
                            ),
                            Spell.CastAtLocation(SNOPower.DemonHunter_Chakram,
                                ctx => ((DiaUnit)ctx).Position,
                                extra => Clusters.GetClusterCount(ZetaDia.Me, CombatTargeting.Instance.LastObjects, ClusterType.Radius, 25f) >= 2
                            ),
                            Spell.CastAOESpell(SNOPower.DemonHunter_Grenades,
                                req => Clusters.GetClusterCount(ZetaDia.Me, CombatTargeting.Instance.LastObjects, ClusterType.Radius, 25f) >= 2
                            ),

                            // Singles
                            Spell.CastAtLocation(SNOPower.DemonHunter_Impale, ctx => ((DiaUnit)ctx).Position),

                            // Hatred Generators
                            Spell.CastAtLocation(SNOPower.DemonHunter_EvasiveFire, ctx => ((DiaUnit)ctx).Position, req => ((DiaUnit)req).Distance < 10f),
                            Spell.CastAtLocation(SNOPower.DemonHunter_HungeringArrow, ctx => ((DiaUnit)ctx).Position),
                            Spell.CastAtLocation(SNOPower.DemonHunter_BolaShot, ctx => ((DiaUnit)ctx).Position), 
                            Spell.CastAtLocation(SNOPower.DemonHunter_EntanglingShot, ctx => ((DiaUnit)ctx).Position)
                       )
                   ),

                   new Action(ret => RunStatus.Success)
                   );
        }

        public static bool HasCompanion
        {
            get
            {
                int dynId = ZetaDia.Me.CommonData.DynamicId;
                //DH_companion_spider DH_Companion_Ferret DH_Companion_RuneE (BAT) DH_companion_Boar DH_Companion_RuneC (WOLF)
                var companion = ZetaDia.Actors.GetActorsOfType<DiaUnit>().FirstOrDefault(u => u.SummonedByACDId == dynId && (u.Name.Contains("DH_companion_spider") || u.Name.Contains("DH_Companion_Ferret") || u.Name.Contains("DH_Companion_RuneE") || u.Name.Contains("DH_companion_Boar") || u.Name.Contains("DH_Companion_RuneC")));
                return companion != null;
            }
        }

        public static void DemonHunterOnLevelUp(object sender, EventArgs e)
        {
            if (ZetaDia.Me.ActorClass != ActorClass.DemonHunter)
                return;

            int myLevel = ZetaDia.Me.Level;

            Logger.Write("Player leveled up, congrats! Your level is now: {0}",
                myLevel
                );

            if (myLevel == 2)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.DemonHunter_Impale, -1, 1);
                Logger.Write("Setting Impale as Secondary skill");
            }

            if (myLevel == 4)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.DemonHunter_Caltrops, -1, 2);
                Logger.Write("Setting Caltrops as Defensive skill");
            }

            if (myLevel == 5)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.DemonHunter_RapidFire, -1, 1);
                Logger.Write("Setting Rapid Fire as Secondary skill");
            }

            if (myLevel == 8)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.DemonHunter_SmokeScreen, -1, 1);
                Logger.Write("Setting Smoke Screen as Defensive skill");
            }

            if (myLevel == 9)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.DemonHunter_Vault, -1, 3);
                Logger.Write("Setting Vault as Hunting skill");
            }
        }
    }
}
