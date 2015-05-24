using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace MD2
{
    public static class DroidResearchMods
    {
        public static void DroidSolarShielding()
        {
            foreach(var def in DefDatabase<DroidKindDef>.AllDefs)
            {
                def.disableOnSolarFlare = false;
            }
        }
    }
}
