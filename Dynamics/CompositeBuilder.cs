using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Zeta.Internals.Actors;
using Zeta.TreeSharp;

namespace Mammon.Dynamics
{
    public static class CompositeBuilder
    {
        private static readonly List<MethodInfo> Methods = new List<MethodInfo>();

        public static Composite GetComposite(ActorClass actorClass, BehaviorType behavior, out int behaviourCount)
        {
            behaviourCount = 0;
            if (Methods.Count <= 0)
            {
                Logger.Write("Building method list");
                foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
                {
                    Methods.AddRange(type.GetMethods(BindingFlags.Static | BindingFlags.Public));
                }
                Logger.Write("Added " + Methods.Count + " methods");
            }
            var matchedMethods = new Dictionary<int, PrioritySelector>();

            foreach (MethodInfo mi in
                Methods.Where(
                    mi =>
                    !mi.IsGenericMethod &&
                    mi.GetParameters().Length == 0)
                    .Where(
                        mi =>
                        mi.ReturnType == typeof(Composite) ||
                        mi.ReturnType.IsSubclassOf(typeof(Composite))))
            {
                //Logger.WriteDebug("[CompositeBuilder] Checking attributes on " + mi.Name);
                bool classMatches = false;
                bool behaviorMatches = false;
                bool hasIgnore = false;
                int thePriority = 0;
                var theBehaviourType = BehaviorType.All;
                var theIgnoreType = BehaviorType.All;
                try
                {
                    foreach (object ca in mi.GetCustomAttributes(false))
                    {
                        if (ca is ClassAttribute)
                        {
                            var attrib = ca as ClassAttribute;
                            // Class specific
                            if (attrib.CharacterClass != actorClass)
                            {
                                //Logger.Write("Attribute Class does not match. [" + attrib.SpecificClass + " != " + torClass + "]");
                                continue;
                            }
                            
                            //Logger.WriteDebug(mi.Name + " has my class");
                            classMatches = true;
                        }
                        else if (ca is BehaviorAttribute)
                        {
                            var attrib = ca as BehaviorAttribute;
                            if ((attrib.Type & behavior) == 0)
                            {
                                continue;
                            }
                            //Logger.WriteDebug(mi.Name + " has my behavior");
                            theBehaviourType = attrib.Type;
                            behaviourCount++;
                            behaviorMatches = true;
                        }
                        else if (ca is PriorityAttribute)
                        {
                            var attrib = ca as PriorityAttribute;
                            thePriority = attrib.PriorityLevel;
                        }
                        else if (ca is IgnoreBehaviorCountAttribute)
                        {
                            var attrib = ca as IgnoreBehaviorCountAttribute;
                            hasIgnore = true;
                            theIgnoreType = attrib.Type;
                        }
                    }
                }
                catch
                {
                    Logger.Write("Error getting custom attributes for " + mi.Name);
                    continue;
                }

                if (behaviorMatches && hasIgnore && theBehaviourType == theIgnoreType)
                {
                    behaviourCount--;
                }

                // If all our attributes match, then mark it as wanted!
                if (classMatches && behaviorMatches)
                {
                    Logger.Write("{0} is a match!", mi.Name);
                    Logger.Write("Using {0} for {1} (Priority: {2})", mi.Name, behavior, thePriority);
                    Composite matched;
                    try
                    {
                        matched = (Composite)mi.Invoke(null, null);
                    }
                    catch (Exception e)
                    {
                        Logger.Write("ERROR Creating composite: {0}\n{1}", mi.Name, e.StackTrace);
                        continue;
                    }
                    if (!matchedMethods.ContainsKey(thePriority))
                    {
                        matchedMethods.Add(thePriority, new PrioritySelector(matched));
                    }
                    else
                    {
                        matchedMethods[thePriority].AddChild(matched);
                    }
                }
            }
            // If we found no methods, rofls!
            if (matchedMethods.Count <= 0)
            {
                return null;
            }

            // Return the composite match we found. (Note: ANY composite return is fine)
            return matchedMethods.OrderByDescending(mm => mm.Key).First().Value;
        }
    }
}
