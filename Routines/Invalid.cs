using Mammon.Dynamics;
using Zeta.Internals.Actors;
using Zeta.TreeSharp;

namespace Mammon.Routines
{
    public class Invalid
    {
        [Class(ActorClass.Invalid)]
        [Behavior(BehaviorType.All)]
        public static Composite InvalidWrapper()
        {
            return new PrioritySelector(
                new Action(ret => Logger.Write("Bot have run into a problem, most likely bot was started while being in the menu, or restarted with a plugin. Currently Mammon All-In-One does not support this."))
                );
        }
    }
}
