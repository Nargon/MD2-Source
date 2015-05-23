using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace MD2
{
    public static class FactionExt
    {
        public static Faction OfDroid
        {
            get
            {
                return Find.FactionManager.FirstFactionOfDef(FactionDef.Named("MD2DroidFaction"));
            }
        }
    }
}
