using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace MD2
{
    public class ListerDroids : MapComponent
    {
        public static ListerDroids listerDroids;
        private List<Droid> allDroids = new List<Droid>();

        public ListerDroids()
        {
            ListerDroids.listerDroids = this;
        }

        public static List<Droid> AllDroids
        {
            get
            {
                return listerDroids.allDroids;
            }
        }

        public static void RegisterDroid(Droid droid)
        {
            if(!listerDroids.allDroids.Contains(droid))
                listerDroids.allDroids.Add(droid);
        }

        public static void DeregisterDroid(Droid droid)
        {
            if (listerDroids.allDroids.Contains(droid))
                listerDroids.allDroids.Remove(droid);
        }


    }
}
