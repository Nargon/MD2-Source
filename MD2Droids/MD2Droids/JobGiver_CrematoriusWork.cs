using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace MD2
{
    public class JobGiver_CrematoriusWork : ThinkNode_JobGiver
    {
        protected override Job TryGiveTerminalJob(Pawn pawn)
        {
            if (pawn.GetType() != typeof(Crematorius))
            {
                return null;
            }
            Crematorius droid = (Crematorius)pawn;
            if (droid.Active)
            {
                List<CrematoriusTarget> list = droid.GetTargets.OrderBy((CrematoriusTarget t) => t.Priority).ThenBy((CrematoriusTarget t)=>t.NaturalPriority).ThenBy((CrematoriusTarget t) => t.Label).ToList();

                for (int i = 0; i < list.Count; i++)
                {
                    CrematoriusTarget target = list[i];
                    if (target.Mode != CrematoriusOperationMode.Off)
                    {
                        Corpse corpse = target.GetCorpse();
                        if (corpse != null)
                        {
                            JobDef jDef = (target.Mode == CrematoriusOperationMode.Butcher) ? DefDatabase<JobDef>.GetNamed("MD2DroidButcherCorpse") : DefDatabase<JobDef>.GetNamed("MD2DroidCremateCorpse");
                            return new Job(jDef, corpse);
                        }
                    }
                }
            }
            return null;
        }
    }
}
