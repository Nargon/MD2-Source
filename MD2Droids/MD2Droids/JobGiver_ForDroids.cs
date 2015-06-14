using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace MD2
{
    public class JobGiver_ForDroids : JobGiver_Work
    {
        public bool emergency;

        protected override Job TryGiveTerminalJob(Pawn pawn)
        {
            
            if (!(pawn is Droid))
                return null;
            Droid droid = pawn as Droid;

            if (!droid.Active)
                return null;

            List<WorkGiver> list = this.emergency ? DroidAllowedWorkUtils.WorkGiversInOrderEmergency(droid) : DroidAllowedWorkUtils.WorkGiversInOrder(droid);

            int num = -999;
            TargetInfo targetInfo = TargetInfo.Invalid;
            WorkGiver workGiver = null;
            for (int i = 0; i < list.Count; i++)
            {
                WorkGiver giver = list[i];
                if (giver.def.priorityInType != num && targetInfo.IsValid)
                {
                    break;
                }
                if (giver.MissingRequiredCapacity(pawn) == null)
                {
                    if (!giver.ShouldSkip(pawn))
                    {
                        try
                        {
                            if (giver.def.scanThings)
                            {
                                Predicate<Thing> predicate = (Thing t) => !t.IsForbidden(pawn) && giver.HasJobOnThing(pawn, t);
                                IEnumerable<Thing> enumerable = giver.PotentialWorkThingsGlobal(pawn);
                                Predicate<Thing> validator = predicate;
                                Thing thing = GenClosest.ClosestThingReachable(pawn.Position, giver.PotentialWorkThingRequest, giver.PathEndMode, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, enumerable, giver.LocalRegionsToScanFirst, enumerable != null);
                                if (thing != null)
                                {
                                    targetInfo = thing;
                                    workGiver = giver;
                                }
                            }
                            if (giver.def.scanCells)
                            {
                                IntVec3 position = pawn.Position;
                                float num2 = 99999f;
                                foreach (IntVec3 current in giver.PotentialWorkCellsGlobal(pawn))
                                {
                                    float lengthHorizontalSquared = (current - position).LengthHorizontalSquared;
                                    if (lengthHorizontalSquared < num2 && !current.IsForbidden(pawn) && giver.HasJobOnCell(pawn, current))
                                    {
                                        targetInfo = current;
                                        workGiver = giver;
                                        num2 = lengthHorizontalSquared;
                                    }
                                }
                            }
                            num = giver.def.priorityInType;
                        }
                        catch (Exception ex)
                        {
                            Log.Error(string.Concat(new object[]
							{
								pawn,
								" threw exception in WorkGiver ",
								giver.def.defName,
								": ",
								ex.ToString()
							}));
                        }
                        finally
                        {
                        }
                        if (targetInfo.IsValid)
                        {
                            pawn.mindState.lastGivenWorkType = giver.def.workType;
                            if (targetInfo.HasThing)
                            {
                                return workGiver.JobOnThing(pawn, targetInfo.Thing);
                            }
                            return workGiver.JobOnCell(pawn, targetInfo.Cell);
                        }
                    }
                }
            }
            return null;
        }
    }
}
