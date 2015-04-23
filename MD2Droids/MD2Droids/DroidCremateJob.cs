using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace MD2
{
    public class DroidCremateJob : JobDriver
    {
        private const TargetIndex CorpseIndex = TargetIndex.A;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            //Set what will cause the job to fail:
            this.FailOnDestroyedOrForbidden(CorpseIndex);
            this.FailOnBurningImmobile(CorpseIndex);

            //Reserve the corpse
            yield return Toils_Reserve.Reserve(CorpseIndex);
            //Go to the corpse
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathMode.ClosestTouch);
            Toil toil = new Toil();
            toil.initAction = () =>
                {
                    //Check if the pawn is set to strip bodies, if yes then strip it, otherwise skip this step
                    Crematorius droid = (Crematorius)pawn;
                    if (droid.stripBodies)
                    {
                        Corpse corpse = (Corpse)this.TargetA.Thing;
                        if (corpse.AnythingToStrip())
                            corpse.Strip();
                    }
                };
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = 200;
            toil.WithEffect(() => DefDatabase<EffecterDef>.GetNamed("Cremate"), CorpseIndex);
            toil.WithSustainer(() => DefDatabase<SoundDef>.GetNamed("Recipe_Cremate"));
            toil.AddFinishAction(() => TargetA.Thing.Destroy());
            yield return toil;
        }
    }
}
