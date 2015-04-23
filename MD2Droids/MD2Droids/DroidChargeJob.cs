using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace MD2
{
    class DroidChargeJob : JobDriver
    {

        protected override IEnumerable<Toil> MakeNewToils()
        {

            DroidChargePad charger = (DroidChargePad)TargetThingA;
            Toil goToPad = Toils_Goto.GotoThing(TargetIndex.A, PathMode.OnCell);
            goToPad.AddFailCondition(() => { return !charger.IsAvailable(pawn); });
            yield return goToPad;
            Droid droid = (Droid)this.pawn;
            Toil charge = new Toil();
            charge.initAction = () =>
                {
                    if (charger.Position != droid.Position)
                    {
                        pawn.jobs.EndCurrentJob(JobCondition.Errored);
                    }
                };
            charge.defaultCompleteMode = ToilCompleteMode.Never;
            charge.AddFailCondition(() =>
                { return !charger.IsAvailable(pawn); });
            charge.AddEndCondition(() =>
                {
                    if (!droid.DesiresCharge())
                    {
                        return JobCondition.Succeeded;
                    }
                    return JobCondition.Ongoing;
                });
            yield return charge;
        }
    }
}
