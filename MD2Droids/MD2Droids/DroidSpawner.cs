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
            //Log.Message("In the spawner");
            DroidGenerator.SpawnDroid(DroidKinds.GetNamed(this.Label), this.Position);
            this.Destroy();

        }

        //public override void Tick()
        //{
        //    base.Tick();
        //    i++;
        //    if (i > 10)
        //    {
        //        DroidGenerator.SpawnDroid(DroidKinds.GetNamed(this.Label), this.Position);
        //        this.Destroy();
        //    }
        //}
    }
}
