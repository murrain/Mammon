using System;
using System.Windows;

using Mammon.Dynamics;
using Mammon.Routines;
using Zeta;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
using Zeta.TreeSharp;


/*
 * I present you Mammon the Price of Greed as he was also known for!
 * What could be more fitting than that for a bot that act upon our greed for gold.
 * 
 * Credtis to Diktat who came up with the name.
 * 
 * 
 * Disclaimer: Mammon is a Forked version of Belphegor, and i do not claim to have written the base for this 
 * routine, the BuddyTeam have done a great job with it and its a really nice starting point for what could become 
 * a very powerfull all-in-one combat routine.
 * 
 * Originally i started out writing code that was copy pasted above the original Belphegor code and it quickly 
 * became visible that doing this will make a crapload of headace. And i had already made plans to start on more 
 * class routines later, by doing this i had to make a fork of Belphegor or atleast a build that could run side-by-
 * side with belphegor, and this will also make it easier for everyone if they/you want to use the Original combat
 * routine that is shipped with Demonbuddy. 
 */

namespace Mammon
{
    public class Mammon : CombatRoutine
    {
        private Version version = new Version(1, 0, 0, 13);

        public Mammon()
        {
            Instance = this;
        }

        public static Mammon Instance { get; private set; }

        #region Overrides of CombatRoutine

        private Composite _combat;
        private Composite _buff;
        //private Composite _pull;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public override void Dispose()
        {
            Pulsator.OnPulse -= SetBehaviorPulse;
        }

        /// <summary>
        /// Gets the name of this <see cref="CombatRoutine"/>.
        /// </summary>
        /// <remarks>Created 2012-04-03</remarks>
        public override string Name { get { return "Mammon All-in-One Ver." + version.ToString(); } }

        /// <summary>
        /// Gets the configuration window.
        /// </summary>
        /// <remarks>Created 2012-04-03</remarks>
        public override Window ConfigWindow { get { return null; } }

        /// <summary>
        /// Gets the class.
        /// </summary>
        /// <remarks>Created 2012-04-03</remarks>
        public override ActorClass Class
        {
            get
            {
                if (!ZetaDia.IsInGame || ZetaDia.IsLoadingWorld)
                {
                    // Return none if we are oog to make sure we can start the bot anytime.
                    return ActorClass.Invalid;
                }
                
                return ZetaDia.Me.ActorClass;
            }
        }

        public override float DestroyObjectDistance
        {
            get { return 30; }
        }

        public override SNOPower DestroyObjectPower { get { return ZetaDia.Me.GetHotbarPowerId(HotbarSlot.HotbarMouseLeft); } }

        /// <summary>
        /// Gets me.
        /// </summary>
        /// <remarks>Created 2012-05-08</remarks>
        public DiaActivePlayer Me { get { return ZetaDia.Me; } }

        /// <summary>
        /// Initializes this <see cref="CombatRoutine"/>.
        /// </summary>
        /// <remarks>Created 2012-04-03</remarks>
        public override void Initialize()
        {
            GameEvents.OnLevelUp += Monk.MonkOnLevelUp;
            GameEvents.OnLevelUp += Wizard.WizardOnLevelUp;
            GameEvents.OnLevelUp += WitchDoctor.WitchDoctorOnLevelUp;
            GameEvents.OnLevelUp += DemonHunter.DemonHunterOnLevelUp;
            GameEvents.OnLevelUp += Barbarian.BarbarianOnLevelUp;

            if (!CreateBehaviors())
            {
                BotMain.Stop();
                return;
            }

            _lastClass = Class;
            Pulsator.OnPulse += SetBehaviorPulse;

            Logger.Write("Behaviors created");
        }

        /// <summary>
        /// Gets the combat behavior.
        /// </summary>
        /// <remarks>Created 2012-04-03</remarks>
        public override Composite Combat { get { return _combat; } }
        public override Composite Buff { get { return _buff; } }
        //public override Composite Pull { get { return _pull; } }

        private ActorClass _lastClass = ActorClass.Invalid;
        public void SetBehaviorPulse(object sender, EventArgs args)
        {
            if (ZetaDia.IsInGame && !ZetaDia.IsLoadingWorld && ZetaDia.Me != null && ZetaDia.Me.CommonData != null)
            {
                if (_combat == null || _buff == null || ZetaDia.Me.IsValid && Class != _lastClass)
                {
                    if (!CreateBehaviors())
                    {
                        BotMain.Stop();
                        return;
                    }

                    Logger.Write("Behaviors created");
                    _lastClass = Class;
                }
            }
        }

        public bool CreateBehaviors()
        {
            int count;
            _combat = CompositeBuilder.GetComposite(Class, BehaviorType.Combat, out count);

            int count2;
            _buff = CompositeBuilder.GetComposite(Class, BehaviorType.OutOfCombat, out count2);

            /*
            int count3;
            _pull = CompositeBuilder.GetComposite(Class, BehaviorType.Pull, out count3);
            */

            if (count == 0 || _combat == null)
            {
                Logger.Write("Combat support for " + Class + " is not currently implemented.");
                return false;
            }

            if (count2 == 0 || _buff == null)
            {
                Logger.Write("Buff support for " + Class + " is not currently implemented.");
                return false;
            }
            /*
            if (count3 == 0 || _pull == null)
            {
                Logger.Write(String.Format("Pull support for {0} is not currently implemented.", Class));
                return false;
            }
             */

            return true;
        }

        #endregion
    }
}
