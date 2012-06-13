using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zeta.Internals.Actors;

namespace Mammon.Helpers
{
    internal static class Clusters
    {
        public static int GetClusterCount(DiaUnit target, IEnumerable<DiaObject> otherUnits, ClusterType type, float clusterRange)
        {
            if (otherUnits.Count() == 0)
                return 0;

            switch (type)
            {
                case ClusterType.Radius:
                    return GetRadiusClusterCount(target, otherUnits, clusterRange);
                case ClusterType.Chained:
                    return GetChainedClusterCount(target, otherUnits, clusterRange);
                //case ClusterType.Cone:
                //    return GetConeClusterCount(target, otherUnits, clusterRange);
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        public static DiaObject GetBestUnitForCluster(IEnumerable<DiaObject> units, ClusterType type, float clusterRange)
        {
            if (units.Count() == 0)
                return null;

            switch (type)
            {
                case ClusterType.Radius:
                    return (from u in units
                            select new { Count = GetRadiusClusterCount(u, units, clusterRange), Unit = u }).OrderByDescending(a => a.Count).
                        FirstOrDefault().Unit;
                case ClusterType.Chained:
                    return (from u in units
                            select new { Count = GetChainedClusterCount(u, units, clusterRange), Unit = u }).OrderByDescending(a => a.Count).
                        FirstOrDefault().Unit;
                // coned doesn't have a best unit, since we are the source
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        //private static int GetConeClusterCount(RActorUnit target, IEnumerable<RActorUnit> otherUnits, float distance)
        //{
        //    var targetLoc = target.Position;
        //    return otherUnits.Count(u => target.IsSafelyFacing(u, 90f) && u.Position.Distance(targetLoc) <= distance); // most (if not all) cone spells are 90 degrees
        //}

        private static int GetRadiusClusterCount(DiaObject target, IEnumerable<DiaObject> otherUnits, float radius)
        {
            var targetLoc = target.Position;
            return otherUnits.Count(u => u.Position.DistanceSqr(targetLoc) <= radius * radius);
        }

        private static int GetChainedClusterCount(DiaObject target, IEnumerable<DiaObject> otherUnits, float chainRange)
        {
            var unitCounters = otherUnits.Select(u => GetUnitsChainWillJumpTo(target, otherUnits.ToList(), chainRange).Count);

            return unitCounters.Max() + 1;
        }

        private static List<DiaObject> GetUnitsChainWillJumpTo(DiaObject target, List<DiaObject> otherUnits, float chainRange)
        {
            var targetLoc = target.Position;
            var targetGuid = target.ACDGuid;
            for (int i = otherUnits.Count - 1; i >= 0; i--)
            {
                if (otherUnits[i].ACDGuid == targetGuid || otherUnits[i].Position.DistanceSqr(targetLoc) > chainRange)
                    otherUnits.RemoveAt(i);
            }
            return otherUnits;
        }
    }
}
