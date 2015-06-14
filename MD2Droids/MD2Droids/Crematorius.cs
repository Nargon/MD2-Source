using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MD2
{
    public class Crematorius : Droid
    {
        private bool stripBodies = true;

        private static readonly Predicate<Thing> animalPredicate = (Thing t) => (t is Corpse) && ((Corpse)t).innerPawn.RaceProps.Animal && !((Corpse)t).innerPawn.RaceProps.mechanoid && !((Corpse)t).innerPawn.RaceProps.Humanlike && !t.IsBuried();
        private static readonly Predicate<Thing> humanPredicate = (Thing t) => (t is Corpse) && !((Corpse)t).innerPawn.RaceProps.Animal && !((Corpse)t).innerPawn.RaceProps.mechanoid && ((Corpse)t).innerPawn.RaceProps.Humanlike && !t.IsBuried();
        private static readonly Predicate<Thing> mechanoidPredicate = (Thing t) => (t is Corpse) && !((Corpse)t).innerPawn.RaceProps.Animal && ((Corpse)t).innerPawn.RaceProps.mechanoid && !((Corpse)t).innerPawn.RaceProps.Humanlike && !t.IsBuried();

        private string animalLabel = "AnimalCorpses".Translate();
        private string humanLabel = "HumanoidCorpses".Translate();
        private string mechanoidLabel = "MechanoidCorpses".Translate();

        private CrematoriusTarget animalLike;
        private CrematoriusTarget humanLike;
        private CrematoriusTarget mechanoid;

        private List<CrematoriusTarget> targetList = new List<CrematoriusTarget>();

        private readonly Texture2D ShirtIcon = ContentFinder<Texture2D>.Get("UI/Commands/ShirtIcon");

        public bool StripBodies
        {
            get
            {
                return stripBodies;
            }
            set
            {
                stripBodies = value;
            }
        }

        public CrematoriusTarget AnimalLike
        {
            get
            {
                return animalLike;
            }
        }

        public CrematoriusTarget HumanLike
        {
            get
            {
                return humanLike;
            }
        }

        public CrematoriusTarget Mechanoid
        {
            get
            {
                return mechanoid;
            }
        }

        public virtual IEnumerable<CrematoriusTarget> GetTargets
        {
            get
            {
                yield return animalLike;
                yield return humanLike;
                yield return mechanoid;
            }
        }

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            animalLike = new CrematoriusTarget(animalLabel, animalPredicate, 2, this);
            humanLike = new CrematoriusTarget(humanLabel, humanPredicate, 1, this);
            mechanoid = new CrematoriusTarget(mechanoidLabel, mechanoidPredicate, 3, this);
        }

        public override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame % 100 == 0 && this.Active)
            {
                GenTemperature.PushHeat(this.Position, 5f);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue(ref this.stripBodies, "stripBodies", true);
            Scribe_Deep.LookDeep(ref this.animalLike, "animalLike", new object[] { animalLabel, animalPredicate, 2, this });
            Scribe_Deep.LookDeep(ref this.humanLike, "humanLike", new object[] { humanLabel, humanPredicate, 1, this });
            Scribe_Deep.LookDeep(ref this.mechanoid, "mechanoid", new object[] { mechanoidLabel, mechanoidPredicate, 3, this });
        }

        public override IEnumerable<FloatMenuOption> GetExtraFloatMenuOptionsFor(IntVec3 sq)
        {
            foreach (var thing in sq.GetThingList())
            {
                if (thing is Corpse)
                {
                    Corpse corpse = thing as Corpse;
                    CompRottable rot = corpse.TryGetComp<CompRottable>();
                    if (rot == null || (rot != null && rot.Stage == RotStage.Fresh))
                    {
                        //Butcher the corpse
                        yield return new FloatMenuOption("DroidButcher".Translate(corpse.LabelCap), delegate
                        {
                            jobs.StartJob(new Job(DefDatabase<JobDef>.GetNamed("MD2DroidButcherCorpse"), corpse), JobCondition.InterruptForced);
                        });
                    }
                    //Cremate the corpse
                    yield return new FloatMenuOption("DroidCremate".Translate(corpse.LabelCap), delegate
                    {
                        jobs.StartJob(new Job(DefDatabase<JobDef>.GetNamed("MD2DroidCremateCorpse"), corpse), JobCondition.InterruptForced);
                    });
                }
            }

            foreach (var o in base.GetExtraFloatMenuOptionsFor(sq))
            {
                yield return o;
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (base.GetGizmos() != null)
            {
                foreach (Gizmo c in base.GetGizmos())
                {
                    yield return c;
                }
            }
            Command_Toggle a = new Command_Toggle();
            a.toggleAction = () =>
            {
                stripBodies = !stripBodies;
            };
            a.activateSound = SoundDefOf.Click;
            if (this.stripBodies)
            {
                a.defaultDesc = "Click to toggle off stripping bodies";
                a.defaultLabel = "Stripping bodies";
            }
            else
            {
                a.defaultDesc = "Click to toggle on stripping bodies";
                a.defaultLabel = "Not stripping bodies";
            }
            a.isActive = () => { return this.stripBodies; };
            a.hotKey = Keys.Named("CrematoriusToggleStripBodies");
            a.disabled = false;
            a.groupKey = 313740005;
            a.icon = this.ShirtIcon;
            yield return a;
        }

        public override string GetInspectString()
        {
            StringBuilder s = new StringBuilder();
            s.Append(base.GetInspectString());
            if (this.stripBodies)
                s.AppendLine("Will strip bodies");
            else
                s.AppendLine("Will not strip bodies");
            return s.ToString();
        }
    }
}
