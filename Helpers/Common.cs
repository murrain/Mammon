using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zeta;
using Zeta.Common.Helpers;
using Zeta.Internals.Actors;
using Zeta.TreeSharp;
using Action = Zeta.TreeSharp.Action;

namespace Mammon.Helpers
{
    public static class Common
    {
        public static Composite CreateWaitWhileIncapacitated()
        {
            return
                new Decorator(ret => 
                    ZetaDia.Me.IsFeared ||
                    ZetaDia.Me.IsStunned ||
                    ZetaDia.Me.IsFrozen ||
                    ZetaDia.Me.IsBlind ||
                    ZetaDia.Me.IsRooted, 

                    new Action(ret => RunStatus.Success)
                );
        }

        public static Composite CreateWaitForAttack()
        {
            return 
                new Decorator(ret => ZetaDia.Me.CommonData.AnimationState == AnimationState.Attacking,
                    new Action(ret => RunStatus.Success)
                );
                
        }

        private static WaitTimer _potionCooldownTimer = WaitTimer.ThirtySeconds;
        public static Composite CreateUsePotion()
        {
            return
                new Decorator(ret => ZetaDia.Me.HitpointsCurrentPct <= 0.25 && _potionCooldownTimer.IsFinished,
                    new PrioritySelector(ctx => ZetaDia.Me.Inventory.Backpack.FirstOrDefault(i => i.IsPotion),

                        new Decorator(ctx => ctx != null,
                            new Sequence(
                                new Action(ctx => ZetaDia.Me.Inventory.UseItem(((ACDItem)ctx).DynamicId)),
                                new Action(ctx => _potionCooldownTimer.Reset()),
                                new Action(ctx => Logger.Write("Health is low, using health potion"))
                //new Action(ctx => Logger.Write("Using health potion at {0}% health.", (int)ZetaDia.Me.HitpointsCurrentPct))
                                )
                            )
                        )
                    );
        }
    }
}
