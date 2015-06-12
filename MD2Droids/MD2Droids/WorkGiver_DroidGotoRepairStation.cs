using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace MD2
{
    public class WorkGiver_DroidGotoRepairStation : WorkGiver
    {
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);
            }
        }

        public override PathMode PathMode
        {
            get
            {
                return PathMode.InteractionCell;
            }
        }

        public override bool ShouldSkip(Pawn pawn)
        {
            RepairableDroid droid = pawn as RepairableDroid;
            return droid != null && !droid.BeingRepaired && !droid.ShouldGetRepairs;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t)
        {
            Building_DroidRepairStation rps = t as Building_DroidRepairStation;
            return rps != null && rps.IsAvailable(pawn) && pawn.CanReserveAndReach(rps, PathMode, Danger.Some, 1);
        }

        public override Job JobOnThing(Pawn pawn, Thing t)
        {
            return new Job(DefDatabase<JobDef>.GetNamed("MD2DroidGotoRepairStation"), t);
        }
    }
}
