using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MD2
{
    public static class DroidAllowedWorkUtils
    {
        public static List<WorkGiver> WorkGiversInOrderEmergency(Droid droid)
        {
            List<WorkGiver> list = new List<WorkGiver>();

            foreach(var wg in droid.workSettings.WorkGiversInOrderEmergency)
            {
                if(droid.KindDef.allowedWorkTypeDefs.Contains(wg.def.workType))
                {
                    list.Add(wg);
                }
            }

            return list;
        }

        public static List<WorkGiver> WorkGiversInOrder(Droid droid)
        {
            List<WorkGiver> list = new List<WorkGiver>();

            foreach(var wg in droid.workSettings.WorkGiversInOrderNormal)
            {
                if(droid.KindDef.allowedWorkTypeDefs.Contains(wg.def.workType))
                {
                    list.Add(wg);
                }
            }

            return list;
        }
    }
}
