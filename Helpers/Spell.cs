using Zeta;
using Zeta.Common;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
using Zeta.TreeSharp;
using Action = Zeta.TreeSharp.Action;

namespace Mammon.Helpers
{
    //public delegate T ValueRetriever<T>(object context);

    public static class Spell
    {
        /// <summary>
        /// Casts the specified power.
        /// </summary>
        /// <param name="power">The sno power.</param>
        /// <param name="positionRetriver">The position retriver.</param>
        /// <param name="levelAreaRetriever">The level area retriever.</param>
        /// <param name="extraCondition">The extra condition if any.</param>
        /// <param name="powerType">Type of the power.</param>
        /// <param name="onUnit">The target acd GUID retriever.</param>
        /// <returns>
        ///   <c>RunStatus.Success</c> if successful, otherwise <c>RunStatus.Failure</c>.
        /// </returns>
        /// <remarks>Created 2012-04-09</remarks>
        public static Composite Cast(SNOPower power, ValueRetriever<Vector3> positionRetriver, ValueRetriever<int> levelAreaRetriever, int powerType, ValueRetriever<int> onUnit, ValueRetriever<bool> extraCondition)
        {
            return new Decorator(ret =>
            {
                bool canCast = PowerManager.CanCast(power);
                bool minReqs = extraCondition != null ? extraCondition(ret) : true;

                return minReqs && canCast;
            },
            new Action(ctx =>
            {
                Vector3 position = positionRetriver != null ? positionRetriver(ctx) : Vector3.Zero;
                int levelArea = levelAreaRetriever != null ? levelAreaRetriever(ctx) : 0;
                int acdGuid = onUnit != null ? onUnit(ctx) : -1;

                ZetaDia.Me.UsePower(power, position, levelArea, powerType, acdGuid);
            })
            );
        }

        /// <summary>
        /// Casts the specified power.
        /// </summary>
        /// <param name="power">The sno power.</param>
        /// <param name="position">The position.</param>
        /// <param name="extraCondition">The extra condition if any.</param>
        /// <param name="powerType">Type of the power.</param>
        /// <param name="onUnit">The target acd GUID retriever.</param>
        /// <returns><c>RunStatus.Success</c> if successful, otherwise <c>RunStatus.Failure</c>.</returns>
        /// <remarks>Created 2012-04-09</remarks>
        public static Composite Cast(SNOPower power, ValueRetriever<Vector3> position, int powerType, ValueRetriever<int> onUnit, ValueRetriever<bool> extraCondition)
        {
            return Cast(power, position, ret => ZetaDia.Me.WorldDynamicId, powerType, onUnit, extraCondition);
        }

        /// <summary>
        /// Casts an AOE spell. eg; "Wave of Force".
        /// </summary>
        /// <param name="power">The sno power.</param>
        /// <param name="extraRequirements">The extra requirements.</param>
        /// <returns>
        ///   <c>RunStatus.Success</c> if successful, otherwise <c>RunStatus.Failure</c>.
        /// </returns>
        /// <remarks>
        /// Created 2012-04-09
        /// </remarks>
        public static Composite CastAOESpell(SNOPower power, ValueRetriever<bool> extraRequirements)
        {
            return Cast(power, ret => Vector3.Zero, 2, null, extraRequirements);
        }

        /// <summary>
        /// Casts a spell on a unit.
        /// </summary>
        /// <param name="power">The sno power.</param>
        /// <param name="powerType">Type of the power.</param>
        /// <param name="onUnit">The on unit.</param>
        /// <param name="extraRequirements">The extra requirements.</param>
        /// <returns>
        ///   <c>RunStatus.Success</c> if successful, otherwise <c>RunStatus.Failure</c>.
        /// </returns>
        /// <remarks>
        /// Created 2012-04-09
        /// </remarks>
        public static Composite CastOnUnit(SNOPower power, int powerType, ValueRetriever<int> onUnit, ValueRetriever<bool> extraRequirements = null)
        {
            return Cast(power, null, null, powerType, onUnit, extraRequirements);
        }

        /// <summary>
        /// Casts a spell on a Vector.
        /// </summary>
        /// <param name="power">The sno power.</param>
        /// <param name="position">The position.</param>
        /// <param name="extraRequirements">The extra requirements.</param>
        /// <returns>
        ///   <c>RunStatus.Success</c> if successful, otherwise <c>RunStatus.Failure</c>.
        /// </returns>
        /// <remarks>
        /// Created 2012-04-09
        /// </remarks>
        public static Composite CastAtLocation(SNOPower power, ValueRetriever<Vector3> position, ValueRetriever<bool> extraRequirements = null)
        {
            return Cast(power, position, 2, null, extraRequirements);
        }

        /// <summary>
        /// Buffs the specified sno power.
        /// </summary>
        /// <param name="power">The sno power.</param>
        /// <param name="extraRequirements">The extra requirements.</param>
        /// <returns></returns>
        /// <remarks>Created 2012-04-09</remarks>
        public static Composite Buff(SNOPower power, ValueRetriever<bool> extraRequirements = null)
        {
            return Cast(power, pos => Vector3.Zero, 2, null, extraRequirements);
        }
    }
}
