using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace MD2
{
    public class CrematoriusTarget : IExposable
    {
        private CrematoriusOperationMode mode = CrematoriusOperationMode.Off;
        private int priority = 4;
        private int naturalPriority;
        private Predicate<Thing> predicate;
        private string label = "";
        private Crematorius crematorius;
        public bool OnlyRotten = false;

        public CrematoriusOperationMode Mode
        {
            get
            {
                return mode;
            }
            set
            {
                mode = value;
            }
        }

        public int Priority
        {
            get
            {
                return priority;
            }
            set
            {
                priority = value;
                if (priority < 1)
                    priority = 4;
                if (priority > 4)
                    priority = 1;
            }
        }

        public int NaturalPriority
        {
            get
            {
                return naturalPriority;
            }
        }

        public Predicate<Thing> Predicate
        {
            get
            {
                if (mode == CrematoriusOperationMode.Butcher)
                {
                    Predicate<Thing> p = (Thing t) => predicate(t) && (t.TryGetComp<CompRottable>() == null || t.TryGetComp<CompRottable>().Stage == RotStage.Fresh);
                    return p;
                }
                return predicate;
                
            }
            set
            {
                predicate = value;
                if (predicate == null)
                    predicate = (Thing c) => false;
            }
        }

        public string Label
        {
            get
            {
                return label;
            }
        }

        public CrematoriusTarget(string label, Predicate<Thing> p, int naturalPriority, Crematorius c)
        {
            this.label = label;
            this.naturalPriority = naturalPriority;
            this.Predicate = p;
            this.crematorius = c;
        }

        public virtual Corpse GetCorpse()
        {
            if (mode != CrematoriusOperationMode.Off)
            {
                return CorpseFinderUtility.FindClosestCorpseFor(this.Predicate, this.crematorius);
            }
            return null;
        }

        public void ExposeData()
        {
            Scribe_Values.LookValue(ref this.mode, "mode", CrematoriusOperationMode.Off);
            Scribe_Values.LookValue(ref this.priority, "priority");
            Scribe_Values.LookValue(ref this.OnlyRotten, "OnlyRotten");
        }
    }
}
