using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace MD2
{
    public class Job_DroidRepair : JobDriver
    {
        private const TargetIndex RepairStationIndex = TargetIndex.A;

        private RepairableDroid Droid
        {
            get
            {
                return this.pawn as RepairableDroid;
            }
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            //Set what will cause the job to fail
            this.FailOnBurningImmobile(RepairStationIndex);
            this.FailOnDestroyedOrForbidden(RepairStationIndex);
            this.FailOn(delegate
            {
                return Droid != null && !Droid.ShouldGetRepairs;
            });

            //Reserve the repair station
            yield return Toils_Reserve.Reserve(RepairStationIndex);
            //Go to the repair station interaction cell
            yield return Toils_Goto.GotoThing(RepairStationIndex, PathMode.InteractionCell);
            //Make a new toil that sets the droid to repair mode, then wait until fully repaired
            RepairableDroid droid = pawn as RepairableDroid;
            Building_DroidRepairStation rps = TargetThingA as Building_DroidRepairStation;
            Toil toil = new Toil();
            toil.FailOnDestroyedOrForbidden(RepairStationIndex);
            toil.FailOn(() =>
                {
                    Pawn p = toil.GetActor();
                    Building_DroidRepairStation rps2 = TargetThingA as Building_DroidRepairStation;
                    if (!(p is RepairableDroid))
                        return true;
                    if (p.Position != TargetThingA.InteractionCell)
                        return true;
                    if (rps2 == null || !rps2.IsAvailable(p))
                        return true;
                    return false;
                });

            toil.initAction = () =>
                {
                    //Log.Message("initAction");
                    droid.BeingRepaired = true;
                    rps.RegisterRepairee(droid);
                };
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = rps.Def.repairTickCount * droid.RepairsNeededCount + 1;
            toil.tickAction = () =>
            {
                //Log.Message("Toil tick");
                if ( droid.RepairStation != null && Find.TickManager.TicksGame % droid.RepairStation.Def.repairTickCount == 0)
                {
                    if (droid.RepairStation.Power != null && !droid.RepairStation.Power.PowerOn)
                    {
                        return;
                    }
                    //Log.Message("Repaired");
                    droid.RepairTick();
                }
            };
            toil.AddEndCondition(delegate
            {
                RepairableDroid d = toil.GetActor() as RepairableDroid;
                if (d != null)
                {
                    if (d.RepairStation.Power != null && !d.RepairStation.Power.PowerOn)
                    {
                        return JobCondition.Incompletable;
                    }
                    if (d.ShouldGetRepairs)
                    {
                        //Log.Message("ongoing");
                        return JobCondition.Ongoing;
                    }
                    else
                    {
                        //Log.Message("Succeeded");
                        return JobCondition.Succeeded;
                    }
                }
                return JobCondition.Errored;

            });
            toil.AddFinishAction(delegate
            {
                //Log.Message("Finish action");
                rps.DeregisterRepairee(droid);
                droid.RepairTick();
                droid.BeingRepaired = false;
            });
            toil.WithEffect(DefDatabase<EffecterDef>.GetNamed("Repair"), TargetIndex.A);
            toil.WithSustainer(() => { return DefDatabase<SoundDef>.GetNamed("Interact_Repair"); });
            yield return toil;
        }
    }
}
