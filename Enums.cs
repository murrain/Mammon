using System;

namespace Mammon
{
    public enum ClusterType
    {
        Radius,
        Chained,
        //Cone
    }

    public enum CastOn
    {
        Never,
        Bosses,
        Players,
        All,
    }

    [Flags]
    public enum GameContext
    {
        None = 0,
        Normal = 0x1,
        Instances = 0x2,
        Battlegrounds = 0x4,

        All = Normal | Instances | Battlegrounds,
    }

    [Flags]
    public enum BehaviorType
    {
        OutOfCombat = 0x1,
        Pull = 0x8,
        Combat = 0x40,

        All = OutOfCombat|Pull|Combat
    }
}
