﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace MD2
{
    public class PlacementRestrictor_OnFissure : PlaceWorker
    {
        private string reportString = "Must be placed on a fissure";
        public override AcceptanceReport AllowsPlacing(EntityDef checkingDef, IntVec3 loc, IntRot rot)
        {
            FissureClass thing = (FissureClass)Find.ThingGrid.ThingAt(loc, ThingDef.Named("mipFissure"));
            if (thing != null && thing.Position == loc)
                return true;
            else
                return reportString;
        }
    }
}
