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
            DroidGenerator.SpawnDroid(DroidKinds.GetNamed(this.Label), this.Position);
            this.Destroy();
        }
    }
}
