using System;
using Mammon.Dynamics;
using Mammon.Helpers;
using Zeta;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
using Zeta.TreeSharp;
using Action = Zeta.TreeSharp.Action;
using System.Linq;
namespace Mammon.Routines
{
    public class WitchDoctor
    {

        private static int ClusterCount
        {
            get { return Clusters.GetClusterCount(CombatTargeting.Instance.FirstNpc, CombatTargeting.Instance.LastObjects, ClusterType.Radius, 60f); }
        }

        private static int nearbycount { get { return Clusters.GetClusterCount(ZetaDia.Me, CombatTargeting.Instance.LastObjects, ClusterType.Radius, 40f); } }

        private static int zombieDogs
        {
            get
            {
                int dynId = ZetaDia.Me.CommonData.DynamicId;
                var summoned = ZetaDia.Actors.GetActorsOfType<DiaUnit>().Count(u => u.SummonedByACDId == dynId && u.ActorSNO == 51353);
                return summoned;
            }
        }

        /// <summary>
        /// He is ted, for he is awesome.
        /// </summary>
        public static bool Ted
        {
            get
            {
                int dynId = ZetaDia.Me.CommonData.DynamicId;
                var summoned = ZetaDia.Actors.GetActorsOfType<DiaUnit>().FirstOrDefault(u => u.SummonedByACDId == dynId && u.ActorSNO == 122305);
                return summoned != null;
            }
        }

        public static DiaObject aoe
        {
            get
            {
                return Clusters.GetBestUnitForCluster(ZetaDia.Actors.GetActorsOfType<DiaUnit>().Where(u => u.IsNPC), ClusterType.Radius, 20);
            }
        }

        [Class(ActorClass.WitchDoctor)]
        [Behavior(BehaviorType.Combat)]
        public static Composite WitchDoctorCombat()
        {
            return new PrioritySelector(ctx => CombatTargeting.Instance.FirstNpc,

                    Common.CreateWaitWhileIncapacitated(),
                    Common.CreateWaitForAttack(),
                    Common.CreateUsePotion(),

                    // Make sure we are within range/line of sight of the unit.
                    Movement.MoveTo(ctx => ((DiaUnit)ctx).Position, 30f),
                    //Movement.MoveToLineOfSight(ctx => (DiaUnit)ctx),

                    Spell.Buff(SNOPower.Witchdoctor_SpiritWalk, extra => ZetaDia.Me.HitpointsCurrentPct <= 0.4),

                     //Pets
                    Spell.Buff(SNOPower.Witchdoctor_Gargantuan, extra => !Ted),
                    Spell.Buff(SNOPower.Witchdoctor_SummonZombieDog, extra => zombieDogs < 3),
                    Spell.Buff(SNOPower.Witchdoctor_Hex),



                    Spell.Buff(SNOPower.Witchdoctor_SoulHarvest, extra => nearbycount >= 4),
                    Spell.Buff(SNOPower.Witchdoctor_BigBadVoodoo, extra => nearbycount >= 4),
                    Spell.Buff(SNOPower.Witchdoctor_FetishArmy, extra => nearbycount >= 4),
                    Spell.Buff(SNOPower.Witchdoctor_Horrify, extra => nearbycount >= 4),

                    Spell.CastAtLocation(SNOPower.Witchdoctor_AcidCloud, ctx => ((DiaUnit)ctx).Position, extra => ClusterCount >= 3),
                    Spell.CastAtLocation(SNOPower.Witchdoctor_Firebats, ctx => ((DiaUnit)ctx).Position, extra => ClusterCount >= 3),
                    Spell.CastAtLocation(SNOPower.Witchdoctor_WallOfZombies, ctx => ((DiaUnit)ctx).Position, extra => ClusterCount >= 3),

                    Spell.CastOnUnit(SNOPower.Witchdoctor_GraspOfTheDead, 1, ctx => ((DiaUnit)ctx).ACDGuid),
                    Spell.CastOnUnit(SNOPower.Witchdoctor_Firebomb, 1, ctx => ((DiaUnit)ctx).ACDGuid),
                    Spell.CastOnUnit(SNOPower.Witchdoctor_PoisonDart, 1, ctx => ((DiaUnit)ctx).ACDGuid),

                    //Other spells
                    Spell.CastOnUnit(SNOPower.Witchdoctor_Locust_Swarm, 1, ctx => ((DiaUnit)ctx).ACDGuid),
                    Spell.CastOnUnit(SNOPower.Witchdoctor_MassConfusion, 1, ctx => ((DiaUnit)ctx).ACDGuid),
                    Spell.CastOnUnit(SNOPower.Witchdoctor_SpiritBarrage, 1, ctx => ((DiaUnit)ctx).ACDGuid),
                    Spell.CastOnUnit(SNOPower.Witchdoctor_ZombieCharger, 1, ctx => ((DiaUnit)ctx).ACDGuid),
                    Spell.CastAtLocation(SNOPower.Witchdoctor_PlagueOfToads, ctx => ((DiaUnit)ctx).Position),
                    Spell.CastAtLocation(SNOPower.Witchdoctor_CorpseSpider, ctx => ((DiaUnit)ctx).Position)
                );

            //Spell.CastAtLocation(SNOPower.Witchdoctor_PoisonDart, ctx => ((DiaUnit)ctx).Position)
        }


        public static void WitchDoctorOnLevelUp(object sender, EventArgs e)
        {
            if (ZetaDia.Me.ActorClass != ActorClass.WitchDoctor)
                return;

            int myLevel = ZetaDia.Me.Level;

            Logger.Write("Player leveled up, congrats! Your level is now: {0}",
                myLevel
                );

            // Set Lashing tail kick once we reach level 2
            if (myLevel == 2)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Witchdoctor_GraspOfTheDead, -1, 1);
                Logger.Write("Setting Grasp of the Dead as Secondary");
            }

            // Set Dead reach it's better then Fists of thunder imo.
            if (myLevel == 3)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Witchdoctor_CorpseSpider, -1, 0);
                Logger.Write("Setting Grasp of the Dead as Secondary");
            }

            // Make sure we set binding flash, useful spell in crowded situations!
            if (myLevel == 4)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Witchdoctor_SummonZombieDog, -1, 2);
                Logger.Write("Setting Summon Zombie Dogs as Defensive");
            }

            // Make sure we set Dashing strike, very cool and useful spell great opener.
            if (myLevel == 9)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Witchdoctor_SoulHarvest, -1, 3);
                Logger.Write("Setting Sould Harvest as Terror");
            }

            if (myLevel == 10)
            {
                ZetaDia.Me.SetTraits(SNOPower.Witchdoctor_Passive_JungleFortitude);
            }
            if (myLevel == 13)
            {
                ZetaDia.Me.SetTraits(SNOPower.Witchdoctor_Passive_SpiritualAttunement);
            }
        }
    }
}