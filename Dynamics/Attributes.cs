using System;

using Zeta.Internals.Actors;

namespace Mammon.Dynamics
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    internal sealed class PriorityAttribute : Attribute
    {
        public PriorityAttribute(int thePriority)
        {
            PriorityLevel = thePriority;
        }

        public int PriorityLevel { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    internal sealed class ClassAttribute : Attribute
    {
        // I know this is .NET 4.0. But for some reason, this is throwing attribute format exceptions when used with a default
        // argument. So this specific ctor overload is to prevent issues finding the correct class/advclass combos.
        public ClassAttribute(ActorClass @class)
        {
            CharacterClass = @class;
        }

        public ActorClass CharacterClass { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    internal sealed class IgnoreBehaviorCountAttribute : Attribute
    {
        public IgnoreBehaviorCountAttribute(BehaviorType type)
        {
            Type = type;
        }

        public BehaviorType Type { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    internal sealed class BehaviorAttribute : Attribute
    {
        public BehaviorAttribute(BehaviorType type)
        {
            Type = type;
        }

        public BehaviorType Type { get; private set; }
    }
}
