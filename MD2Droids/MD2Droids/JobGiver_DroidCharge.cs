using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace MD2
{
    public class JobGiver_DroidCharge : ThinkNode_JobGiver
    {
        public float chargeThreshold;
        public float distance;
        public float healthThreshold;

        public JobGiver_DroidCharge(float chargeThreshold, float distance, float healthThreshold)
        {
            this.chargeThreshold = chargeThreshold;
            this.distance = distance;
            this.healthThreshold = healthThreshold;
        }

        protected override Job TryGiveTerminalJob(Pawn pawn)
        {

            if (!(pawn is Droid))
            {
                return null;
            }
            Droid droid = (Droid)pawn;
            if (!droid.Active)
                return null;
            //Check the charge level
            if (droid.TotalCharge < droid.maxEnergy * chargeThreshold)
            {
                IEnumerable<Thing> chargers;
                List<Thing> list = new List<Thing>();
                foreach(var c in Find.ListerBuildings.AllBuildingsColonistOfClass<DroidChargePad>())
                {
                    list.Add((Thing)c);
                }
                chargers = list.AsEnumerable();
                Predicate<Thing> pred = (Thing thing) => { return ((DroidChargePad)thing).IsAvailable(droid); };
                Thing target = GenClosest.ClosestThing_Global_Reachable(pawn.Position, chargers, PathMode.OnCell, TraverseParms.For(pawn), distance, pred);
                if (target != null)
                {
                    return new Job(DefDatabase<JobDef>.GetNamed("MD2ChargeDroid"), new TargetInfo(target));
                }
            }
            return null;
        }
    }

    public class JobGiver_TopUp : JobGiver_DroidCharge
    {
        public JobGiver_TopUp() : base(0.9f, 10f, 0.8f) { }
    }
    public class JobGiver_StaySafe : JobGiver_DroidCharge
    {
        public JobGiver_StaySafe() : base(0.5f, 50f, 0.5f) { }
    }
    public class JobGiver_Critical : JobGiver_DroidCharge
    {
        public JobGiver_Critical() : base(0.1f, 9999f, 0.2f) { }
    }
}
