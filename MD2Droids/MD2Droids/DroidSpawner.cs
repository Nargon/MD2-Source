using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace MD2
{
    public class DroidSpawner : ThingWithComps
    {
        public int i = 0;
        public override void SpawnSetup()
        {
            base.SpawnSetup();
            DroidKindDef def = DefDatabase<PawnKindDef>.GetNamed(this.def.label) as DroidKindDef;
            DroidGenerator.SpawnDroid(def, this.Position);
            this.Destroy();
        }
    }
}
