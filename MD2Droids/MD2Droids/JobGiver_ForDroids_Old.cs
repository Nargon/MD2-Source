using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace MD2
{
    public class JobGiver_ForDroids_Old : JobGiver_Work
    {
        private List<WorkGiver> workGivers = new List<WorkGiver>();
        public IEnumerable<WorkGiver> AllWorkGivers
        {
            get
            {
                return this.workGivers;
            }
        }
        public override void ResolveReferences()
        {
            base.ResolveReferences();
            foreach (WorkGiverDef current in
                from d in DefDatabase<WorkGiverDef>.AllDefs
                orderby d.priorityInType descending
                select d)
            {
                WorkGiver workGiver = (WorkGiver)Activator.CreateInstance(current.giverClass);
                workGiver.def = current;
                this.workGivers.Add(workGiver);
            }
        }

        private DroidKindDef droidKindDef(Pawn pawn)
        {
            return (DroidKindDef)pawn.kindDef;
        }

        protected override Job TryGiveTerminalJob(Pawn pawn)
        {
            if (!(pawn is Droid))
                return null;
            Droid droid = pawn as Droid;
            if (!droid.Active)
                return null;

            List<WorkTypeDef> list = new List<WorkTypeDef>();
            if (Find.PlaySettings.useWorkPriorities)
            {
                list = droidKindDef(pawn).allowedWorkTypeDefs.Where((WorkTypeDef def) => pawn.workSettings.WorkIsActive(def)).ToList();
                list = list.OrderBy(a => pawn.workSettings.GetPriority(a)).ThenByDescending(b => b.naturalPriority).ToList();
                //foreach (var c in list)
                //    Log.Message(c.defName);
            }
            else
            {
                list = droidKindDef(pawn).allowedWorkTypeDefs.Where((WorkTypeDef def) => pawn.workSettings.WorkIsActive(def)).ToList();
                list = list.OrderByDescending(b => b.naturalPriority).ToList();
                //foreach (var c in list)
                //    Log.Message(c.defName);
            }

            foreach (WorkTypeDef current in list)
            {
                if (!this.emergency || current.emergencyCat != EmergencyWorkCategory.None)
                {
                    if (this.emergency || current.emergencyCat != EmergencyWorkCategory.All)
                    {
                        int num = -999;
                        TargetInfo targetInfo = TargetInfo.Invalid;
                        WorkGiver workGiver = null;
                        for (int i = 0; i < this.workGivers.Count; i++)
                        {
                            WorkGiver giver = this.workGivers[i];
                            if (giver.def.workType == current)
                            {
                                if (giver.def.priorityInType != num && targetInfo.Valid)
                                {
                                    break;
                                }
                                if (this.emergency == giver.def.emergency)
                                {
                                    if (giver.MissingRequiredCapacity(pawn) == null)
                                    {
                                        if (!giver.ShouldSkip(pawn))
                                        {
                                            try
                                            {
                                                if (giver.def.scanThings)
                                                {
                                                    Predicate<Thing> predicate = (Thing t) => !t.IsForbidden(pawn.Faction) && giver.HasJobOnThing(pawn, t);
                                                    Predicate<Thing> validator = predicate;
                                                    Thing thing = GenClosest.ClosestThingReachable(pawn.Position, giver.PotentialWorkThingRequest, giver.PathMode, TraverseParms.For(pawn, Danger.Deadly, true), 9999f, validator, giver.PotentialWorkThingsGlobal(pawn), -1);
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
                                                    foreach (IntVec3 current2 in giver.PotentialWorkCellsGlobal(pawn))
                                                    {
                                                        float lengthHorizontalSquared = (current2 - position).LengthHorizontalSquared;
                                                        if (lengthHorizontalSquared < num2 && giver.HasJobOnCell(pawn, current2))
                                                        {
                                                            targetInfo = current2;
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
                                        }
                                    }
                                }
                            }
                        }
                        if (targetInfo.Valid)
                        {
                            pawn.mindState.lastGivenWorkType = current;
                            Job result;
                            if (targetInfo.HasThing)
                            {
                                result = workGiver.JobOnThing(pawn, targetInfo.Thing);
                                return result;
                            }
                            result = workGiver.JobOnCell(pawn, targetInfo.Cell);
                            return result;
                        }
                    }
                }
            }
            return null;
        }
    }
}
